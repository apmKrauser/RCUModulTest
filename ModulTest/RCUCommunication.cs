using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ModulTest
{
    public class RCUCommunication
    {
        public int adc_buffer_size { get; set; }
        public double ADCBinMax { get; set; }
        public double ADCVoltMax { get; set; }
        public double ADCSampleRate { get; set; }
        public int ADCReadTimeout { get; set; }

        public event Action<double?> ProgressChanged;

        public enum RCUCommand : byte
        {
            NOOP = 0x00,
            GetBufferADC1 = 0x01,
            GetBufferADC2 = 0x02
        }

        public SerialConfiguration Connection;

        public RCUCommunication(SerialConfiguration _conf)
        {
            this.Connection = _conf;
            adc_buffer_size = 2048;
            ADCBinMax = 4096;
            ADCVoltMax = 3.0;
            ADCSampleRate = 1000;
            ADCReadTimeout = 2000;            
        }



        public UInt16[] GetADCBufferOnce( int numberOfADC)
        {
            UInt16[] RxArray = new UInt16[0];
            RCUCommand rc = RCUCommand.NOOP;
            switch (numberOfADC)
            {
                case 1:
                    rc = RCUCommand.GetBufferADC1;
                    break;
                case 2:
                    rc = RCUCommand.GetBufferADC2;
                    break;
                default:
                    return RxArray;
            }
            using (Connection.Open())
            {
                RaiseProgressChanged(null);
                var sp = Connection.SerialPortObject;
                sp.WriteByte((byte)rc);
                RxArray = ReadADCBuffer(Connection.SerialPortObject);
            }
            return RxArray;
        }


        private UInt16[] ReadADCBuffer(AdvancedSerialPort sp)
        {
            UInt16[] RxArray;
            try
            {
                sp.TransferProgressEvent += RaiseProgressChanged;
                RxArray = sp.ReadUInt16Array(adc_buffer_size, new TimeSpan(0, 0, 0, 0, ADCReadTimeout));
            }
            catch (TimeoutException ex)
            {
                Debug.WriteLine("[ReadADCBuffer ComPort Timeout]:{0} ", ex.Message);
                throw;
            } 
            finally
            {
                sp.TransferProgressEvent -= RaiseProgressChanged;
            }
            return RxArray;
        }

        void RaiseProgressChanged(double? val)
        {
            if (ProgressChanged != null)
                ProgressChanged(val);
        }

        

    }
}
