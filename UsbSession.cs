using LibUsbDotNet;
using LibUsbDotNet.LibUsb;
using LibUsbDotNet.Main;
using System;
using System.Linq;
using System.Text;

namespace UsbSession
{
    public class UsbSession
    {
        private IUsbDevice Device;
        private UsbEndpointReader reader;
        private UsbEndpointWriter writer;
        private UsbContext context;

        public UsbSession()
        {
            context = new UsbContext();
            
            context.SetDebugLevel(LogLevel.Info);

            //TODO Device.Descriptor contains "Crestron"
            var CrestronDevices = context.List().Where(d => d.VendorId == 0x14BE);
            if (CrestronDevices.Count() == 0)
            {
                throw new Exception("No Crestron Devices Present");
            }
            else if (CrestronDevices.Count() > 1)
            {
                throw new Exception("More than one Crestron device present, please only connect the one you want to connect to");
            }
            else
            {
                Device = CrestronDevices.FirstOrDefault();
            }
        }

        public UsbSession(int VendorID)
        {
            context = new UsbContext();
            context.SetDebugLevel(LogLevel.Info);

            var usbDeviceCollection = context.List();
            Device = usbDeviceCollection.FirstOrDefault(
                d => d.VendorId == VendorID);
        }
        public UsbSession(int ProductID, int VendorID)
        {
            context = new UsbContext();
            context.SetDebugLevel(LogLevel.Info);

            var usbDeviceCollection = context.List();
            Device = usbDeviceCollection.FirstOrDefault(
                d => d.ProductId == ProductID && d.VendorId == VendorID);//PID is 0x9, VID is 0x14BE
        }

        public void Open()
        {
            try
            {
                if(!(Device is object))
                {
                    throw new Exception("Open() called on null USB device");
                }
                if (Device.IsOpen)
                {
                    return;
                }

                Device.Open();

                //Do I have to do that "whole" check here too?
                // If this is a "whole" usb device (libusb-win32, linux libusb-1.0)
                // it exposes an IUsbDevice interface. If not (WinUSB) the 
                // 'wholeUsbDevice' variable will be null indicating this is 
                // an interface of a device; it does not require or support 
                // configuration and interface selection.
                //IUsbDevice wholeUsbDevice = Device as IUsbDevice;
                //if (!ReferenceEquals(wholeUsbDevice, null))
                //{
                    // This is a "whole" USB device. Before it can be used, 
                    // the desired configuration and interface must be selected.

                    // Select config #1
                    //wholeUsbDevice.SetConfiguration(1);
                    Device.SetConfiguration(1);

                    // Claim interface #0.
                    Device.ClaimInterface(Device.Configs[0].Interfaces[0].Number);
                //}
                

                writer = Device.OpenEndpointWriter(WriteEndpointID.Ep02); //02
                reader = Device.OpenEndpointReader(ReadEndpointID.Ep01); //129, but looks like crestron also supports 131?
            }
            catch (Exception ex)
            {
                this.Close();
                throw ex;
            }

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

                Error ec = writer.Write(Encoding.ASCII.GetBytes(TerminatedCommand), 3000, out int bytesWritten);
                if (ec != Error.Success) throw new Exception("Writer error");// switchUsbDevice.LastErrorString);

                byte[] readBuffer = new byte[1];
                while (ec == Error.Success)
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
                    //IUsbDevice wholeUsbDevice = Device as IUsbDevice;
                    //if (!ReferenceEquals(wholeUsbDevice, null))
                    //{
                    // Release interface #0.
                    //wholeUsbDevice.ReleaseInterface(Device.Configs[0].Interfaces[0].Number);
                    bool result = Device.ReleaseInterface(Device.Configs[0].Interfaces[0].Number);
                    
                    //}
                    //writer.Device.Close();
                    //reader.Device.Close();
                    Device.ResetDevice();
                    writer.Device.Dispose();
                    reader.Device.Dispose();
                    Device.Dispose();
                    Device = null;
                    context.Dispose();
                }
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

