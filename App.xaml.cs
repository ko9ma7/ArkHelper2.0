using ArkHelper.Xaml;
using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Windows.Foundation.Collections;
using Windows.System;
using static ArkHelper.Pages.Message;
using Application = System.Windows.Application;

namespace ArkHelper
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        #region 托盘和后台
        public static System.Windows.Forms.NotifyIcon notifyIcon = new System.Windows.Forms.NotifyIcon()
        {
            Visible = false,
            Text = "ArkHelper",
            Icon = new System.Drawing.Icon(Address.res + "\\ArkHelper.ico")
        };
        static ContextMenu NotifyIconMenu;
        public static bool IsMainWindowInShow = false;
        public static bool isexit = false; //确定退出
        public void NotifyClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)//左键被按下
            {
                notifyIcon.Visible = false;
                OpenMainWindow();
            }
            else //右键按下
            {
                NotifyIconMenu.IsOpen = true;
                NotifyIconMenu.Visibility = Visibility.Visible;
            }
        }
        private void MenuOpen_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItemClicked = sender as MenuItem;
            notifyIcon.Visible = false;
            if (menuItemClicked.Name == "MenuOpen")
            {
                OpenMainWindow();
            }
            if (menuItemClicked.Name == "MenuExit")
            {
                OpenMainWindow();
                ExitApp();
            }
        }
        private void OpenMainWindow()
        {
            if (!IsMainWindowInShow)
            {
                new MainWindow().Show();
                IsMainWindowInShow = true;
            }
        }
        public static void ExitApp()
        {
            isexit = true;
            Current.Shutdown();
        }
        #endregion

        #region Arg
        public static UniData.ArkHelperArg mainArg = new UniData.ArkHelperArg();
        #endregion

        #region 消息
        public static ArrayList UserList;
        public static bool isMessageInited = false;
        public static List<ArkHelperMessage> messages = new List<ArkHelperMessage>();
        #endregion

        #region 主页
        public static bool IsMissionRunning = false;
        #endregion

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            #region 监听toast
            // Listen to notification activation
            ToastNotificationManagerCompat.OnActivated += toastArgs =>
            {
                ToastArguments args = ToastArguments.Parse(toastArgs.Argument);
                ValueSet userInput = toastArgs.UserInput;
                if (args.ToString() == "Message")
                {
                    mainArg = new UniData.ArkHelperArg(UniData.ArgKind.Navigate, "Message", "MainWindow");
                    Application.Current.Dispatcher.Invoke(delegate
                    {
                        OpenMainWindow();
                    });
                }
            };
            #endregion

            #region 托盘和后台
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            NotifyIconMenu = (ContextMenu)FindResource("NotifyIconMenu"); //右键菜单
            App.notifyIcon.MouseClick += NotifyClick; //绑定事件
            #endregion

            PinnedData.Server.Load();
            if (!File.Exists(Address.config) || File.Exists(Address.programData + @"\data\new.ini")) //配置文件缺失或未正确配置
            {
                if (Directory.Exists(Address.programData)) { Directory.Delete(Address.programData, true); } //删除残缺的数据文件
                new NewUser().ShowDialog(); //导航到新用户窗口
            }
            else //main
            {
                Data.Load();
                if (e.Args.Contains("SCHT"))
                {
                    if (!Data.SCHT.status)
                    {
                        ExitApp();
                    }
                    mainArg = new UniData.ArkHelperArg(UniData.ArgKind.Navigate, "SCHTRunning", "MainWindow");
                }
                if (e.Args.Contains("test"))
                {
                    MessageBox.Show("test");
                }
                OpenMainWindow();
                Task userInitTask = new Task(() =>
                {
                    UserList = new ArrayList
                    {
                        new Pages.Message.User(UniData.MessageSource.weibo, "7745672941"),//ENT
                        new Pages.Message.User(UniData.MessageSource.weibo, "6441489862"),//CHO
                        new Pages.Message.User(UniData.MessageSource.weibo, "7499841383"),//TLM
                        new Pages.Message.User(UniData.MessageSource.weibo, "7506039414"),//OMT
                        new Pages.Message.User(UniData.MessageSource.weibo, "7461423907"),//HYP
                        new Pages.Message.User(UniData.MessageSource.weibo, "6279793937"),//ARK
                        new Pages.Message.User(UniData.MessageSource.official_communication,""), //COM
                        //new Pages.Message.User(UniData.MessageSource.weibo, "7404330062") //test
                    };

                    for (; ; Thread.Sleep(60000))
                    {
                        isMessageInited = false;
                        int _a = messages.Count;
                        var createat = DateTime.Now;
                        if (_a > 0) createat = messages[0].CreateAt;
                        messages.Clear();
                        foreach (Pages.Message.User user in UserList)
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
                                        messageToast.AddArgument("Message");
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
                userInitTask.Start();
                Task adbConnect = Task.Run(() =>
                {
                    for (; ; Thread.Sleep(3000)) ADB.Connect();
                });
            }
        }
    }
}
