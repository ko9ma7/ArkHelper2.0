using System;
using System.Windows;
using System.Diagnostics;
using System.Collections.Generic;
using ArkHelper.Xaml.NewUser;
using System.IO;

namespace ArkHelper
{
    public partial class NewUser : Window
    {
        #region
        public static readonly List<string> PageList = new List<string>()
        {
            @"\Xaml\NewUser\Welcome.xaml",
            @"\Xaml\NewUser\Check.xaml",
            @"\Xaml\NewUser\Simulator.xaml",
            @"\Xaml\NewUser\OK.xaml",
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
                    if (Check.CheckAll())nextpage++;
                    break;
                case @"\Xaml\NewUser\OK.xaml":
                    GuideEnd();
                    return;
                
                default:
                    break;
            }
            Page(nextpage);
        }

        private void GuideEnd()
        {
            Address.Create();
            File.Create(Address.config).Dispose();
            App.SaveData();
            this.Close();
        }

        /// <summary>
        /// 页面导航
        /// </summary>
        /// <param name="next"></param>
        private void Page(int next)
        {
            PagesNavigation.Navigate(new Uri(PageList[next], UriKind.RelativeOrAbsolute));
            nowpage = next;
        }

        #region NEXT开关
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
        #endregion
    }
}
