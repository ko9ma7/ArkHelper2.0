using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArkHelper.Pages.NewUserList
{
    /// <summary>
    /// Welcome.xaml 的交互逻辑
    /// </summary>
    public partial class Welcome : Page
    {
        public Welcome()
        {
            InitializeComponent();
        }
        
        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(Address.EULA);
        }
        private void Hyperlink_Click1(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(Address.PrivatePolicy);
        }

        public delegate void ClickDele(bool ischecked);
        public static event ClickDele ClickEvent;

        private void Wel_checkbox_Click(object sender, RoutedEventArgs e)
        {
            ClickEvent((bool)Wel_checkbox.IsChecked);
        }
    }
}
