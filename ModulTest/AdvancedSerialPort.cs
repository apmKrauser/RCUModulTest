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
    /// <summary>
    /// Extends Serialport with BinaryReader/Writer functions
    /// Designed for ARM little endian
    /// </summary>
    public class AdvancedSerialPort : SerialPort
    {

        public event Action<double?> TransferProgressEvent;

        private List<byte>   RxBufferU8 = new List<byte>();
        private List<UInt16> RxBufferU16 = new List<UInt16>();
        private List<UInt32> RxBufferU32 = new List<UInt32>();
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
        internal void WriteUInt32(UInt32 val)
        {
            byte[] byteArr = BitConverter.GetBytes(val);
            Write(byteArr, 0, 4);
        }

        internal void WriteUInt16(UInt16[] val)
        {
            byte[] TxBufferU8 = val.SelectMany(i => BitConverter.GetBytes(i)).ToArray();
            Write(TxBufferU8, 0, TxBufferU8.Length);
        }


        /// <summary>
        /// calls Event for updating a progress bar
        /// </summary>
        /// <param name="val">0-100</param>
        private void setProgressValue(double? val)
        {
            if (TransferProgressEvent != null)
                TransferProgressEvent(val);
        }

        /// <summary>
        /// Read Array of UInt16 elements. Method is Blocking
        /// </summary>
        /// <param name="elementsExpected">Number of UInt16s expected</param>
        /// <param name="timeOut">Timeout (Throws TimeoutException)</param>
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
                this.DataReceived += StreamSerialPort_DataReceived16bit;
                try
                {
                    while (sw.Elapsed < timeOut)
                    {
                        if (RxBufferU16.Count >= elementsExpected)
                            return RxBufferU16.ToArray();
                        Thread.Sleep(20);
                        setProgressValue(100*RxBufferU16.Count/elementsExpected);
                    }
                }
                finally
                {
                    this.DataReceived -= StreamSerialPort_DataReceived16bit;
                    RxBufferU16.Clear();
                    sw.Stop();
                    this.BaseStream.Flush();
                }

                throw new TimeoutException("[AdvancedSerialport:RxBufferU16]: Timeout");
            }
        }

        /// <summary>
        /// Read Array of UInt8 elements. Method is Blocking
        /// </summary>
        /// <param name="elementsExpected">Number of bytes expected</param>
        /// <param name="timeOut">Timeout (Throws TimeoutException)</param>
        /// <returns></returns>
        public byte[] ReadUInt8Array(int elementsExpected, TimeSpan timeOut)
        {
            Stopwatch sw = new Stopwatch();
            RxBufferU8.Clear();

            // BinaryReader provides ReadUInt16 in Little Endian. ARM Coretex-M gcc is also Little Endian
            // Leave Stream open
            using (BinReader = new BinaryReader(this.BaseStream, Encoding.UTF8, true))
            {

                sw.Start();
                this.DataReceived += StreamSerialPort_DataReceived8bin;
                try
                {
                    while (sw.Elapsed < timeOut)
                    {
                        if (RxBufferU8.Count >= elementsExpected)
                            return RxBufferU8.ToArray();
                        Thread.Sleep(20);
                    }
                }
                finally
                {
                    this.DataReceived -= StreamSerialPort_DataReceived8bin;
                    RxBufferU8.Clear();
                    sw.Stop();
                    this.BaseStream.Flush();
                }

                throw new TimeoutException("[AdvancedSerialport:RxBufferU8]: Timeout");
            }
        }

        /// <summary>
        /// Read Array of UInt32 elements. Method is Blocking
        /// </summary>
        /// <param name="elementsExpected">Number of UInt32s expected</param>
        /// <param name="timeOut">Timeout (Throws TimeoutException)</param>
        /// <returns></returns>
        public UInt32[] ReadUInt32Array(int elementsExpected, TimeSpan timeOut)
        {
            Stopwatch sw = new Stopwatch();
            RxBufferU32.Clear();

            // BinaryReader provides ReadUInt16 in Little Endian. ARM Coretex-M gcc is also Little Endian
            // -- Leave Stream open --
            using (BinReader = new BinaryReader(this.BaseStream, Encoding.UTF8, true))
            {

                sw.Start();
                this.DataReceived += StreamSerialPort_DataReceived32bit;
                try
                {
                    while (sw.Elapsed < timeOut)
                    {
                        if (RxBufferU32.Count >= 1)
                            return RxBufferU32.ToArray();
                        Thread.Sleep(20);
                        setProgressValue(100 * RxBufferU32.Count / elementsExpected);
                    }
                }
                finally
                {
                    this.DataReceived -= StreamSerialPort_DataReceived32bit;
                    RxBufferU32.Clear();
                    sw.Stop();
                    this.BaseStream.Flush();
                }

                throw new TimeoutException("[AdvancedSerialport:RxBufferU32]: Timeout");
            }
        }


        /// <summary>
        /// SerialPort received data event handler (bytes)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void StreamSerialPort_DataReceived8bin(object sender, SerialDataReceivedEventArgs e)
        {
            byte val;
            // can read 16 bit?
            while (this.BytesToRead >= 1)
            {
                val = BinReader.ReadByte();
                RxBufferU8.Add(val);
            }
        }

        /// <summary>
        /// SerialPort received data event handler (uint16)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void StreamSerialPort_DataReceived16bit(object sender, SerialDataReceivedEventArgs e)
        {
            UInt16 val;
            // can read 16 bit?
            while (this.BytesToRead >= 2)
            {
                val = BinReader.ReadUInt16();
                RxBufferU16.Add(val);
            }
        }

        /// <summary>
        /// SerialPort received data event handler (uint32)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void StreamSerialPort_DataReceived32bit(object sender, SerialDataReceivedEventArgs e)
        {
            UInt16 val;
            // can read 32 bit?
            while (this.BytesToRead >= 4)
            {
                val = BinReader.ReadUInt16();
                RxBufferU32.Add(val);
            }
        }

    }
}
