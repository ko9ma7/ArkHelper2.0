using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;

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
            public Menu(string name, string id, PackIconKind icon)
            {
                Name = name;
                ID = id;
                Icon = icon;
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
                new Menu("连续作战","MB",PackIconKind.MotionPlay),
                //new Menu("RogueLike","RogueLike",PackIconKind.GamepadRoundUp),
                new Menu("信息流终端","Message",PackIconKind.AndroidMessages),
                new Menu("SCHT控制台","SCHT",PackIconKind.ThermostatAuto),
                //new Menu("材料计算器","MaterialCalc",PackIconKind.Material),
                new Menu("SCHT","SCHTRunning",PackIconKind.ThermostatAuto),
            },
            new List<Menu>()
            {
                new Menu("设置","Setting",PackIconKind.Settings),
                new Menu("Test","Test",PackIconKind.TestTube),
            },
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
            frame.Navigate(new Uri(@"\Xaml\" + page + ".xaml", UriKind.RelativeOrAbsolute));
        }
        private void Navigate_event(object sender, RoutedEventArgs e)
        {
            RadioButton Navigate_button_clicked = sender as RadioButton;
            var _name = Navigate_button_clicked.Name;
            Navigate(_name);
            WithSystem.GarbageCollect();
        }
        /*
        #region 折叠按钮
        private void SwitchFold(object sender, RoutedEventArgs e)
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
        }
        #endregion*/
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
                    new ToastContentBuilder().AddArgument("kind","Background").AddText("提示").AddText("ArkHelper已进入后台运行").Show();
                }

                WithSystem.GarbageCollect();
            }
        }
        #endregion
    }
}
