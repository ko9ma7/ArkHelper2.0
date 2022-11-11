using System.Windows;
using System.Windows.Controls;
using System.Data;
using System;

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
            ((RadioButton)GetType().GetField(App.Data.scht.first.unit.Replace("PR-", "PR") + "First", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(this)).IsChecked = true;
            ((RadioButton)GetType().GetField(App.Data.scht.second.unit + "Second", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(this)).IsChecked = true;
            ((RadioButton)GetType().GetField(App.Data.scht.ann.select, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(this)).IsChecked = true;
            if (App.Data.scht.first.unit != "custom")
            {
                first_cp_combobox.SelectedIndex = Convert.ToInt32(App.Data.scht.first.cp) - 1;
            }
            else
            {
                first_cp_combobox.SelectedIndex = 0;
            }
            if (App.Data.scht.second.unit != "custom")
            {
                second_cp_combobox.SelectedIndex = Convert.ToInt32(App.Data.scht.second.cp) - 1;
            }
            else
            {
                second_cp_combobox.SelectedIndex = 0;
            }

            ann_custom_time_status_checkbox.IsChecked = App.Data.scht.ann.customTime;
            
            VisChange();

            inited = true;
        }

        private void status_togglebutton_Click(object sender, RoutedEventArgs e)
        {
            App.Data.scht.status = (bool)status_togglebutton.IsChecked;
            VisChange();
        }

        private void VisChange()
        {
            first_unit.Visibility = first_cp.Visibility = second_unit.Visibility = ann_stack.Visibility = fcm_stack.Visibility = server_stack.Visibility = (bool)(status_togglebutton.IsChecked) ? Visibility.Visible : Visibility.Collapsed;
            if ((bool)status_togglebutton.IsChecked && ((bool)LSFirst.IsChecked || (bool)customFirst.IsChecked)) { second_unit.Visibility = Visibility.Collapsed; }
        }


        private void First_Unit_Selected(object sender, RoutedEventArgs e)
        {
            //检测是哪个button被激活
            int _num = first_cp_combobox.SelectedIndex;
            App.Data.scht.first.unit = (sender as RadioButton).Name.Replace("First", "").Replace("PR", "PR-"); //first.unit

            //备选状态切换
            second_unit.Visibility = (App.Data.scht.first.unit == "LS" || App.Data.scht.first.unit == "custom") ? Visibility.Collapsed : Visibility.Visible;

            //处理下拉栏列表
            DataTable First_cp_info = new DataTable();
            First_cp_info.Columns.Add(new DataColumn("name", typeof(string)));

            //关卡
            if (App.Data.scht.first.unit == "custom")
            {
                //关闭关卡框
                first_cp_combobox.Visibility = Visibility.Collapsed;
                //选取作战配置
                App.Data.scht.first.cp = OpenFileAsAkhcpi();
                //返回空值则选取默认值
                if (App.Data.scht.first.cp == "")
                {
                    LSFirst.IsChecked = true;
                    return;
                }
            }
            else
            {
                //开启关卡框
                first_cp_combobox.Visibility = Visibility.Visible;

                First_cp_info.Rows.Add(App.Data.scht.first.unit + "-1");
                First_cp_info.Rows.Add(App.Data.scht.first.unit + "-2");
                if (App.Data.scht.first.unit == "LS" || App.Data.scht.first.unit == "CE" || App.Data.scht.first.unit == "AP" || App.Data.scht.first.unit == "SK" || App.Data.scht.first.unit == "CA")
                {
                    First_cp_info.Rows.Add(App.Data.scht.first.unit + "-3");
                    First_cp_info.Rows.Add(App.Data.scht.first.unit + "-4");
                    First_cp_info.Rows.Add(App.Data.scht.first.unit + "-5");
                    if (App.Data.scht.first.unit == "LS" || App.Data.scht.first.unit == "CE")
                    {
                        First_cp_info.Rows.Add(App.Data.scht.first.unit + "-6");
                    }
                }
            }
            first_cp_combobox.ItemsSource = First_cp_info.DefaultView;
            if (First_cp_info.Rows.Count < _num)
            {
                first_cp_combobox.SelectedIndex = 0;
            }
            else
            {
                first_cp_combobox.SelectedIndex = _num;
            }
            
        }
        private void first_cp_combobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!inited) return;
            App.Data.scht.first.cp = (first_cp_combobox.SelectedIndex + 1).ToString();
        }
        private void Second_Unit_Selected(object sender, RoutedEventArgs e)
        {
            int _num = second_cp_combobox.SelectedIndex;
            App.Data.scht.second.unit = (sender as RadioButton).Name.Replace("Second", "");

            DataTable Second_cp_info = new DataTable();
            Second_cp_info.Columns.Add(new DataColumn("name", typeof(string)));

            if (App.Data.scht.second.unit == "custom")
            {
                //关闭关卡框
                second_cp_combobox.Visibility = Visibility.Collapsed;
                //选取配置
                App.Data.scht.second.cp = OpenFileAsAkhcpi();
                //返回空值则选取默认值
                if (App.Data.scht.second.cp == "")
                {
                    LSSecond.IsChecked = true;
                    return;
                }
            }
            else
            {
                //关闭关卡框
                second_cp_combobox.Visibility = Visibility.Visible;

                for(int i = 1; i <= 6; i++)
                {
                    Second_cp_info.Rows.Add(App.Data.scht.second.unit + "-"+i);
                }
            }
            second_cp_combobox.ItemsSource = Second_cp_info.DefaultView;
            if (Second_cp_info.Rows.Count < _num)
            {
                second_cp_combobox.SelectedIndex = 0;
            }
            else
            {
                second_cp_combobox.SelectedIndex = _num;
            }
        }
        private void second_cp_combobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!inited) return;
            App.Data.scht.second.cp = (second_cp_combobox.SelectedIndex + 1).ToString();
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

        private string OpenFileAsAkhcpi()
        {
            string aa = WithSystem.OpenFile("选择关卡配置", "ArkHelper关卡配置文件(*.akhcpi)|*.akhcpi");
            return aa;
        }


        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            /*if (!inited) return;
            try
            {
                App.Data.scht.ann.time.Mon = Convert.ToInt32(w1.Text);
                App.Data.scht.ann.time.Tue = Convert.ToInt32(w2.Text);
                ann.time.Wed = Convert.ToInt32(w3.Text);
                App.Data.scht.ann.time.Thu = Convert.ToInt32(w4.Text);
                ann.time.Fri = Convert.ToInt32(w5.Text);
                ann.time.Sat = Convert.ToInt32(w6.Text);
                ann.time.Sun = Convert.ToInt32(w7.Text);
            }
            catch { }*/
        }

        private void ann_custom_time_status_checkbox_Click(object sender, RoutedEventArgs e)
        {
            App.Data.scht.ann.customTime = (bool)ann_custom_time_status_checkbox.IsChecked;
        }
    }
}
