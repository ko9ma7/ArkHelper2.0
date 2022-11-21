using Microsoft.Toolkit.Uwp.Notifications;
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
using System.Windows.Shapes;

namespace ArkHelper.Xaml
{
    /// <summary>
    /// NewUserPolicyWindow.xaml 的交互逻辑
    /// </summary>
    public partial class NewUserPolicyWindow : Window
    {
        public NewUserPolicyWindow()
        {
            InitializeComponent();
        }

        private void next_Click(object sender, RoutedEventArgs e)
        {
            Address.Create(); App.SaveData();
            this.Close();
        }
        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(Address.EULA);
        }
        private void Hyperlink_Click1(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(Address.PrivatePolicy);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
