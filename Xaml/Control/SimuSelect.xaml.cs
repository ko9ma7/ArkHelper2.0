using ArkHelper.Pages.OtherList;
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

namespace ArkHelper.Xaml.Control
{
    /// <summary>
    /// SimuSelect.xaml 的交互逻辑
    /// </summary>
    public partial class SimuSelect : UserControl
    {
        public bool IsSelected { get; set; } = false;
        public SimuSelect()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Setting.SelectSimu() != "")
            {
                IsSelected = true;
                SSelected();
                text.Text = "√ 已选择";
            };
        }

        public delegate void Selected();
        public static event Selected SSelected;
    }
}
