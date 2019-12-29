using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LibUsbDotNet;
using LibUsbDotNet.Info;
using LibUsbDotNet.Main;
using System.Collections.ObjectModel;
using System.Threading;

namespace UsbTester
{
    public partial class Form1 : Form
    {
        byte[] readBuffer = new byte[8];
        public static UsbDevice MyUsbDevice;
        UsbEndpointWriter writer;
        ErrorCode ec = ErrorCode.None;
        IUsbDevice wholeUsbDevice;

        public Form1()
        {
            InitializeComponent();

  
            UsbDeviceFinder MyUsbFinder = new UsbDeviceFinder(0x15A2, 0x007F);

            // Find and open the usb device.
            MyUsbDevice = UsbDevice.OpenUsbDevice(MyUsbFinder);
              
            DumpAllDeviceDescriptors();
             
            wholeUsbDevice = MyUsbDevice as IUsbDevice;
            if (!ReferenceEquals(wholeUsbDevice, null))
            {
                wholeUsbDevice.ClaimInterface(0);
                wholeUsbDevice.SetConfiguration(1);
            }

            reader = MyUsbDevice.OpenEndpointReader(ReadEndpointID.Ep01);
            //          reader.DataReceivedEnabled = true;
            //        reader.DataReceived += (OnRxEndPointData);


            writer = MyUsbDevice.OpenEndpointWriter(WriteEndpointID.Ep02);


        }

        void DumpAllDeviceDescriptors()
        {
            // Dump all devices and descriptor information to console output.
            UsbRegDeviceList allDevices = UsbDevice.AllDevices;
            foreach (UsbRegistry usbRegistry in allDevices)
            {
                if (usbRegistry.Open(out MyUsbDevice))
                {
                    textBox1.AppendText(MyUsbDevice.Info.ToString() + Environment.NewLine);
                    for (int iConfig = 0; iConfig < MyUsbDevice.Configs.Count; iConfig++)
                    {
                        UsbConfigInfo configInfo = MyUsbDevice.Configs[iConfig];
                        textBox1.AppendText(configInfo.ToString() + Environment.NewLine);

                        ReadOnlyCollection<UsbInterfaceInfo> interfaceList = configInfo.InterfaceInfoList;
                        for (int iInterface = 0; iInterface < interfaceList.Count; iInterface++)
                        {
                            UsbInterfaceInfo interfaceInfo = interfaceList[iInterface];
                            textBox1.AppendText(interfaceInfo.ToString() + Environment.NewLine);

                            ReadOnlyCollection<UsbEndpointInfo> endpointList = interfaceInfo.EndpointInfoList;
                            for (int iEndpoint = 0; iEndpoint < endpointList.Count; iEndpoint++)
                            {
                                textBox1.AppendText(endpointList[iEndpoint].ToString() + Environment.NewLine);
                            }
                        }
                    }
                }
                
            }
        }

        UsbEndpointReader reader;
        private void button1_Click(object sender, EventArgs e)
        {
           

            int bytesWritten;
            ec = writer.Write(new byte[] { 0x12 , 0x33, 0x71, 0x41, 0x87, 0x98,0x88, 0x99 }, 2000, out bytesWritten); // specify data to send

            if (ec != ErrorCode.None)
            {
                MessageBox.Show("Write Error: " + UsbDevice.LastErrorString);
                return;
            }
              
            int bytesRead;
            
            ec = reader.Read(readBuffer, 500, out bytesRead);
            if (ec != ErrorCode.None)
            {
                MessageBox.Show("Read error: " + UsbDevice.LastErrorString);
                return;
            } else
            {
                MessageBox.Show("Success read");
            }

        }
        private void OnRxEndPointData(object sender, EndpointDataEventArgs e)
        {
             
            Console.Write(Encoding.Default.GetString(e.Buffer, 0, e.Count));
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            MyUsbDevice.Close();
        }
    }
}
