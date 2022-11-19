using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Toolkit.Uwp.Notifications;
using static ArkHelper.ADB;
using Windows.Data.Json;
using System.Text.Json;
using System.Collections.Generic;
using Windows.ApplicationModel.VoiceCommands;
using ArkHelper.Xaml;

namespace ArkHelper.Pages.OtherList
{
    /// <summary>
    /// SCHTRunning.xaml 的交互逻辑
    /// </summary>
    public partial class SCHTRunning : Page
    {
        public SCHTRunning()
        {
            InitializeComponent();
        }

        #region standard
        void Akhcmd(AKHcmd akhcmd)
        {
            Info(akhcmd.OutputText);
            akhcmd.RunCmd();
        }
        void Akhcmd(string body, string show = "null", int wait = 0, int repeat = 1)
        {
            var akhcmd = new AKHcmd(body, show, wait, repeat);
            Akhcmd(akhcmd);
        }

        //表达 + Log
        void Info(string content, bool show = true, Output.InfoKind infokind = Output.InfoKind.Infomational)
        {
            Output.Log(content, "SCHT", infokind);

            if (show == true)
            {
                //表达
                string forcolor = "#00FFFFFF";
                string bakcolor = "#00FFFFFF";
                if (infokind == Output.InfoKind.Infomational) { forcolor = "#003472"; }
                if (infokind == Output.InfoKind.Warning) { forcolor = "#000000"; bakcolor = "#fff143"; }
                if (infokind == Output.InfoKind.Error) { forcolor = "#f20c00"; }
                if (infokind == Output.InfoKind.Emergency) { forcolor = "#FFFFFF"; bakcolor = "#ff3300"; }
                Application.Current.Dispatcher.Invoke(() =>
                {
                    TextBlock loglist_item = new TextBlock() { FontSize = 14, Text = content, Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(forcolor)), Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(bakcolor)) };
                    loglist.Items.Add(loglist_item);
                });
            }
        }

        #endregion

        void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //开始工作
            Task SCHT = Task.Run(() =>
            {
                //时间
                string starttime = DateTime.Now.ToString("g");
                long starttime_sec = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();

                //游戏
                string packname = PinnedData.Server.dataSheet.Select("id = '" + App.Data.scht.server.id + "'")[0][3].ToString();

                //周期
                bool AMmode;
                if (4 < DateTime.Now.Hour && 16 > DateTime.Now.Hour) { AMmode = true; } else { AMmode = false; }
                //外服适配
                //EN时差-1（天数），早晚反转
                if (App.Data.scht.server.id == "EN") { DateTime.Now.AddDays(-1).DayOfWeek.ToString(); AMmode = !AMmode; }

                //代理指挥位置调整
                //适用场景 外服开始作战块跟进下沉之前
                int scht_mb_adjust_check_daili_xy_y1 = 680;
                int scht_mb_adjust_check_daili_xy_y2 = 684;
                if (App.Data.scht.server.id == "CO" || App.Data.scht.server.id == "CB") { } else { scht_mb_adjust_check_daili_xy_y1 = 669; scht_mb_adjust_check_daili_xy_y2 = 665; }

                //adcmd计算
                var week = DateTime.Now.DayOfWeek;

                //剿灭次数
                int anntime = 0;

                anntime = 1; //normal

                if (App.Data.scht.fcm.status)
                {
                    switch ((int)week)
                    {
                        case 5:
                        case 6:
                            anntime = 2;
                            break;
                        case 0:
                            anntime = 1;
                            break;
                    }
                }//fcm

                if (App.Data.scht.ann.customTime)
                {
                    var _wek = (int)week - 1;
                    if (_wek < 0) { _wek += 7; }
                    anntime = App.Data.scht.ann.time[_wek];
                }//custom

                //输出到log
                //Info("name=" + name + "," + "ann.status=" + ann.status + "," + "ann.select=" + ann.select + "," + "anntime=" + anntime, false);

                //模拟器未启动则启动
                Info("/// 正在启动神经网络依托平台...");
                Process.Start(Address.dataExternal + @"\simulator.lnk");
                while (ADB.ConnectedInfo == null) Thread.Sleep(2000);
                Thread.Sleep(50000);

                Akhcmd("shell am start -n " + packname + "/com.u8.sdk.U8UnityContext", "/// 正在启动" + packname + "...", 5);

                for (; ; )
                {
                    if (PictureProcess.ColorCheck(719, 759, "#FFD802", 720, 759, "#FFD802")) { break; }
                    Thread.Sleep(3000);
                }

                for (; ; )
                {
                    if (DateTime.Now.Hour >= 20 || !App.Data.scht.fcm.status) { break; }
                    Thread.Sleep(2000);
                }

                Akhcmd("shell input tap 934 220", "/// 指令：START", 9);
                if (App.Data.scht.server.id != "CB") { Akhcmd("shell input tap 721 574", "/// 指令：开始唤醒", 15); }
                Akhcmd("shell input tap 722 719", "/// 指令：收取", 3);
                Akhcmd("shell input tap 1354 90", "/// 指令：退出签到界面", 3);
                Akhcmd("shell input tap 1389 64", "/// 指令：关闭公告", 3);
                Akhcmd("shell input tap 677 49", "/// 指令：关闭窗口", 3);
                for (int i = 1; i <= 2; i++)
                {
                    if (PictureProcess.ColorCheck(887, 508, "#424242", 398, 638, "#424242")) { break; }
                    else
                    {
                        Info("/// 遇到无法关闭的窗口，正在尝试重启游戏解决...", true, Output.InfoKind.Warning);
                        Akhcmd("shell am force-stop " + packname, "/// 指令：强制结束" + packname, 1);
                        Akhcmd("shell am start -n " + packname + "/com.u8.sdk.U8UnityContext", "/// 正在启动" + packname + "...", 30);
                        Akhcmd("shell input tap 934 220", "/// 指令：START", 15);
                        Akhcmd("shell input tap 724 577", "/// 指令：开始唤醒", 25);
                    }
                }
                //邮件
                Akhcmd("shell input tap 217 48", "/// 指令：打开邮件列表", 3);
                Akhcmd("shell input tap 1294 748", "/// 指令：一键收取", 3);
                Akhcmd("shell input tap 715 54", "/// 指令：收取", 2);
                Akhcmd("shell input tap 715 54", "/// 指令：", 2);
                Akhcmd("shell input tap 109 51", "/// 指令：返回", 2);
                //基建
                Akhcmd("shell input tap 1154 697", "/// 指令：基建", 7);
                Akhcmd("shell input tap 1383 111", "/// 指令：EMERGENCY", 2);
                Akhcmd("shell input tap 211 771", "/// 指令：待办事项1", 1);
                Akhcmd("shell input tap 211 771", "/// 指令：待办事项1", 1);
                Akhcmd("shell input tap 211 771", "/// 指令：待办事项1", 1);
                Akhcmd("shell input tap 1378 162", "/// 指令：NOTIFICATION", 2);
                Akhcmd("shell input tap 211 771", "/// 指令：待办事项1", 1);
                Akhcmd("shell input tap 211 771", "/// 指令：待办事项1", 1);
                Akhcmd("shell input tap 211 771", "/// 指令：待办事项1", 1);
                Akhcmd("shell input tap 417 354", "/// 指令：制造站1", 2);
                Akhcmd("shell input tap 327 691", "/// 指令：制造计划", 2);
                Akhcmd("shell input tap 1084 233", "/// 指令：最多", 1);
                Akhcmd("shell input tap 1064 670", "/// 指令：确定", 2);
                Akhcmd("shell input tap 131 334", "/// 指令：制造站2", 1);
                Akhcmd("shell input tap 1371 603", "/// 指令：加速", 2);
                Akhcmd("shell input tap 1081 377", "/// 指令：最多", 1);
                Akhcmd("shell input tap 1069 653", "/// 指令：确定", 2);
                Akhcmd("shell input tap 1258 711", "/// 指令：收取", 2);
                Akhcmd("shell input tap 89 50", "/// 指令：返回", 2);
                Akhcmd("shell input tap 89 50", "/// 指令：返回", 2);
                Akhcmd("shell input tap 89 50", "/// 指令：返回", 2);
                Akhcmd("shell input tap 962 555", "/// 指令：确认", 5);
                if (AMmode == true)
                {
                    void Buy(int x, int y)
                    {
                        Akhcmd("shell input tap " + x + " " + y, "/// 指令：商品", 2);
                        Akhcmd("shell input tap 1042 655", "/// 指令：购买物品", 3);
                        Akhcmd("shell input tap 756 59", "/// 指令：收取", 2);
                    }
                    //线索交流
                    Akhcmd("shell input tap 407 646", "/// 指令：好友", 2);
                    Akhcmd("shell input tap 132 254", "/// 指令：好友列表", 2);
                    Akhcmd("shell input tap 1122 188", "/// 指令：访问基建", 5);
                    for (int i = 1; i <= 10; i++) { Akhcmd("shell input tap 1334 706", "/// 指令：访问下位", 5); }
                    Akhcmd("shell input tap 299 46", "/// 指令：菜单", 2);
                    Akhcmd("shell input tap 103 305", "/// 指令：首页", 1);
                    Akhcmd("shell input tap 962 555", "/// 指令：确认", 5);
                    //信用商店
                    Akhcmd("shell input tap 941 550", "/// 指令：采购中心", 3);
                    Akhcmd("shell input tap 1307 121", "/// 指令：信用交易所", 2);
                    Akhcmd("shell input tap 1146 48", "/// 指令：领取信用", 3);
                    Akhcmd("shell input tap 722 719", "/// 指令：收取", 2);
                    Akhcmd("shell input tap 746 51", "/// 指令：", 2);

                    Buy(146, 309);
                    Buy(426, 290);
                    Buy(716, 298);
                    Buy(1010, 300);
                    Buy(1290, 305);

                    Buy(160, 592);
                    Buy(443, 578);
                    Buy(733, 608);
                    Buy(997, 592);
                    Buy(1290, 583);

                    Akhcmd("shell input tap 299 46", "/// 指令：菜单", 1);
                    Akhcmd("shell input tap 103 305", "/// 指令：首页", 3);
                }

                //剿灭
                if (App.Data.scht.ann.status)
                {
                    Akhcmd("shell input tap 921 214", "/// 指令：终端", 2);
                    Akhcmd("shell input tap 986 754", "/// 指令：常态事务", 2);
                    Akhcmd("shell input tap 706 517", "/// 指令：当期委托", 3);

                    if (App.Data.scht.ann.select != "TT")
                    {
                        Akhcmd("shell input tap 109 51", "/// 指令：返回", 2);
                        Akhcmd("shell input tap 1270 763", "/// 指令：切换", 3);
                        if (App.Data.scht.ann.select == "CHNB") { Akhcmd("shell input tap 1009 529", "/// 指令：切尔诺伯格", 3); }
                        if (App.Data.scht.ann.select == "LMOB") { Akhcmd("shell input tap 1044 295", "/// 指令：龙门外环", 3); }
                        if (App.Data.scht.ann.select == "LMDT") { Akhcmd("shell input tap 1086 417", "/// 指令：龙门市区", 3); }
                    }

                    for (int i = 0; i < anntime; i++)
                    {
                        if (PictureProcess.ColorCheck(431, 770, "#FFFFFF", 432, 770, "#FFFFFF")) { break; }
                        else
                        {
                            int a = 0;
                            if (PictureProcess.ColorCheck(1263, 765, "#232323", 1263, 764, "#232323")) { }
                            else
                            {
                                Akhcmd("shell input tap 1090 666", "/// 指令：激活全权委托", 1);
                                if (PictureProcess.ColorCheck(1263, 765, "#232323", 1263, 764, "#232323")) { /*成功激活*/} else { a = 1; }
                            }

                            if (a == 0)
                            {
                                //使用剿灭卡
                                Akhcmd("shell input tap 1295 745", "/// 指令：开始行动", 2);
                                if (PictureProcess.ColorCheck(1241, 449, "#313131", 859, 646, "#313131"))
                                {
                                    //没有理智
                                    Akhcmd("shell input tap 871 651", wait: 1);
                                    goto emptysan;//走了走了
                                }
                                Akhcmd("shell input tap 1281 740", "/// 指令：确认使用", 2);
                                Thread.Sleep(10000);
                            }
                            else
                            {
                                //不用剿灭卡
                                if (PictureProcess.ColorCheck(1200, 670, "#FFFFFF", 1200, 671, "#FFFFFF")) { }
                                else
                                {
                                    Akhcmd("shell input tap 1200 680", "/// 指令：激活代理指挥", 1);
                                }
                                Akhcmd("shell input tap 1295 745", "/// 指令：开始行动", 2);
                                if (PictureProcess.ColorCheck(1241, 449, "#313131", 859, 646, "#313131"))
                                {
                                    //没有理智
                                    Akhcmd("shell input tap 871 651", wait: 1);
                                    goto emptysan;//走了走了
                                }
                                Akhcmd("shell input tap 1240 559", "/// 指令：开始行动", 720);
                                for (; ; )
                                {
                                    Thread.Sleep(4000);
                                    //检查是否在本里
                                    if (PictureProcess.ColorCheck(77, 70, "#8C8C8C", 1341, 62, "#FFFFFF") == false)
                                    {
                                        Thread.Sleep(4500);
                                        break;
                                    }
                                }
                                Akhcmd("shell input tap 1390 140", "/// 指令：", 4);
                            }
                            Akhcmd("shell input tap 1204 290", "/// 指令：", 1);
                            Akhcmd("shell input tap 1204 290", "/// 指令：", 4);
                        }
                    }
                    Akhcmd("shell input tap 299 46", "/// 指令：菜单", 1);
                    Akhcmd("shell input tap 103 305", "/// 指令：首页", 3);
                }

                //准备作战 //fu关卡
                void TouchCp()
                {
                    using (var _screenshot = new ADB.Screenshot())
                    {
                        var _point = _screenshot.PicToPoint(Address.res + "\\pic\\battle\\" + "threeStarCp" + ".png");
                        if (_point.Count != 0)
                        {
                            System.Drawing.Point point1 = new System.Drawing.Point(0, 10000);
                            foreach (var point in _point)
                            {
                                if (point.Y < point1.Y) point1 = point;
                            }

                            ADB.Tap(point1);
                        }
                    }
                }
                void GetCustomCp(string _unit)
                {
                    var akhcpiaddress = _unit.Replace("custom:", "");
                    foreach (var akhcmd in akhcpiMaker.ReadFromAKHcpi(akhcpiaddress))
                    {
                        Akhcmd(akhcmd);
                    }
                }
                //first
                if (!App.Data.scht.first.unit.Contains("custom"))
                {
                    AKHcmd.FormatAKHcmd["zhongduan"].RunCmd();
                    AKHcmd.FormatAKHcmd["ziyuanshouji"].RunCmd();

                    if (App.Data.scht.first.unit == "LS")
                    {
                        ADB.Tap(717, 374);
                        goto NativeUnitInited;
                    }
                    else
                    {
                        if (App.Data.scht.first.unit.Contains("-"))
                        {
                            //左滑找PR
                            ADB.Swipe(1404, 591, 0, 591);
                        }
                        else
                        {
                            //右滑找本
                            ADB.Swipe(0, 591, 1404, 591);
                        }
                        using (var _screenshot = new ADB.Screenshot())
                        {
                            var _point = _screenshot.PicToPoint(Address.res + "\\pic\\battle\\" + App.Data.scht.first.unit + ".png");
                            if (_point.Count != 0)
                            {
                                var allowEnter = true;
                                using (var _screenshot2 = new ADB.Screenshot())
                                {
                                    var point2 = _screenshot2.PicToPoint(Address.res + "\\pic\\battle\\cannotEnter.png");
                                    foreach (var point in point2)
                                    {
                                        if (Math.Abs(point.X - _point[0].X) < 100)
                                        {
                                            allowEnter = false;
                                        }
                                    }
                                }
                                if (allowEnter)
                                {
                                    ADB.Tap(_point[0]);
                                    goto NativeUnitInited;
                                }
                                    
                            }
                        }
                    }
                }
                else
                {
                    GetCustomCp(App.Data.scht.first.unit);
                    goto UnitInited;
                }
                //second
                if (!App.Data.scht.second.unit.Contains("custom"))
                {
                    AKHcmd.FormatAKHcmd["zhongduan_menu_zhongduan"].RunCmd();
                    AKHcmd.FormatAKHcmd["ziyuanshouji"].RunCmd();
                    if (App.Data.scht.second.unit == "LS")
                    {
                        ADB.Tap(717, 374);
                        goto NativeUnitInited;
                    }
                }
                else
                {
                    AKHcmd.FormatAKHcmd["menu"].RunCmd();
                    AKHcmd.FormatAKHcmd["menu_home"].RunCmd();
                    GetCustomCp(App.Data.scht.second.unit);
                    goto UnitInited;
                }
            NativeUnitInited:;
                TouchCp();
            UnitInited:;

                if (PictureProcess.ColorCheck(1200, scht_mb_adjust_check_daili_xy_y1, "#FFFFFF", 1196, scht_mb_adjust_check_daili_xy_y2, "#FFFFFF")) { }
                else //检测代理指挥是否已经勾选，否则勾选
                {
                    Akhcmd(@"shell input tap 1200 680", "/// 指令：激活代理指挥"); //激活代理指挥
                }
                for (; ; )
                {
                    if (App.Data.scht.fcm.status && Convert.ToInt32(DateTime.Now.Minute) > 56) { break; }
                    Akhcmd(@"shell input tap 1266 753", "/// 指令：开始行动", 1);//开始作战

                    if (PictureProcess.ColorCheck(1241, 449, "#313131", 859, 646, "#313131"))
                    {
                        Akhcmd("shell input tap 871 651"); //点叉
                        goto emptysan;
                    }

                    Akhcmd("shell input tap 1240 559", "/// 指令：开始行动", 30);//开始行动

                    for (; ; )
                    {
                        Thread.Sleep(3000);
                        //检查是否在本里
                        if (PictureProcess.ColorCheck(77, 70, "#8C8C8C", 1341, 62, "#FFFFFF") == false)
                        {
                            Thread.Sleep(4500);
                            break;
                        }
                    }

                    GetScreenshot(Address.Screenshot.MB, ArkHelperDataStandard.Screenshot);

                    for (int i = 1; i <= 2; i++)
                    {
                        Akhcmd("shell input tap 1204 290", "/// 指令：", 1); //点击空白
                    }
                    Thread.Sleep(1500);
                }
            emptysan:
                Akhcmd("shell input tap 299 46", "/// 指令：菜单", 1);
                Akhcmd("shell input tap 103 305", "/// 指令：首页", 3);
                Akhcmd("shell input tap 908 676", "/// 指令：任务", 2);
                Akhcmd("shell input tap 767 41", "/// 指令：日常任务", 3);
                Akhcmd("shell input tap 1246 168", "/// 指令：收集", 2, 3);
                if (week == DayOfWeek.Sunday)
                {
                    Akhcmd("shell input tap 1051 51", "/// 指令：周常任务", 3);
                    Akhcmd("shell input tap 1246 168", "/// 指令：收集", 2, 3);
                }
                Akhcmd("shell input tap 109 51", "/// 指令：返回", 2);
                GetScreenshot(Address.Screenshot.SCHT, ArkHelperDataStandard.Screenshot);
                WithSystem.KillSimulator();
                Info("/// 正在关闭模拟器...");
                Info("/// 系统任务运行完毕。正在终止...");
                new ToastContentBuilder().AddArgument("kind", "SCHT").AddText("提示：定时事项处理指挥器任务已结束").AddText("开始时间：" + starttime + "\n" + "结束时间：" + DateTime.Now.ToString("g") + "\n" + "即将关闭运行终端...").Show(); //结束通知
                Thread.Sleep(2000);
                Application.Current.Dispatcher.Invoke(() => App.ExitApp());
            });
        }
    }
}
