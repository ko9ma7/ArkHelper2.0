using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;
using System.Windows.Media.Animation;
using ArkHelper.Pages;
using Windows.ApplicationModel.Contacts.DataProvider;
using ArkHelper.Pages.OtherList;

namespace ArkHelper.Xaml
{
    public partial class MainWindow : Window
    {
        #region 侧栏菜单
        private class Menu
        {
            #region 通用字符串
            private static string NormalAddress(string name) => @"\Xaml\" + name + @".xaml";
            #endregion

            #region 属性
            public string ID { get; set; }

            public string Text { get; set; }
            public PackIconKind Icon { get; set; }

            public string XamlFileAddress { get; set; }
            public bool Sync { get; set; }
            public bool IsCollapsed { get; set; }
            public bool IsCheckedWhenInit { get; set; }
            #endregion

            #region 构造函数
            public Menu(string id, string text, PackIconKind icon, string xamlFileAddress = null)
            {
                ID = id;
                Text = text;
                Icon = icon;
                if (xamlFileAddress != null)
                {
                    XamlFileAddress = xamlFileAddress;
                }
                else
                {
                    XamlFileAddress = NormalAddress(id);
                }
            }
            #endregion

            #region 存储列表
            public static readonly List<List<Menu>> MenuItems = new List<List<Menu>>()
            {
                new List<Menu>()
                {
                    new Menu("Home", "主页", PackIconKind.HomeOutline)
                    {
                        IsCheckedWhenInit= true,
                    },
                },
                new List<Menu>()
                {
                    new Menu("MB", "连续作战", PackIconKind.MotionPlay)
                    {
                        Sync = true
                    },
                    //new Menu("RogueLike","RogueLike",PackIconKind.GamepadRoundUp),
                    new Menu("Message","信息流终端",PackIconKind.AndroidMessages),
                    new Menu("UserData_Gacha", "寻访记录查询",PackIconKind.AccountCheck),
                    new Menu("SCHT", "SCHT控制台",PackIconKind.ThermostatAuto),
                    //new Menu("材料计算器","MaterialCalc",PackIconKind.Material),
                    new Menu("SCHTRunning", "SCHT运行时", PackIconKind.ThermostatAuto)
                    {
                        IsCollapsed = true,
                        Sync = true
                    },
                },
                new List<Menu>()
                {
                    new Menu("Setting","设置",PackIconKind.Settings),
    #if DEBUG
                    new Menu("Test","Test",PackIconKind.TestTube),
    #endif
                },
            };
            #endregion

            #region UI
            public Control.SelectButton GetControl()
            {
                return new Control.SelectButton()
                {
                    Text = this.Text,
                    Icon = this.Icon,
                    IsHaveProgressBar = false,
                    IsProgressBarStatic = true,
                };
            }
            #endregion
        }
        #region 折叠按钮
        private void SwitchFold(bool fold)
        {
            foreach (Control.SelectButton sb in FuncList.Children)
            {
                sb.IsFolded = fold;
            }

            //SwitchFoldBtnIcon.Kind = !_fold ? PackIconKind.UnfoldLessVertical : PackIconKind.UnfoldMoreVertical;
        }
        #endregion
        #endregion

        #region 动画
        ThicknessAnimation FrameThicknessAnimation = new ThicknessAnimation()
        {
            From = new Thickness(-20, 0, 20, 0),
            DecelerationRatio = 0.7,
            To = new Thickness(0, 0, 0, 0),
            Duration = new TimeSpan(0, 0, 0, 0, 300)
        };
        DoubleAnimation FrameOpacityAnimation = new DoubleAnimation()
        {
            From = 0,
            To = 1,
            Duration = new TimeSpan(0, 0, 0, 0, 240)
        };
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            this.SizeChanged += (s, e) => SwitchFold(this.Width <= 864);

            #region 装载侧栏菜单
            foreach (List<Menu> menuList in Menu.MenuItems)
            {
                int index = 0;
                foreach (Menu menu in menuList)
                {
                    var menuUIControl = menu.GetControl();
                    if (index == 0) menuUIControl.Margin = new Thickness(0, 15, 0, 0);
                    else menuUIControl.Margin = new Thickness(0, 2, 0, 0);

                    if (menu.IsCheckedWhenInit)
                    {
                        menuUIControl.IsChecked = true;
                        Navigate(menu);
                    }
                    if (menu.IsCollapsed) menuUIControl.Visibility = Visibility.Collapsed;

                    menuUIControl.Click += Navigate_event;
                    menuUIControl.Tag = menu;

                    FuncList.Children.Add(menuUIControl);
                    index++;
                }
            }
            #endregion

            #region 处理ArkHelperArg
            if (App.mainArg.Target == "MainWindow")
            {
                if (App.mainArg.Arg == ArkHelperDataStandard.ArkHelperArg.ArgKind.Navigate)
                {
                    foreach (var menuList in Menu.MenuItems)
                    {
                        var menu = menuList.Find(t => t.ID == App.mainArg.ArgContent);
                        if (menu != null)
                        {
                            Navigate(menu);
                        }
                    }
                    foreach (Control.SelectButton control in FuncList.Children)
                    {
                        if ((control.Tag as Menu).ID == App.mainArg.ArgContent)
                        {
                            control.Visibility = Visibility.Visible;
                            control.IsChecked = true;
                            break;
                        }
                    }
                }
                App.mainArg.Dispose();
            }
            #endregion

            #region 监控pgb改变
            //MB
            MB.BattleStatusChangeEvent += (s, e) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var _btn = SearchMenu("MB");
                    _btn.IsHaveProgressBar = e;
                });
            };
            SCHTRunning.StatusChanged += (s, e) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var _btn = SearchMenu("SCHTRunning");
                    _btn.IsProgressBarStatic = false;
                    _btn.ProgressBarValue = 0;
                    _btn.IsHaveProgressBar = e;
                });
            };
            MB.ProgressChangeEvent += (s, e) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var _btn = SearchMenu("MB");
                    if (e == 0)
                    {
                        _btn.IsProgressBarStatic = false;
                    }
                    else
                    {
                        _btn.IsProgressBarStatic = true;
                    }
                    _btn.ProgressBarValue = e;
                });
            };
            Control.SelectButton SearchMenu(string id)
            {
                foreach (var menuList in FuncList.Children)
                {
                    var btn = menuList as Control.SelectButton;
                    var tag = btn.Tag as Menu;
                    if (tag.ID == id) return btn;
                }
                return null;
            }
            #endregion
        }

        #region 导航
        private Frame ShowingFrame = null;
        private class FrameConfig
        {
            public Menu ShowingPage { get; set; }
            public FrameConfig(Menu menu) { ShowingPage = menu; }
        }
        private void Navigate(Menu menu)
        {
            //需要切换的page是否需要独立frame
            bool needIndependentFrame = menu.Sync;

            if (ShowingFrame != null)
            {
                if ((ShowingFrame.Tag as FrameConfig).ShowingPage == menu)
                {
                    return;
                }
                else
                    ShowingFrame.Visibility = Visibility.Collapsed;//隐藏当前frame
            }

            if (needIndependentFrame)
            {
                //判断当前是否有匹配的frame
                bool ChildrenHaveFrameInNeed = false;
                foreach (var eachInFramegrid in framegrid.Children)
                {
                    var _eachInFramegrid = (Frame)eachInFramegrid;
                    if ((_eachInFramegrid.Tag as FrameConfig).ShowingPage == menu)
                    {
                        ShowingFrame = _eachInFramegrid;
                        ChildrenHaveFrameInNeed = true;
                        _eachInFramegrid.Visibility = Visibility.Visible;
                        break;
                    }
                }
                //没有就自建，有就visible出来
                if (ChildrenHaveFrameInNeed)
                {
                    ShowingFrame.Visibility = Visibility.Visible;
                }
                else
                {
                    //新建一个Frame
                    Frame newFrame = new Frame()
                    {
                        NavigationUIVisibility = System.Windows.Navigation.NavigationUIVisibility.Hidden,
                        Visibility = Visibility.Visible,
                        Tag = new FrameConfig(menu)
                    };
                    //更换
                    ShowingFrame = newFrame;
                    //frame导航到专有页面
                    newFrame.Navigate(new Uri(menu.XamlFileAddress, UriKind.RelativeOrAbsolute));
                    //加进framegrid里
                    framegrid.Children.Add(newFrame);
                }
                PublicFrame.Navigate(new Uri("\\Xaml\\Home.xaml", UriKind.RelativeOrAbsolute));
                Animation(ShowingFrame);
            }
            else
            {
                ShowingFrame = PublicFrame;
                PublicFrame.Visibility = Visibility.Visible;
                PublicFrame.Navigate(new Uri(menu.XamlFileAddress, UriKind.RelativeOrAbsolute));
                PublicFrame.Tag = new FrameConfig(menu);
                Animation(PublicFrame);
            }
            void Animation(Frame frameForAni)
            {
                if (true)
                {
                    frameForAni.BeginAnimation(Frame.MarginProperty, FrameThicknessAnimation);
                    frameForAni.BeginAnimation(Frame.OpacityProperty, FrameOpacityAnimation);
                }
            }
        }
        private void Navigate_event(object sender, RoutedEventArgs e)
        {
            Navigate((sender as Control.SelectButton).Tag as Menu);
            WithSystem.GarbageCollect();
        }
        #endregion

        #region 关闭

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            WithSystem.GarbageCollect();
            if (App.Data.arkHelper.pure && App.OKtoOpenSCHT)
            {
                if (App.Data.scht.status)
                {
                    if (System.Windows.Forms.MessageBox.Show("【后台纯净】开启时，关闭ArkHelper将会导致SCHT无法在指定时间运行。仍要关闭ArkHelper吗？\n注：【后台纯净】可在设置中关闭", "ArkHelper", System.Windows.Forms.MessageBoxButtons.OKCancel, System.Windows.Forms.MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.OK)
                        App.ExitApp();
                    else
                        e.Cancel = true;
                }
                else
                {
                    App.ExitApp();
                }
            }
            else
            {
                App.SaveData();
                new ToastContentBuilder().AddArgument("kind", "Background").AddText("提示").AddText("ArkHelper已进入后台运行").Show();
            }
        }
        #endregion
    }
}
