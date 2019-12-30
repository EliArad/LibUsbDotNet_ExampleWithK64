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
        bool m_useCallback = false;
        public static UsbDevice MyUsbDevice;
        UsbEndpointWriter writer;
        ErrorCode ec = ErrorCode.None;
        IUsbDevice wholeUsbDevice;

        public Form1()
        {
            InitializeComponent();

            Control.CheckForIllegalCrossThreadCalls = false;
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
            if (m_useCallback == true)
            {
                reader.DataReceivedEnabled = true;
                reader.DataReceived += (OnRxEndPointData);
            }

            writer = MyUsbDevice.OpenEndpointWriter(WriteEndpointID.Ep02);

            outBuffer = new byte[m_maxPacketSize];
            inBuffer = new byte[m_maxPacketSize];

            label4.Text = m_maxPacketSize.ToString();

            for (int i = 0; i < outBuffer.Length; i++)
            {
                outBuffer[i] = (byte)(0x22 + i);
            }
            outBuffer[0] = 0x12;
            outBuffer[1] = 0x34;
            outBuffer[2] = 0x56;
            outBuffer[3] = 0x78;
            outBuffer[4] = 0x90;
            outBuffer[5] = 0xab;
            outBuffer[6] = 0xcd;
            outBuffer[7] = 0xef;
        }

        byte[] outBuffer;
        byte[] inBuffer;
        int m_maxPacketSize = 8;
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
                                m_maxPacketSize = endpointList[iEndpoint].Descriptor.MaxPacketSize;
                                textBox1.AppendText(endpointList[iEndpoint].ToString() + Environment.NewLine);
                            }
                        }
                    }
                }                
            }
        }

        UsbEndpointReader reader;
        Thread m_thread = null;
        bool m_readAndWrite = false;
        private void button1_Click(object sender, EventArgs e)
        {
            m_readAndWrite = rbReadWrite.Checked;

            m_thread = new Thread(ReadWriteProcess);
            m_thread.Start();

        }
        bool m_running = false;
        void ReadWriteProcess()
        {
            int bytesWritten;

            m_running = true;
            int counter = 0;
            while (m_running)
            {
                ec = writer.Write(outBuffer, 2000, out bytesWritten); // specify data to send

                if (ec != ErrorCode.None)
                {
                    MessageBox.Show("Write Error: " + UsbDevice.LastErrorString);
                    return;
                }
                if (m_useCallback == true)
                    return;

                int bytesRead;
                if (m_readAndWrite == false)
                {
                    counter++;
                    label1.Text = (m_maxPacketSize * counter).ToString();
                    continue;
                }

                ec = reader.Read(inBuffer, 500, out bytesRead);
                if (ec != ErrorCode.None)
                {
                    MessageBox.Show("Read error: " + UsbDevice.LastErrorString);
                    return;
                }
                else
                {
                    counter++;
                    label1.Text = (m_maxPacketSize * counter).ToString();
                }
            }
        }
        private void OnRxEndPointData(object sender, EndpointDataEventArgs e)
        {
             
            Console.Write(Encoding.Default.GetString(e.Buffer, 0, e.Count));
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            m_running = false;
            if (m_thread != null)
                m_thread.Join();
            MyUsbDevice.Close();
        }
    }
}
