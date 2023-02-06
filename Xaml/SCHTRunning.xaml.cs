﻿using System;
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
using RestSharp;
using System.Security.Policy;
using System.Net;
using Windows.ApplicationModel;
using Windows.Media.Playback;
using Windows.Networking.Vpn;
using Windows.ApplicationModel.Contacts.DataProvider;
using System.Reflection;
using System.Linq;
using System.Drawing;
using Point = System.Drawing.Point;
using ArkHelper.Modules.MB;
using System.Security.Claims;

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
            //开始工作
            Thread SCHT = new Thread(() =>
            {
                var starttime = DateTime.Now;//开始时间

                StatusChanged?.Invoke(this, true);

                //模拟器未启动则启动
                if (Modules.Connect.ConnectionInfo.Device == null)
                {
                    Info("正在启动神经网络依托平台...");
                    Process.Start(Address.dataExternal + @"\simulator.lnk");
                }
                WaitingSimulator();

                ADB.RegisterADBUsing("SCHTRunning");

                MainTask(App.Data.scht.data);
                foreach (var scht in App.Data.beta.ms) MainTask(scht);
                End();
                UIEnd();


                void MainTask(Data.SCHT.SCHTData schtData)
                {
                    string packname;//游戏包名
                    bool AMmode;//是否是上午
                    DayOfWeek week;//周期
                    int anntime;//剿灭次数

                    void InitData()
                    {
                        packname = ADB.GetGamePackageName(schtData.server.id);

                        week = DateTime.Now.DayOfWeek;

                        if (4 < DateTime.Now.Hour && 16 > DateTime.Now.Hour) { AMmode = true; } else { AMmode = false; }
                        if (schtData.server.id == "EN") { DateTime.Now.AddDays(-1).DayOfWeek.ToString(); AMmode = !AMmode; }

                        anntime = 1; //normal
                        if (schtData.ann.customTime)
                        {
                            anntime = schtData.ann.time[GetWeekSubInChinese(week)];
                        }//custom
                    }
                    void GetUpdate()
                    {
                        if (schtData.server.id == "CO")
                        {
                            //获取最新版本链接
                            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://ak.hypergryph.com/downloads/android_lastest");
                            req.AllowAutoRedirect = false;
                            WebResponse response = req.GetResponse();
                            string NewestLink = response.Headers["Location"];
                            //获取游戏版本号
                            string _infoText = ADB.CMD(@"shell ""pm dump com.hypergryph.arknights | grep versionName""");
                            string verNow = _infoText.Substring(_infoText.IndexOf("=") + 1, _infoText.IndexOf("\r") - _infoText.IndexOf("=") - "=".Length).Replace(".", "");
                            //检查版本是否匹配
                            if (!NewestLink.Contains(verNow + ".apk"))
                            {
                                //不匹配就下载最新安装包并安装
                                Info("游戏当前不是最新版本");
                                Info("正在下载更新中...");
                                /*string add = Address.Cache.main + "\\newApk.apk";
                                Net.DownloadFile(NewestLink, add);*/
                                string addre = "/sdcard/new.apk";
                                ADB.DownloadFile(NewestLink.Replace("https", "http"), addre);
                                Info("正在安装更新中...");
                                ADB.InstallFromLocal(addre);//安装
                                ADB.DeleteFile(addre);
                                WithSystem.Wait(3000);
                            }
                        }
                    }
                    void StartGame()
                    {
                        Akhcmd("shell am force-stop " + packname, "关闭" + packname, 1);
                        Akhcmd("shell am start -n " + packname + "/com.u8.sdk.U8UnityContext", "启动" + packname, 7);//启动游戏
                        while
                        (!PictureProcess.ColorCheck(719, 759, "#FFD802", 720, 759, "#FFD802")//START图标未显示（可能是在更新或者未加载好）
                        )
                            WithSystem.Wait(3000);

                        Akhcmd("shell input tap 934 220", "START", 6);
                        //持续等待直到出现开始唤醒标志出现，点击开始唤醒
                        switch (schtData.server.id)
                        {
                            case "CO":
                            case "EN":
                            case "JP":
                                for (; ; )
                                {
                                    using (ADB.Screenshot sc = new ADB.Screenshot())
                                    {
                                        var point = sc.PicToPoint(Address.res + "\\pic\\UI\\loginButton" + schtData.server.id + ".png");
                                        if (point.Count != 0)
                                        {
                                            if (schtData.server.id == "CO" && schtData.server.login.status)
                                            {
                                                Akhcmd("shell input tap 1039 769", "账号管理", 2);
                                                Akhcmd("shell input tap 593 573", "账号登录", 3);
                                                Akhcmd("shell input tap 695 483", "账号输入", 2);
                                                Akhcmd("shell input text " + schtData.server.login.account, "账号：" + schtData.server.login.account, 2);
                                                Akhcmd("shell input tap 729 341", "", 2);
                                                Akhcmd("shell input tap 695 542", "密码输入", 2);
                                                string _ = ""; foreach (char __ in schtData.server.login.password) _ += "*";
                                                Akhcmd("shell input text " + schtData.server.login.password, "密码：" + _, 2);
                                                Akhcmd("shell input tap 729 341", "", 2);
                                                Akhcmd("shell input tap 718 654", "登录", 0);
                                            }
                                            else
                                            {
                                                Akhcmd("shell input tap 721 574", "开始唤醒", 0);
                                            }
                                            break;
                                        }
                                    }
                                    WithSystem.Wait(3000);
                                }
                                break;
                            case "CB":
                                WithSystem.Wait(6000);//B服自动登录，不用点击开始唤醒
                                break;
                            default:
                                WithSystem.Wait(6000);
                                Akhcmd("shell input tap 721 574", "开始唤醒", 0);
                                break;
                        }
                        WithSystem.Wait(3000);
                    }
                    void WaitGame()
                    {
                        for (int i = 0; ; i++)
                        {
                            WithSystem.Wait(2000);
                            using (Screenshot sc = new Screenshot())
                            {
                                var itemPosition = sc.PicToPoint(Address.res + @"\pic\UI\signItems.png", opencv_errorCon: 0.5);
                                if (itemPosition.Count != 0)
                                {
                                    Akhcmd("shell input tap 722 719", "收取");
                                    continue;
                                }

                                var closePosition = sc.PicToPoint(Address.res + @"\pic\UI\close.png");
                                if (closePosition.Count != 0)
                                {
                                    Tap(closePosition[0]);
                                    Info("指令：关闭窗口");
                                    continue;
                                }

                                var loginSucceedSymbolPosition = sc.PicToPoint(Address.res + @"\pic\UI\shopCenter.png");
                                if (loginSucceedSymbolPosition.Count != 0)
                                {
                                    WithSystem.Wait(3000);
                                    //有时候会先显示UI再显示公告框，再识别一次保证准确性
                                    using (Screenshot sc1 = new Screenshot())
                                    {
                                        if (sc1.PicToPoint(Address.res + @"\pic\UI\shopCenter.png").Count != 0)
                                        {
                                            break;//如果确实加载完毕，break
                                        }
                                        else
                                        {
                                            continue;//如果没有加载，continue
                                        }
                                    }
                                }
                            }
                            if (i > 12)
                            {
                                i = 0;
                                Info("遇到无法关闭的窗口，正在尝试重启游戏解决...", Output.InfoKind.Warning);
                                Akhcmd("shell am force-stop " + packname, "强制结束" + packname, 1);
                                StartGame();
                            }
                        }
                    }
                    void GetEmail()
                    {
                        Akhcmd("shell input tap 217 48", "打开邮件列表", 3);
                        Akhcmd("shell input tap 1294 748", "一键收取", 3);
                        Akhcmd("shell input tap 715 54", "收取", 2);
                        Akhcmd("shell input tap 715 54", "", 2);
                        Akhcmd("shell input tap 109 51", "返回", 2);
                    }
                    void GotoBase()
                    {
                        Akhcmd("shell input tap 1154 697", "基建", 1);
                        Akhcmd("shell input tap 349 555", "确认", 7);
                    }
                    void Base_OneClickGet()
                    {
                        using (ADB.Screenshot sc = new Screenshot())
                        {
                            var notiPosition = sc.PicToPoint(Address.res + @"\pic\UI\notification.png");
                            if (notiPosition.Count != 0)
                            {
                                Tap(notiPosition[0]);
                                Info("指令：Notification");
                                WithSystem.Wait(2000);
                                for (int i = 0; i < 3; i++)
                                {
                                    Akhcmd("shell input tap 211 771", "待办事项", 2);
                                }
                                Akhcmd("shell input tap 473 214", "", 1);
                            }
                        }
                    }
                    void Base_MenuFactory()
                    {
                        using (ADB.Screenshot sc = new Screenshot())
                        {
                            var mfPosition = sc.PicToPoint(Address.res + @"\pic\UI\manufacturStationInAllView" + schtData.server.id + ".png");
                            if (mfPosition.Count != 0)
                            {
                                Tap(mfPosition[0]);
                                Info("指令：制造站");
                                WithSystem.Wait(2000);
                                Akhcmd("shell input tap 327 691", "制造计划", 2);
                                if (schtData.control.usingUAVToSpeedUpProduction)
                                {
                                    Akhcmd("shell input tap 1371 603", "加速", 2);
                                    Akhcmd("shell input tap 1081 377", "最多", 1);
                                    Akhcmd("shell input tap 1069 653", "确定", 2);
                                    Akhcmd("shell input tap 1258 711", "收取", 2);
                                }
                                Akhcmd("shell input tap 89 50", "返回", 2);
                                Akhcmd("shell input tap 89 50", "返回", 2);
                            }
                        }
                    }
                    void Base_Clue()
                    {
                        if (schtData.control.clue && AMmode)
                        {
                            var cluePinnedInfo = new List<Tuple<int, int, int, int, int, int, int>>()
                        {
                            Tuple.Create(1,433,252,0,0,437,252),
                            Tuple.Create(2,671,332,0,0,658,335),
                            Tuple.Create(3,882,231,0,0,892,228),
                            Tuple.Create(4,1120,295,0,0,1140,291),
                            Tuple.Create(5,740,547,752,567,447,589),
                            Tuple.Create(6,1001,527,1007,524,0,0),
                            Tuple.Create(7,482,506,0,0,0,0)
                        };//线索号，CN,JP,EN
                            var isOnBoard = new bool[7]
                            {
                            true,true,true,true,true,true,true
                            };
                            Akhcmd("shell input tap 1339 225", "会客室", 2);
                            Akhcmd("shell input tap 345 684", "", 2);
                            using (ADB.Screenshot sc = new ADB.Screenshot())
                            {
                                if (sc.PicToPoint(Address.res + "\\pic\\UI\\creditPointInExchangeEndScreen.png").Count != 0)
                                    Akhcmd("shell input tap 89 50", "返回", 2);
                            }
                            //截图检查线索摆放情况
                            using (ADB.Screenshot sc = new ADB.Screenshot())
                            {
                                foreach (var clue in cluePinnedInfo)
                                {
                                    int clueNum = clue.Item1;
                                    /*if (sc.PicToPoint(Address.res + "\\pic\\UI\\clue" + clueNum + ".png",opencv_errorCon:0.1).Count != 0)
                                    {
                                        isOnBoard[clueNum - 1] = true;
                                    }*/
                                    int XForColorPick = clue.Item2;
                                    int YForColorPick = clue.Item3;
                                    switch (schtData.server.id)
                                    {
                                        case "JP":
                                            if (clue.Item4 != 0) XForColorPick = clue.Item4;
                                            if (clue.Item5 != 0) YForColorPick = clue.Item5;
                                            break;
                                        case "EN":
                                            if (clue.Item6 != 0) XForColorPick = clue.Item6;
                                            if (clue.Item7 != 0) YForColorPick = clue.Item7;
                                            break;
                                    }
                                    if (PictureProcess.ColorPick(sc.Location, XForColorPick, YForColorPick)[0] == "#FFFFFF")
                                    {
                                        isOnBoard[clueNum - 1] = false;
                                    }
                                }
                                var _newPosition = sc.PicToPoint(Address.res + "\\pic\\UI\\clueNew.png", 0.9);
                                var newPosition = new List<System.Drawing.Point>();
                                for (; ; )
                                {
                                    if (_newPosition.Count == 0) break;
                                    var examplePoint = _newPosition[0];
                                    _newPosition.RemoveAll(t => Math.Abs(t.Y - examplePoint.Y) < 50);
                                    newPosition.Add(examplePoint);
                                }

                                foreach (var @new in newPosition)
                                {
                                    Akhcmd("shell input tap " + (@new.X - 50) + " " + (@new.Y + 20), "", 2);
                                    bool getSelf = false;
                                    using (var sc1 = new ADB.Screenshot())
                                    {
                                        if (sc1.PicToPoint(Address.res + "\\pic\\UI\\close.png").Count != 0) getSelf = true;
                                    }
                                    if (getSelf)
                                    {
                                        Akhcmd("shell input tap 907 651", "领取线索", 5);
                                        Akhcmd("shell input tap 1108 112", "关闭", 2);
                                    }
                                    else
                                    {
                                        Akhcmd("shell input tap 1193 764", "全部收取", 2);
                                        Akhcmd("shell input tap 322 117", "", 2);
                                    }
                                }
                            }

                            foreach (var clue in cluePinnedInfo)
                            {
                                int clueNum = clue.Item1;
                                if (!isOnBoard[clueNum - 1])
                                {
                                    Akhcmd("shell input tap " + clue.Item2 + " " + clue.Item3, "线索" + clueNum, 3);
                                    Akhcmd("shell input tap 1160 264", "第一条线索", 2);
                                    Akhcmd("shell input tap 322 117", "", 2);
                                }
                            }

                            Akhcmd("shell input tap 1349 441", "传递线索", 4);
                            Akhcmd("shell input tap 231 251", "第一条线索", 1);
                            Akhcmd("shell input tap 1345 142", "传递线索", 3);
                            Akhcmd("shell input tap 1399 43", "关闭", 2);
                            Akhcmd("shell input tap 765 740", "线索交流", 3);
                            using (var sc = new ADB.Screenshot())
                            {
                                if (sc.PicToPoint(Address.res + "\\pic\\UI\\searchingClueSymbol.png").Count == 0)
                                {
                                    Akhcmd("shell input tap 89 50", "返回", 2);
                                }
                            }
                            Akhcmd("shell input tap 89 50", "返回", 2);
                        }
                    }
                    void Base_ChangeWork()
                    {
                        if (schtData.control.changeOperatorWork && AMmode)
                        {
                            Akhcmd("shell input tap 147 141", "进驻一览", 2);
                            for (int i = 0; ; i++)
                            {
                                if (i == 5) break;
                                Akhcmd("shell input tap 1237 48", "撤下干员", 2);
                                using (var sc = new ADB.Screenshot())
                                {
                                    var tkdPosition = sc.PicToPoint(Address.res + "\\pic\\UI\\takeDownOperator.png", opencv_errorCon: 0.9);
                                    tkdPosition.RemoveAll(t => t.Y < 100);
                                    foreach (var poi in tkdPosition)
                                    {
                                        ADB.Tap(poi); Info("指令：撤下干员"); WithSystem.Wait(1300);
                                        Akhcmd("shell input tap 1417 553", "确定", 2);
                                    }

                                    Akhcmd("shell input tap 1237 48", "取消撤下干员", 2);
                                    var sc1 = new ADB.Screenshot();
                                    var plusPosition = sc1.PicToPoint(Address.res + "\\pic\\UI\\takeDownPlus.png", opencv_errorCon: 0.85);
                                    sc1.Dispose();
                                    var roomInfo = new List<Tuple<int, System.Drawing.Point>>();//房间容纳数，第一个目标点
                                    for (; ; )
                                    {
                                        if (plusPosition.Count == 0) break;
                                        List<System.Drawing.Point> roomOperators = new List<System.Drawing.Point>();
                                        var examplePoint = plusPosition[0];
                                        var _ = plusPosition.FindAll(t => Math.Abs(t.Y - examplePoint.Y) < 150);
                                        roomOperators.AddRange((IEnumerable<System.Drawing.Point>)_);
                                        plusPosition.RemoveAll(t => Math.Abs(t.Y - examplePoint.Y) < 150);
                                        roomInfo.Add(Tuple.Create(roomOperators.Count, roomOperators[0]));
                                    }
                                    foreach (var room in roomInfo)
                                    {
                                        ADB.Tap(room.Item2); Info("指令：房间"); WithSystem.Wait(2000);

                                        var operatorNotToUseSkillIconPosition = new List<Point>();
                                        using (Screenshot screenshot = new Screenshot())
                                        {
                                            foreach (string operatorNotToUse in schtData.control.operatorsNotToUse)
                                            {
                                                var bs = screenshot.PicToPoint(Address.res + "\\pic\\BaseSkill\\" + operatorNotToUse + ".png", 0.95);
                                                if (bs.Count != 0)
                                                {
                                                    operatorNotToUseSkillIconPosition.AddRange(bs);
                                                }
                                            }
                                        }

                                        for (int index = 0, alreadySelected = 0; ;)
                                        {
                                            if (alreadySelected == room.Item1 || index > 10) break;

                                            index++;

                                            int X = 549 + ((index - 1) / 2) * 161;
                                            int Y = (index % 2 == 0) ? 555 : 234;

                                            if (operatorNotToUseSkillIconPosition.Exists
                                            (t => X - t.X < 80
                                            && X - t.X > 0
                                            && t.Y - Y < 100
                                            && t.Y - Y > 0))
                                            {
                                                continue;
                                            }
                                            else
                                            {
                                                alreadySelected++;
                                            }

                                            Akhcmd("shell input tap " + X + " " + Y, "干员" + alreadySelected, 1);
                                        }
                                        Akhcmd("shell input tap 1326 768", "确认", 2);
                                        using (var sc2 = new ADB.Screenshot())
                                        {
                                            if (sc2.PicToPoint(Address.res + "\\pic\\UI\\confirm.png").Count != 0)
                                            {
                                                Akhcmd("shell input tap 356 768", "取消", 2);
                                                Akhcmd("shell input tap 89 50", "返回", 2);
                                            }
                                        }
                                    }

                                    Akhcmd("shell input swipe 1350 " + tkdPosition.Max(t => t.Y) + " 1350 100 2000");
                                }
                            }
                            Akhcmd("shell input tap 89 50", "返回", 2);
                        }
                    }
                    void ExitBase()
                    {
                        Akhcmd("shell input tap 89 50", "返回", 2);
                        Akhcmd("shell input tap 962 555", "确认", 5);
                    }
                    void ClueChange()
                    {
                        if (!AMmode) return;
                        Akhcmd("shell input tap 407 646", "好友", 2);
                        Akhcmd("shell input tap 132 254", "好友列表", 2);
                        Akhcmd("shell input tap 1122 188", "访问基建", 5);
                        for (int i = 1; i <= 10; i++) { Akhcmd("shell input tap 1334 706", "访问下位", 5); }
                        Akhcmd("shell input tap 299 46", "菜单", 2);
                        Akhcmd("shell input tap 103 305", "首页", 1);
                        Akhcmd("shell input tap 962 555", "确认", 5);
                    }
                    void ClueShop()
                    {
                        if (!AMmode) return;
                        Akhcmd("shell input tap 941 550", "采购中心", 3);
                        Akhcmd("shell input tap 1307 121", "信用交易所", 2);
                        Akhcmd("shell input tap 1146 48", "领取信用", 3);
                        Akhcmd("shell input tap 722 719", "收取", 2);
                        Akhcmd("shell input tap 746 51", "", 2);

                        for (int i = 1; i <= 2 && schtData.control.buyThingsInShop; i++)
                        {
                            for (int j = 1; j <= 5; j++)
                            {
                                if (i == 1 && j == 1) { continue; }
                                Akhcmd("shell input tap " + ((j - 1) * 280 + 150) + " " + (300 * i), "商品", 2);
                                Akhcmd("shell input tap 1042 655", "购买物品", 3);
                                using (var sc = new ADB.Screenshot())
                                {
                                    if (sc.PicToPoint(Address.res + @"\pic\UI\shopBuyIcon.png").Count != 0)
                                    {
                                        i = 2;
                                        break;
                                    }
                                    else
                                    {
                                        Akhcmd("shell input tap 756 59", "收取", 2);
                                    }
                                }
                            }
                        }

                        Akhcmd("shell input tap 299 46", "菜单", 1);
                        Akhcmd("shell input tap 103 305", "首页", 3);
                    }
                    void Ann()
                    {
                        if (!schtData.ann.status) return;
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

                        if (PictureProcess.ColorCheck(431, 770, "#FFFFFF", 432, 770, "#FFFFFF")) { }
                        else
                        {
                            MBCore mb = new MBCore(MBCore.ModeType.time,
                            anntime,
                            false,
                            ann_cardToUse: schtData.ann.allowToUseCard ? 10 : 0);
                            mb.MessageUpdated += Info;
                            mb.Run();
                        }

                        Akhcmd("shell input tap 299 46", "菜单", 1);
                        Akhcmd("shell input tap 103 305", "首页", 3);
                    }
                    void Battle()
                    {
                        void MoveToCPInfo(int num = 0)
                        {
                            using (var _screenshot = new ADB.Screenshot())
                            {
                                var points = _screenshot.PicToPoint(Address.res + "\\pic\\battle\\" + "threeStarCp" + ".png", 0.95);
                                if (points.Count == 0)
                                    goto End;

                                //消去重复项
                                List<Point> reslst = new List<Point>();
                                while (points.Count != 0)
                                {
                                    var _pot = points[0];
                                    reslst.Add(_pot);
                                    points.RemoveAll(t => Math.Abs(_pot.X - t.X) < 881 - 848
                                                          && Math.Abs(_pot.Y - t.Y) < 881 - 848
                                                          );
                                }

                                if (num == 0)
                                {
                                    reslst.Sort((x, y) => x.Y.CompareTo(y.Y));//Y最小的排第一
                                    foreach (var point in reslst)
                                    {
                                        ADB.Tap(point);
                                        MBCore mb = new MBCore(MBCore.ModeType.time, 0);
                                        if (mb.Run().Type != MBCoreResult.ResultType.Error_AutoDeployNotAvailable)
                                            break;
                                        else
                                            Akhcmd("shell input tap 89 50", "返回", 2);
                                    }
                                }
                                else
                                {
                                    reslst.Sort((x, y) => -x.Y.CompareTo(y.Y));//Y最大的排第一
                                    ADB.Tap(reslst[num - 1]);
                                }
                            }
                        End:;
                            WithSystem.Wait(1000);
                        }
                        void MoveToCustomCP(string cpinfo)
                        {
                            var akhcpiaddress = cpinfo;
                            akhcpiMaker.Exe(akhcpiMaker.ReadFromAKHcpi(akhcpiaddress));
                        }
                        Data.SCHT.SCHTData.Cp exeCpInfo;
                        //first
                        if (schtData.first.unit != ("custom"))
                        {
                            AKHcmd.FormatAKHcmd["zhongduan"].RunCmd();
                            AKHcmd.FormatAKHcmd["ziyuanshouji"].RunCmd();
                            exeCpInfo = schtData.first;

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
                                    var _point = _screenshot.PicToPoint(Address.res + "\\pic\\battle\\" + schtData.first.unit + ".png", opencv_errorCon: 0.95);
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
                            MoveToCustomCP(schtData.first.cp);
                            goto UnitInited;
                        }
                        //second
                        if (!schtData.second.unit.Contains("custom"))
                        {
                            exeCpInfo = schtData.second;
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
                            MoveToCustomCP(schtData.second.cp);
                            goto UnitInited;
                        }
                    NativeUnitInited:;
                        WithSystem.Wait(2000);
                        MoveToCPInfo(exeCpInfo.cp == "auto" ? 0 : Convert.ToInt32(exeCpInfo.cp));
                    UnitInited:;
                        MBCore MB = new MBCore(mode: MBCore.ModeType.san);
                        MB.MessageUpdated += Info;
                        MB.Run();

                        Akhcmd("shell input tap 299 46", "菜单", 1);
                        Akhcmd("shell input tap 103 305", "首页", 3);
                    }
                    void Task()
                    {
                        Akhcmd("shell input tap 908 676", "任务", 2);
                        Akhcmd("shell input tap 767 41", "日常任务", 3);
                        Akhcmd("shell input tap 1246 168", "收集", 2, 3);
                        if (true)
                        {
                            Akhcmd("shell input tap 1051 51", "周常任务", 3);
                            Akhcmd("shell input tap 1246 168", "收集", 2, 3);
                        }
                        Akhcmd("shell input tap 109 51", "返回", 2);
                    }
                    void GetScreenshot()
                    {
                        using (var sc = new ADB.Screenshot())
                        {
                            sc.Save(Address.Screenshot.SCHT, ArkHelperDataStandard.Screenshot);
                        }
                    }
                    void ExitGame()
                    {
                        Akhcmd("shell am force-stop " + packname, "关闭" + packname, 1);
                    }

                    InitData();
                    GetUpdate();//检查更新（官）
                    StartGame();//启动游戏并登录
                    WaitGame();//等待游戏可操作
                    GetEmail();//领邮件
                    GotoBase();//进入基建
                    Base_OneClickGet();//一键领取
                    Base_MenuFactory();//操作制造站
                    Base_Clue();//会客室操作
                    Base_ChangeWork();//基建换班
                    ExitBase();//退出基建
                    ClueChange();//线索交流
                    ClueShop();//信用商店
                    Ann();//剿灭
                    Battle();//刷本
                    Task();//收取任务奖励
                    GetScreenshot();//截个图
                    ExitGame();//退出游戏
                }
                void End()
                {
                    //结束
                    WithSystem.KillSimulator(Modules.Connect.ConnectionInfo.Connections.First(t=>t.Value.Connected).Key);
                    Info("正在关闭模拟器神经网络依托平台...");
                    Info("系统任务运行完毕");
                    new ToastContentBuilder()
                    .AddArgument("kind", "SCHT")
                    .AddText("提示：定时事项处理指挥器任务已结束")
                    .AddText("开始时间：" + starttime.ToString("g") + "\n" + "结束时间：" + DateTime.Now.ToString("g"))
                    .Show(); //结束通知
                }
            });
            SCHT.Start();

            stopevent = () =>
            {
                SCHT.Abort();
                UIEnd(true);
            };

            void UIEnd(bool NewWindow = false)
            {
                StatusChanged?.Invoke(this, false);
                ADB.UnregisterADBUsing("SCHTRunning");
                Application.Current.Dispatcher.Invoke(() => (Window.GetWindow(this)).Close());
                App.OKtoOpenSCHT = true;
                if (NewWindow) new MainWindow().Show();
            }

        }

        private delegate void STOP();
        static STOP stopevent;

        private void StopClick(object sender, RoutedEventArgs e)
        {
            stopevent();
        }

        public static event EventHandler<bool> StatusChanged;
    }
}