using System.Windows;
using System.Windows.Controls;
using System.Data;
using System;
using ArkHelper.Xaml;

namespace ArkHelper.Pages.OtherList
{
    public partial class SCHT : Page
    {
        bool inited = false;
        public SCHT()
        {
            InitializeComponent();

            //UI
            server_combobox.ItemsSource = PinnedData.Server.dataSheet.DefaultView;
            ann_status_togglebutton.IsChecked = App.Data.scht.ann.status;
            status_togglebutton.IsChecked = App.Data.scht.status;
            fcm_status_togglebutton.IsChecked = App.Data.scht.fcm.status;
            server_combobox.SelectedValue = App.Data.scht.server.id;
            if (!App.Data.scht.first.unit.Contains("custom"))
            {
                ((RadioButton)GetType().GetField(App.Data.scht.first.unit.Replace("PR-", "PR") + "First", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(this)).IsChecked = true;
            }
            else
            {
                customFirst.IsChecked = true;
            }
            if (!App.Data.scht.second.unit.Contains("custom"))
            {
                ((RadioButton)GetType().GetField(App.Data.scht.second.unit + "Second", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(this)).IsChecked = true;
            }
            else
            {
                customSecond.IsChecked = true;
            }
            ((RadioButton)GetType().GetField(App.Data.scht.ann.select, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(this)).IsChecked = true;
            ann_custom_time_status_checkbox.IsChecked = App.Data.scht.ann.customTime;

            string[] strArr = { "一", "二", "三", "四", "五", "六", "日" };
            int _num = 0;
            foreach(int num in App.Data.scht.ann.time)
            {
                WrapPanel wrapPanel = new WrapPanel();
                TextBlock textBlock = new TextBlock()
                {
                    Text = "星期" + strArr[_num] + "：",
                    VerticalAlignment = VerticalAlignment.Center,
                };
                TextBox textBox = new TextBox()
                {
                    MinWidth = 40,
                    Margin = new Thickness(5, 0, 0, 0),
                    Text = num.ToString(),
                    Tag = _num
                };
                textBox.TextChanged += TextBox_TextChanged;
                wrapPanel.Children.Add(textBlock);
                wrapPanel.Children.Add(textBox);
                WeekBox.Children.Add(wrapPanel);
                _num++;
            }

            inited = true;
            VisChange();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!inited) return;
            try
            {
                var _sender = (TextBox)sender;
                App.Data.scht.ann.time[(int)_sender.Tag] = Convert.ToInt32(_sender.Text);
            }
            catch { }
        }

        private void status_togglebutton_Click(object sender, RoutedEventArgs e)
        {
            App.Data.scht.status = (bool)status_togglebutton.IsChecked;
            VisChange();
        }

        private void VisChange()
        {
            first_unit.Visibility = second_unit.Visibility = ann_stack.Visibility = fcm_stack.Visibility = server_stack.Visibility = (bool)(status_togglebutton.IsChecked) ? Visibility.Visible : Visibility.Collapsed;
            if ((bool)status_togglebutton.IsChecked && ((bool)LSFirst.IsChecked || (bool)customFirst.IsChecked)) { second_unit.Visibility = Visibility.Collapsed; }
        }

        private void First_Unit_Selected(object sender, RoutedEventArgs e)
        {
            //检测是哪个button被激活
            var unit = (sender as RadioButton).Name.Replace("First", "").Replace("PR", "PR-"); //first.unit

            //备选状态切换
            second_unit.Visibility = (unit == "LS" || unit == "custom") ? Visibility.Collapsed : Visibility.Visible;

            //关卡
            if (unit == "custom" && inited)
            {
                //选取作战配置
                string cpiAddress = OpenFileAsAkhcpi();
                //返回空值则选取默认值
                if (cpiAddress == "")
                {
                    LSFirst.IsChecked = true;
                    return;
                }
                else
                {
                    App.Data.scht.first.unit = unit + ":" + cpiAddress;
                }
            }
            else
            {
                App.Data.scht.first.unit = unit;
            }
        }
        private void Second_Unit_Selected(object sender, RoutedEventArgs e)
        {
            var unit = (sender as RadioButton).Name.Replace("Second", "");

            if (unit == "custom" && inited)
            {
                //选取配置
                string cpiAddress = OpenFileAsAkhcpi();
                //返回空值则选取默认值
                if (cpiAddress == "")
                {
                    LSSecond.IsChecked = true;
                    return;
                }
                else
                {
                    App.Data.scht.second.unit = unit + ":" + cpiAddress;
                }
            }
            else
            {
                App.Data.scht.second.unit = unit;
            }
        }
        private void ann_status_togglebutton_Click(object sender, RoutedEventArgs e)
        {
            App.Data.scht.ann.status = (bool)ann_status_togglebutton.IsChecked;
        }
        private void ann_Selected(object sender, RoutedEventArgs e)
        {
            //检测是哪个button被激活
            App.Data.scht.ann.select = (sender as RadioButton).Name;
        }
        private void fcm_status_togglebutton_Click(object sender, RoutedEventArgs e)
        {
            App.Data.scht.fcm.status = (bool)fcm_status_togglebutton.IsChecked;
        }
        private void server_combobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            App.Data.scht.server.id = server_combobox.SelectedValue.ToString();
        }

        public static string OpenFileAsAkhcpi()
        {
            string aa = WithSystem.OpenFile("选择关卡方案", "ArkHelper关卡方案文件(*.akhcpi)|*.akhcpi");
            return aa;
        }

        private void ann_custom_time_status_checkbox_Click(object sender, RoutedEventArgs e)
        {
            App.Data.scht.ann.customTime = (bool)ann_custom_time_status_checkbox.IsChecked;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            new akhcpiMaker().Show();
        }
    }
}
