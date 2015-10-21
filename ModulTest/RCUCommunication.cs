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
    /// <summary>
    /// Base communication and configuration with RCU firmware
    /// </summary>
    public class RCUCommunication : INotifyPropertyChanged
    {
        public int adc_buffer_size { get; set; }
        public UInt32 ADCBinMax { get; set; }
        public double ADCVoltMax { get; set; }
        public int ADCReadTimeout { get; set; }
        public double VCOOffset { get; set; }
        public int FilterGainIdx { get; set; }
        public ObservableCollection<string> FilterHPList { get; set; }
        public ObservableCollection<string> FilterLPList { get; set; }        

        private int _filterHighPassIdx = 0;
        private int _filterLowPassIdx = 0;

        public int FilterHighPassIdx
        {
            get
            {
                return _filterHighPassIdx;
            }

            set
            {
                _filterHighPassIdx = value;
                OnPropertyChanged();
            }
        }

        public int FilterLowPassIdx 
        {
            get
            {
                return _filterLowPassIdx;
            }

            set
            {
                _filterLowPassIdx = value;
                OnPropertyChanged();
            }
        }

        
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
                return RCUAdcSampleRates[ADCSampleRateIndex];
            }
        }

        private double _filterBaseFreq;
        public double FilterBaseFreq
        {
            get
            {
                return _filterBaseFreq;
            }

            set
            {
                _filterBaseFreq = value;
                OnPropertyChanged();
                // Generate Filterlists based on base frequency
                var temp = FilterHighPassIdx;
                var temp2 = FilterLowPassIdx;
                FilterHPList.Clear();
                FilterHPList.Add(String.Format("{0:##0.0} kHz", (_filterBaseFreq / 6000 * 1000)));
                FilterHPList.Add(String.Format("{0:##0.0} kHz", (_filterBaseFreq / 3000 * 1000)));
                FilterHPList.Add(String.Format("{0:##0.0} kHz", (_filterBaseFreq / 1000 * 1000)));
                FilterHPList.Add(String.Format("bypassed"));
                FilterLPList.Clear();
                FilterLPList.Add(String.Format("{0:##0.0} kHz", (_filterBaseFreq / 600 * 1000)));
                FilterLPList.Add(String.Format("{0:##0.0} kHz", (_filterBaseFreq / 300 * 1000)));
                FilterLPList.Add(String.Format("{0:##0.0} kHz", (_filterBaseFreq / 100 * 1000)));
                FilterLPList.Add(String.Format("{0:##0.0} kHz", (_filterBaseFreq / 100 * 1000)));
                FilterLowPassIdx = temp2;
                FilterHighPassIdx = temp;
                
            }
        }

        public ObservableCollection<string> FilterGainList
        {
            get 
            {
                ObservableCollection<string> lst = new ObservableCollection<string>();
                lst.Add("0 dB");
                lst.Add("12 dB");
                lst.Add("24 dB");
                lst.Add("32 dB");
                return lst; 
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

        /// <summary>
        /// calculated from ADC cycles per conversion and std board config
        /// </summary>
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

        /// <summary>
        /// indicates which ADCCycleTime to use; 0 based while rcu parameter is 1 based
        /// </summary>
        public UInt32 ADCSampleRateIndex { get; set; }

        public SerialConfiguration Connection;

        public RCUCommunication(SerialConfiguration _conf)
        {
            this.Connection = _conf;
            adc_buffer_size = 2048;
            ADCBinMax = 4096;
            ADCVoltMax = 3.0;
            ADCSampleRateIndex = 0;
            ADCReadTimeout = 2000;
            VCOFreqency = 3000;
            VCOOffset = 0.128;
            FilterHighPassIdx = 0;
            FilterLowPassIdx = 0;
            FilterHPList = new ObservableCollection<string>();
            FilterLPList = new ObservableCollection<string>();
            FilterBaseFreq = 40;
        }

        /// <summary>
        /// wpf gui update
        /// </summary>
        /// <param name="p"></param>
        private void OnPropertyChanged([CallerMemberName] string p = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(p));
        }


        /// <summary>
        /// Send VCO configuration
        /// </summary>
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

        public bool SetConfigFilter()
        {
            using (Connection.Open())
            {
                var sp = Connection.SerialPortObject;
                UInt32 basefreq = (UInt32)(FilterBaseFreq*1000);
                UInt32 hpf =  (UInt32)(FilterHighPassIdx & 0x03);
                UInt32 lpf =  (UInt32)(FilterLowPassIdx  & 0x03);
                UInt32 gain = (UInt32)(FilterGainIdx     & 0x03);
                UInt32 bandp = (lpf << 2) | hpf;
                sp.WriteByte((byte)RCUCommand.ConfigFilter);
                sp.WriteUInt32(basefreq);
                sp.WriteUInt32(gain);
                sp.WriteUInt32(bandp);
                byte[] ret = sp.ReadUInt8Array(1, new TimeSpan(0, 0, 0, 0, 1000));
                return ret[0] == 0x00;
            }
        }

        /// <summary>
        /// Change Samplerate of ADC1&2
        /// indeed alters adc_sampletime_cycles
        /// </summary>
        /// <returns></returns>
        public bool SetADCSampleRate()
        {
            Debug.WriteLine("=> ADC samplerate:" + ADCSampleRateIndex);
            using (Connection.Open())
            {
                var sp = Connection.SerialPortObject;
                sp.WriteByte((byte)RCUCommand.ConfigADCRate);
                sp.WriteUInt32(ADCSampleRateIndex + 1);
                sp.WriteUInt32(0);
                sp.WriteUInt32(0);
                byte[] ret = sp.ReadUInt8Array(1, new TimeSpan(0, 0, 0, 0, 1000));
                return ret[0] == 0;
            }
        }

        /// <summary>
        /// Acquire data and send the resulting ADCBuffer
        /// </summary>
        /// <param name="numberOfADC">indicates which adc buffer to send</param>
        /// <returns>buffer as uint16</returns>
        public UInt16[] GetAndSendADCBufferOnce( int numberOfADC)
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


        /// <summary>
        /// Reads ADCBuffer 
        /// </summary>
        /// <param name="sp"></param>
        /// <returns></returns>
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
