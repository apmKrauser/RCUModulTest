using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModulTest
{
    public class RCUCommunication
    {
        const int adc_buffer_size = 2048;
        const double ADCBinMax = 4096;
        const double ADCVoltMax = 3.0;
        const double ADCSampleRate = 1000;
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
        }

        public void GetADCBufferOnce(ObservableCollection<VoltPoint> ADCValues, int numberOfADC)
        {
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
                    return;
            }
            using (Connection.Open())
            {
                var sp = Connection.SerialPortObject;
                sp.WriteByte((byte)rc);
                ReadADCBuffer(Connection.SerialPortObject, ADCValues);
            }
        }


        private void ReadADCBuffer(AdvancedSerialPort sp, ObservableCollection<VoltPoint> ADCValues)
        {
            UInt16[] RxArray = sp.ReadUInt16Array(adc_buffer_size, new TimeSpan(0, 0, 1));
            ADCValues.Clear();
            for (int i = 0; i < RxArray.Length; i++)
            {
                ADCValues.Add(new VoltPoint((RxArray[i] / ADCBinMax) * ADCVoltMax, i / ADCSampleRate));
            }
        }

    }
}
