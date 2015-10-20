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

        public RCUConfig()
        {
            InitializeComponent();
        }

        private void RCUCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var serTest = this.DataContext as SerialConnTest;
            e.CanExecute = !serTest.Busy;
        }
    }
}
