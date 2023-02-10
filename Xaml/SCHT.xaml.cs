using System.Windows;
using System.Windows.Controls;
using System;
using ArkHelper.Xaml;
using System.IO;
using MaterialDesignThemes.Wpf;
using ArkHelper.Xaml.Control;
using Microsoft.Win32;
using ArkHelper.Modules.SCHT.Xaml;
using System.Collections.Generic;
using System.Drawing;

namespace ArkHelper.Pages.OtherList
{
    //全是屎山 快逃
    public partial class SCHT : Page
    {
        bool inited = false;
        ArkHelper.ArkHelperDataStandard.Data.SCHT.SCHTData schtData = App.Data.scht.data;

        private class Unit
        {
            public virtual string Name { get; }
            public virtual string ToolTip { get; }
            public virtual string ID { get; }
            /// <summary>
            /// 背景内容，支持纯色、资源图片地址
            /// </summary>
            public virtual object Bkg { get; }
            //普通Unit构造函数
            public Unit(string id, string name, string gain)
            {
                ID = id;
                ToolTip = "关卡编号：" + id + "\n" + "掉落物：" + gain;
                Name = name;
                if (id.Contains("PR"))
                {
                    id = "chips";
                }
                Bkg = "/Asset/SCHT/" + id + ".png";

            }
            //特殊Unit构造函数
            public Unit(string id, string name = null, string discribe = null, string picLoaction = null, Color picColor = new Color())
            {
                ID = id;
                Name = name ?? "";
                ToolTip = discribe;
                if (picLoaction != null)
                {
                    Bkg = picLoaction;
                }
                else
                {
                    Bkg = picColor;
                }
            }

            public virtual UnitButton GetBtn()
            {
                return new UnitButton()
                {
                    Text = Name,
                    ToolTip = ToolTip,
                    BKG = Bkg,
                    Tag = this
                };
            }

            public static List<Unit> units = new List<Unit>()
            {
                new Unit("LS","战术演习","作战记录"),
                new Unit("CE","货物运送","龙门币"),
                new Unit("AP","粉碎防御","采购凭证"),
                new Unit("SK","资源保障","碳、家具零件"),
                new Unit("CA","空中威胁","技巧概要"),
                new Unit("PR-A","固若金汤","医疗、重装芯片"),
                new Unit("PR-B","摧枯拉朽","术师、狙击芯片"),
                new Unit("PR-C","势不可当","辅助、先锋芯片"),
                new Unit("PR-D","身先士卒","特种、近卫芯片"),
                new Unit("custom","自定义","自定义关卡",picColor: ColorTranslator.FromHtml("#DDDDDD"))
            };
        }
        private class Ann : Unit
        {
            public Ann(string id, string name, string type) : base(id, name, type)
            {
                ID = id;
                Name = name;
                Bkg = "/Asset/SCHT/" + ID + ".png";
                ToolTip = "类型：" + type;
            }

            public Ann(string id, string name = null, string discribe = null, string picLoaction = null, Color picColor = default) : base(id, name, discribe, picLoaction, picColor)
            {
                ID = id;
                Name = name ?? "";
                ToolTip = discribe;
                if (picLoaction != null)
                {
                    Bkg = picLoaction;
                }
                else
                {
                    Bkg = picColor;
                }
            }

            public override UnitButton GetBtn()
            {
                return new UnitButton()
                {
                    Text = Name,
                    ToolTip = ToolTip,
                    BKG = Bkg,
                    Tag = this
                };
            }

            public override string ID { get; }
            public override string Name { get; }
            public override string ToolTip { get; }
            public override object Bkg { get; }

            public static List<Unit> anns = new List<Unit>()
            {
                new Ann("CHNB","切尔诺伯格","长期剿灭委托"),
                new Ann("LMOB","龙门外环","长期剿灭委托"),
                new Ann("LMDT","龙门市区","长期剿灭委托"),
                new Ann("TT","当期委托","轮换剿灭委托",picColor: ColorTranslator.FromHtml("#DDDDDD"))
            };
        }

        public SCHT()
        {
            InitializeComponent();

            nextRunTime.Text = GetNextRunTimeStringFormat();
            if (App.Data.scht.showHelper) helper.Visibility = Visibility.Visible;
            if (File.Exists(Address.dataExternal + "\\simulator.lnk"))
            {
                CSimuSelWrap();
            }

            //UI
            server_combobox.ItemsSource = PinnedData.Server.dataSheet.DefaultView;
            ann_status_togglebutton.IsChecked = schtData.ann.status;
            status_togglebutton.IsChecked = App.Data.scht.status;
            server_combobox.SelectedValue = schtData.server.id;
            ann_useCard_checkbox.IsChecked = schtData.ann.allowToUseCard;
            //UI:装载FirstUnit
            foreach (var _unit in Unit.units)
            {
                var _btn = _unit.GetBtn();
                _btn.IsChecked = schtData.first.unit == _unit.ID;
                _btn.Click += First_Unit_Selected;
                FirstGrid.Children.Add(_btn);
            }
            //UI:装载SecondUnit
            foreach (var _unit in Unit.units.FindAll(t => t.ID == "custom" || t.ID == "LS"))
            {
                var _btn = _unit.GetBtn();
                _btn.IsChecked = schtData.second.unit == _unit.ID;
                _btn.Click += Second_Unit_Selected;
                SecondGrid.Children.Add(_btn);
            }
            //UI:装载Ann
            foreach (var _ann in Ann.anns)
            {
                var _btn = _ann.GetBtn();
                _btn.IsChecked = schtData.ann.select == _ann.ID;
                _btn.Click += ann_Selected;
                AnnGrid.Children.Add(_btn);
            }

            ann_custom_time_status_checkbox.IsChecked = schtData.ann.customTime;

            string[] strArr = { "一", "二", "三", "四", "五", "六", "日" };

            int _num = 0;
            foreach (int num in schtData.ann.time)
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

            foreach (bool num in App.Data.scht.ct.weekFliter)
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

            foreach (DateTime num in App.Data.scht.ct.times)
            {
                CreateNewTime(num);
            }
            foreach (DateTime num in App.Data.scht.ct.forceTimes)
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

        private bool GetUnitCheckStatusInArea(string id,UIElement uIElement)
        {
            foreach (var _btn in (uIElement as WrapPanel).Children)
            {
                var btn = _btn as UnitButton;
                var cls = btn.Tag as Unit;
                if(cls.ID== id)
                {
                    return btn.IsChecked;
                }
            }
            return false;
        }
        private void VisChange()
        {
            cuscpi.Visibility = first_unit.Visibility = second_unit.Visibility = ann_stack.Visibility = ct_stack.Visibility = server_stack.Visibility = (bool)(status_togglebutton.IsChecked) ? Visibility.Visible : Visibility.Collapsed;
            if ((bool)status_togglebutton.IsChecked && (GetUnitCheckStatusInArea("LS",FirstGrid) || GetUnitCheckStatusInArea("custom", FirstGrid))) 
            { second_unit.Visibility = Visibility.Collapsed; }
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
            CreateNewTime(DateTime.Now);
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
                schtData.ann.time[(int)_sender.Tag] = Convert.ToInt32(_sender.Text);
            }
            catch { }
        }
        private void status_togglebutton_Click(object sender, RoutedEventArgs e)
        {
            App.Data.scht.status = (bool)status_togglebutton.IsChecked;
            if (App.Data.scht.showGuide)
            {
                if (App.Data.scht.status) dialog.IsOpen = true;
            }
            VisChange();
        }
        private void First_Unit_Selected(object sender, RoutedEventArgs e)
        {
            if (!inited) return;

            //检测是哪个button被激活
            var unit = ((sender as UnitButton).Tag as Unit).ID;

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
                    foreach (var _btn in FirstGrid.Children)
                    {
                        var btn = _btn as UnitButton;
                        if ((btn.Tag as Unit).ID == "LS")
                        {
                            btn.IsChecked = true;
                            break;
                        }
                    }
                    return;
                }
                else
                {
                    schtData.first.unit = unit;
                    schtData.first.cp = cpiAddress;
                }
            }
            else
            {
                schtData.first.unit = unit;
            }
        }
        private void Second_Unit_Selected(object sender, RoutedEventArgs e)
        {
            if (!inited) return;

            var unit = ((sender as UnitButton).Tag as Unit).ID;

            if (unit.Contains("custom"))
            {
                //选取配置
                string cpiAddress = OpenFileAsAkhcpi();
                //返回空值则选取默认值
                if (cpiAddress == "")
                {
                    foreach (var _btn in SecondGrid.Children)
                    {
                        var btn = _btn as UnitButton;
                        if ((btn.Tag as Unit).ID == "LS")
                        {
                            btn.IsChecked = true;
                            break;
                        }
                    }
                    return;
                }
                else
                {
                    schtData.second.unit = unit;
                    schtData.second.cp = cpiAddress;
                }
            }
            else
            {
                schtData.second.unit = unit;
            }
        }
        private void ann_Selected(object sender, RoutedEventArgs e)
        {
            //检测是哪个button被激活
            schtData.ann.select = ((sender as UnitButton).Tag as Unit).ID;
        }
        private void ann_custom_time_status_checkbox_Click(object sender, RoutedEventArgs e)
        {
            schtData.ann.customTime = (bool)ann_custom_time_status_checkbox.IsChecked;
        }
        private void server_combobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            schtData.server.id = server_combobox.SelectedValue.ToString();
        }
        private void ann_status_togglebutton_Click(object sender, RoutedEventArgs e)
        {
            schtData.ann.status = (bool)ann_status_togglebutton.IsChecked;
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
                */
                dialog.IsOpen = false;
                App.Data.scht.showGuide = false;
                helper.Visibility = Visibility.Visible;
                App.Data.scht.showHelper = true;

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
                for (int i = 0; i < 8; i++)//不成就加一，代表次日
                {
                    foreach (DateTime ableToRunTimeInList in App.Data.scht.ct.times)
                    {
                        DateTime time = new DateTime(year: nowTime.Year,
                                                     month: nowTime.Month,
                                                     day: nowTime.Day,
                                                     hour: ableToRunTimeInList.Hour,
                                                     minute: ableToRunTimeInList.Minute,
                                                     second: 59) + new TimeSpan(i, 0, 0, 0);
                        if (time >= nowTime
                            && App.Data.scht.ct.weekFliter[ArkHelperDataStandard.GetWeekSubInChinese(time.DayOfWeek)])
                        {
                            weekEarliestTime = time; goto end;//如果生成时间晚于或等于当前时间，且当日未被禁用，则返回这个
                        }
                    }
                }
            end:;
                var forceEarlistTime = nullTime;
                foreach (DateTime time in App.Data.scht.ct.forceTimes)
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
            App.Data.scht.ct.times.Clear();
            App.Data.scht.ct.forceTimes.Clear();
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
                    App.Data.scht.ct.forceTimes.Add(new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, 59));
                }
                else
                {
                    App.Data.scht.ct.times.Add(new DateTime(2000, 1, 1, time.Hour, time.Minute, 59));
                }
            }
            App.Data.scht.ct.times.Sort();
            App.Data.scht.ct.forceTimes.Sort();
            int aa = 0;
            foreach (var item in ctWeek.Children)
            {
                foreach (var item2 in (item as WrapPanel).Children)
                {
                    if (item2.GetType() == typeof(CheckBox))
                    {
                        App.Data.scht.ct.weekFliter[aa] = (bool)(item2 as CheckBox).IsChecked;
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
            WithSystem.GarbageCollect();
        }

        private void ann_useCard_checkbox_Click(object sender, RoutedEventArgs e)
        {
            schtData.ann.allowToUseCard = (bool)ann_useCard_checkbox.IsChecked;
        }
    }
}