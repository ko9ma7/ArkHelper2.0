using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

namespace ArkHelper.Xaml.NewUser
{
    /// <summary>
    /// aaaaa
    /// </summary>
    public partial class Configure : Page
    {
        public Configure()
        {
            InitializeComponent();

            //初始化
            UAC.IsEnabled = !ExamUAC();
        }

        /// <summary>
        /// 判断是否需要进入此页面
        /// </summary>
        /// <returns>需要则为true，否则为false</returns>
        public static bool Exam()
        {
            return (ExamUAC());
        }

        private void DisUAC(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("即将尝试关闭UAC。\n/接下来弹出的安全提醒，请一律允许。\n/建议关闭杀毒软件再执行此操作。\n/关闭后系统会弹出通知要求重启，请等到ArkHelper设置向导结束后再重启。", "ArkHelper");
            WithSystem.Cmd(@"start " + Address.cmd + @" /k ""%windir%\System32\reg.exe ADD HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System /v EnableLUA /t REG_DWORD /d 0 /f& exit""");
            
            UAC.IsEnabled = false;
        }

        public static bool ExamUAC()
        {
            //检查UAC
            if ((int)Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Microsoft").OpenSubKey("Windows").OpenSubKey("CurrentVersion").OpenSubKey("Policies").OpenSubKey("System").GetValue("EnableLUA") == 0)
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
