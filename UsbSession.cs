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

        public UsbSession()
        {
            UsbRegDeviceList AllDevices = UsbDevice.AllDevices; 
            UsbRegDeviceList CrestronDevices = AllDevices.FindAll( d => Regex.Match(d.Name,"Crestron*").Success );
 
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

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public void ClearReadBuffer()
        {
            if (Device == null || !Device.IsOpen)
            {
                throw new Exception("Open the device before trying to clear the buffer");
            }

            reader.ReadFlush();
        }

        public string Invoke(string Command)
        {
            try
            {
                if (Device == null || !Device.IsOpen)
                {
                    throw new Exception("Open the device before invoking a command");
                }

                string TerminatedCommand = Command + "\r\n";
                string response = "";

                //Write the command
                ErrorCode ec = writer.Write(Encoding.ASCII.GetBytes(TerminatedCommand), 3000, out int bytesWritten);
                if (ec != ErrorCode.None) throw new Exception("Writer error");// switchUsbDevice.LastErrorString);

                //Read the response
                byte[] readBuffer = new byte[1];
                while (ec == ErrorCode.None)
                {
                    // If the device hasn't sent data in the last 100 milliseconds,
                    // a timeout error (ec = IoTimedOut) will occur. 
                    ec = reader.Read(readBuffer, 100, out int bytesRead);

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
                    if (newChar.Equals(">"))
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
        /// Read x bytes the RX buffer.
        /// </summary>
        /// <returns>Number of bytes read</returns>
        public int Read()
        {
            byte[] readBuffer = new byte[1000];
                   
            reader.Read(readBuffer, 0, out int bytesRead);
            
            return bytesRead;
        }

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
        

        public bool TestSession()
        {
            if(Device is object)
            {
                return Device.IsOpen;
            }

            return false;
        } 
    }
}

