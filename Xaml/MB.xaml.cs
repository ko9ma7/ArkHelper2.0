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

namespace ArkHelper
{
    public partial class MB : Page
    {
        #region UI
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

        //数据
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
        private void reactNext()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Dtime += 1;
                if (data_speed_text.Text == "--" && Dtime == 1)
                {
                    TimeSpan UsingTime = DateTime.Now - DStartTime;
                    data_speed_text.Text = UsingTime.TotalSeconds.ToString("0.00");
                    if ((bool)mode_time.IsChecked)
                    {
                        var endtime = DStartTime + new TimeSpan(UsingTime.Ticks * DT);
                        data_end_text.Text = endtime.ToString("g");
                    }
                }
            });
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Info += Show;
            Next += reactNext;
        }
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Info -= Show;
            Next -= reactNext;

        }


        public MB()
        {
            InitializeComponent();
        }
        private async void start(object sender, RoutedEventArgs e)
        {
            Mode mode = Mode.san;
            int time = -1;

            //判断模式
            if ((bool)mode_san.IsChecked)
            {
                mode = Mode.san;
                data_end.Visibility = Visibility.Collapsed;
                data_progress_T.Text = "--";
            }
            if ((bool)mode_time.IsChecked)
            {
                mode = Mode.time;
                time = Convert.ToInt32(times_setting.Text);
                DT = time;
                data_end.Visibility = Visibility.Visible;
            }

            //UI
            start_button.Visibility = Visibility.Collapsed;
            logreport_wrappanel.Visibility = Visibility.Visible;
            battle_setting_wrappanel.IsEnabled = false;
            waiting_processbar.Visibility = Visibility.Visible;

            DStartTime = DateTime.Now;
            data_speed_text.Text = "--";
            data_end_text.Text = "--";
            Dtime = 0;

            Task run = new Task(() =>
            {
                var startTime = DateTime.Now;//启动时间

                var result = MBCore(mode: mode, time: time);

                if (result.Type == MBResult.ResultType.Succeed)
                {
                    var alreadyTime = result.Time;
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
                        Thread.Sleep(1000);

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
                }
                else
                {
                    if (result.Type == MBResult.ResultType.Error_NotDetectACheckpoint)
                    {
                        Show("未检测到关卡信息界面 /请切换至关卡信息界面", Output.InfoKind.Warning);
                        Thread.Sleep(3000);
                    }
                    if (result.Type == MBResult.ResultType.Error_AutoDeployNotAvailable)
                    {
                        Show("代理指挥不可用 /请换一个关卡", Output.InfoKind.Warning);
                        Thread.Sleep(3000);
                    }
                    if (result.Type == MBResult.ResultType.Error_UndefinedError)
                    {
                        Show("发生未知错误 /请重启ArkHelper重试", Output.InfoKind.Error);
                        Thread.Sleep(3000);
                    }
                }
            });

            run.Start();
            await run;

            logreport_wrappanel.Visibility = Visibility.Collapsed;
            start_button.Visibility = Visibility.Visible;
            battle_setting_wrappanel.IsEnabled = true;
            waiting_processbar.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region core
        /// <summary>
        /// MB的模式
        /// </summary>
        public enum Mode
        {
            /// <summary>
            /// 理智模式，理智消耗殆尽后结束
            /// </summary>
            san,
            /// <summary>
            /// 次数模式，达到指定次数后结束
            /// </summary>
            time
        }

        /// <summary>
        /// MB核心单元返回类
        /// </summary>
        public class MBResult
        {
            /// <summary>
            /// MB作战次数
            /// </summary>
            public int Time { get; }
            /// <summary>
            /// MB作战结果
            /// </summary>
            public enum ResultType
            {
                /// <summary>
                /// 成功作战结束
                /// </summary>
                Succeed,
                /// <summary>
                /// 未检测到关卡界面
                /// </summary>
                Error_NotDetectACheckpoint,
                /// <summary>
                /// 代理指挥不可用
                /// </summary>
                Error_AutoDeployNotAvailable,
                /// <summary>
                /// 未知错误
                /// </summary>
                Error_UndefinedError
            }
            public ResultType Type { get; }

            public MBResult(ResultType resultType, int time = 0)
            {
                Time = time;
                Type = resultType;
                Logger("Code=" + resultType
                    , resultType == ResultType.Succeed ?
                    InfoKind.Infomational :
                    InfoKind.Error);
                Logger("--- MB END ---");
            }
        }

        /// <summary>
        /// MB日志输出
        /// </summary>
        /// <param name="content">日志内容</param>
        /// <param name="infoKind">日志重要程度</param>
        private static void Logger(string content, Output.InfoKind infoKind = Output.InfoKind.Infomational)
        {
            Output.Log(content, "MBCore", infoKind);
        }

        public delegate void MBMessage(string content, Output.InfoKind infoKind = Output.InfoKind.Infomational);
        /// <summary>
        /// MB进程更新事件
        /// </summary>
        public static event MBMessage Info;

        public delegate void MBNextTime();
        /// <summary>
        /// MB进程更新事件
        /// </summary>
        public static event MBNextTime Next;

        /// <summary>
        /// MB的核心作战单元
        /// </summary>
        /// <param name="mode">模式</param>
        /// <param name="time">次数（仅当mode=Mode.time时可用）</param>
        /// <param name="ann_cardToUse">可用剿灭代理卡数量（仅当作战类型为剿灭时可用）</param>
        /// <returns></returns>
        public static MBResult MBCore(Mode mode, int time = -1, int ann_cardToUse = -1)
        {
            int battleKind = 0;//0：普通，1：剿灭

            int firstSleepTime = 35000;//进入作战到开始检测等待时间

            int alreadyTime = 0;//已经执行作战次数
            int ann_cardAlreadyUsed = 0;//已经使用过的剿灭卡数量

            #region core
            //准备运行
            Logger("--- MB START ---");
            Info("连续作战指挥系统启动");
            //等待连接
            ADB.WaitingSimulator();

            //读服
            string server = ADB.GetCurrentGameKind();
            //log记录，初始化
            Logger("mode=" + mode + "," + "time=" + time);
            //进本前检查和准备
            using (ADB.Screenshot screenshot = new ADB.Screenshot())
            {
                //作战状态：不在本前：
                if (screenshot.PicToPoint(Address.res + "\\pic\\UI\\BigStar.png").Count == 0)
                {
                    return new MBResult(MBResult.ResultType.Error_NotDetectACheckpoint);
                }

                //检测代理指挥是否可用
                if (screenshot.PicToPoint(Address.res + "\\pic\\UI\\AutoDeployLocked.png").Count != 0)
                {
                    return new MBResult(MBResult.ResultType.Error_AutoDeployNotAvailable);
                }

                //检测代理指挥是否已经勾选，否则勾选
                if (screenshot.PicToPoint(Address.res + "\\pic\\UI\\AutoDeployON.png", opencv_errorCon: 0.95).Count == 0)
                {
                    Info("代理指挥模块未激活 /正在激活代理指挥模块...");
                    ADB.Tap(1200, 680); //激活代理指挥
                    Thread.Sleep(500);
                }
            }
        //刷本入口点
        battle:;
            //检查次数
            if (mode == Mode.time && alreadyTime >= time) { goto MBend; }
            Logger("(" + (alreadyTime + 1) + "/" + time + ")");

        //开始行动
        beginTask:;
            ADB.Tap(1266, 753);
            Info("指令：开始行动");
            Thread.Sleep(2000);

            using (ADB.Screenshot screenshot = new ADB.Screenshot())
            {
                //检查是否有回理智界面
                if (screenshot.PicToPoint(Address.res + "\\pic\\UI\\RecoverSanitySymbol.png").Count != 0)
                {
                    //理智模式直接退出
                    if (mode == Mode.san)
                    {
                        Info("剩余理智不足以指挥本次作战");
                        ADB.Tap(871, 651); //点叉
                        goto MBend;
                    }
                    //次数模式恢复理智
                    if (mode == Mode.time)
                    {
                        Info("剩余理智不足以指挥本次作战 /正在使用理智恢复物恢复理智...");
                        ADB.Tap(1224, 648);//点对号
                        Thread.Sleep(3000);

                        goto beginTask;
                    }
                }
            }
            ADB.Tap(1240, 559);//开始行动（红）
            Info("指令：开始行动");

            //已进本，等待出本
            Info("代理指挥作战运行中");
            Thread.Sleep(firstSleepTime);
            //循环检查是否在本里
            for (; ; )
            {
                Thread.Sleep(4000);
                if (!PictureProcess.ColorCheck(77, 70, "#8C8C8C", 1341, 62, "#FFFFFF"))
                {
                    Thread.Sleep(4500);
                    break;
                }
            }

            //截图
            ADB.GetScreenshot(Address.Screenshot.MB, ArkHelperDataStandard.Screenshot);

            //退出作战
            for (int i = 1; i <= 2; i++)
            {
                ADB.Tap(1204, 290); //点击空白
                Info("指令：退出作战");
                Thread.Sleep(1000);
            }
            Thread.Sleep(1500);

            //回到入口等待下一轮检测
            alreadyTime += 1;
            try
            {
                Next();
            }
            catch
            {

            }

            goto battle;
        #endregion

        MBend:;
            Thread.Sleep(3000);
            //结束
            Info("连续作战指挥系统运行结束");
            return new MBResult(MBResult.ResultType.Succeed, time: alreadyTime);
        }
        #endregion
    }
}