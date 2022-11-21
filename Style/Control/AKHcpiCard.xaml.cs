using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

namespace ArkHelper.Style.Control
{
    /// <summary>
    /// AKHcpiCard.xaml 的交互逻辑
    /// </summary>
    public partial class AKHcpiCard : UserControl, IComparable<AKHcpiCard>
    {

        bool inited = false;
        private int num = 1;
        public int Num
        {
            get { return num; }
            set
            {
                num = value;
                if (inited)
                    numVis.Text = num.ToString();
            }
        }

        public int x { get; set; } = 0;
        public int y { get; set; } = 0;
        public int x1 { get; set; } = 0;
        public int x2 { get; set; } = 0;
        public int y1 { get; set; } = 0;
        public int y2 { get; set; } = 0;
        public int time { get; set; } = 0;
        public int fortimes { get; set; } = 1;
        public string custom { get; set; } = "";
        public AKHcpiCard(string code = "null")
        {
            Num = 1;
            InitializeComponent();
            this.DataContext = this;
            tapinfo.Visibility = Visibility.Collapsed;
            swipeinfo.Visibility = Visibility.Collapsed;
            custominfo.Visibility = Visibility.Collapsed;
            if (code != "null")
            {
                var _ak = new AKHcmd(code);
                fortimes = _ak.ForTimes;
                time = _ak.WaitTime;
                custom = _ak.ADBcmd;
                custominfo.Visibility = Visibility.Visible;
                customaa.IsSelected = true;
            }
            else
            {
                tap.IsSelected = true;
                tapinfo.Visibility = Visibility.Visible;
            }

            inited = true;
        }
        public override string ToString()
        {
            string aa = "";
            switch ((select.SelectedItem as ComboBoxItem).Name)
            {
                case "tap":
                    aa = "shell input tap " + x + " " + y;
                    break;
                case "swipe":
                    aa = "shell input swipe " + x1 + " " + y1 + " " + x2 + " " + y2;
                    break;
                case "customaa":
                    aa = custom;
                    break;
            };
            aa += "####" + time + "#;" +
                  "$$$$" + fortimes + "$;";
            return aa;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!inited) return;
            tapinfo.Visibility = Visibility.Collapsed;
            swipeinfo.Visibility = Visibility.Collapsed;
            custominfo.Visibility = Visibility.Collapsed;
            switch ((select.SelectedItem as ComboBoxItem).Name)
            {
                case "tap":
                    tapinfo.Visibility = Visibility.Visible;
                    break;
                case "swipe":
                    swipeinfo.Visibility = Visibility.Visible;
                    break;
                case "customaa":
                    custominfo.Visibility = Visibility.Visible;
                    break;
            };
        }

        public int CompareTo(AKHcpiCard other)
        {
            if (other == null)
            {
                return 1;
            }
            return other.Num.CompareTo(this.Num);
        }

        public delegate void MoveDele(int num, bool up);
        public static event MoveDele moveEvent;

        public delegate void DelDele(int num);
        public static event DelDele delEvent;

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            delEvent(Num);
        }

        /// <summary>
        /// 上移
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            moveEvent(Num, true);
        }

        /// <summary>
        /// 下移
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            moveEvent(Num, false);
        }
    }
}
