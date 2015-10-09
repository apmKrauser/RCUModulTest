using MahApps.Metro;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ModulTest
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        public SerialConfiguration CurrentSerialConnection { get; set; }


        public SerialConfiguration.SerialPortDescriber CurrentSerialPort
        {
            get { return CurrentSerialConnection.CurrentPort; }
            set { CurrentSerialConnection.CurrentPort = value; }
        }

        public int CurrentSerialBaudRate
        {
            get { return CurrentSerialConnection.BaudRate; }
            set { CurrentSerialConnection.BaudRate = value; }
        }

        
        
        

        protected override void OnStartup(StartupEventArgs e)
        {
            CurrentSerialConnection = new SerialConfiguration();

            // add custom accent and theme resource dictionaries
            ThemeManager.AddAccent("custom_style", new Uri("pack://application:,,,/custom_style.xaml"));

            // get the theme from the current application
            var theme = ThemeManager.DetectAppStyle(Application.Current);

            // now use the custom accent
            ThemeManager.ChangeAppStyle(Application.Current,
                                    ThemeManager.GetAccent("Blue"),
                                    theme.Item1);

            base.OnStartup(e);
        }
    }
}
