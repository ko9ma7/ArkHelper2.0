using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System;
using Microsoft.Win32;
using System.IO;
using static ArkHelper.Data.SCHT;

namespace ArkHelper.Pages.OtherList
{
    public partial class SCHT : Page
    {
        bool inited = false;
        public SCHT()
        {
            InitializeComponent();

            this.Margin = new Thickness();

            //UI
            server_combobox.ItemsSource = PinnedData.Server.dataSheet.DefaultView;
            ann_status_togglebutton.IsChecked = ann.status;
            status_togglebutton.IsChecked = status;
            fcm_status_togglebutton.IsChecked = fcm.status;
            server_combobox.SelectedValue = server.id;
            ((RadioButton)GetType().GetField(first.unit.Replace("PR-", "PR") + "First", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(this)).IsChecked = true;
            ((RadioButton)GetType().GetField(second.unit + "Second", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(this)).IsChecked = true;
            ((RadioButton)GetType().GetField(ann.select, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(this)).IsChecked = true;
            if (first.unit != "custom")
            {
                first_cp_combobox.SelectedIndex = Convert.ToInt32(first.cp) - 1;
            }
            else
            {
                first_cp_combobox.SelectedIndex = 0;
            }
            if (second.unit != "custom")
            {
                second_cp_combobox.SelectedIndex = Convert.ToInt32(second.cp) - 1;
            }
            else
            {
                second_cp_combobox.SelectedIndex = 0;
            }
            ann_custom_time_status_checkbox.IsChecked = ann.time.custom;
            w1.Text = ann.time.Mon.ToString();
            w2.Text = ann.time.Tue.ToString();
            w3.Text = ann.time.Wed.ToString();
            w4.Text = ann.time.Thu.ToString();
            w5.Text = ann.time.Fri.ToString();
            w6.Text = ann.time.Sat.ToString();
            w7.Text = ann.time.Sun.ToString();

            VisChange();

            inited = true;
        }
        private void status_togglebutton_Click(object sender, RoutedEventArgs e)
        {
            status = (bool)status_togglebutton.IsChecked;
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
            first.unit = (sender as RadioButton).Name.Replace("First", "").Replace("PR", "PR-"); //first.unit

            //备选状态切换
            second_unit.Visibility = (first.unit == "LS" || first.unit == "custom") ? Visibility.Collapsed : Visibility.Visible;

            //处理下拉栏列表
            DataTable First_cp_info = new DataTable();
            First_cp_info.Columns.Add(new DataColumn("name", typeof(string)));

            //关卡
            if (first.unit == "custom")
            {
                //关闭关卡框
                first_cp_combobox.Visibility = Visibility.Collapsed;
                //选取作战配置
                first.cp = OpenFileAsAkhcpi();
                //返回空值则选取默认值
                if (first.cp == "")
                {
                    LSFirst.IsChecked = true;
                    return;
                }
            }
            else
            {
                //开启关卡框
                first_cp_combobox.Visibility = Visibility.Visible;

                First_cp_info.Rows.Add(first.unit + "-1");
                First_cp_info.Rows.Add(first.unit + "-2");
                if (first.unit == "LS" || first.unit == "CE" || first.unit == "AP" || first.unit == "SK" || first.unit == "CA")
                {
                    First_cp_info.Rows.Add(first.unit + "-3");
                    First_cp_info.Rows.Add(first.unit + "-4");
                    First_cp_info.Rows.Add(first.unit + "-5");
                    if (first.unit == "LS" || first.unit == "CE")
                    {
                        First_cp_info.Rows.Add(first.unit + "-6");
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
            first.cp = (first_cp_combobox.SelectedIndex + 1).ToString();
        }
        private void Second_Unit_Selected(object sender, RoutedEventArgs e)
        {
            int _num = second_cp_combobox.SelectedIndex;
            second.unit = (sender as RadioButton).Name.Replace("Second", "");

            DataTable Second_cp_info = new DataTable();
            Second_cp_info.Columns.Add(new DataColumn("name", typeof(string)));

            if (second.unit == "custom")
            {
                //关闭关卡框
                second_cp_combobox.Visibility = Visibility.Collapsed;
                //选取配置
                second.cp = OpenFileAsAkhcpi();
                //返回空值则选取默认值
                if (second.cp == "")
                {
                    LSSecond.IsChecked = true;
                    return;
                }
            }
            else
            {
                //关闭关卡框
                second_cp_combobox.Visibility = Visibility.Visible;

                Second_cp_info.Rows.Add(second.unit + "-1");
                Second_cp_info.Rows.Add(second.unit + "-2");
                Second_cp_info.Rows.Add(second.unit + "-3");
                Second_cp_info.Rows.Add(second.unit + "-4");
                Second_cp_info.Rows.Add(second.unit + "-5");
                Second_cp_info.Rows.Add(second.unit + "-6");
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
            second.cp = (second_cp_combobox.SelectedIndex + 1).ToString();
        }
        private void ann_status_togglebutton_Click(object sender, RoutedEventArgs e)
        {
            ann.status = (bool)ann_status_togglebutton.IsChecked;
        }
        private void ann_Selected(object sender, RoutedEventArgs e)
        {
            //检测是哪个button被激活
            ann.select = (sender as RadioButton).Name;
        }
        private void fcm_status_togglebutton_Click(object sender, RoutedEventArgs e)
        {
            fcm.status = (bool)fcm_status_togglebutton.IsChecked;
        }
        private void server_combobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            server.id = server_combobox.SelectedValue.ToString();
        }

        private string OpenFileAsAkhcpi()
        {
            string aa = WithSystem.OpenFile("选择关卡配置", "ArkHelper关卡配置文件(*.akhcpi)|*.akhcpi");
            return aa;
        }


        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!inited) return;
            try
            {
                ann.time.Mon = Convert.ToInt32(w1.Text);
                ann.time.Tue = Convert.ToInt32(w2.Text);
                ann.time.Wed = Convert.ToInt32(w3.Text);
                ann.time.Thu = Convert.ToInt32(w4.Text);
                ann.time.Fri = Convert.ToInt32(w5.Text);
                ann.time.Sat = Convert.ToInt32(w6.Text);
                ann.time.Sun = Convert.ToInt32(w7.Text);
            }
            catch { }
        }

        private void ann_custom_time_status_checkbox_Click(object sender, RoutedEventArgs e)
        {
            ann.time.custom = (bool)ann_custom_time_status_checkbox.IsChecked;

        }
    }
}
