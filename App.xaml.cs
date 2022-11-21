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

        #region 消息
        public static ArrayList UserList;
        public static bool isMessageInited = false;
        public static List<ArkHelperMessage> messages = new List<ArkHelperMessage>();
        public static Task MessageInit = new Task(() =>
        {
            UserList = new ArrayList
            {
                new User(ArkHelperDataStandard.MessageSource.weibo, "7745672941"),//END
                new User(ArkHelperDataStandard.MessageSource.weibo, "6441489862"),//CHO
                new User(ArkHelperDataStandard.MessageSource.weibo, "7499841383"),//TER
                new User(ArkHelperDataStandard.MessageSource.weibo, "7506039414"),//MOU
                new User(ArkHelperDataStandard.MessageSource.weibo, "7461423907"),//HYP
                new User(ArkHelperDataStandard.MessageSource.weibo, "6279793937"),//ARK
                new User(ArkHelperDataStandard.MessageSource.weibo, "7753678921"),//GAW
                new User(ArkHelperDataStandard.MessageSource.weibo, "2954409082"),//PLG
                new User(ArkHelperDataStandard.MessageSource.official_communication,""), //COM
                //new Pages.Message.User(UniData.MessageSource.weibo, "7404330062") //test
            };

            for (; ; Thread.Sleep(60000))
            {
                isMessageInited = false;
                int _a = messages.Count;
                var createat = DateTime.Now;
                if (_a > 0) createat = messages[0].CreateAt;
                messages.Clear();
                foreach (User user in UserList)
                {
                    var _me = user.UpdateMessage();
                    if (_a > 0)
                    {
                        foreach (ArkHelperMessage _message in _me)
                        {
                            //更新通知
                            if (_message.CreateAt > createat)
                            {
                                ToastContentBuilder messageToast = new ToastContentBuilder();
                                messageToast.AddArgument("kind", "Message");
                                messageToast.AddText(user.Name + "发布了新的动态");
                                messageToast.AddText(_message.Text);
                                messageToast.AddCustomTimeStamp(_message.CreateAt);
                                foreach (var me in _message.Medias)
                                {
                                    if (me.Type == Pages.Message.ArkHelperMessage.Media.MediaType.photo)
                                    {
                                        messageToast.AddHeroImage(new Uri(me.Link));
                                        break;
                                    }
                                    if (me.Type == Pages.Message.ArkHelperMessage.Media.MediaType.video)
                                    {
                                        messageToast.AddHeroImage(new Uri(me.Small));
                                        break;
                                    }
                                }
                                messageToast.Show(_toast =>
                                {
                                    _toast.Tag = "Message";
                                });
                            }
                            else { break; }
                        }
                    }

                    foreach (ArkHelperMessage message in _me)
                    {
                        messages.Add(message);
                    }
                }
                messages.Sort();
                //if (messages.Count > 20) { messages.RemoveRange(19, messages.Count - 19); }
                isMessageInited = true;
            }
        });
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
                    //
                    if ((DateTime.Now.Hour == 7 || DateTime.Now.Hour == 19) && DateTime.Now.Minute == 58 && OKtoOpenSCHT && Data.scht.status)
                    {
                        if (Data.scht.fcm.status)
                        {
                            if (DateTime.Now.Hour == 7 || !(DateTime.Now.DayOfWeek == DayOfWeek.Friday || DateTime.Now.DayOfWeek == DayOfWeek.Saturday || DateTime.Now.DayOfWeek == DayOfWeek.Sunday))
                                goto end;
                        }
                        OKtoOpenSCHT = false;
                        Application.Current.Dispatcher.Invoke(delegate
                        {
                            foreach (Window window in Application.Current.Windows)
                            {
                                if (window.GetType() == typeof(MainWindow))
                                {
                                    window.Close();
                                    mainArg = new ArkHelperArg(ArkHelperDataStandard.ArkHelperArg.ArgKind.Navigate, "SCHTRunning", "MainWindow");
                                    OpenMainWindow();
                                }
                            }
                        });
                    }
                end:;
                }
            });
            #endregion
        }
    }
}
