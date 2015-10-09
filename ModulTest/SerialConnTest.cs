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

        public void GetADC1ValuesOnce ()
        {
            try
            {
                Busy = true;
                RxArray = RCUCom.GetADCBufferOnce(1);
            }
            finally
            {
                Busy = false;
            }
        }

        public void GetADC2ValuesOnce()
        {
            try
            {
                Busy = true;
                RxArray = RCUCom.GetADCBufferOnce(2);
            }
            finally
            {
                Busy = false;
            }
        }

        public void BuildData()
        {
            if (RxArray == null) return;
            for (int i = 0; i < RxArray.Length; i++)
            {
                ADCValues.Add(new VoltPoint((RxArray[i] / RCUCom.ADCBinMax) * RCUCom.ADCVoltMax, i / RCUCom.ADCSampleRate));
            }
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
