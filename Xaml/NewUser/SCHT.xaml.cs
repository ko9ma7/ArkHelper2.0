using System;
using System.Windows;
using System.Windows.Controls;

namespace ArkHelper.Pages.NewUserList
{    
    public partial class SCHT : Page
    {
        public static bool enabled = true;
        public SCHT()
        {
            InitializeComponent();
        }

        private void checkbox_Click(object sender, RoutedEventArgs e)
        {
            enabled = (bool)checkbox.IsChecked;
        }
    }
}
