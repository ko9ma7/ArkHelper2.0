using System;
using System.Windows;
using System.Diagnostics;
using System.Collections.Generic;

namespace ArkHelper
{
    public partial class NewUser : Window
    {
        #region
        public static readonly List<string> PageList = new List<string>()
        {
            @"\Xaml\NewUser\Welcome.xaml",
            @"\Xaml\NewUser\SCHT.xaml",
            @"\Xaml\SCHT.xaml",
            @"\Xaml\NewUser\Simulator.xaml",
            @"\Xaml\NewUser\Configure.xaml",
            @"\Xaml\NewUser\Guide.xaml",
        };
        int nowpage = -1;
        #endregion

        public NewUser()
        {
            InitializeComponent();
            Page(0);
        }
        private void next_Click(object sender, RoutedEventArgs e)
        {
            int nextpage = nowpage + 1;
            switch (PageList[nowpage])
            {
                case @"\Xaml\NewUser\Welcome.xaml":
                    Address.Create();
                    break;
                case @"\Xaml\NewUser\SCHT.xaml":
                    if (Pages.NewUserList.SCHT.enabled)
                    {
                        App.Data.scht.status = true;
                    }
                    else
                    {
                        Page(nextpage + 1);
                    }
                    break;
                case @"\Xaml\SCHT.xaml":
                    PagesNavigation.Margin = new Thickness(0);
                    break;
                case @"\Xaml\NewUser\Guide.xaml":
                    App.SaveData();
                    Close();
                    Process.Start(Address.akh + "\\ArkHelper.exe");
                    return;
                default:
                    break;
            }
            Page(nextpage);
        }

        private void Page(int next)
        {
            if (PageList[next] == @"\Xaml\NewUser\Configure.xaml")
            {
                if (!Xaml.NewUser.Configure.Exam()) { next += 1; }
            }
            if (PageList[next] == @"\Xaml\SCHT.xaml")
            {
                PagesNavigation.Margin = new Thickness(40, 0, 0, 0);
            }
            PagesNavigation.Navigate(new Uri(PageList[next], UriKind.RelativeOrAbsolute));
            nowpage = next;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //按钮开关接收器
            Pages.NewUserList.Welcome.ClickEvent += seten;
        }

        //next按钮可用状态
        private void seten(bool status) => next.IsEnabled = status;

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            //按钮开关接收器 取消注册
            Pages.NewUserList.Welcome.ClickEvent -= seten;
        }
    }
}
