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
    /// Interaktionslogik für SerialConfig.xaml
    /// </summary>
    public partial class SerialConfig : UserControl
    {
        public static RoutedCommand MyCommand = new RoutedCommand();

        public SerialConfig()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
           
        }
    }
}
