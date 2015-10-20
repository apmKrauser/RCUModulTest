using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ModulTest
{
    /// <summary>
    /// Interaktionslogik für RCUConfig.xaml
    /// </summary>
    public partial class RCUConfig : UserControl
    {
        public static RoutedCommand FilterCommand = new RoutedCommand();
        public static RoutedCommand VCOCommand = new RoutedCommand();
        public static RoutedCommand ADCFreqCommand = new RoutedCommand();

        SerialConnTest serTest;

        public RCUConfig()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            serTest = this.DataContext as SerialConnTest;
            cbFreqAdc.SelectedIndex = (int)serTest.RCUCom.ADCSampleRateIndex;
        }

        private void RCUCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var serTest = this.DataContext as SerialConnTest;
            bool canex = false;
            if (serTest != null)
                canex = !serTest.Busy;
            e.CanExecute = canex;
        }

        private void ADCFreq_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            serTest = this.DataContext as SerialConnTest;
            serTest.RCUCom.ADCSampleRateIndex = (UInt32)cbFreqAdc.SelectedIndex;
            TryCatch(() =>
            {
                serTest.RCUCom.SetADCSampleRate();
            });
        }

        private void Filter_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            serTest = this.DataContext as SerialConnTest;
            TryCatch(() =>
            {
                serTest.RCUCom.SetConfigFilter();
            });
        }

        private void SetConfigVCO_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            serTest = this.DataContext as SerialConnTest;
            TryCatch(() =>
            {
                serTest.RCUCom.SetConfigVCO();
            });

        }

        private void TryCatch(Action act)
        {
            try
            {
                serTest.Busy = true;
                act();
            }
            catch (TimeoutException ex)
            {
                this.Dispatcher.Invoke(() => MetroWindow_MessageBox("Serial port timeout", ex.Message));
            }
            catch (Exception ex)
            {
                this.Dispatcher.Invoke(() => MetroWindow_MessageBox("Connection failed", ex.Message));
            }
            finally
            {
                serTest.Busy = false;
            }
        }

        private void MetroWindow_MessageBox(string title, string text)
        {
            var window = Window.GetWindow(this) as MetroWindow;
            try
            {
                if (window == null)
                    throw new Exception("Parent window == null ?!");
                Action<string, string> del = async (_title, _text) => { await window.ShowMessageAsync(_title, _text); };
                del(title, text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(title, text);
            }
        }
    }
}
