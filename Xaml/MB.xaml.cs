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
                    if (uimode == UIMode.time || uimode == UIMode.SXYS_time)
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
            SXYSInfo += Show;
            Next += reactNext;
            SXYSNext += reactNext;
        }
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Info -= Show;
            SXYSInfo -= Show;
            Next -= reactNext;
            SXYSNext -= reactNext;
        }


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

            //用于MBCore的参数
            Mode mode = Mode.san;
            int time = -1;

            //用于生息演算的参数
            bool SXYS = false;
            int time_sxys = -1;

            //判断模式
            if ((bool)mode_san.IsChecked)
            {
                uimode = UIMode.san;
                mode = Mode.san;
                data_end.Visibility = Visibility.Collapsed;
                data_progress_T.Text = "--";
            }
            if ((bool)mode_time.IsChecked)
            {
                uimode = UIMode.time;
                mode = Mode.time;
                time = Convert.ToInt32(times_setting.Text);
                DT = time;
                data_end.Visibility = Visibility.Visible;
            }
            if ((bool)mode_SXYS.IsChecked)
            {
                SXYS = true;
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

                if (!SXYS)
                {
                    var result = MBCore(mode: mode, time: time, allowToRecoverSantiy: false);
                    if (result.Type == MBResult.ResultType.Succeed)
                    {
                        alreadyTime = result.Time;
                        MBUITaskEndAction();
                    }
                    else
                    {
                        if (result.Type == MBResult.ResultType.Error_NotDetectACheckpoint)
                        {
                            Show("未检测到关卡信息界面 /请切换至关卡信息界面", Output.InfoKind.Warning);
                            WithSystem.Wait(3000);
                        }
                        if (result.Type == MBResult.ResultType.Error_AutoDeployNotAvailable)
                        {
                            Show("代理指挥不可用 /请换一个关卡", Output.InfoKind.Warning);
                            WithSystem.Wait(3000);
                        }
                        if (result.Type == MBResult.ResultType.Error_UndefinedError)
                        {
                            Show("发生未知错误 /请重启ArkHelper重试", Output.InfoKind.Error);
                            WithSystem.Wait(3000);
                        }
                        MISSION_END();
                    }
                }
                else
                {
                    var result = SXYSCore(time_sxys);
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
        private enum UIMode
        {
            san, time, SXYS, SXYS_time
        }
        private UIMode uimode;
        private bool IsBattling = false;
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
        public static MBResult MBCore(Mode mode, int time = -1, bool allowToRecoverSantiy = false, int ann_cardToUse = 0)
        {
            //ann_cardToUse = 1;//debug
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
            Logger("mode=" + mode + "," + "time=" + time + "," + "ann_cardToUse=" + ann_cardToUse);
            //进本前检查和准备
            using (ADB.Screenshot screenshot = new ADB.Screenshot())
            {
                //检测作战类型
                if (screenshot.PicToPoint(Address.res + "\\pic\\UI\\BigStar.png").Count != 0)
                {
                    battleKind = 0;
                }
                else
                {
                    if (screenshot.PicToPoint(Address.res + "\\pic\\UI\\jade.png").Count != 0)
                    {
                        battleKind = 1;
                    }
                    else
                    {
                        return new MBResult(MBResult.ResultType.Error_NotDetectACheckpoint);
                    }
                }
                Logger("battleKind=" + battleKind);

                //检测代理指挥是否可用
                if (screenshot.PicToPoint(Address.res + "\\pic\\UI\\AutoDeployLocked.png").Count != 0)
                {
                    return new MBResult(MBResult.ResultType.Error_AutoDeployNotAvailable);
                }

                //检测代理指挥是否已经勾选，否则勾选
                if (battleKind == 0)
                {
                    if (screenshot.PicToPoint(Address.res + "\\pic\\UI\\AutoDeployON.png", opencv_errorCon: 0.95).Count == 0)
                    {
                        Info("代理指挥模块未激活 /正在激活代理指挥模块...");
                        ADB.Tap(1200, 680); //激活代理指挥
                        WithSystem.Wait(500);
                    }
                }
            }
        //刷本入口点
        battle:;
            if (mode == Mode.time && alreadyTime >= time) { goto MBend; } //检查次数
            if (battleKind == 1)
            {
                Logger("ann_cardAlreadyUsed=" + ann_cardAlreadyUsed);

                /* result
                 * 0:可以使用委托卡，但是没有激活
                 * 1:可以使用委托卡，并且已经激活
                 * 2:不可以使用委托卡
                 */
                int result;

                //给result赋值
                using (ADB.Screenshot screenshot = new ADB.Screenshot())
                {
                    if (screenshot.PicToPoint(Address.res + "\\pic\\UI\\annEntrustAbleToUseAndNotActivated.png", opencv_errorCon: 0.95).Count != 0)
                        result = 0;
                    else
                        if (screenshot.PicToPoint(Address.res + "\\pic\\UI\\annEntrustAbleToUseAndActivated.png", opencv_errorCon: 0.95).Count != 0)
                        result = 1;
                    else
                        result = 2;
                }

                void NotUseCard()
                {
                    using (var screenshot = new ADB.Screenshot())
                    {
                        if (screenshot.PicToPoint(Address.res + "\\pic\\UI\\AutoDeployON.png", opencv_errorCon: 0.95).Count == 0)
                        {
                            Info("代理指挥模块未激活 /正在激活代理指挥模块...");
                            ADB.Tap(1200, 680); //激活代理指挥
                            WithSystem.Wait(500);
                        }
                    }
                    Logger("usingCard=false");
                    firstSleepTime = 35000;
                }
                void UseCard()
                {
                    firstSleepTime = 15000;
                    Logger("usingCard=true");
                    ann_cardAlreadyUsed++;
                }
                switch (result)
                {
                    case 0:
                        if (ann_cardAlreadyUsed < ann_cardToUse)
                        {
                            ADB.Tap(1146, 666);
                            Info("指令：启用全权委托");
                            WithSystem.Wait(500);
                            UseCard();
                        }
                        else
                        {
                            NotUseCard();
                        }
                        break;
                    case 1:
                        if (ann_cardAlreadyUsed >= ann_cardToUse)
                        {
                            ADB.Tap(1146, 666);
                            Info("指令：禁用全权委托");
                            WithSystem.Wait(500);
                            NotUseCard();
                        }
                        else
                        {
                            UseCard();
                        }
                        break;
                    case 2:
                        NotUseCard();
                        break;
                }
            }
            Logger("(" + (alreadyTime + 1) + "/" + time + ")");

        //开始行动
        beginTask:;
            ADB.Tap(1266, 753);
            Info("指令：开始行动");
            WithSystem.Wait(3500);

            using (ADB.Screenshot screenshot = new ADB.Screenshot())
            {
                //检查是否有回理智界面
                if (screenshot.PicToPoint(Address.res + "\\pic\\UI\\RecoverSanitySymbol.png").Count != 0)
                {
                    Info("剩余理智不足以指挥本次作战");
                    //理智模式直接退出
                    if (mode == Mode.san)
                    {
                        ADB.Tap(871, 651); //点叉
                        goto MBend;
                    }
                    //次数模式恢复理智
                    if (mode == Mode.time)
                    {
                        if (allowToRecoverSantiy) goto MBend;
                        Info("指令：使用理智恢复物恢复理智");
                        ADB.Tap(1224, 648);//点对号
                        WithSystem.Wait(3000);

                        goto beginTask;
                    }
                }
            }
            ADB.Tap(1258, 717);//开始行动（红）
            Info("指令：开始行动");

            //已进本，等待出本
            Info("代理指挥作战运行中");
            WithSystem.Wait(firstSleepTime);
            //循环检查是否在本里
            for (; ; )
            {
                WithSystem.Wait(5000);
                if (!PictureProcess.ColorCheck(77, 70, "#8C8C8C", 1341, 62, "#FFFFFF"))
                {
                    WithSystem.Wait(4500);
                    break;
                }
            }

            //退出作战
            WithSystem.Wait(2000);
            for (; ; )
            {
                using (var screenshot = new ADB.Screenshot())
                {
                    if (screenshot.PicToPoint(Address.res + "\\pic\\UI\\missionEndSymbol.png", opencv_errorCon: 0.95).Count != 0)
                    {
                        WithSystem.Wait(2000);
                        screenshot.Save(Address.Screenshot.MB, ArkHelperDataStandard.Screenshot);
                        Touch(4000);
                        break;
                    }
                    else
                    {
                        Touch();
                    }
                }
                void Touch(int waitTime = 4000)
                {
                    ADB.Tap(1204, 290); //点击空白
                    Info("指令：退出作战");
                    WithSystem.Wait(waitTime);
                }
            }

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
            WithSystem.Wait(3000);
            //结束
            Info("连续作战指挥系统运行结束");
            return new MBResult(MBResult.ResultType.Succeed, time: alreadyTime);
        }
        #endregion

        #region SXYS
        public delegate void SXYSMessage(string content, Output.InfoKind infoKind = Output.InfoKind.Infomational);
        /// <summary>
        /// MB进程更新事件
        /// </summary>
        public static event SXYSMessage SXYSInfo;

        public delegate void SXYSNextTime();
        /// <summary>
        /// MB进程更新事件
        /// </summary>
        public static event SXYSNextTime SXYSNext;
        public class SXYSResult
        {
            /// <summary>
            /// 生息演算作战结果
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
                /// 未检测到关卡界面
                /// </summary>
                Error_LastTimeNotEnd,
                /// <summary>
                /// 未知错误
                /// </summary>
                Error_UndefinedError
            }
            public ResultType Type { get; }

            public SXYSResult(ResultType resultType)
            {
                Type = resultType;
                Logger("Code=" + resultType
                    , resultType == ResultType.Succeed ?
                    InfoKind.Infomational :
                    InfoKind.Error);
                Logger("--- 生息演算 END ---");
            }
        }
        /// <summary>
        /// SXYS日志输出
        /// </summary>
        /// <param name="content">日志内容</param>
        /// <param name="infoKind">日志重要程度</param>
        private static void SXYSLogger(string content, Output.InfoKind infoKind = Output.InfoKind.Infomational)
        {
            Output.Log(content, "SXYS", infoKind);
        }
        private static SXYSResult SXYSCore(int time = -1)
        {
            #region tool
            List<Point> getPoint(string pic, double errCon = 0.8, ADB.Screenshot screenshot = null)
            {
                bool newsc = false;
                if (!File.Exists(pic)) return new List<Point>();
                if (screenshot == null)
                {
                    screenshot = new ADB.Screenshot();
                    newsc = true;
                }
                var _pot = screenshot.PicToPoint(pic, opencv_errorCon: errCon);
                if (newsc) screenshot.Dispose();
                return _pot;
            }
            bool isHave(string pic, double errCon = 0.8, ADB.Screenshot screenshot = null)
            {
                if (getPoint(pic, errCon, screenshot).Count == 0) return false;
                else return true;
            }
            string address(string pic) => Address.res + "\\pic\\SXYS\\" + pic + ".png";
            #endregion

            #region voids
            void sleep(double second)
            {
                second = second * 1000;
                WithSystem.Wait((int)second);
            }
            #endregion


            SXYSLogger("--- 生息演算 START ---");
            SXYSInfo("连续生息演算开始");

            var scs = new ADB.Screenshot();
            if (!isHave(address("availableSymbol"), screenshot: scs)) return new SXYSResult(SXYSResult.ResultType.Error_NotDetectACheckpoint);
            if (isHave(address("cancel"), screenshot: scs)) return new SXYSResult(SXYSResult.ResultType.Error_LastTimeNotEnd);
            scs.Dispose();
            int exetimes = 0;

        SXYS_start:;

            ADB.Tap(1358, 740);
            SXYSInfo("开始演算");
            sleep(2);

            ADB.Tap(1301, 54);
            SXYSInfo("选择干员");
            sleep(2);


            ADB.Tap(1301, 54);
            SXYSInfo("快捷编队");
            sleep(1);

            ADB.Tap(530, 769);
            SXYSInfo("清空选择");
            sleep(1);

            ADB.Tap(661, 149);
            SXYSInfo("干员1");
            sleep(1);

            ADB.Tap(1143, 762);
            SXYSInfo("确认");
            sleep(2);

            ADB.Tap(1143, 762);
            SXYSInfo("补充");
            sleep(4);

            ADB.Tap(1143, 762);
            SXYSInfo("确认");
            sleep(10);

            for (; ; ) //天数循环
            {
                if (isHave(address("news"))) sleep(18);

                ADB.Tap(1369, 761);
                SXYSInfo("关闭日报");
                sleep(7);

                if (isHave(address("emergency")))
                {
                    break;
                }

                int judge = 0;

                for (; ; )
                {
                    if (judge == 2) break;

                    ADB.Tap(44, 759);
                    SXYSInfo("缩小地图");
                    sleep(2);

                    var portPoint = getPoint(Address.res + "\\pic\\SXYS\\port.png", 0.9);
                    portPoint.RemoveAll(t => t.Y < 200);

                    foreach (var port in portPoint)//port循环
                    {
                        ADB.Tap(44, 759);
                        SXYSInfo("缩小地图");
                        sleep(2);

                        ADB.Tap(port);
                        SXYSInfo("点击节点");
                        sleep(3);

                        var a = ADB.WaitOnePicture(new List<string>()
                        {
                            address("huntPlace"),
                            address("resourcePlace"),
                        }, 2, 0.7);
                        if (!a.IsEmpty)
                        {
                            ADB.Tap(a);
                            SXYSInfo("点击节点");
                            sleep(2.5);

                            break;
                        }
                    }

                    ADB.Tap(1279, 695);
                    SXYSInfo("开始行动");
                    sleep(2.5);

                    judge++;

                    ADB.Tap(1263, 744);
                    SXYSInfo("准备");
                    sleep(2);

                    ADB.Tap(1277, 356);
                    SXYSInfo("开始行动");
                    sleep(3);

                    ADB.Tap(1411, 550);
                    SXYSInfo("确认");
                    sleep(10);

                    ADB.WaitPicture(address("pack"),30,0.7);
                    sleep(2);

                    ADB.Tap(84, 58);
                    SXYSInfo("退出");
                    sleep(2);

                    ADB.Tap(1161, 473);
                    SXYSInfo("确认离开");
                    sleep(6);

                    ADB.Tap(1258, 283);
                    SXYSInfo("");
                    sleep(5);
                }

                ADB.Tap(1313, 60);
                SXYSInfo("进入下一天");
                sleep(12);
            }

            ADB.Tap(42, 46);
            SXYSInfo("结束");
            sleep(2);

            ADB.Tap(938, 743);
            SXYSInfo("放弃");
            sleep(1);
            
            ADB.Tap(977, 562);
            SXYSInfo("确定");
            sleep(3.5);
            
            ADB.Tap(1302, 701);
            SXYSInfo("确定");
            sleep(1.5);
            
            ADB.Tap(1302, 701);
            SXYSInfo("确定");
            sleep(1.5);
            
            ADB.Tap(1302, 701);
            SXYSInfo("确定");
            sleep(3);

            exetimes++;
            SXYSNext();
            if (exetimes != time) goto SXYS_start;

            return new SXYSResult(SXYSResult.ResultType.Succeed);
        }
        #endregion
    }
}