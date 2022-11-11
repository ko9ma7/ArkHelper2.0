using System.Windows.Controls;
using System.Windows;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;

namespace ArkHelper.Pages.NewUserList
{
    /// <summary>
    /// Guide.xaml 的交互逻辑
    /// </summary>
    public partial class Guide : Page
    {
        public Guide()
        {
            InitializeComponent();
            if (Data.scht.status) { guidea.Visibility = Visibility.Visible;  }
        }

    }
}
