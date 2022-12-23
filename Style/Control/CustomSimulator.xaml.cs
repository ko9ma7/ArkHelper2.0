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

namespace ArkHelper.Style.Control
{
    /// <summary>
    /// CustomSimulator.xaml 的交互逻辑
    /// </summary>
    public partial class CustomSimulator : UserControl
    {
        public CustomSimulator()
        {
            InitializeComponent();
            im.Text = App.Data.simulator.custom.im;
            port.Text = App.Data.simulator.custom.port.ToString();
            custom.IsChecked = App.Data.simulator.custom.status;
        }
        private void port_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                App.Data.simulator.custom.port = Convert.ToInt32(port.Text);
            }
            catch
            {

            }
        }

        private void im_TextChanged(object sender, TextChangedEventArgs e)
        {
            App.Data.simulator.custom.im = im.Text;
        }

        private void custom_Click(object sender, RoutedEventArgs e)
        {
            App.Data.simulator.custom.status = (bool)custom.IsChecked;
        }
    }
}
