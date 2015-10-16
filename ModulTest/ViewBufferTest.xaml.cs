using System;
using System.Collections.Generic;
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
using OxyPlot;
using OxyPlot.Series;
using System.Diagnostics;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System.ComponentModel;


namespace ModulTest
{
    /// <summary>
    /// Interaktionslogik für ViewBufferTest.xaml
    /// </summary>
    public partial class ViewBufferTest : UserControl
    {

        public static RoutedCommand ADC1Command = new RoutedCommand();
        public static RoutedCommand ADC2Command = new RoutedCommand();
        public static RoutedCommand VCOCommand = new RoutedCommand();
        public static RoutedCommand ADCFreqCommand = new RoutedCommand();


        SerialConnTest serTest;
        BackgroundWorker worker;

        public ViewBufferTest()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            serTest = this.DataContext as SerialConnTest;
            serTest.ProgressChanged += setProgressValue;
            cbFreqAdc.SelectedIndex = (int)serTest.RCUCom.ADCSampleRateIndex;
        }

        private void RCUCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var serTest = this.DataContext as SerialConnTest;
            e.CanExecute = !serTest.Busy;
        }

        private void ADC1GetBuffer_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Plot1.Visibility = Visibility.Collapsed;
            worker = new BackgroundWorker();
            worker.DoWork += worker_DoGetADC1Values;
            // Use own delegates so RCUComm and AdvancedSerialport do not require a backgroundworker object
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.WorkerReportsProgress = true;
            worker.RunWorkerAsync();
        }

        private void ADC2GetBuffer_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Plot1.Visibility = Visibility.Collapsed;
            worker = new BackgroundWorker();
            worker.DoWork += worker_DoGetADC2Values;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.WorkerReportsProgress = true;
            worker.RunWorkerAsync();
        }


        private void ADCFreq_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            serTest.RCUCom.ADCSampleRateIndex = (UInt32)cbFreqAdc.SelectedIndex;
            try
            {
                serTest.Busy = true;
                serTest.RCUCom.SetADCSampleRate();
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

        private void SetConfigVCO_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                serTest.Busy = true;
                serTest.RCUCom.SetConfigVCO();
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

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Plot1.Visibility = Visibility.Visible;
            this.Dispatcher.Invoke( () => { serTest.BuildData(); });
        }


        void worker_DoGetADC1Values(object sender, DoWorkEventArgs e)
        {
            try
            {
                serTest.GetADC1ValuesOnce();
            }
            catch (TimeoutException ex)
            {
                this.Dispatcher.Invoke(() => MetroWindow_MessageBox("Serial port timeout", ex.Message));
            }
            catch (Exception ex)
            {
                this.Dispatcher.Invoke(() => MetroWindow_MessageBox("Connection failed", ex.Message));
            }
        }

        void worker_DoGetADC2Values(object sender, DoWorkEventArgs e)
        {
            try
            {
                serTest.GetADC2ValuesOnce();
            }
            catch (TimeoutException ex)
            {
                this.Dispatcher.Invoke(() => MetroWindow_MessageBox("Serial port timeout", ex.Message));
            }
            catch (Exception ex)
            {
                this.Dispatcher.Invoke(() => MetroWindow_MessageBox("Connection failed", ex.Message));
            }
        }


        public void setProgressValue(double? percent)
        {
            // dispatcher nutzen !!!
            var app = Application.Current as App;
            if (app == null) return;
            if (percent.HasValue)
            {
                pgRCUComm.Dispatcher.Invoke(() => { pgRCUComm.Value = percent.Value; });
                pgRCUComm.Dispatcher.Invoke(() => { pgRCUComm.IsIndeterminate = false; });
            }
            else
            {
                pgRCUComm.Dispatcher.Invoke(() => { pgRCUComm.IsIndeterminate = true; });
            }
        }

        public void MetroWindow_MessageBox(string title, string text)
        {
            var window = Window.GetWindow(this) as MetroWindow;
            try
            {
                if (window == null)
                    throw new Exception("Parent window == null ?!");
                Action<string, string> del = async (_title, _text) => { await window.ShowMessageAsync(_title, _text); };
                del(title, text);
            } catch (Exception ex)
            {
                MessageBox.Show(title, text);
            }
        }

    }
}
