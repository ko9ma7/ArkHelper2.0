using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Windows.Media;
using static ArkHelper.Output;
using System.Web.UI.WebControls;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using System.Globalization;
using System.ComponentModel;
using Windows.Services.Maps;
using System.Security.Cryptography;
using System.IO;
using System.Collections.Generic;
using Point = System.Drawing.Point;
using Windows.Devices.PointOfService;
using ArkHelper.Modules.MB;
using Windows.ApplicationModel.Core;
using System.Linq;

namespace ArkHelper
{
    public partial class MB : Page
    {
        #region tool
        private static void UILogger(string content, Output.InfoKind infoKind = Output.InfoKind.Infomational)
        {
            Output.Log(content, "MB", infoKind);
        }
        //方法 表达 + Output
        private void Show(string content, Output.InfoKind infokind = Output.InfoKind.Infomational)
        {
            UILogger(content, infokind);
            if (true)
            {
                string fontcolor = "#00FFFFFF";
                switch (infokind)
                {
                    case Output.InfoKind.Infomational:
                        fontcolor = "#003472";
                        break;
                    case Output.InfoKind.Warning:
                        fontcolor = "#ffa400";
                        break;
                    case Output.InfoKind.Error:
                        fontcolor = "#f20c00";
                        break;
                    case Output.InfoKind.Emergency:
                        fontcolor = "#f20c00";
                        break;
                }
                Application.Current.Dispatcher.Invoke(() =>
                {
                    log_textblock.Text = "/// " + content;
                    log_textblock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(fontcolor));
                });
            }
        }
        #endregion

        #region data
        DateTime _startTime;
        DateTime startTime
        {
            get
            {
                return _startTime;
            }
            set
            {
                _startTime = value;
                monitor_module_start_text.Text = value.ToString("g");
            }
        }

        int _alreadyDone;
        int alreadyDone
        {
            get
            {
                return _alreadyDone;
            }
            set
            {
                _alreadyDone = value;
                monitor_module_progress_up.Text = value.ToString();
            }
        }

        int _aim;
        int aim
        {
            get
            {
                return _aim;
            }
            set
            {
                _aim = value;
                monitor_module_progress_down.Text = (value > 0) ? value.ToString() : "-";
            }
        }

        void InitDataAndFreshUIToInitStatus()
        {
            startTime = DateTime.Now;
            aim = 0;
            alreadyDone = 0;
        }

        void FreshDataWhenStart()
        {
            startTime = DateTime.Now;
            if (uimode == UIMode.MBCore_time || uimode == UIMode.SXYS_time)
            {
                monitor_module_end.Visibility= Visibility.Visible;
            }
            else
            {
                monitor_module_end.Visibility = Visibility.Collapsed;
                aim = 0;
            }
            alreadyDone = 0;
            monitor_module_speed_text.Text = "-";
            monitor_module_end_text.Text = "-";
        }

        private enum UIMode
        {
            MBCore_san, MBCore_time, SXYS, SXYS_time
        }
        private UIMode uimode;

        private void FreshDataAndFreshMonitor(MBImplementation.NextArg nextArg)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                alreadyDone++;

                double speed;
                DateTime end;

                TimeSpan UsingTime = DateTime.Now - startTime;
                speed = UsingTime.TotalSeconds/alreadyDone;
                end = startTime + TimeSpan.FromSeconds(speed*aim);

                monitor_module_speed_text.Text = speed.ToString("0.00");
                if (uimode == UIMode.MBCore_time || uimode == UIMode.SXYS_time)
                {
                    monitor_module_end_text.Text = end.ToString("g");
                }

            });
        }
        #endregion

        #region UI
        public MB()
        {
            InitializeComponent();
            InitDataAndFreshUIToInitStatus();
            mode_san.IsChecked = true;
            Task.Run(() =>
            {
                for (; ; )
                {
                    if (ADB.CheckADBCanUse(new List<string>() { "MB" }))
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            start_button.IsEnabled = true;
                            error.Visibility = Visibility.Collapsed;
                        });
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            start_button.IsEnabled = false;
                            error.Visibility = Visibility.Visible;
                            error.Visibility = start_button.Visibility;
                        });
                    }
                    Thread.Sleep(1000);
                }
            });
        }

        #endregion

        #region 逻辑
        private bool IsBattling = false;
        private void start(object sender, RoutedEventArgs e)
        {
            if (IsBattling)
            {
                stopevent();
            }
            else
                START_MISSION();

        }
        private void START_MISSION()
        {
            ADB.RegisterADBUsing("MB");
            IsBattling = true;

            //用于选择作战方式的参数

            //用于MBCore的参数
            MBCore.ModeType mode = MBCore.ModeType.san;
            int time = -1;

            //用于生息演算的参数
            int time_sxys = -1;

            //判断模式
            if ((bool)mode_san.IsChecked)
            {
                uimode = UIMode.MBCore_san;
                mode = MBCore.ModeType.san;
            }
            if ((bool)mode_time.IsChecked)
            {
                uimode = UIMode.MBCore_time;
                mode = MBCore.ModeType.time;
                time = Convert.ToInt32(times_setting.Text);
                aim = time;
            }
            if ((bool)mode_SXYS.IsChecked)
            {
                uimode = UIMode.SXYS;
                if ((bool)SXYS_time_status.IsChecked)
                {
                    uimode = UIMode.SXYS_time;
                    time_sxys = Convert.ToInt32(SXYS_time.Text);
                    aim = time_sxys;
                }
                else
                {
                }
            }

            //UI
            start_button_icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Stop;
            logreport.Visibility = Visibility.Visible;
            battle_setting_wrappanel.IsEnabled = false;

            FreshDataWhenStart();

            var run = new Thread(() =>
            {
                var startTime = DateTime.Now;//启动时间
                int alreadyTime = 0;

                if (uimode == UIMode.MBCore_san || uimode == UIMode.MBCore_time)
                {
                    var battleMB = new MBCore(mode: mode, time: time, allowToRecoverSantiy: false);
                    battleMB.MessageUpdated += Show;
                    battleMB.MoveToNext += FreshDataAndFreshMonitor;
                    var result = battleMB.Run();

                    if (result.Type == MBCoreResult.ResultType.Succeed)
                    {
                        alreadyTime = result.Time;
                        MBUITaskEndAction();
                    }
                    else
                    {
                        if (result.Type == MBCoreResult.ResultType.Error_NotDetectACheckpoint)
                        {
                            Show("未检测到关卡信息界面 /请切换至关卡信息界面", Output.InfoKind.Warning);
                            Thread.Sleep(3000);
                        }
                        if (result.Type == MBCoreResult.ResultType.Error_AutoDeployNotAvailable)
                        {
                            Show("代理指挥不可用 /请换一个关卡", Output.InfoKind.Warning);
                            Thread.Sleep(3000);
                        }
                        if (result.Type == MBCoreResult.ResultType.Error_UndefinedError)
                        {
                            Show("发生未知错误 /请重启ArkHelper重试", Output.InfoKind.Error);
                            Thread.Sleep(3000);
                        }
                        MISSION_END();
                    }
                }
                if (uimode == UIMode.SXYS || uimode == UIMode.SXYS_time)
                {
                    var battleSXYS = new SXYS(time_sxys);
                    battleSXYS.MessageUpdated += Show;
                    battleSXYS.MoveToNext += FreshDataAndFreshMonitor;

                    var result = battleSXYS.Run();
                    if (result.Type == SXYSResult.ResultType.Succeed)
                    {
                        alreadyTime = 0;
                        MBUITaskEndAction();
                    }
                    else
                    {
                        if (result.Type == SXYSResult.ResultType.Error_NotDetectACheckpoint)
                        {
                            Show("错误 /请切换至生息演算界面", Output.InfoKind.Warning);
                            Thread.Sleep(3000);
                        }
                        if (result.Type == SXYSResult.ResultType.Error_LastTimeNotEnd)
                        {
                            Show("错误 /请结束上次生息演算", Output.InfoKind.Warning);
                            Thread.Sleep(3000);
                        }
                        if (result.Type == SXYSResult.ResultType.Error_UndefinedError)
                        {
                            Show("发生未知错误 /请重启ArkHelper重试", Output.InfoKind.Error);
                            Thread.Sleep(3000);
                        }
                        MISSION_END();
                    }
                }

                void MBUITaskEndAction()
                {
                    var endTime = DateTime.Now;//结束时间
                    TimeSpan UsingTime = endTime - startTime;
                    double speed = (alreadyTime == 0) ? -1 : (UsingTime.TotalSeconds / alreadyTime);

                    //通知
                    var toast = new ToastContentBuilder();
                    toast.AddArgument("kind", "MB");
                    toast.AddText("提示：连续作战指挥已结束");
                    toast.AddText("开始时间：" + startTime.ToString("g") + "\n" + "结束时间：" + endTime.ToString("g"));
                    toast.AddText("作战次数：" + alreadyTime + ((alreadyTime != 0) ? ("\n单次配速：" + speed.ToString("0.00") + "秒/次") : ""));
                    toast.Show();

                    //读取action
                    string after_action = "null";

                    after_action = Application.Current.Dispatcher.Invoke(() =>
                    (after_action_select.SelectedValue as ComboBoxItem).Tag.ToString());

                    UILogger("action=" + after_action);

                    //判定行动
                    if (after_action == "backToHome")
                    {
                        ADB.Tap(301, 45);//呼出菜单
                        Show("呼出菜单");
                        WithSystem.Wait(1000);

                        ADB.Tap(102, 192);//返回主页
                        Show("正在返回游戏首页...");
                    }
                    if (after_action == "closeGame")
                    {
                        string package = ADB.GetGamePackageName(ADB.GetCurrentGameKind());
                        ADB.CMD("shell am force-stop " + package);//结束进程
                        Show("正在结束" + package + "进程...");
                    }
                    if (after_action == "closeSimulator")
                    {
                        WithSystem.KillSimulator(Modules.Connect.ConnectionInfo.Connections.First(t => t.Value.Connected).Key);
                        Show("正在关闭模拟器...");
                    }
                    if (after_action == "shutdown") { WithSystem.Shutdown(); }
                    if (after_action == "lock") { WithSystem.LockWorkStation(); }
                    if (after_action == "sleep") { WithSystem.Sleep(); }

                    MISSION_END();
                }
            });

            stopevent = () =>
            {
                run.Abort();
                MISSION_END();
            };

            run.Start();
        }
        private void MISSION_END()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                IsBattling = false;
                logreport.Visibility = Visibility.Collapsed;
                start_button_icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play;
                battle_setting_wrappanel.IsEnabled = true;
            });

            ADB.UnregisterADBUsing("MB");
        }
        private delegate void STOP();
        static STOP stopevent;
        #endregion
    }
}