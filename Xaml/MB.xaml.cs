using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Windows.Media;
using static ArkHelper.Output;
using System.Web.UI.WebControls;

namespace ArkHelper
{
    public partial class MB : Page
    {
        #region UI
        private void WriteIntoLog(string content,Output.InfoKind infoKind = Output.InfoKind.Infomational)
        {
            Output.Log(content, "MB", infoKind);
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Info += Show;
        }
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Info -= Show;
        }
        //方法 表达 + Output
        private void Show(string content, Output.InfoKind infokind = Output.InfoKind.Infomational)
        {
            WriteIntoLog(content, infokind);
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
                    log_textblock.Text = content;
                    log_textblock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(fontcolor));
                });
            }
        }

        public MB()
        {
            InitializeComponent();
        }
        private void start(object sender, RoutedEventArgs e)
        {
            Mode mode = Mode.san;
            int time = -1;

            //判断模式
            if ((bool)mode_san.IsChecked) { mode = Mode.san; }
            if ((bool)mode_time.IsChecked)
            {
                mode = Mode.time;
                time = Convert.ToInt32(times_setting.Text);
            }

            //UI
            start_button.Visibility = Visibility.Collapsed;
            logreport_wrappanel.Visibility = Visibility.Visible;
            battle_setting_wrappanel.IsEnabled = false;

            Task.Run(() =>
            {
                var startTime = DateTime.Now;//启动时间

                var alreadyTime = MBCore(mode, time);

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
                string after_action;
                if ((bool)Application.Current.Dispatcher.Invoke(() => is_action.IsChecked))
                {
                    after_action = Application.Current.Dispatcher.Invoke(() =>
                    after_action_select.SelectedValue.ToString()).Replace("System.Windows.Controls.ComboBoxItem: ", "");
                }
                else
                {
                    after_action = "null";
                }
                WriteIntoLog("action=" + after_action);

                //判定行动
                if (after_action == "返回游戏首页")
                {
                    ADB.Tap(301, 45);//呼出菜单
                    Show("/// 呼出菜单");
                    Thread.Sleep(1000);

                    ADB.Tap(102, 192);//返回主页
                    Show("/// 正在返回游戏首页...");
                }
                if (after_action == "关闭游戏")
                {
                    string package = ADB.GetGamePackageName(ADB.GetCurrentGameKind());
                    ADB.CMD("shell am force-stop " + package);//结束进程
                    Show("/// 正在结束" + package + "进程...");
                }
                if (after_action == "关闭模拟器")
                {
                    WithSystem.KillSimulator();
                    Show("/// 正在关闭模拟器...");
                }
                if (after_action == "关机") { WithSystem.Shutdown(); }
                if (after_action == "锁定") { WithSystem.LockWorkStation(); }
                if (after_action == "睡眠") { WithSystem.Sleep(); }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    logreport_wrappanel.Visibility = Visibility.Collapsed;
                    start_button.Visibility = Visibility.Visible;
                    battle_setting_wrappanel.IsEnabled = true;
                });
            });
        }
        #endregion

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
        public delegate void MBMessage(string content, Output.InfoKind infoKind = Output.InfoKind.Infomational);
        public static event MBMessage Info;
        public static int MBCore(Mode mode, int time = -1)
        {
            void Logger(string content, Output.InfoKind infoKind = Output.InfoKind.Infomational)
            {
                Output.Log(content, "MBCore", infoKind);
            }

            int alreadyTime = 0;//已经执行作战次数

            //准备运行
            Logger("--- MB START ---");
            Info("/// 连续作战指挥系统启动");
            //读取服务器
            string server = ADB.GetCurrentGameKind();
            //log记录，初始化
            Logger("mode=" + mode + "," + "times=" + time);
            //代理指挥位置调整
            //适用场景：外服开始作战块跟进下沉之前
            int mb_adjust_check_daili_xy_y1 = 680;
            int mb_adjust_check_daili_xy_y2 = 684;
            if (server == "CO" || server == "CB") { }
            else
            {
                mb_adjust_check_daili_xy_y1 = 669;
                mb_adjust_check_daili_xy_y2 = 665;
            }

            //进本前检查和准备
            using (ADB.Screenshot screenshot = new ADB.Screenshot())
            {
                //作战状态：不在本前：
                if (screenshot.ColorPick(1384, 212) != "#C65342" && screenshot.ColorPick(1371, 211) != "#C65342")
                {
                    Info("/// 未检测到关卡信息界面 /请切换至关卡信息界面", Output.InfoKind.Warning);
                    Thread.Sleep(3000);
                    return 0;
                }

                //检测代理指挥是否已经勾选，否则勾选
                if (screenshot.ColorPick(1200, mb_adjust_check_daili_xy_y1) != "#FFFFFF" && screenshot.ColorPick(1196, mb_adjust_check_daili_xy_y2) != "#FFFFFF")
                {
                    Info("/// 代理指挥模块未激活 /正在激活代理指挥模块...");
                    ADB.Tap(1200, 680); //激活代理指挥
                }
            }
        //刷本入口点
        battle:;
            //检查次数
            if (mode == Mode.time && alreadyTime >= time) { goto MBend; }
            Logger("(" + (alreadyTime+1) + "/" + time + ")");

            //开始行动
            ADB.Tap(1266, 753);
            Info("/// 开始行动");
            Thread.Sleep(1000);

            //检查是否有回理智界面
            if (PictureProcess.ColorCheck(1241, 449, "#313131", 859, 646, "#313131"))
            {
                if (mode == Mode.san)
                {
                    Info("/// 剩余理智不足以指挥本次作战");
                    ADB.Tap(871, 651); //点叉
                    goto MBend;
                }
                if (mode == Mode.time)
                {
                    Info("/// 剩余理智不足以指挥本次作战 /正在使用理智恢复物恢复理智...");
                    ADB.Tap(1224, 648);//点对号
                    Thread.Sleep(3000);

                    ADB.Tap(1266, 753);//开始行动（蓝）
                    Info("/// 开始行动");
                    Thread.Sleep(2000);

                    //检查是否进入编队展示界面
                    if (!PictureProcess.ColorCheck(1281, 455, "#953000", 1298, 432, "#C14600"))
                    {
                        ADB.Tap(871, 651); //点空白
                        Info("/// 理智恢复物不足以恢复理智");
                        goto MBend;
                    }
                }
            }
            ADB.Tap(1240, 559);//开始行动（红）
            Info("/// 开始行动");

            //已进本，等待出本
            Info("/// 代理指挥作战运行中");
            Thread.Sleep(35000);
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
            ADB.GetScreenshot(Address.Screenshot.MB, ArkHelper.ArkHelperDataStandard.Screenshot);

            //退出作战
            for (int i = 1; i <= 2; i++)
            {
                ADB.Tap(1204, 290); //点击空白
                Info("/// 正在退出作战...");
                Thread.Sleep(1000);
            }
            Thread.Sleep(1500);

            //回到入口等待下一轮检测
            alreadyTime += 1;
            goto battle;

        MBend:;
            //结束通知
            Info("/// 连续作战指挥系统运行结束");
            Logger("--- MB END ---");

            return alreadyTime;
        }
    }
}