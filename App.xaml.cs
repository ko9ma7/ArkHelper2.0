using ArkHelper.Modules.Connect;
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
        private static Data _data = new Data();
        public static Data Data {
            get
            {
                return _data;
            }
            set
            {
                _data = value;
            }
        }
        public static void LoadData()
        {
            try
            {
                App.Data = JsonSerializer.Deserialize<Data>(File.ReadAllText(Address.config));
            }
            catch
            {
                App.Data = new Data();
            }
            //if (true) ;

            if (Version.Current.Type != Version.Data.VersionType.realese) App.Data.arkHelper.debug = true ;

            App.Data.scht.ct.times.Sort();
            App.Data.scht.ct.forceTimes.Sort();
        }
        public static void SaveData()
        {
            if (File.Exists(Address.config))
            {
                File.Create(Address.config).Dispose();
            }
            try
            {
                File.WriteAllText(Address.config, JsonSerializer.Serialize(App.Data));
            }
            catch { }
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
                        window.WindowState = WindowState.Normal;
                        window.Activate();
                    }
                }
            });
            if (!windowIsOpen) new MainWindow().Show();
        }

        public static void ExitApp()
        {
            notifyIcon.Visible = false;
            ADBInteraction.KillAllAdbProcessesWithShell();

            App.SaveData();
            Output.CloseTextStream();
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
                //Version.Update.Search();
            });
            #endregion

            PinnedData.Server.Load();//fu

#if DEBUG
            App.Data.arkHelper.debug = true;
#endif

            if (!File.Exists(Address.config)) //配置文件缺失
            {
                //if (Directory.Exists(Address.programData)) { Directory.Delete(Address.programData, true); } //删除残缺的数据文件
                //new NewUser().ShowDialog(); //导航到新用户窗口
                var win = new NewUser();
                win.ShowDialog();
            }
            if (!File.Exists(Address.config)) //配置文件缺失
            {
                Current.Shutdown();
            }
            else
            {
                App.LoadData();
                OpenMainWindow();
                notifyIcon.Visible = true;
            }

            Output.Log("ArkHelper Startup,ver=" + Version.Current.ToString() + ",currentDirectory=" + Address.akh + ",dataDirectory=" + Address.data);

            #region 启动message装载

            if (Data.message.status)
                MessageInit.Start();
            #endregion
            #region 按频率持续保存配置
            Task SaveDataBg = Task.Run(() =>
            {
                while (true)
                {
                    int _t = 1000;
                    Thread.Sleep(_t);
                    App.SaveData();
                }
            });
            #endregion
            #region 启动ADB连接
            Task adbConnect = Task.Run(() =>
            {
                ADBStarter.Start();
                Connector.IPConnectionChange += (simu, ble) =>
                {
                    new ToastContentBuilder()
                    .AddArgument("kind", "ADB")
                    .AddText("提示")
                    .AddText("已" + (ble?"取得":"失去") + "与" + (simu as ConnectionInfo.SimuInfo).Name + "的连接")
                    .Show();
                };
            });
            #endregion
            #region SCHT等待
            Task SCHT = Task.Run(() =>
            {
                for (; ; Thread.Sleep(1000))
                {
                    bool isTimeEq(DateTime selTime)
                    {
                        var dateTime = DateTime.Now;
                        return (selTime.Year == dateTime.Year
                        && selTime.Month == dateTime.Month
                        && selTime.Day == dateTime.Day
                        && selTime.Hour == dateTime.Hour
                        && selTime.Minute == dateTime.Minute);
                    }

                    if (!OKtoOpenSCHT
                    || !Data.scht.status
                    //&& false
                    ) goto end;

                    if (isTimeEq(Pages.OtherList.SCHT.GetNextRunTime()))
                    {
                        Data.scht.ct.forceTimes.RemoveAll(dt => isTimeEq(dt));

                        OKtoOpenSCHT = false;
                        Application.Current.Dispatcher.Invoke(delegate
                        {
                            CloseMainWindow();
                            mainArg = new ArkHelperArg(ArkHelperDataStandard.ArkHelperArg.ArgKind.Navigate, "SCHTRunning", "MainWindow");
                            OpenMainWindow();
                        });
                    }

                end:;
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
