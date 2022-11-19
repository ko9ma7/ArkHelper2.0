using System.Windows;
using System.Windows.Controls;

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
