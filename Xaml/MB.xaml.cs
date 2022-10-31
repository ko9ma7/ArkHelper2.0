using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Windows.Media;

namespace ArkHelper.Asset
{
    public partial class MB : Page
    {
        enum Mode
        {
            san,
            time
        }

        Mode mode;
        int time;
        int exeingtime = 0;

        //方法 表达 + Output
        private void info(string content, bool show = true, Output.InfoKind infokind = Output.InfoKind.Infomational)
        {
            Output.Log(content, "MB", infokind);
            if (show == true)
            {
                //表达
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
                    if (mode == Mode.san)
                    {
                        log_textblock.Text = content;
                    }
                    else
                    {
                        log_textblock.Text = "（" + exeingtime + "/" + time + "）" + "  " + content;
                    }
                    log_textblock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(fontcolor));
                });
            }
        }

        public MB()
        {
            InitializeComponent();
            times_setting.Text = "1";
        }

        private void start(object sender, RoutedEventArgs e)
        {
            //运行前准备：判断
            if ((bool)mode_san.IsChecked) { mode = Mode.san; }
            if ((bool)mode_time.IsChecked) { mode = Mode.time; time = Convert.ToInt32(times_setting.Text); }
            exeingtime = 0;

            //记录 //待优化
            string starttime = DateTime.Now.ToString("g");
            long starttime_sec = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();

            //UI
            start_button.Visibility = Visibility.Collapsed;
            logreport_wrappanel.Visibility = Visibility.Visible;
            battle_setting_wrappanel.IsEnabled = false;

            //准备运行
            info("--- MB START ---", false);
            info("/// 连续作战指挥系统启动");

            //运行 独立线程（隔绝UI线程）
            Task.Run(() =>
            {
                //读取服务器
                string _res = ADB.CMD("shell \"dumpsys window | grep mCurrentFocus\"");
                string server = "CO";
                if (_res.Contains("com.hypergryph.arknights")) { server = "CO"; }
                if (_res.Contains("com.hypergryph.arknights.bilibili")) { server = "CB"; }
                if (_res.Contains("com.YoStarJP.Arknights")) { server = "JP"; }
                if (_res.Contains("com.YoStarEN.Arknights")) { server = "EN"; }
                if (_res.Contains("com.YoStarKR.Arknights")) { server = "KR"; }
                if (_res.Contains("tw.txwy.and.arknights")) { server = "TW"; }
                string package = PinnedData.Server.dataSheet.Select("id = '" + server + "'")[0][3].ToString();
                ADB.process.WaitForExit();

                //log记录，初始化
                info("mode=" + mode + "," + "times=" + time + "," + "server=" + server, false);

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
                    //作战状态：在本里：
                    if (screenshot.ColorPick(77, 70) == "#8C8C8C" || screenshot.ColorPick(1339, 57) == "#FFFFFF")
                    {
                        //等出本
                        info("/// 正在等待前一次作战结束...");
                        for (; ; )
                        {
                            if (!PictureProcess.ColorCheck(77, 70, "#8C8C8C", 1339, 57, "#FFFFFF"))
                            {
                                Thread.Sleep(4500);
                                break;
                            }
                            Thread.Sleep(3000);
                        }
                        for (int i = 1; i <= 2; i++)
                        {
                            ADB.Tap(1204, 290);
                            info("/// 正在退出作战...");
                            Thread.Sleep(1000);
                        }
                        Thread.Sleep(2000);
                        //检测代理指挥是否已经勾选，否则勾选
                        if (!PictureProcess.ColorCheck(1200, mb_adjust_check_daili_xy_y1, "#FFFFFF", 1196, mb_adjust_check_daili_xy_y2, "#FFFFFF"))
                        {
                            info("/// 代理指挥模块未激活 /正在激活代理指挥模块...");
                            ADB.Tap(1200, 680); //激活代理指挥
                        }
                    }
                    //作战状态：不在本里：
                    else
                    {
                        //作战状态：不在本前：
                        if (screenshot.ColorPick(1384, 212) != "#C65342" && screenshot.ColorPick(1371, 211) != "#C65342")
                        {
                            info("/// 未检测到关卡信息界面 /请切换至关卡信息界面", infokind: Output.InfoKind.Warning);
                            Thread.Sleep(3000);
                            goto MBend;
                        }

                        //检测代理指挥是否已经勾选，否则勾选
                        if (screenshot.ColorPick(1200, mb_adjust_check_daili_xy_y1) == "#FFFFFF" || screenshot.ColorPick(1196, mb_adjust_check_daili_xy_y2) == "#FFFFFF") { }
                        else
                        {
                            info("/// 代理指挥模块未激活 /正在激活代理指挥模块...");
                            ADB.Tap(1200, 680); //激活代理指挥
                        }
                    }
                }

            //刷本入口点
            battle:;
                //检查次数
                if (mode == Mode.san) { }
                if (mode == Mode.time && exeingtime >= time) { goto MBend; }
                exeingtime += 1;
                info(" - " + exeingtime + "/" + time + " - ", false);

                //开始行动
                ADB.Tap(1266, 753);
                info("/// 开始行动");
                Thread.Sleep(1000);

                //检查是否有回理智界面
                if (PictureProcess.ColorCheck(1241, 449, "#313131", 859, 646, "#313131"))
                {
                    if (mode == Mode.san)
                    {
                        info("/// 剩余理智不足以指挥本次作战");
                        ADB.Tap(871, 651); //点叉
                        goto MBend;
                    }
                    if (mode == Mode.time)
                    {
                        info("/// 剩余理智不足以指挥本次作战 /正在使用理智恢复物恢复理智...");
                        ADB.Tap(1224, 648);//点对号
                        Thread.Sleep(3000);

                        ADB.Tap(1266, 753);//开始行动（蓝）
                        info("/// 开始行动");
                        Thread.Sleep(2000);

                        //检查是否进入编队展示界面
                        if (!PictureProcess.ColorCheck(1281, 455, "#953000", 1298, 432, "#C14600"))
                        {
                            ADB.Tap(871, 651); //点空白
                            info("/// 理智恢复物不足以恢复理智");
                            goto MBend;
                        }
                    }
                }
                ADB.Tap(1240, 559);//开始行动（红）
                info("/// 开始行动");

                //已进本，等待出本
                info("/// 代理指挥作战运行中");
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
                ADB.GetScreenshot(Address.Screenshot.MB, ArkHelper.UniData.Screenshot);

                //退出作战
                for (int i = 1; i <= 2; i++)
                {
                    ADB.Tap(1204, 290); //点击空白
                    info("/// 正在退出作战...");
                    Thread.Sleep(1000);
                }
                Thread.Sleep(1500);

                //回到入口等待下一轮检测
                goto battle;

            MBend:;
                //结束通知
                info("/// 连续作战指挥系统运行结束");
                string speed_text = "";
                if (exeingtime == 0) { }
                else
                {
                    long speed = (new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() - starttime_sec) / (exeingtime);
                    speed_text = "\n平均配速：" + speed + " 秒/次";
                }
                new ToastContentBuilder().AddArgument("MB")
                .AddText("提示：连续作战指挥已结束")
                .AddText("开始时间：" + starttime + "\n"
                + "结束时间：" + DateTime.Now.ToString("g") + "\n" 
                + "作战次数：" + (exeingtime) + " 次" 
                + speed_text
                ).Show(); //结束通知

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
                info("action=" + after_action, false);

                //判定行动
                if (after_action == "返回游戏首页")
                {
                    ADB.Tap(301, 45);//呼出菜单
                    info("/// 呼出菜单");
                    Thread.Sleep(1000);

                    ADB.Tap(102, 192);//返回主页
                    info("/// 正在返回游戏首页...");
                }
                if (after_action == "关闭游戏")
                {
                    ADB.CMD("shell am force-stop " + package);//结束进程
                    info("/// 正在结束" + package + "进程...");
                }
                if (after_action == "关闭模拟器")
                {
                    WithSystem.KillSimulator();
                    info("/// 正在关闭模拟器...");
                }
                if (after_action == "关机") { WithSystem.Shutdown(); }
                if (after_action == "锁定") { WithSystem.LockWorkStation(); }
                if (after_action == "睡眠") { WithSystem.Sleep(); }

                info("--- MB END ---",false);

                //UI
                Application.Current.Dispatcher.Invoke(() =>
                {
                    logreport_wrappanel.Visibility = Visibility.Collapsed;
                    start_button.Visibility = Visibility.Visible;
                    battle_setting_wrappanel.IsEnabled = true;
                });
            });
        }
    }
}