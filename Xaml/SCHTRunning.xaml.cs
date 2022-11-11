using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Drawing;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;
using Microsoft.Toolkit.Uwp.Notifications;
using static ArkHelper.ADB;
using static ArkHelper.Data.scht;

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

        void Akhcmd(string body, string show = "null", int wait = 0, int repeat = 1)
        {
            var akhcmd = new ArkHelperDataStandard.AKHcmd(body,show,wait,repeat);
            Info(akhcmd.OutputText);
            akhcmd.RunCmd();
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

        void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //开始工作
            Task SCHT = Task.Run(() =>
            {
                //时间
                string starttime = DateTime.Now.ToString("g");
                long starttime_sec = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();

                //游戏
                string packname = ArkHelper.PinnedData.Server.dataSheet.Select("id = '" + server.id + "'")[0][3].ToString();

                //周期
                string week = DateTime.Now.DayOfWeek.ToString();
                bool AMmode;
                if (4 < DateTime.Now.Hour && 16 > DateTime.Now.Hour) { AMmode = true; } else { AMmode = false; }
                //外服适配
                //EN时差-1（天数），早晚反转
                if (server.id == "EN") { DateTime.Now.AddDays(-1).DayOfWeek.ToString(); AMmode = !AMmode; }

                //代理指挥位置调整
                //适用场景 外服开始作战块跟进下沉之前
                int scht_mb_adjust_check_daili_xy_y1 = 680;
                int scht_mb_adjust_check_daili_xy_y2 = 684;
                if (server.id == "CO" || server.id == "CB") { } else { scht_mb_adjust_check_daili_xy_y1 = 669; scht_mb_adjust_check_daili_xy_y2 = 665; }

                //adcmd计算
                string abx = "null";
                string unit = "null";
                string cp = "null";
                string name = "null";
                string adcmd1; string adcmd2; string adcmd3; string adcmd4; string adcmd5; string adcmd6; string adcmd7; string adcmd8; string adcmd9; string adcmd10;
                if (first.unit == "LS") { abx = "L0"; }
                if (first.unit == "custom") { abx = "custom"; }
                if (week == "Monday") { if (first.unit == "AP") { abx = "L2"; } if (first.unit == "SK") { abx = "L1"; } if (first.unit == "PR-A") { abx = "R1"; } if (first.unit == "PR-B") { abx = "R2"; } }
                if (week == "Tuesday") { if (first.unit == "CE") { abx = "L1"; } if (first.unit == "CA") { abx = "L2"; } if (first.unit == "PR-B") { abx = "R1"; } if (first.unit == "PR-D") { abx = "R2"; } }
                if (week == "Wednesday") { if (first.unit == "SK") { abx = "L1"; } if (first.unit == "CA") { abx = "L2"; } if (first.unit == "PR-C") { abx = "R1"; } if (first.unit == "PR-D") { abx = "R2"; } }
                if (week == "Thursday") { if (first.unit == "CE") { abx = "L1"; } if (first.unit == "AP") { abx = "L2"; } if (first.unit == "PR-A") { abx = "R1"; } if (first.unit == "PR-C") { abx = "R2"; } }
                if (week == "Friday") { if (first.unit == "SK") { abx = "L1"; } if (first.unit == "CA") { abx = "L2"; } if (first.unit == "PR-A") { abx = "R1"; } if (first.unit == "PR-B") { abx = "R2"; } }
                if (week == "Saturday") { if (first.unit == "CE") { abx = "L1"; } if (first.unit == "AP") { abx = "L3"; } if (first.unit == "SK") { abx = "L2"; } if (first.unit == "PR-B") { abx = "R1"; } if (first.unit == "PR-C") { abx = "R2"; } if (first.unit == "PR-D") { abx = "R3"; } }
                if (week == "Sunday") { if (first.unit == "CE") { abx = "L1"; } if (first.unit == "AP") { abx = "L3"; } if (first.unit == "CA") { abx = "L2"; } if (first.unit == "PR-A") { abx = "R1"; } if (first.unit == "PR-C") { abx = "R2"; } if (first.unit == "PR-D") { abx = "R3"; } }
                if (abx == "null")
                {
                    unit = second.unit;
                    cp = second.cp;
                    if (second.unit == "LS") { abx = "L0"; }
                }
                else
                {
                    unit = first.unit;
                    cp = first.cp;
                }
                if (unit == "custom")
                {
                    StreamReader ctreader = File.OpenText(cp);
                    JsonTextReader ctjsonTextReader = new JsonTextReader(ctreader);
                    JObject ctjsonObject = (JObject)JToken.ReadFrom(ctjsonTextReader);
                    adcmd1 = ctjsonObject["adcmd1"].ToString(); adcmd2 = ctjsonObject["adcmd2"].ToString(); adcmd3 = ctjsonObject["adcmd3"].ToString(); adcmd4 = ctjsonObject["adcmd4"].ToString(); adcmd5 = ctjsonObject["adcmd5"].ToString(); adcmd6 = ctjsonObject["adcmd6"].ToString(); adcmd7 = ctjsonObject["adcmd7"].ToString(); adcmd8 = ctjsonObject["adcmd8"].ToString(); adcmd9 = ctjsonObject["adcmd9"].ToString(); adcmd10 = ctjsonObject["adcmd10"].ToString();
                    //timeout1 = Convert.ToInt32(jsonObject["timeout1"].ToString()); timeout2 = Convert.ToInt32(jsonObject["timeout2"].ToString()); timeout3 = Convert.ToInt32(jsonObject["timeout3"].ToString()); timeout4 = Convert.ToInt32(jsonObject["timeout4"].ToString()); timeout5 = Convert.ToInt32(jsonObject["timeout5"].ToString()); timeout6 = Convert.ToInt32(jsonObject["timeout6"].ToString()); timeout7 = Convert.ToInt32(jsonObject["timeout7"].ToString()); timeout8 = Convert.ToInt32(jsonObject["timeout8"].ToString()); timeout9 = Convert.ToInt32(jsonObject["timeout9"].ToString()); timeout10 = Convert.ToInt32(jsonObject["timeout10"].ToString());
                    name = ctjsonObject["name"].ToString();
                    ctreader.Close();
                }
                else
                {
                    name = unit + "-" + cp;
                    adcmd1 = "zhongduan";
                    adcmd2 = "ziyuanshouji";
                    adcmd3 = "null";
                    if (abx == "L0") { adcmd3 = "721 376"; }
                    if (abx == "L1") { adcmd3 = "489 376"; }
                    if (abx == "L2") { adcmd3 = "253 376"; }
                    if (abx == "L3") { adcmd3 = "48 376"; }
                    if (abx == "R1") { adcmd3 = "965 376"; }
                    if (abx == "R2") { adcmd3 = "1183 376"; }
                    if (abx == "R3") { adcmd3 = "1414 376"; }
                    adcmd3 = "shell input tap " + adcmd3 + "####2#;";
                    adcmd4 = "null";
                    if (unit == "PR-A" || unit == "PR-B" || unit == "PR-C" || unit == "PR-D")
                    {
                        if (cp == "1") { adcmd4 = "487 499"; }
                        else { adcmd4 = "932 293"; }
                    }
                    else
                    {
                        if (unit == "LS" || unit == "CE")
                        {
                            if (cp == "1") { adcmd4 = "249 645"; }
                            if (cp == "2") { adcmd4 = "545 618"; }
                            if (cp == "3") { adcmd4 = "785 547"; }
                            if (cp == "4") { adcmd4 = "980 453"; }
                            if (cp == "5") { adcmd4 = "1105 326"; }
                            if (cp == "6") { adcmd4 = "1166 191"; }
                        }
                        else
                        {
                            if (cp == "1") { adcmd4 = "219 641"; }
                            if (cp == "2") { adcmd4 = "538 577"; }
                            if (cp == "3") { adcmd4 = "764 462"; }
                            if (cp == "4") { adcmd4 = "951 330"; }
                            if (cp == "5") { adcmd4 = "1054 203"; }
                        }
                    }
                    adcmd4 = "shell input tap " + adcmd4 + "####2#;";
                    adcmd5 = "null";
                    adcmd6 = "null";
                    adcmd7 = "null";
                    adcmd8 = "null";
                    adcmd9 = "null";
                    adcmd10 = "null";
                }
                if (adcmd1 == "zhongduan") { adcmd1 = "shell input tap 1090 185 ####2#;"; }
                if (adcmd1 == "huodongfengmian") { adcmd1 = "shell input tap 1320 158 ####4#;"; }
                if (adcmd2 == "ziyuanshouji") { adcmd2 = "shell input tap 806 756####1#;"; }
                if (adcmd2 == "zhutiqu") { adcmd2 = "shell input tap 266 756####1#;"; }
                if (adcmd2 == "chaqu") { adcmd2 = "shell input tap 447 756####1#;"; }
                if (adcmd2 == "biezhuan") { adcmd2 = "shell input tap 627 756####1#;"; }
                if (adcmd3 == "act0") { adcmd3 = "shell input tap 72 151####2#;"; }
                if (adcmd3 == "act1") { adcmd3 = "shell input tap 72 151####2#; $$$$2$;"; }
                if (adcmd4 == "jinruhuodong") { adcmd3 = "shell input tap 1304 589####2#;;"; }
                if (adcmd4 == "actno1") { adcmd4 = "shell input tap 162 461####2#;"; }
                if (adcmd4 == "actno2") { adcmd4 = "shell input tap 369 756####2#;"; }
                if (adcmd4 == "actno3") { adcmd4 = "shell input tap 699 756####2#;"; }
                if (adcmd4 == "actno4") { adcmd4 = "shell input tap 1074 756####2#;"; }
                //剿灭次数
                int anntime = 0;
                if (DateTime.Now.DayOfWeek == DayOfWeek.Monday) { anntime = ann.time.Mon; }
                if (DateTime.Now.DayOfWeek == DayOfWeek.Tuesday) { anntime = ann.time.Tue; }
                if (DateTime.Now.DayOfWeek == DayOfWeek.Wednesday) { anntime = ann.time.Wed; }
                if (DateTime.Now.DayOfWeek == DayOfWeek.Thursday) { anntime = ann.time.Thu; }
                if (DateTime.Now.DayOfWeek == DayOfWeek.Friday) { anntime = ann.time.Fri; }
                if (DateTime.Now.DayOfWeek == DayOfWeek.Saturday) { anntime = ann.time.Sat; }
                if (DateTime.Now.DayOfWeek == DayOfWeek.Sunday) { anntime = ann.time.Sun; }

                //输出到log
                Info("name=" + name + "," + "ann.status=" + ann.status + "," + "ann.select=" + ann.select + "," + "anntime=" + anntime, false);

                //模拟器未启动则启动它
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
                    if (DateTime.Now.Hour >= 20 || !fcm.status) { break; }
                    Thread.Sleep(2000);
                }

                Akhcmd("shell input tap 934 220", "/// 指令：START", 9);
                if (server.id != "CB") { Akhcmd("shell input tap 721 574", "/// 指令：开始唤醒", 15); }
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
                    void _Buy(int x, int y)
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

                    _Buy(146, 309);
                    _Buy(426, 290);
                    _Buy(716, 298);
                    _Buy(1010, 300);
                    _Buy(1290, 305);

                    _Buy(160, 592);
                    _Buy(443, 578);
                    _Buy(733, 608);
                    _Buy(997, 592);
                    _Buy(1290, 583);

                    Akhcmd("shell input tap 299 46", "/// 指令：菜单", 1);
                    Akhcmd("shell input tap 103 305", "/// 指令：首页", 3);
                }

                //剿灭
                if (ann.status)
                {
                    Akhcmd("shell input tap 921 214", "/// 指令：终端", 2);
                    Akhcmd("shell input tap 986 754", "/// 指令：常态事务", 2);
                    Akhcmd("shell input tap 706 517", "/// 指令：当期委托", 3);

                    if (ann.select != "TT")
                    {
                        Akhcmd("shell input tap 109 51", "/// 指令：返回", 2);
                        Akhcmd("shell input tap 1270 763", "/// 指令：切换", 3);
                        if (ann.select == "CHNB") { Akhcmd("shell input tap 1009 529", "/// 指令：切尔诺伯格", 3); }
                        if (ann.select == "LMOB") { Akhcmd("shell input tap 1044 295", "/// 指令：龙门外环", 3); }
                        if (ann.select == "LMDT") { Akhcmd("shell input tap 1086 417", "/// 指令：龙门市区", 3); }
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

                //打仗
                Akhcmd(adcmd1);
                Akhcmd(adcmd2);
                Akhcmd(adcmd3);
                Akhcmd(adcmd4);
                Akhcmd(adcmd5);
                Akhcmd(adcmd6);
                Akhcmd(adcmd7);
                Akhcmd(adcmd8);
                Akhcmd(adcmd9);
                Akhcmd(adcmd10);

                if (PictureProcess.ColorCheck(1200, scht_mb_adjust_check_daili_xy_y1, "#FFFFFF", 1196, scht_mb_adjust_check_daili_xy_y2, "#FFFFFF")) { }
                else //检测代理指挥是否已经勾选，否则勾选
                {
                    Akhcmd(@"shell input tap 1200 680", "/// 指令：激活代理指挥"); //激活代理指挥
                }
                for (; ; )
                {
                    if (fcm.status && Convert.ToInt32(DateTime.Now.Minute) > 56) { break; }
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
                if (week == "Sunday")
                {
                    Akhcmd("shell input tap 1051 51", "/// 指令：周常任务", 3);
                    Akhcmd("shell input tap 1246 168", "/// 指令：收集", 2, 3);
                }
                Akhcmd("shell input tap 109 51", "/// 指令：返回", 2);
                GetScreenshot(Address.Screenshot.SCHT, ArkHelperDataStandard.Screenshot);
                WithSystem.Cmd(@"start " + Address.cmd + @" /k ""taskkill /f /t /im " + ConnectedInfo.IM + @" & exit""");
                Info("/// 正在关闭模拟器...");
                Info("/// 系统任务运行完毕。正在终止...");
                new ToastContentBuilder().AddArgument("kind","SCHT").AddText("提示：定时事项处理指挥器任务已结束").AddText("开始时间：" + starttime + "\n" + "结束时间：" + DateTime.Now.ToString("g") + "\n" + "即将关闭运行终端...").Show(); //结束通知
                Thread.Sleep(2000);
                Application.Current.Dispatcher.Invoke(() => App.ExitApp());
            });
        }
    }
}
