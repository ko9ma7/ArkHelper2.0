using System.Windows;
using System.Windows.Controls;
using System.Data;
using System;
using ArkHelper.Xaml;
using ArkHelper.Xaml.NewUser;
using System.IO;
using MaterialDesignThemes.Wpf;
using System.Diagnostics;
using Windows.ApplicationModel.Wallet;
using static System.Net.Mime.MediaTypeNames;
using System.ComponentModel;

namespace ArkHelper.Pages.OtherList
{
    //全是屎山 快逃
    public partial class SCHT : Page
    {
        bool inited = false;
        public SCHT()
        {
            InitializeComponent();

            nextRunTime.Text = ArkHelper.Xaml.Widget.SCHTstatus.GetTime();
            foreach (ArkHelper.PinnedData.Simulator.SimuInfo simulator in ArkHelper.PinnedData.Simulator.Support)
            {
                SimuSupport.Text += simulator.Name + " ";
            }
            if (File.Exists(Address.dataExternal + "\\simulator.lnk"))
            {
                SimuSel.Visibility = Visibility.Collapsed;
            }

            //UI
            server_combobox.ItemsSource = PinnedData.Server.dataSheet.DefaultView;
            ann_status_togglebutton.IsChecked = App.Data.scht.ann.status;
            status_togglebutton.IsChecked = App.Data.scht.status;
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
            foreach (int num in App.Data.scht.ann.time)
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
            _num = 0;

            foreach (bool num in App.Data.arkHelper.schtct.weekFliter)
            {
                WrapPanel wrapPanel = new WrapPanel() { Margin = new Thickness(0, 0, 5, 0) };
                CheckBox checkBox = new CheckBox()
                {
                    IsChecked = num,
                    Tag = _num
                };
                TextBlock textBlock = new TextBlock()
                {
                    Text = "星期" + strArr[_num],
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(5, 0, 0, 0),
                };
                checkBox.Click += weekFilterChanged;
                wrapPanel.Children.Add(checkBox);
                wrapPanel.Children.Add(textBlock);
                ctWeek.Children.Add(wrapPanel);
                _num++;
            }

            foreach (DateTime num in App.Data.arkHelper.schtct.times)
            {
                CreateNewTime(num);
            }

            inited = true;
            VisChange();
        }

        private void CreateNewTime(DateTime num)
        {
            WrapPanel wrapPanel = new WrapPanel() { Margin = new Thickness(0, 0, 0, 0) };
            Button button = new Button()
            {
                Content = new PackIcon()
                {
                    Kind = PackIconKind.Close,
                    /*Height = 30,
                    Width = 30,*/
                },
                Height = 40,
                Width = 40,
                Style = (System.Windows.Style)FindResource("MaterialDesignIconButton"),
            };
            button.Click += DatetimeDelete;
            TimePicker timePicker = new TimePicker()
            {
                Width = 93,
                SelectedTime = num,
                Margin = new Thickness(0, 0, 5, 0)
            };
            timePicker.SelectedTimeChanged += TimePicker_SelectedTimeChanged;
            wrapPanel.Children.Add(timePicker);
            wrapPanel.Children.Add(button);

            ctTime.Children.Add(wrapPanel);
        }

        private void TimePicker_SelectedTimeChanged(object sender, RoutedPropertyChangedEventArgs<DateTime?> e)
        {
            ctTimeChanged();
        }

        private void TimeAdd(object sender, RoutedEventArgs e)
        {
            CreateNewTime(new DateTime(2000, 1, 1, 1, 0, 0));
            ctTimeChanged();
        }

        private void DatetimeDelete(object sender, RoutedEventArgs e)
        {
            ctTime.Children.Remove((sender as Button).Parent as WrapPanel);
            ctTimeChanged();
        }

        private void weekFilterChanged(object sender, RoutedEventArgs e)
        {
            if (!inited) return;
            try
            {
                var _sender = (CheckBox)sender;
                App.Data.arkHelper.schtct.weekFliter[(int)_sender.Tag] = (bool)_sender.IsChecked;

            }
            catch { }
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
            if (App.Data.arkHelper.showGuideInSCHT)
            {
                if (App.Data.scht.status) dialog.IsOpen = true;
            }
            VisChange();
        }

        private void VisChange()
        {
            first_unit.Visibility = second_unit.Visibility = ann_stack.Visibility = ct_stack.Visibility = server_stack.Visibility = (bool)(status_togglebutton.IsChecked) ? Visibility.Visible : Visibility.Collapsed;
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
                    App.Data.scht.first.unit = unit + ":##" + cpiAddress + "##";
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
                    App.Data.scht.second.unit = unit + ":##" + cpiAddress + "##";
                }
            }
            else
            {
                App.Data.scht.second.unit = unit;
            }
        }
        private void ann_Selected(object sender, RoutedEventArgs e)
        {
            //检测是哪个button被激活
            App.Data.scht.ann.select = (sender as RadioButton).Name;
        }

        /// <summary>
        /// 选择一个Akhcpi文件
        /// </summary>
        /// <returns>关卡方案，若选择窗口被关闭返回""</returns>
        public static string OpenFileAsAkhcpi()
        {
            string aa = WithSystem.OpenFile("选择关卡方案", "ArkHelper关卡方案文件(*.akhcpi)|*.akhcpi");
            return aa;
        }

        /// <summary>
        /// AKHCPI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            new akhcpiMaker().Show();
        }

        #region Guide
        int nowPage = 1;
        private void next_Click(object sender, RoutedEventArgs e)
        {
            ((Grid)GetType().GetField("GuidePage" + nowPage, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(this)).Visibility = Visibility.Collapsed;
            try
            {
                ((Grid)GetType().GetField("GuidePage" + (nowPage + 1), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(this)).Visibility = Visibility.Visible;

            }
            catch
            {
                dialog.IsOpen = false;
                App.Data.arkHelper.showGuideInSCHT = false;
            }
            nowPage++;
        }
        private void closeUAC(object sender, RoutedEventArgs e)
        {
            Process.Start("https://cn.bing.com/search?q=%E5%85%B3%E9%97%ADuac");
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (Setting.SelectSimu() != "")
            {
                SimuSel.Visibility = Visibility.Collapsed;
            };
        }
        #endregion

        private void ann_custom_time_status_checkbox_Click(object sender, RoutedEventArgs e)
        {
            App.Data.scht.ann.customTime = (bool)ann_custom_time_status_checkbox.IsChecked;
        }

        private void server_combobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            App.Data.scht.server.id = server_combobox.SelectedValue.ToString();
        }

        private void ann_status_togglebutton_Click(object sender, RoutedEventArgs e)
        {
            App.Data.scht.ann.status = (bool)ann_status_togglebutton.IsChecked;
        }

        private void ctTimeChanged()
        {
            if (!inited) return;
            App.Data.arkHelper.schtct.times.Clear();
            foreach (var timebox in ctTime.Children)
                foreach (var item in (timebox as WrapPanel).Children)
                    if (item.GetType() == typeof(TimePicker))
                    {
                        App.Data.arkHelper.schtct.times.Add((DateTime)(item as TimePicker).SelectedTime);
                    }
        }
    }
}