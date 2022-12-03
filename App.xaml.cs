using ArkHelper.Pages;
using ArkHelper.Xaml;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static ArkHelper.ArkHelperDataStandard;
using static ArkHelper.Pages.Message;

namespace ArkHelper
{
    public partial class App : Application
    {
        #region 应用配置数据
        public static Data Data = new Data();
        public static void LoadData()
        {
            App.Data = JsonSerializer.Deserialize<Data>(File.ReadAllText(Address.config));

            App.Data.arkHelper.schtct.times.Sort();
        }
        public static void SaveData()
        {
            if (!File.Exists(Address.config))
            {
                File.Create(Address.config).Dispose();
            }
            File.WriteAllText(Address.config, JsonSerializer.Serialize(App.Data));
        }
        #endregion

        #region 托盘和后台
        public static System.Windows.Forms.NotifyIcon notifyIcon = new System.Windows.Forms.NotifyIcon()
        {
            Visible = false,
            Text = "ArkHelper",
            Icon = new System.Drawing.Icon(Address.res + "\\ArkHelper.ico")
        };
        static ContextMenu NotifyIconMenu;
        public static bool OKtoOpenSCHT = true;
        public void NotifyClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)//左键被按下
            {
                OpenMainWindow();
            }
            else //右键按下
            {
                NotifyIconMenu.IsOpen = true;
            }
        }
        private void MenuOpen_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItemClicked = sender as MenuItem;
            if (menuItemClicked.Name == "MenuOpen")
            {
                OpenMainWindow();
            }
            if (menuItemClicked.Name == "MenuExit")
            {
                ExitApp();
            }
        }

        public void OpenMainWindow()
        {
            bool windowIsOpen = false;
            Application.Current.Dispatcher.Invoke(delegate
            {
                foreach (Window window in Application.Current.Windows)
                {
                    if (window.GetType() == typeof(MainWindow))
                    {
                        windowIsOpen = true;
                    }
                }
            });
            if (!windowIsOpen) new MainWindow().Show();
        }

        public static void ExitApp()
        {
            notifyIcon.Visible = false;
            
            App.SaveData();
            Process.GetCurrentProcess().Kill();
            Current.Shutdown();
        }
        #endregion

        #region Arg
        public static ArkHelperArg mainArg = new ArkHelperArg();
        #endregion

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            #region shutdown
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            #endregion

            #region 检测启动
            if (Process.GetProcessesByName("ArkHelper").Count() > 1)
            {
                MessageBox.Show("ArkHelper已经在运行。", "ArkHelper");
                ExitApp();
            }
            #endregion

            #region 监听toast
            ToastNotificationManagerCompat.OnActivated += toastArgs =>
            {
                ToastArguments args = ToastArguments.Parse(toastArgs.Argument);
                //ValueSet userInput = toastArgs.UserInput;
                if (args["kind"].ToString() == "Message")
                {
                    mainArg = new ArkHelperArg(ArkHelperDataStandard.ArkHelperArg.ArgKind.Navigate, "Message", "MainWindow");
                    Application.Current.Dispatcher.Invoke(delegate
                    {
                        OpenMainWindow();
                    });
                }
            };
            #endregion

            #region 托盘和后台
            NotifyIconMenu = (ContextMenu)FindResource("NotifyIconMenu"); //右键菜单
            App.notifyIcon.MouseClick += NotifyClick; //绑定事件
            #endregion

            #region 更新
            Task update = Task.Run(() =>
            {
                Version.Update.Search();
            });
            #endregion

            PinnedData.Server.Load();//fu

            if (!File.Exists(Address.config)) //配置文件缺失
            {
                //if (Directory.Exists(Address.programData)) { Directory.Delete(Address.programData, true); } //删除残缺的数据文件
                //new NewUser().ShowDialog(); //导航到新用户窗口
                var win = new NewUserPolicyWindow();
                win.ShowDialog();
            }
            try
            {
                App.LoadData();
                OpenMainWindow();
                notifyIcon.Visible = true;
            }
            catch
            {
                Current.Shutdown();
            }

            #region 启动message装载

            if (Data.message.status)
                MessageInit.Start();
            #endregion
            #region 启动ADB连接
            Task adbConnect = Task.Run(() =>
            {
                for (; ; Thread.Sleep(3000)) ADB.Connect();
            });
            #endregion
            #region SCHT等待
            Task SCHT = Task.Run(() =>
            {
                for (; ; Thread.Sleep(1000))
                {
                    var nextTime = ArkHelper.Pages.OtherList.SCHT.GetNextRunTime();
                    var dateTime = DateTime.Now;
                    if (OKtoOpenSCHT 
                    && Data.scht.status
                    && nextTime.Year == dateTime.Year
                    && nextTime.Month == dateTime.Month
                    && nextTime.Day == dateTime.Day
                    && nextTime.Hour == dateTime.Hour
                    && nextTime.Minute == dateTime.Minute
                    )
                    {
                        OKtoOpenSCHT = false;
                        Application.Current.Dispatcher.Invoke(delegate
                        {
                            CloseMainWindow();
                            mainArg = new ArkHelperArg(ArkHelperDataStandard.ArkHelperArg.ArgKind.Navigate, "SCHTRunning", "MainWindow");
                            OpenMainWindow();
                        });
                    }
                }
            });
            #endregion
        }

        private static void CloseMainWindow()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.GetType() == typeof(MainWindow))
                {
                    window.Close();
                    WithSystem.GarbageCollect();
                }
            }
        }
    }
}
