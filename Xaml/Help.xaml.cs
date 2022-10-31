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

namespace ArkHelper.Xaml
{
    /// <summary>
    /// Help.xaml 的交互逻辑
    /// </summary>
    public partial class Help : Page
    {
        public Help()
        {
            InitializeComponent();
            if (ADB.ConnectedInfo != null)
            {
                simulator_name.Text = ADB.ConnectedInfo.Name;
                simulator_ip.Text = "127.0.0.1:" + ADB.ConnectedInfo.Port;
                simulator_im.Text = ADB.ConnectedInfo.IM;
            }
            foreach(PinnedData.Simulator.SimuInfo simuInfos in PinnedData.Simulator.Support)
            {
                simulator_support.Text +="\n"+" · "+ simuInfos.Name;
            }
        }
    }
}
