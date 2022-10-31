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

namespace ArkHelper.Xaml.NewUser
{
    /// <summary>
    /// Simulator.xaml 的交互逻辑
    /// </summary>
    public partial class Simulator : Page
    {
        public Simulator()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            for (; ; )
            {
                if (ArkHelper.Pages.OtherList.Setting.SelectSimu() != "") break;
            }
            button.Content = "√ 成功";
        }
    }
}
