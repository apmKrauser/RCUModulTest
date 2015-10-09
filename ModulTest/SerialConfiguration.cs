using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace ModulTest
{
    public class SerialConfiguration : IDisposable
    {
        public class SerialPortDescriber
        {
            public string FriendlyName { get; set; }
            public string PortName { get; set; }
            public string DisplayName 
            {
                get
                {
                    return PortName + " : " + FriendlyName;
                }
            }

            public SerialPortDescriber()
            {
                PortName = "";
            }
        }

        public int BaudRate { get; set; }

        public SerialPortDescriber CurrentPort { get; set; }

        public AdvancedSerialPort SerialPortObject { get; set; }

        public SerialConfiguration()
        {
            CurrentPort = PortList[0];
            BaudRate = BaudRates[9];
        }

        public SerialConfiguration(string port, int baudRate)
            : this()
        {
            CurrentPort.PortName = port;
            BaudRate = baudRate;
            Open();
        }

        public AdvancedSerialPort Open()
        {
            SerialPortObject = new AdvancedSerialPort(CurrentPort.PortName, BaudRate, Parity.None, 8, StopBits.One);
            SerialPortObject.Open();
            return SerialPortObject;
        }

        public AdvancedSerialPort Open(string port, int baudRate)
        {
            CurrentPort.PortName = port;
            BaudRate = baudRate;
            return Open();
        }

        public void Dispose()
        {
            if (SerialPortObject != null)
                SerialPortObject.Close();
        }

        ~SerialConfiguration()
        {
            Dispose();
        }

        public static int[] BaudRates
        {
            get
            {
                return new int[] {
                        300,
                        600,
                        1200,
                        2400,
                        9600,
                        14400,
                        19200,
                        38400,
                        57600,
                        115200       
                    };
            }
        }

        public static List<SerialPortDescriber> PortList
        {
            get 
            {
                using (var mgmtObj = new ManagementObjectSearcher
                    ("SELECT * FROM WIN32_SerialPort"))
                {
                    string[] portnames = AdvancedSerialPort.GetPortNames();
                    var ports = mgmtObj.Get().Cast<ManagementBaseObject>().ToList();
                    var ret = (from n in portnames
                               join p in ports on n equals p["DeviceID"].ToString()
                               select new SerialPortDescriber(){PortName = n, FriendlyName = p["Caption"].ToString()}).ToList();

                    return ret;
                }                
            }
        }
    }
}
