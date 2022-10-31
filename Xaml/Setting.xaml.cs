using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArkHelper.Pages.OtherList
{
    /// <summary>
    /// Setting.xaml 的交互逻辑
    /// </summary>
    sealed partial class Setting : Page
    {
        public Setting()
        {
            InitializeComponent();

            address.Text = Address.programData;

            pure.IsChecked = Data.ArkHelper.pure;
            im.Text = Data.simulator.custom.im;
            port.Text = Data.simulator.custom.port.ToString();
            custom.IsChecked = Data.simulator.custom.status;
            /*if (Data.SCHT.status)
            {
                pure.IsChecked = true;
                pure.IsEnabled = false;
                pure.ToolTip = "SCHT开启时，无法启用后台纯净。";
            }*/
        }

        private void Button_Click(object sender, RoutedEventArgs e) => SelectSimu();

        #region 链接
        private void Hyperlink_github(object sender, RoutedEventArgs e) => Process.Start(Address.github);
        private void Hyperlink_github_issue(object sender, RoutedEventArgs e) => Process.Start(Address.github + @"/issues");
        private void Hyperlink_tg(object sender, RoutedEventArgs e) => Process.Start(Address.tg);
        private void Hyperlink_email(object sender, RoutedEventArgs e)
        {
            Snackbar.MessageQueue.Enqueue("邮箱地址已复制");
            Clipboard.SetDataObject("ArkHelper@proton.me");
        }
        private void Hyperlink_EULA(object sender, RoutedEventArgs e) => Process.Start(Address.EULA);
        private void Hyperlink_PriPol(object sender, RoutedEventArgs e) => Process.Start(Address.PrivatePolicy);
        #endregion
        #region 文件操作
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(Address.cache);
                FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();  //返回目录中所有文件和子目录
                foreach (FileSystemInfo i in fileinfo)
                {
                    if (i is DirectoryInfo)            //判断是否文件夹
                    {
                        DirectoryInfo subdir = new DirectoryInfo(i.FullName);
                        subdir.Delete(true);          //删除子目录和文件
                    }
                    else
                    {
                        File.Delete(i.FullName);      //删除指定文件
                    }
                }
                MessageBox.Show("/// 已经成功地删除了缓存文件", "ArkHelper");
            }
            catch
            {
                MessageBox.Show("/// 发生错误，指定的操作未能执行。可能是资源文件正在占用中 \n/建议重启ArkHelper再尝试。", "ArkHelper");
            }
        }
        private void reset(object sender, RoutedEventArgs e)
        {
            if (System.Windows.Forms.MessageBox.Show("确定删除ArkHelper的所有配置文件并关闭ArkHelper吗?", "ArkHelper", System.Windows.Forms.MessageBoxButtons.OKCancel, System.Windows.Forms.MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    Directory.Delete(Address.programData, true);
                    App.ExitApp();
                }
                catch
                {
                    MessageBox.Show("/// 发生错误，指定的操作未能执行。可能是资源文件正在占用中 \n/建议重启ArkHelper再尝试。", "ArkHelper");
                }
            }
        }
        #endregion
        private void pure_Click(object sender, RoutedEventArgs e) => Data.ArkHelper.pure = (bool)pure.IsChecked;

        private void port_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                Data.simulator.custom.port = Convert.ToInt32(port.Text);
            }
            catch
            {

            }
        }

        private void im_TextChanged(object sender, TextChangedEventArgs e)
        {
            Data.simulator.custom.im = im.Text;
        }

        private void custom_Click(object sender, RoutedEventArgs e)
        {
            Data.simulator.custom.status = (bool)custom.IsChecked;
        }

        public static string SelectSimu()
        {
            string _name = WithSystem.OpenFile("选择模拟器快捷方式", "快捷方式(*.lnk)|*.lnk");
            if (_name != "")
            {
                new FileInfo(_name).CopyTo(Address.dataExternal + @"\simulator.lnk", true);
            }
            return _name;
        }
    }
}
