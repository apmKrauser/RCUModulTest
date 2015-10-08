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

        public ObservableCollection<VoltPoint> ADC1Values { get; private set; }


        SerialConfiguration Connection;
        RCUCommunication RCUCom;

        public SerialConnTest()
        {
            ADC1Values = new ObservableCollection<VoltPoint>();
            Connection = (Application.Current as App).CurrentSerialConnection;
            RCUCom = new RCUCommunication(Connection);
        }

        public void GetADC1ValuesOnce ()
        {
            try
            {
                Busy = true;
                RCUCom.GetADCBufferOnce(ADC1Values, 1);
            }
            finally
            {
                Busy = false;
            }
        }


    }
}
