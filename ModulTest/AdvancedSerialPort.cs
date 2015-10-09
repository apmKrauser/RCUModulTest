using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;

namespace ModulTest
{
    public class AdvancedSerialPort : SerialPort
    {

        public event Action<double?> TransferProgressEvent;

        private List<UInt16> RxBufferU16 = new List<UInt16>();
        private BinaryReader BinReader;

        public AdvancedSerialPort(string portName, int baudRate, System.IO.Ports.Parity parity, int dataBits, System.IO.Ports.StopBits stopBits)
            : base(portName, baudRate, parity, dataBits, stopBits)
        {   }

        public AdvancedSerialPort(string portName, int baudRate)
            : this(portName, baudRate, Parity.None, 8, StopBits.One)
        {   }

        public void WriteByte(byte theByte)
        {
            byte[] byteArr = new byte[] {theByte};
            Write(byteArr, 0, 1);
        }

        private void setProgressValue(double? val)
        {
            if (TransferProgressEvent != null)
                TransferProgressEvent(val);
        }

        /// <summary>
        /// Read Array of UInt16 elements. Method is Blocking
        /// </summary>
        /// <param name="elementsExpected">Number of UInt16s expected</param>
        /// <param name="timeOut">Timeout</param>
        /// <returns></returns>
        public UInt16[] ReadUInt16Array(int elementsExpected, TimeSpan timeOut)
        {
            Stopwatch sw = new Stopwatch();
            RxBufferU16.Clear();

            // BinaryReader provides ReadUInt16 in Little Endian. ARM Coretex-M gcc is also Little Endian
            // Leave Stream open
            using (BinReader = new BinaryReader(this.BaseStream, Encoding.UTF8, true))
            {

                sw.Start();
                this.DataReceived += StreamSerialPort_DataReceived;
                try
                {
                    while (sw.Elapsed < timeOut)
                    {
                        if (RxBufferU16.Count >= elementsExpected)
                            return RxBufferU16.ToArray();
                        Thread.Sleep(100);
                        setProgressValue(100*RxBufferU16.Count/elementsExpected);
                    }
                }
                finally
                {
                    this.DataReceived -= StreamSerialPort_DataReceived;
                    RxBufferU16.Clear();
                    sw.Stop();
                    this.BaseStream.Flush();
                }

                throw new TimeoutException("[AdvancedSerialport:RxBufferU16]: Timeout");
            }
        }

        /// <summary>
        /// Serial Port Received Data Callback
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void StreamSerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            UInt16 val;
            // can read 16 bit?
            while (this.BytesToRead >= 2)
            {
                val = BinReader.ReadUInt16();
                RxBufferU16.Add(val);
            }
        }
    }
}
