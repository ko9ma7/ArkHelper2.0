using System.Windows;
using System.Windows.Controls;
using System;
using ArkHelper.Xaml;
using System.IO;
using MaterialDesignThemes.Wpf;
using ArkHelper.Style.Control;
using Microsoft.Win32;

namespace ArkHelper.Pages.OtherList
{
    //全是屎山 快逃
    public partial class SCHT : Page
    {
        bool inited = false;
        public SCHT()
        {
            InitializeComponent();

            nextRunTime.Text = GetNextRunTimeStringFormat();
            if (File.Exists(Address.dataExternal + "\\simulator.lnk"))
            {
                CSimuSelWrap();
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
                };
                TextBlock textBlock = new TextBlock()
                {
                    Text = "星期" + strArr[_num],
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(5, 0, 0, 0),
                };
                wrapPanel.Children.Add(checkBox);
                wrapPanel.Children.Add(textBlock);
                ctWeek.Children.Add(wrapPanel);
                _num++;
            }

            foreach (DateTime num in App.Data.arkHelper.schtct.times)
            {
                CreateNewTime(num);
            }
            foreach (DateTime num in App.Data.arkHelper.schtct.forceTimes)
            {
                CreateNewTime(num, true);
            }

            inited = true;
            VisChange();
        }

        private void CSimuSelWrap()
        {
            SimuSel.Visibility = Visibility.Collapsed;
        }

        private void VisChange()
        {
            first_unit.Visibility = second_unit.Visibility = ann_stack.Visibility = ct_stack.Visibility = server_stack.Visibility = (bool)(status_togglebutton.IsChecked) ? Visibility.Visible : Visibility.Collapsed;
            if ((bool)status_togglebutton.IsChecked && ((bool)LSFirst.IsChecked || (bool)customFirst.IsChecked)) { second_unit.Visibility = Visibility.Collapsed; }
        }
        private void CreateNewTime(DateTime num, bool isForce = false)
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
                Margin = new Thickness(0, 0, 0, 0),
                Height = 40,
                Width = 40,
                Style = (System.Windows.Style)FindResource("MaterialDesignIconButton"),
            };
            button.Click += DatetimeDelete;
            Button changeButton = new Button()
            {
                Content = new PackIcon()
                {
                    Kind = PackIconKind.CalendarEdit,
                    Height = 22,
                    Width = 22,
                },
                Height = 40,
                Width = 40,
                Margin = new Thickness(10, 0, 0, 0),
                ToolTip = "从“周期时间”和“固定时间”之间转换",
                Style = (System.Windows.Style)FindResource("MaterialDesignIconButton"),
            };
            changeButton.Click += TimeKindChange;
            TimePicker timePicker = new TimePicker()
            {
                Width = 85,
                SelectedTime = num,
                Margin = new Thickness(0, 0, 5, 0)
            };
            TextBlock textBlock = new TextBlock()
            {
                Text = "每个勾选日期的",
                VerticalAlignment = VerticalAlignment.Center,
                Width = 93,/*
                FontSize=18*/
            };
            DatePicker dp = new DatePicker()
            {
                SelectedDate = num,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 6, 0),
                Width = 87
                /*
                FontSize=18*/
            };
            TextBlock textBlock1 = new TextBlock()
            {
                VerticalAlignment = VerticalAlignment.Center,
                Text = "："
            };

            if (isForce)
            {
                textBlock.Visibility = Visibility.Collapsed;
            }
            else
            {
                dp.Visibility = Visibility.Collapsed;
            }
            wrapPanel.Children.Add(textBlock);
            wrapPanel.Children.Add(dp);/*
            wrapPanel.Children.Add(textBlock1);*/
            wrapPanel.Children.Add(timePicker);
            wrapPanel.Children.Add(changeButton);
            wrapPanel.Children.Add(button);

            ctTime.Children.Add(wrapPanel);
        }

        private void TimeKindChange(object sender, RoutedEventArgs e)
        {
            var a = (Button)sender;
            var wrap = a.Parent as WrapPanel;
            foreach (var item in wrap.Children)
            {
                if (item.GetType() == typeof(DatePicker))
                {
                    if ((item as DatePicker).Visibility == Visibility.Visible)
                        (item as DatePicker).Visibility = Visibility.Collapsed;
                    else
                        (item as DatePicker).Visibility = Visibility.Visible;

                }
                if (item.GetType() == typeof(TextBlock) && (item as TextBlock).Text.Contains("勾选日期"))
                {
                    if ((item as TextBlock).Visibility == Visibility.Visible)
                        (item as TextBlock).Visibility = Visibility.Collapsed;
                    else
                        (item as TextBlock).Visibility = Visibility.Visible;

                }
            }

        }
        private void TimeAdd(object sender, RoutedEventArgs e)
        {
            CreateNewTime(ArkHelperDataStandard.GetDateTimeFromDateAndTime(DateTime.Now, new DateTime(2000, 1, 1, 0, 0, 0)));
        }
        private void DatetimeDelete(object sender, RoutedEventArgs e)
        {
            ctTime.Children.Remove((sender as Button).Parent as WrapPanel);
        }

        #region 监听页面
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
        private void First_Unit_Selected(object sender, RoutedEventArgs e)
        {
            if (!inited) return;

            //检测是哪个button被激活
            var unit = (sender as RadioButton).Name.Replace("First", "").Replace("PR", "PR-"); //first.unit

            //备选状态切换
            second_unit.Visibility = (unit == "LS" || unit == "custom") ? Visibility.Collapsed : Visibility.Visible;

            //关卡
            if (unit.Contains("custom"))
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
            if (!inited) return;

            var unit = (sender as RadioButton).Name.Replace("Second", "");

            if (unit.Contains("custom"))
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
        #endregion

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
        /// 打开AKHCPIMaker
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
                /*RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                registryKey.SetValue("ArkHelper", Address.akh + "\\ArkHelper.exe");//申请开机启动
                */dialog.IsOpen = false;
                App.Data.arkHelper.showGuideInSCHT = false;
            }
            nowPage++;
        }
        #endregion

        #region
        /// <summary>
        /// 获取下一次SCHT运行的时间。
        /// </summary>
        /// <returns></returns>
        public static DateTime GetNextRunTime()
        {
            DateTime nowTime = DateTime.Now;
            DateTime nullTime = new DateTime(2000, 1, 1, 0, 0, 0);

            if (App.Data.scht.status)
            {
                //找到周期时间中最早的DateTime
                var weekEarliestTime = nullTime;
                for (int i = 0; i < 14; i++)//不成就加一，代表次日
                {
                    foreach (DateTime ableToRunTimeInList in App.Data.arkHelper.schtct.times)
                    {
                        DateTime time = new DateTime(year: nowTime.Year,
                                                     month: nowTime.Month,
                                                     day: nowTime.Day + i,
                                                     hour: ableToRunTimeInList.Hour,
                                                     minute: ableToRunTimeInList.Minute,
                                                     second: 59);
                        if (time >= nowTime
                            && App.Data.arkHelper.schtct.weekFliter[ArkHelperDataStandard.GetWeekSubInChinese(time.DayOfWeek)])
                        {
                            weekEarliestTime = time; goto end;//如果生成时间晚于或等于当前时间，且当日未被禁用，则返回这个
                        }
                    }
                }
            end:;
                var forceEarlistTime = nullTime;
                foreach (DateTime time in App.Data.arkHelper.schtct.forceTimes)
                {
                    if (time >= nowTime)
                    {
                        forceEarlistTime = time;
                        break;
                    }
                }

                if (forceEarlistTime.Year != 2000 && weekEarliestTime.Year != 2000)
                {
                    if (forceEarlistTime <= weekEarliestTime)
                    {
                        return forceEarlistTime;
                    }
                    else
                    {
                        return weekEarliestTime;
                    }
                }
                else
                {
                    if (forceEarlistTime.Year == 2000) return weekEarliestTime;
                    else return forceEarlistTime;
                }
            }
            else
            {
                return nullTime;
            }
        }

        /// <summary>
        /// 获取下一次SCHT运行的时间（G格式）。
        /// </summary>
        /// <returns></returns>
        public static string GetNextRunTimeStringFormat()
        {
            var a = GetNextRunTime();
            if (a.Year == 2000)
            {
                return "不会运行";
            }
            else
            {
                return a.ToString("g");
            }
        }
        #endregion

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (!inited) return;
            App.Data.arkHelper.schtct.times.Clear();
            App.Data.arkHelper.schtct.forceTimes.Clear();
            foreach (var timebox in ctTime.Children)
            {
                bool force = false;
                DateTime date = DateTime.Now;
                DateTime time = DateTime.Now;
                foreach (var item in (timebox as WrapPanel).Children)
                {
                    if (item.GetType() == typeof(DatePicker))
                    {
                        if ((item as DatePicker).Visibility == Visibility.Visible)
                        {
                            force = true;
                            date = (DateTime)(item as DatePicker).SelectedDate;
                        }
                    }

                    if (item.GetType() == typeof(TimePicker))
                    {
                        time = (DateTime)(item as TimePicker).SelectedTime;
                    }
                }
                if (force)
                {
                    App.Data.arkHelper.schtct.forceTimes.Add(new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, 59));
                }
                else
                {
                    App.Data.arkHelper.schtct.times.Add(new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, 59));
                }
            }
            App.Data.arkHelper.schtct.times.Sort();
            App.Data.arkHelper.schtct.forceTimes.Sort();
            int aa = 0;
            foreach(var item in ctWeek.Children)
            {
                foreach(var item2 in (item as WrapPanel).Children)
                {
                    if (item2.GetType() == typeof(CheckBox))
                    {
                        App.Data.arkHelper.schtct.weekFliter[aa] = (bool)(item2 as CheckBox).IsChecked;
                    }
                }
                aa++;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            SimuSelect.SSelected += CSimuSelWrap;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            SimuSelect.SSelected -= CSimuSelWrap;

        }
    }
}