using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ModulTest
{
    public class RCUCommunication : INotifyPropertyChanged
    {
        public int adc_buffer_size { get; set; }
        public UInt32 ADCBinMax { get; set; }
        public double ADCVoltMax { get; set; }
        public int ADCReadTimeout { get; set; }
        public double VCOOffset { get; set; }
        
        private UInt32 _VCOFreqency;
        public UInt32 VCOFreqency
        {
            get { return _VCOFreqency; }
            set { _VCOFreqency = value; OnPropertyChanged(); }
        }
        public double ADCSampleRate 
        { 
            get
            {
                return RCUAdcSampleRates[RCUADCSelectedSampleRate];
            }
        }
        
        
        public event Action<double?> ProgressChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public enum RCUCommand : byte
        {
            NOOP = 0x00,
            GetBufferADC1  = 0x01,
            GetBufferADC2  = 0x02,
            GetAndSendADC1 = 0x03,
            GetAndSendADC2 = 0x04,
            ConfigVCO      = 0x11,
            ConfigFilter   = 0x12,
            ConfigADCRate  = 0x13,
            StreamToBuffer = 0x41
        }

        public UInt32[] RCUAdcSampleRates
        {
            get 
            {
                double f = 84e6 / 4.0 / 15.0;
                UInt32[] ret = new UInt32[8];
                ret[0] = (UInt32)(f / 480);
                ret[1] = (UInt32)(f / 144);
                ret[2] = (UInt32)(f / 112);
                ret[3] = (UInt32)(f / 84);
                ret[4] = (UInt32)(f / 56);
                ret[5] = (UInt32)(f / 28);
                ret[6] = (UInt32)(f / 15);
                ret[7] = (UInt32)(f / 3);
                return ret;
            }            
        }

        private UInt32 ADCSampleRateIndex = 1;

        public UInt32 RCUADCSelectedSampleRate
        {
            get { return ADCSampleRateIndex = 1-1; }
            set { ADCSampleRateIndex = value+1; }
        }
        

        public SerialConfiguration Connection;

        public RCUCommunication(SerialConfiguration _conf)
        {
            this.Connection = _conf;
            adc_buffer_size = 2048;
            ADCBinMax = 4096;
            ADCVoltMax = 3.0;
            ADCReadTimeout = 2000;
            VCOFreqency = 3000;
            VCOOffset = 0.128;
        }

        private void OnPropertyChanged([CallerMemberName] string p = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(p));
        }

        public void SetConfigVCO()
        {
            UInt32 offset;
            using (Connection.Open())
            {
                var sp = Connection.SerialPortObject;
                offset = (UInt32)(ADCBinMax * VCOOffset / ADCVoltMax);
                sp.WriteByte((byte)RCUCommand.ConfigVCO);
                sp.WriteUInt32(VCOFreqency);
                sp.WriteUInt32(offset);
                sp.WriteUInt32(0);
                UInt32[] ret = sp.ReadUInt32Array(1, new TimeSpan(0, 0, 0, 0, 1000));
                VCOFreqency = ret[0];
            }
        }

        public bool SetADCSampleRate()
        {
            
            using (Connection.Open())
            {
                var sp = Connection.SerialPortObject;
                sp.WriteByte((byte)RCUCommand.ConfigADCRate);
                sp.WriteUInt32(ADCSampleRateIndex);
                sp.WriteUInt32(0);
                sp.WriteUInt32(0);
                UInt32[] ret = sp.ReadUInt32Array(1, new TimeSpan(0, 0, 0, 0, 1000));
                return ret[0] == 0;
            }
        }

        public UInt16[] GetADCBufferOnce( int numberOfADC)
        {
            UInt16[] RxArray = new UInt16[0];
            RCUCommand rc = RCUCommand.NOOP;
            switch (numberOfADC)
            {
                case 1:
                    rc = RCUCommand.GetAndSendADC1;
                    break;
                case 2:
                    rc = RCUCommand.GetAndSendADC2;
                    break;
                default:
                    return RxArray;
            }
            RaiseProgressChanged(null);
            using (Connection.Open())
            {
                var sp = Connection.SerialPortObject;
                sp.WriteByte((byte)rc);
                sp.WriteUInt32(0);
                sp.WriteUInt32(0);
                sp.WriteUInt32(0);
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
