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
    /// SimuSupport.xaml 的交互逻辑
    /// </summary>
    public partial class SimuSupport : UserControl
    {
        public SimuSupport()
        {
            InitializeComponent();
            foreach (ArkHelper.PinnedData.Simulator.SimuInfo simulator in ArkHelper.PinnedData.Simulator.Support)
            {
                text.Text += simulator.Name + " ";
            }

        }
    }
}
