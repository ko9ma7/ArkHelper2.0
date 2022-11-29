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
using static ArkHelper.ArkHelperDataStandard;

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
            Info("指令：" + akhcmd.OutputText);
            akhcmd.RunCmd();
        }
        void Akhcmd(string body, string show = "", int wait = 0, int repeat = 1)
        {
            var akhcmd = new AKHcmd(body, show, wait, repeat);
            Akhcmd(akhcmd);
        }

        //表达 + Log
        void Info(string content, Output.InfoKind infokind = Output.InfoKind.Infomational)
        {
            Output.Log(content, "SCHT", infokind);

            if (true)
            {
                content = "/// " + content;
                //表达
                string forcolor = "#00FFFFFF";
                string bakcolor = "#00FFFFFF";
                if (infokind == Output.InfoKind.Infomational)
                {
                    forcolor = "#003472";
                }
                if (infokind == Output.InfoKind.Warning)
                {
                    forcolor = "#000000"; bakcolor = "#fff143";
                }
                if (infokind == Output.InfoKind.Error)
                {
                    forcolor = "#f20c00";
                }
                if (infokind == Output.InfoKind.Emergency)
                {
                    forcolor = "#FFFFFF"; bakcolor = "#ff3300";
                }
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
            MB.Info += Info;
            //开始工作
            Task SCHT = Task.Run(() =>
            {
                void main(Data.SCHT schtData)
                {
                    //游戏
                    string packname = ADB.GetGamePackageName(schtData.server.id);

                    //周期
                    bool AMmode;
                    if (4 < DateTime.Now.Hour && 16 > DateTime.Now.Hour) { AMmode = true; } else { AMmode = false; }
                    //外服适配
                    //EN时差-1（天数），早晚反转
                    if (schtData.server.id == "EN") { DateTime.Now.AddDays(-1).DayOfWeek.ToString(); AMmode = !AMmode; }

                    //adcmd计算
                    var week = DateTime.Now.DayOfWeek;

                    //剿灭次数
                    int anntime = 0;

                    anntime = 1; //normal
                    if (schtData.fcm.status)
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
                    if (schtData.ann.customTime)
                    {
                        var _wek = (int)week - 1;
                        if (_wek < 0) { _wek += 7; }
                        anntime = schtData.ann.time[_wek];
                    }//custom

                    //模拟器未启动则启动
                    if (ADB.ConnectedInfo == null)
                    {
                        Info("正在启动神经网络依托平台...");
                        Process.Start(Address.dataExternal + @"\simulator.lnk");
                        Thread.Sleep(50000);
                    }
                    while (ADB.ConnectedInfo == null)
                    {
                        Thread.Sleep(4000);
                    }
                    void StartGame()
                    {
                        Akhcmd("shell am start -n " + packname + "/com.u8.sdk.U8UnityContext", "启动" + packname, 7);
                        while (!PictureProcess.ColorCheck(719, 759, "#FFD802", 720, 759, "#FFD802")
                        || (DateTime.Now.Hour < 20 && schtData.fcm.status)
                        )
                            Thread.Sleep(3000);

                        Akhcmd("shell input tap 934 220", "START", 12);
                        if (schtData.server.id != "CB")
                        {
                            Akhcmd("shell input tap 721 574", "开始唤醒", 0);
                        }
                        Thread.Sleep(25000);
                    }

                    StartGame();

                    for (; ; )
                    {
                        using (Screenshot sc = new Screenshot())
                        {
                            var itemPosition = sc.PicToPoint(Address.res + @"pic\UI\signItems.png");
                            if (itemPosition.Count != 0)
                            {
                                Akhcmd("shell input tap 722 719", "收取", 3);
                                continue;
                            }

                            var closePosition = sc.PicToPoint(Address.res + @"\pic\UI\close.png");
                            if (closePosition.Count != 0)
                            {
                                Tap(closePosition[0]);
                                Info("指令：关闭窗口");
                                Thread.Sleep(2000);
                            }
                            else { break; }
                        }
                    }

                    for (; ; )
                    {
                        using (ADB.Screenshot sc = new Screenshot())
                        {
                            if (sc.PicToPoint(Address.res + @"\pic\UI\shopCenter.png").Count != 0)
                            {
                                break;
                            }
                        }
                        Info("遇到无法关闭的窗口，正在尝试重启游戏解决...", Output.InfoKind.Warning);
                        Akhcmd("shell am force-stop " + packname, "强制结束" + packname, 1);
                        StartGame();
                    }

                    //邮件
                    Akhcmd("shell input tap 217 48", "打开邮件列表", 3);
                    Akhcmd("shell input tap 1294 748", "一键收取", 3);
                    Akhcmd("shell input tap 715 54", "收取", 2);
                    Akhcmd("shell input tap 715 54", "", 2);
                    Akhcmd("shell input tap 109 51", "返回", 2);
                    //基建
                    Akhcmd("shell input tap 1154 697", "基建", 1);
                    Akhcmd("shell input tap 349 555", "确认", 7);
                    using (ADB.Screenshot sc = new Screenshot())
                    {
                        var notiPosition = sc.PicToPoint(Address.res + @"\pic\UI\notification.png");
                        if (notiPosition.Count != 0)
                        {
                            Tap(notiPosition[0]);
                            Info("指令：Notification");
                            Thread.Sleep(2000);
                            for (int i = 0; i < 3; i++)
                            {
                                Akhcmd("shell input tap 211 771", "待办事项", 2);
                            }
                            Akhcmd("shell input tap 473 214", "", 1);
                        }
                    }

                    using (ADB.Screenshot sc = new Screenshot())
                    {
                        var mfPosition = sc.PicToPoint(Address.res + @"\pic\UI\manufacturStationInAllView" + schtData.server.id + ".png");
                        if (mfPosition.Count != 0)
                        {
                            Tap(mfPosition[0]);
                            Info("指令：制造站");
                            Thread.Sleep(2000);
                            Akhcmd("shell input tap 327 691", "制造计划", 2);
                            Akhcmd("shell input tap 1371 603", "加速", 2);
                            Akhcmd("shell input tap 1081 377", "最多", 1);
                            Akhcmd("shell input tap 1069 653", "确定", 2);
                            Akhcmd("shell input tap 1258 711", "收取", 2);
                            Akhcmd("shell input tap 89 50", "返回", 2);
                            Akhcmd("shell input tap 89 50", "返回", 2);
                        }
                    }
                    Akhcmd("shell input tap 89 50", "返回", 2);
                    Akhcmd("shell input tap 962 555", "确认", 5);
                    if (AMmode == true)
                    {
                        void Buy(int x, int y)
                        {
                            Akhcmd("shell input tap " + x + " " + y, "商品", 2);
                            Akhcmd("shell input tap 1042 655", "购买物品", 3);
                            Akhcmd("shell input tap 756 59", "收取", 2);
                        }
                        //线索交流
                        Akhcmd("shell input tap 407 646", "好友", 2);
                        Akhcmd("shell input tap 132 254", "好友列表", 2);
                        Akhcmd("shell input tap 1122 188", "访问基建", 5);
                        for (int i = 1; i <= 10; i++) { Akhcmd("shell input tap 1334 706", "访问下位", 5); }
                        Akhcmd("shell input tap 299 46", "菜单", 2);
                        Akhcmd("shell input tap 103 305", "首页", 1);
                        Akhcmd("shell input tap 962 555", "确认", 5);
                        //信用商店
                        Akhcmd("shell input tap 941 550", "采购中心", 3);
                        Akhcmd("shell input tap 1307 121", "信用交易所", 2);
                        Akhcmd("shell input tap 1146 48", "领取信用", 3);
                        Akhcmd("shell input tap 722 719", "收取", 2);
                        Akhcmd("shell input tap 746 51", "", 2);

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

                        Akhcmd("shell input tap 299 46", "菜单", 1);
                        Akhcmd("shell input tap 103 305", "首页", 3);
                    }

                    //剿灭
                    if (schtData.ann.status)
                    {
                        Akhcmd("shell input tap 921 214", "终端", 2);
                        Akhcmd("shell input tap 986 754", "常态事务", 2);
                        Akhcmd("shell input tap 706 517", "当期委托", 3);

                        if (schtData.ann.select != "TT")
                        {
                            Akhcmd("shell input tap 109 51", "返回", 2);
                            Akhcmd("shell input tap 1270 763", "切换", 3);
                            if (schtData.ann.select == "CHNB") { Akhcmd("shell input tap 1009 529", "切尔诺伯格", 3); }
                            if (schtData.ann.select == "LMOB") { Akhcmd("shell input tap 1044 295", "龙门外环", 3); }
                            if (schtData.ann.select == "LMDT") { Akhcmd("shell input tap 1086 417", "龙门市区", 3); }
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
                                    Akhcmd("shell input tap 1090 666", "激活全权委托", 1);
                                    if (PictureProcess.ColorCheck(1263, 765, "#232323", 1263, 764, "#232323")) { /*成功激活*/} else { a = 1; }
                                }

                                if (a == 0)
                                {
                                    //使用剿灭卡
                                    Akhcmd("shell input tap 1295 745", "开始行动", 2);
                                    if (PictureProcess.ColorCheck(1241, 449, "#313131", 859, 646, "#313131"))
                                    {
                                        //没有理智
                                        Akhcmd("shell input tap 871 651", wait: 1);
                                        goto emptysan;//走了走了
                                    }
                                    Akhcmd("shell input tap 1281 740", "确认使用", 2);
                                    Thread.Sleep(10000);
                                }
                                else
                                {
                                    //不用剿灭卡
                                    if (PictureProcess.ColorCheck(1200, 670, "#FFFFFF", 1200, 671, "#FFFFFF")) { }
                                    else
                                    {
                                        Akhcmd("shell input tap 1200 680", "激活代理指挥", 1);
                                    }
                                    Akhcmd("shell input tap 1295 745", "开始行动", 2);
                                    if (PictureProcess.ColorCheck(1241, 449, "#313131", 859, 646, "#313131"))
                                    {
                                        //没有理智
                                        Akhcmd("shell input tap 871 651", wait: 1);
                                        goto emptysan;//走了走了
                                    }
                                    Akhcmd("shell input tap 1240 559", "开始行动", 720);
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
                                    Akhcmd("shell input tap 1390 140", "", 4);
                                }
                                Akhcmd("shell input tap 1204 290", "", 1);
                                Akhcmd("shell input tap 1204 290", "", 4);
                            }
                        }
                        Akhcmd("shell input tap 299 46", "菜单", 1);
                        Akhcmd("shell input tap 103 305", "首页", 3);
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
                        Thread.Sleep(1000);
                    }
                    void GetCustomCp(string _unit)
                    {
                        var akhcpiaddress = _unit.Replace("custom:##", "").Replace("##", "");
                        foreach (var akhcmd in akhcpiMaker.ReadFromAKHcpi(akhcpiaddress))
                        {
                            Akhcmd(akhcmd);
                        }
                    }
                    //first
                    if (!schtData.first.unit.Contains("custom"))
                    {
                        AKHcmd.FormatAKHcmd["zhongduan"].RunCmd();
                        AKHcmd.FormatAKHcmd["ziyuanshouji"].RunCmd();

                        if (schtData.first.unit == "LS")
                        {
                            ADB.Tap(717, 374);
                            goto NativeUnitInited;
                        }
                        else
                        {
                            if (schtData.first.unit.Contains("-"))
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
                                var _point = _screenshot.PicToPoint(Address.res + "\\pic\\battle\\" + schtData.first.unit + ".png", opencv_errorCon: 0.9);
                                if (_point.Count != 0)
                                {
                                    ADB.Tap(_point[0]);
                                    goto NativeUnitInited;
                                }
                            }
                        }
                    }
                    else
                    {
                        GetCustomCp(schtData.first.unit);
                        goto UnitInited;
                    }
                    //second
                    if (!schtData.second.unit.Contains("custom"))
                    {
                        AKHcmd.FormatAKHcmd["zhongduan_menu_zhongduan"].RunCmd();
                        AKHcmd.FormatAKHcmd["ziyuanshouji"].RunCmd();
                        if (schtData.second.unit == "LS")
                        {
                            ADB.Tap(717, 374);
                            goto NativeUnitInited;
                        }
                    }
                    else
                    {
                        AKHcmd.FormatAKHcmd["menu"].RunCmd();
                        AKHcmd.FormatAKHcmd["menu_home"].RunCmd();
                        GetCustomCp(schtData.second.unit);
                        goto UnitInited;
                    }
                NativeUnitInited:;
                    Thread.Sleep(2000);
                    TouchCp();
                UnitInited:;
                    MB.MBCore(mode: MB.Mode.san);
                emptysan:;
                    Akhcmd("shell input tap 299 46", "菜单", 1);
                    Akhcmd("shell input tap 103 305", "首页", 3);
                    Akhcmd("shell input tap 908 676", "任务", 2);
                    Akhcmd("shell input tap 767 41", "日常任务", 3);
                    Akhcmd("shell input tap 1246 168", "收集", 2, 3);
                    if (week == DayOfWeek.Sunday || schtData.fcm.status)
                    {
                        Akhcmd("shell input tap 1051 51", "周常任务", 3);
                        Akhcmd("shell input tap 1246 168", "收集", 2, 3);
                    }
                    Akhcmd("shell input tap 109 51", "返回", 2);
                    GetScreenshot(Address.Screenshot.SCHT, ArkHelperDataStandard.Screenshot);
                    Akhcmd("shell am force-stop " + packname, "关闭" + packname, 1);
                }

                //开始
                var starttime = DateTime.Now;//时间
                //Thread.Sleep(300000);

                main(App.Data.scht);
                if (File.Exists(Address.dataExternal + "\\moreSCHT.json"))
                {
                    var schts = JsonSerializer.Deserialize<JsonElement>(File.ReadAllText(Address.dataExternal + "\\moreSCHT.json"));
                    foreach (var scht in schts.EnumerateArray())
                    {
                        var schtdt = JsonSerializer.Deserialize<Data.SCHT>(scht.ToString());
                        main(schtdt);
                    }
                }

                //结束
                WithSystem.KillSimulator();
                Info("正在关闭模拟器神经网络依托平台...");
                Info("系统任务运行完毕");
                new ToastContentBuilder()
                .AddArgument("kind", "SCHT")
                .AddText("提示：定时事项处理指挥器任务已结束")
                .AddText("开始时间：" + starttime.ToString("g") + "\n" + "结束时间：" + DateTime.Now.ToString("g"))
                .Show(); //结束通知
                App.OKtoOpenSCHT = true;
                Application.Current.Dispatcher.Invoke(() => (Window.GetWindow(this)).Close());
            });
        }
    }
}