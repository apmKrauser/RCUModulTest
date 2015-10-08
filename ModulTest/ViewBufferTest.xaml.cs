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


namespace ModulTest
{
    /// <summary>
    /// Interaktionslogik für ViewBufferTest.xaml
    /// </summary>
    public partial class ViewBufferTest : UserControl
    {

        public static RoutedCommand ADC1Command = new RoutedCommand();

        public ViewBufferTest()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void RCUCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var serTest = this.DataContext as SerialConnTest;
            e.CanExecute = !serTest.Busy;
        }

        private void ADC1GetBuffer_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var serTest = this.DataContext as SerialConnTest;
            serTest.GetADC1ValuesOnce();
        }


    }
}
