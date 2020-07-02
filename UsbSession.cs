using LibUsbDotNet;
using LibUsbDotNet.Main;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace UsbSession
{
    public class UsbSession
    {
        private static UsbDevice Device;
        private UsbEndpointReader reader;
        private UsbEndpointWriter writer;
        private UsbDeviceFinder Finder;

        /// <summary>
        /// Constructor looks for a single Crestron device connected to system and initializes resources for interfacing with it
        /// </summary>
        /// <throws>
        /// Exception thrown if there are no Crestron devices or more than one Crestron device. Currently the library doesn't work with multiple Crestron USB devices
        /// </throws>
        public UsbSession()
        {
            UsbRegDeviceList AllDevices = UsbDevice.AllDevices; 
            UsbRegDeviceList CrestronDevices = AllDevices.FindAll( d => (Regex.Match(d.Name,"Crestron*").Success || (d.Vid == 0x14BE) ) );
 
            if (CrestronDevices.Count == 0)
            {
                throw new Exception("No Crestron Devices Present");
            }
            else if (CrestronDevices.Count > 1)
            {
                throw new Exception("More than one Crestron device present, please only connect the one you want to connect to");
            }
            else
            {
                //RMC3 has a device ID of 0x9
                //Will document others here.
                //Crestron Vendor ID is 0x14BE
                Finder = new UsbDeviceFinder(0x14BE);
                Device = UsbDevice.OpenUsbDevice(Finder);
            }
        }

        /// <summary>
        /// Opens a single Crestron USB device for sending a command to. Does not force flush buffers.
        /// </summary>
        public void Open()
        {
            try
            {
                // If the device is open and ready
                if (Device == null) throw new Exception("Device Not Found.");

                // If this is a "whole" usb device (libusb-win32, linux libusb)
                // it will have an IUsbDevice interface. If not (WinUSB) the 
                // variable will be null indicating this is an interface of a 
                // device.
                IUsbDevice wholeUsbDevice = Device as IUsbDevice;
                if (!ReferenceEquals(wholeUsbDevice, null))
                {
                    // This is a "whole" USB device. Before it can be used, 
                    // the desired configuration and interface must be selected.

                    // Select config #1
                    wholeUsbDevice.SetConfiguration(1);

                    // Claim interface #0.
                    wholeUsbDevice.ClaimInterface(0);
                }

                writer = Device.OpenEndpointWriter(WriteEndpointID.Ep02); //02
                reader = Device.OpenEndpointReader(ReadEndpointID.Ep01); //129, but looks like crestron also supports 131?
                //reader = Device.OpenEndpointReader(ReadEndpointID.Ep03); //131

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Flush the Read buffer for the USB device
        /// </summary>
        public void ClearReadBuffer()
        {
            if (Device == null || !Device.IsOpen)
            {
                throw new Exception("Open the device before trying to clear the buffer");
            }

            reader.ReadFlush();
        }

        /// <summary>
        /// Invokes a command on the console of the Crestron device. 
        /// </summary>
        /// <param name="Command">Console command, as typed out on the console. Do not add a LF or CR at the end.</param>
        /// <param name="Prompt">  Specifies the prompt to wait for in the response. This may be a character or a string 
        /// but must be entered using a regular expression. Defaults to the right angle bracket '>'.</param>
        /// <returns>Output of the command to console until the "prompt" is encountered</returns>
        public string Invoke(string Command, string Prompt = ">")
        {
            try
            {
                if (Device == null || !Device.IsOpen)
                {
                    throw new Exception("Open the device before invoking a command");
                }

                //Convert the Prompt to a regex
                Regex regex = new Regex(Prompt);

                string TerminatedCommand = Command + "\r\n";
                string response = "";

                //Write the command
                ErrorCode ec = writer.Write(Encoding.ASCII.GetBytes(TerminatedCommand), 3000, out int bytesWritten);
                if (ec != ErrorCode.None) throw new Exception("Writer error");// switchUsbDevice.LastErrorString);

                //Read the response
                //According to libusb mailing lists they suggest this buffer be a multiple of the endpoint interface
                //max transfer size (512 in this case)
                byte[] readBuffer = new byte[1024];
                while (ec == ErrorCode.None)
                {
                    // If the device hasn't sent data in the last 5000 milliseconds,
                    // a timeout error (ec = IoTimedOut) will occur.
                    // Was originally 100 ms but it seems that when authentication is enabled it can cause large read delays.
                    ec = reader.Read(readBuffer, 5000, out int bytesRead);

                    //Don't want to throw this exception, authentication may be enabled!
                    //if (bytesRead == 0) throw new Exception("No more bytes!");
                    if (bytesRead == 0)
                    {
                        return response;
                    }

                    // Write that output to the console.
                    //Console.Write(Encoding.Default.GetString(readBuffer, 0, bytesRead));

                    string newChar = Encoding.ASCII.GetString(readBuffer, 0, bytesRead);
                    response += newChar;
                    if (regex.Match(response).Success)
                    {
                        return response;
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                this.Close();
                throw ex;
            }
        }

        /// <summary>
        /// Read bytes the RX buffer.
        /// </summary>
        /// <returns>Number of bytes read</returns>
        public int Read()
        {
            byte[] readBuffer = new byte[1000];

            ErrorCode ec = reader.Read(readBuffer, 5000, out int bytesRead);

            if (ec != ErrorCode.None) throw new Exception("Reader error");

            return bytesRead;
        }

        /// <summary>
        /// Closes a USB device and disposes of resources and handles. Does not completely free it from the underlying OS - call Exit() for that
        /// </summary>
        public void Close()
        {
            if (Device != null)
            {
                if (Device.IsOpen)
                {
                    // If this is a "whole" usb device (libusb-win32, linux libusb-1.0)
                    // it exposes an IUsbDevice interface. If not (WinUSB) the 
                    // 'wholeUsbDevice' variable will be null indicating this is 
                    // an interface of a device; it does not require or support 
                    // configuration and interface selection.
                    IUsbDevice wholeUsbDevice = Device as IUsbDevice;
                    if (!ReferenceEquals(wholeUsbDevice, null))
                    {
                        // Release interface #0.
                        wholeUsbDevice.ReleaseInterface(0);
                    }

                    Device.Close();
                }
                Device = null;

                // Free usb resources - Similar to unplugging the usb
                //UsbDevice.Exit();

            }
        }

        /// <summary>
        /// Free the USB Resources - similar to unplugging the usb or "Safely ejecting"
        /// Calls close() if the device is still "open"
        /// </summary>
        public void Exit()
        {
            if (Device != null)
            {
                if (Device.IsOpen)
                {
                    // If this is a "whole" usb device (libusb-win32, linux libusb-1.0)
                    // it exposes an IUsbDevice interface. If not (WinUSB) the 
                    // 'wholeUsbDevice' variable will be null indicating this is 
                    // an interface of a device; it does not require or support 
                    // configuration and interface selection.
                    IUsbDevice wholeUsbDevice = Device as IUsbDevice;
                    if (!ReferenceEquals(wholeUsbDevice, null))
                    {
                        // Release interface #0.
                        wholeUsbDevice.ReleaseInterface(0);
                    }

                    Device.Close();
                }
                Device = null;

                // Free usb resources - Similar to unplugging the usb
                UsbDevice.Exit();
            }
            else
            {
                // Free usb resources - Similar to unplugging the usb
                UsbDevice.Exit();
            }
        }
        
        /// <summary>
        /// Test if the USB device has been opened with Open()
        /// </summary>
        /// <returns>True if the device is Open and ready to send commands to with Invoke(), and false otherwise</returns>
        public bool TestSession()
        {
            if(Device is object)
            {
                return Device.IsOpen;
            }
            else
            {
                return false;
            }
        } 
    }
}

