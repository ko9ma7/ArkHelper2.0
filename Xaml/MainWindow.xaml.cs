using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;
using System.Windows.Media.Animation;

namespace ArkHelper.Xaml
{
    public partial class MainWindow : Window
    {
        #region 侧栏菜单
        private class Menu
        {
            public string Name { get; set; }
            public string ID { get; set; }
            public PackIconKind Icon { get; set; }
            public bool IsHidden { get; set; }
            public Menu(string name, string id, PackIconKind icon, bool ishidden = false)
            {
                Name = name;
                ID = id;
                Icon = icon;
                IsHidden = ishidden;
            }
        }

        readonly List<List<Menu>> MenuItems = new List<List<Menu>>()
        {
            new List<Menu>()
            {
                new Menu("主页","Home",PackIconKind.HomeOutline),
            },
            new List<Menu>()
            {
                new Menu("连续作战","MB",PackIconKind.MotionPlay,true),
                //new Menu("RogueLike","RogueLike",PackIconKind.GamepadRoundUp),
                new Menu("信息流终端","Message",PackIconKind.AndroidMessages,false),
                new Menu("寻访记录查询","UserData_Gacha",PackIconKind.AccountCheck),
                new Menu("SCHT控制台","SCHT",PackIconKind.ThermostatAuto,true),
                //new Menu("材料计算器","MaterialCalc",PackIconKind.Material),
                new Menu("SCHT","SCHTRunning",PackIconKind.ThermostatAuto,true),
            },
            new List<Menu>()
            {
                new Menu("设置","Setting",PackIconKind.Settings),
                new Menu("Test","Test",PackIconKind.TestTube),
            },
        };
        #endregion
        #region 切页
        public static Frame ThisFrame = null;
        #endregion
        #region 动画
        ThicknessAnimation FrameThicknessAnimation = new ThicknessAnimation()
        {
            From = new Thickness(-20,0,20,0),
            DecelerationRatio = 0.7,
            To = new Thickness(0,0,0,0),
            Duration = new TimeSpan(0,0,0,0,300)
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

            #region 侧栏菜单

            var style = (System.Windows.Style)FindResource("RadioButtonDock");
            //装载菜单资源
            foreach (List<Menu> menuList in MenuItems)
            {
                int index = 0;
                foreach (Menu menu in menuList)
                {
                    var rb = new RadioButton()
                    {
                        Name = menu.ID,
                        Tag = menu.Icon.ToString(),
                        ContentStringFormat = menu.Name,
                        Style = style,
                    };
                    if (index == 0) rb.Margin = new Thickness(0, 15, 0, 0);
                    if (menu.ID == "Home") rb.IsChecked = true;
                    if (menu.ID == "SCHTRunning") rb.Visibility = Visibility.Collapsed;
                    rb.Click += Navigate_event;

                    FuncList.Children.Add(rb);
                    index++;
                }
            }
            Navigate("Home");
            #endregion

            if (App.mainArg.Target == "MainWindow")
            {
                if (App.mainArg.Arg == UniData.ArgKind.Navigate)
                {
                    Navigate(App.mainArg.ArgContent);
                    foreach (RadioButton rb in FuncList.Children)
                    {
                        if (rb.Name == App.mainArg.ArgContent)
                        {
                            rb.Visibility = Visibility.Visible;
                            rb.IsChecked = true;
                            break;
                        }
                    }
                }
                App.mainArg.Dispose();
            }
        }

        #region 导航
        private void Navigate(string page)
        {
            //判断需要切换的page是否需要独立frame
            bool needIndependentFrame = false;
            foreach (var menua in MenuItems)
            {
                foreach (var menub in menua)
                {
                    if (page == menub.ID)
                    {
                        needIndependentFrame = menub.IsHidden;
                        goto end;
                    }
                }
            }
        end:;

            
            if (ThisFrame != null)
            {
                if (ThisFrame.Tag.ToString() == page)
                {
                    return;
                }
                //隐藏当前frame
                ThisFrame.Visibility = Visibility.Collapsed;
            }

            if (needIndependentFrame)
            {
                //判断当前是否有匹配的frame
                bool ChildrenHaveFrameInNeed = false;
                foreach (var eachInFramegrid in framegrid.Children)
                {
                    var _eachInFramegrid = (Frame)eachInFramegrid;
                    if (_eachInFramegrid.Tag.ToString() == page)
                    {
                        ThisFrame = _eachInFramegrid;
                        ChildrenHaveFrameInNeed = true;
                        _eachInFramegrid.Visibility = Visibility.Visible;
                        break;
                    }
                }
                //没有就自建，有就visible出来
                if (ChildrenHaveFrameInNeed)
                {
                    ThisFrame.Visibility = Visibility.Visible;
                }
                else
                {
                    //新建一个Frame
                    Frame newFrame = new Frame()
                    {
                        NavigationUIVisibility = System.Windows.Navigation.NavigationUIVisibility.Hidden,
                        Visibility = Visibility.Visible,
                        Tag = page
                    };
                    //更换
                    ThisFrame = newFrame;
                    //frame导航到专有页面
                    newFrame.Navigate(new Uri(@"\Xaml\" + page + ".xaml", UriKind.RelativeOrAbsolute));
                    //加进framegrid里
                    framegrid.Children.Add(newFrame);
                }
                Animation(ThisFrame);
            }
            else
            {
                ThisFrame = frame;
                frame.Visibility = Visibility.Visible;
                frame.Navigate(new Uri(@"\Xaml\" + page + ".xaml", UriKind.RelativeOrAbsolute));
                frame.Tag = page;
                Animation(frame);
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
            RadioButton Navigate_button_clicked = sender as RadioButton;
            var _name = Navigate_button_clicked.Name;
            Navigate(_name);
            WithSystem.GarbageCollect();
        }

        #region 折叠按钮
        /*private void SwitchFold(object sender, RoutedEventArgs e)
        {
            if (Home_button.Style == (System.Windows.Style)FindResource("RadioButtonDock"))
            {
                Home_button.Style = (System.Windows.Style)FindResource("RadioButtonDockCollpase");
                SwitchFoldBtnIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.UnfoldMoreVertical;
            }
            else
            {
                Home_button.Style = (System.Windows.Style)FindResource("RadioButtonDock");
                SwitchFoldBtnIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.UnfoldLessVertical;
            }
        }*/
        #endregion

        #endregion

        #region 关闭

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (App.IsMissionRunning)
            {
                e.Cancel = true;
                WithSystem.Message("任务正在运行，暂时无法关闭ArkHelper");
                return;
            }
            Data.Save();
            if (Data.ArkHelper.pure && !Data.SCHT.status)
            {
                Application.Current.Shutdown();
            }
            else
            {
                App.notifyIcon.Visible = !App.isexit;
                App.IsMainWindowInShow = false;
                if (!App.isexit)
                {
                    new ToastContentBuilder().AddArgument("kind", "Background").AddText("提示").AddText("ArkHelper已进入后台运行").Show();
                }

                WithSystem.GarbageCollect();
            }
        }
        #endregion
    }
}
