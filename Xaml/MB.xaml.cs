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

        #region binding
        DateTime _DStartTime = DateTime.Now;
        DateTime DStartTime
        {
            get { return _DStartTime; }
            set
            {
                _DStartTime = value;
                Application.Current.Dispatcher.Invoke(() => data_begin_text.Text = value.ToString("g"));
            }
        }
        int _Dtime = 0;
        int Dtime
        {
            get { return _Dtime; }
            set { _Dtime = value; Application.Current.Dispatcher.Invoke(() => data_progress_alreadyT.Text = value.ToString("g")); }
        }
        int _DT = 0;
        int DT
        {
            get { return _DT; }
            set { _DT = value; data_progress_T.Text = value + ""; }
        }
        private void reactNext(MBImplementation.NextArg nextArg)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Dtime += 1;
                if (data_speed_text.Text == "--" && Dtime == 1)
                {
                    TimeSpan UsingTime = DateTime.Now - DStartTime;
                    data_speed_text.Text = UsingTime.TotalSeconds.ToString("0.00");
                    if (uimode == UIMode.time || uimode == UIMode.SXYS_time)
                    {
                        var endtime = DStartTime + new TimeSpan(UsingTime.Ticks * DT);
                        data_end_text.Text = endtime.ToString("g");
                    }
                }
            });
        }
        #endregion

        #region UI
        public MB()
        {
            InitializeComponent();
            Task.Run(() =>
            {
                for (; ; )
                {
                    if (ADB.CheckADBCanUsed(new List<string>() { "MB"}))
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
                    WithSystem.Wait(1000);
                }
            });
        }
        private enum UIMode
        {
            san, time, SXYS, SXYS_time
        }
        private UIMode uimode;
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
            IsBattling= true;

            //用于选择作战方式的参数

            //用于MBCore的参数
            MBCore.ModeType mode = MBCore.ModeType.san;
            int time = -1;

            //用于生息演算的参数
            int time_sxys = -1;

            //判断模式
            if ((bool)mode_san.IsChecked)
            {
                uimode = UIMode.san;
                mode = MBCore.ModeType.san;
                data_end.Visibility = Visibility.Collapsed;
                data_progress_T.Text = "--";
            }
            if ((bool)mode_time.IsChecked)
            {
                uimode = UIMode.time;
                mode = MBCore.ModeType.time;
                time = Convert.ToInt32(times_setting.Text);
                DT = time;
                data_end.Visibility = Visibility.Visible;
            }
            if ((bool)mode_SXYS.IsChecked)
            {
                uimode = UIMode.SXYS;
                if ((bool)SXYS_time_status.IsChecked)
                {
                    uimode = UIMode.SXYS_time;
                    time_sxys = Convert.ToInt32(SXYS_time.Text);
                    data_end.Visibility = Visibility.Visible;
                    DT = time_sxys;
                }
                else
                {
                    data_progress_T.Text = "--";
                    data_end.Visibility = Visibility.Collapsed;
                }
            }

            //UI
            start_button_icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Stop;
            logreport.Visibility = Visibility.Visible;
            battle_setting_wrappanel.IsEnabled = false;

            DStartTime = DateTime.Now;
            data_speed_text.Text = "--";
            data_end_text.Text = "--";
            Dtime = 0;

            CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
            var run = new Thread(()=>
            {
                var startTime = DateTime.Now;//启动时间
                int alreadyTime = 0;

                if (uimode == UIMode.san || uimode == UIMode.time)
                {
                    var battleMB = new MBCore(mode: mode, time: time, allowToRecoverSantiy: false);
                    battleMB.MessageUpdated += Show;
                    battleMB.MoveToNext += reactNext;
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
                            WithSystem.Wait(3000);
                        }
                        if (result.Type == MBCoreResult.ResultType.Error_AutoDeployNotAvailable)
                        {
                            Show("代理指挥不可用 /请换一个关卡", Output.InfoKind.Warning);
                            WithSystem.Wait(3000);
                        }
                        if (result.Type == MBCoreResult.ResultType.Error_UndefinedError)
                        {
                            Show("发生未知错误 /请重启ArkHelper重试", Output.InfoKind.Error);
                            WithSystem.Wait(3000);
                        }
                        MISSION_END();
                    }
                }
                if (uimode == UIMode.SXYS || uimode == UIMode.SXYS_time)
                {
                    var battleSXYS = new SXYS(time_sxys);
                    battleSXYS.MessageUpdated += Show;
                    battleSXYS.MoveToNext += reactNext;

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
                            WithSystem.Wait(3000);
                        }
                        if (result.Type == SXYSResult.ResultType.Error_LastTimeNotEnd)
                        {
                            Show("错误 /请结束上次生息演算", Output.InfoKind.Warning);
                            WithSystem.Wait(3000);
                        }
                        if (result.Type == SXYSResult.ResultType.Error_UndefinedError)
                        {
                            Show("发生未知错误 /请重启ArkHelper重试", Output.InfoKind.Error);
                            WithSystem.Wait(3000);
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
                        WithSystem.KillSimulator();
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