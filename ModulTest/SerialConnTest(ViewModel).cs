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
using System.Windows.Threading;

namespace ModulTest
{
    /// <summary>
    /// Provides Data for RCU Tests
    /// Viewmodel of ViewBufferTest.xaml
    /// </summary>
    public class SerialConnTest 
    {

        // true during data acquisition
        private static bool _busy = false;
        private object _busysem =  new object(); // semaphore
        public bool Busy 
        {
            get { lock (_busysem) return _busy; } 
            set { lock (_busysem) _busy =  value; } 
        }

        public event Action<double?> ProgressChanged;

        public ObservableList<VoltPoint> ADCValues { get; private set; }

        public RCUCommunication RCUCom { get; private set; }

        public SerialConnTest Self
        {
            get { return this; }
        }
        

        SerialConfiguration Connection;
        UInt16[] RxArray;

        public SerialConnTest()
        {
            ADCValues = new ObservableList<VoltPoint>();
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
                RxArray = RCUCom.GetAndSendADCOnce(1);
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
                RxArray = RCUCom.GetAndSendADCOnce(2);
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
            ADCValues.Clear();
            if (RxArray == null) return;
            VoltPoint[] temp = new VoltPoint[RxArray.Length];
            RaiseProgressChanged(null);
            Debug.WriteLine("=> filling ADCValues");
            for (int i = 0; i < RxArray.Length; i++)
            {
                temp[i] = (new VoltPoint(((double)RxArray[i] / (double)RCUCom.ADCBinMax) * RCUCom.ADCVoltMax, i / RCUCom.ADCSampleRate));
            }
            ADCValues.AddRange(temp);
            //foreach (var item in temp)
            //{
            //    ADCValues.Add(item);
            //}
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
