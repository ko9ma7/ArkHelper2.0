using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Check.xaml 的交互逻辑
    /// </summary>
    public partial class Check : Page
    {
        public Check()
        {
            InitializeComponent();
        }
        private void closeUAC(object sender, RoutedEventArgs e)
        {
            Process.Start("https://cn.bing.com/search?q=%E5%85%B3%E9%97%ADuac");
        }
        /// <summary>
        /// 检查所需权限是否已经全部获取
        /// </summary>
        /// <returns>若是，返回true</returns>
        public static bool CheckAll()
        {
            //全部true，才返回true
            return (CheckUAC() && true);
        }
        /// <summary>
        /// 检查UAC是否关闭
        /// </summary>
        /// <returns>若已经关闭，返回true</returns>
        private static bool CheckUAC()
        {
            object obj = Microsoft.Win32.Registry.LocalMachine
                        .OpenSubKey("SOFTWARE")
                        .OpenSubKey("Microsoft")
                        .OpenSubKey("Windows")
                        .OpenSubKey("CurrentVersion")
                        .OpenSubKey("Policies")
                        .OpenSubKey("System")
                        .GetValue("ConsentPromptBehaviorAdmin");
            if ((int)obj == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
