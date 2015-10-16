using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot;
using OxyPlot.Series;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace ModulTest
{
    /// <summary>
    /// Provides Data for RCU Tests
    /// Viewmodel of ViewBufferTest.xaml
    /// </summary>
    public class SerialConnTest 
    {

        // true during data acquisition
        public bool Busy { get; set; }

        public event Action<double?> ProgressChanged;

        public ObservableCollection<VoltPoint> ADCValues { get; private set; }

        public RCUCommunication RCUCom { get; private set; }

        SerialConfiguration Connection;
        UInt16[] RxArray;

        public SerialConnTest()
        {
            ADCValues = new ObservableCollection<VoltPoint>();
            Connection = (Application.Current as App).CurrentSerialConnection;
            RCUCom = new RCUCommunication(Connection);
            RCUCom.ProgressChanged += RaiseProgressChanged;
        }


        /// <summary>
        ///  Acquire data and read ADC buffer
        /// </summary>
        public void GetADC1ValuesOnce ()
        {
            try
            {
                Busy = true;
                RxArray = RCUCom.GetAndSendADCBufferOnce(1);
            }
            finally
            {
                Busy = false;
            }
        }

        /// <summary>
        ///  Acquire data and read ADC buffer
        /// </summary>
        public void GetADC2ValuesOnce()
        {
            try
            {
                Busy = true;
                RxArray = RCUCom.GetAndSendADCBufferOnce(2);
            }
            finally
            {
                Busy = false;
            }
        }

        /// <summary>
        ///  Prepare ADC data for visualization
        /// </summary>
        public void BuildData()
        {
            VoltPoint[] temp = new VoltPoint[RxArray.Length];
            //ADCValues.Clear();
            if (RxArray == null) return;
            RaiseProgressChanged(null);
            Debug.WriteLine("=> filling ADCValues");
            for (int i = 0; i < RxArray.Length; i++)
            {
                temp[i] = (new VoltPoint(((double)RxArray[i] / (double)RCUCom.ADCBinMax) * RCUCom.ADCVoltMax, i / RCUCom.ADCSampleRate));
            }
            ADCValues = new ObservableCollection<VoltPoint>(temp);               
            Debug.WriteLine("=> filling done.");

        }

        public void RaiseProgressChanged(double? val)
        {
            if (ProgressChanged != null)
            {
                ProgressChanged(val);
            }
        }

    }
}
