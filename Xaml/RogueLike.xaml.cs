using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Point = System.Drawing.Point;

namespace ArkHelper.Xaml
{
    /// <summary>
    /// RogueLike.xaml 的交互逻辑
    /// </summary>
    public partial class RogueLike : Page
    {

        public class Person
        {
            public string Name { get; set; }
            public string[] Pics { get; set; }
            public string[] PicsInHelp { get; set; }
            public bool IsGot { get; set; }

        }
        public class Event
        {
            public string NamePic { set; get; }

            public virtual bool Action() { return true; }
        }
        public class PersonPoint
        {
            public Point place { set; get; }
            public Point swipe { set; get; }
            public Point touch { set; get; }
        }
        public class Cp : Event
        {
            public PersonPoint FirstPoint { set; get; }
            public PersonPoint SecondPoint { set; get; }
            public PersonPoint ThirdPoint { set; get; }
            public override bool Action()
            {
                return true;
            }
        }
        public class OtherEvent : Event
        {
            public string Name { get; set; }
            public OtherEvent(string name)
            {
                Name = name;
                NamePic = name + ".png";
            }
        }
        //静态组 用于存储关卡
        Cp[] cps = new Cp[4]
        {
            new Cp()
            {
                NamePic = "射手部队.png",
                FirstPoint = new PersonPoint()
                {
                    place = new Point(1150,330),
                    swipe = new Point(1150,720),
                    touch = new Point(1070,360)
                },
                SecondPoint = new PersonPoint()
                {
                    place = new Point(1030,330),
                    swipe = new Point(1030,720),
                },
                ThirdPoint = new PersonPoint()
                {
                    place = new Point(930,440),
                    swipe = new Point(1300,440),
                }
            },
            new Cp()
            {
                NamePic = "共生.png",
                FirstPoint = new PersonPoint()
                {
                    place = new Point(820,455),
                    swipe = new Point(820,100),
                    touch = new Point(720,460)
                },
                SecondPoint = new PersonPoint()
                {
                    place = new Point(930,430),
                    swipe = new Point(630,430),
                },
                ThirdPoint = new PersonPoint()
                {
                    place = new Point(1050,430),
                    swipe = new Point(650,430),
                }
            },
            new Cp()
            {
                NamePic = "蓄水池.png",
                FirstPoint = new PersonPoint()
                {
                    place = new Point(1070,500),
                    swipe = new Point(770,500),
                    touch = new Point(990,540)
                },
                SecondPoint = new PersonPoint()
                {
                    place = new Point(1060,380),
                    swipe = new Point(1060,600),
                },
                ThirdPoint = new PersonPoint()
                {
                    place = new Point(1040,290),
                    swipe = new Point(1040,600)
                }
            },
            new Cp()
            {
                NamePic = "虫群横行.png",
                FirstPoint = new PersonPoint()
                {
                    place = new Point(1050,440),
                    swipe = new Point(1050,700),
                    touch = new Point(970,460)
                },
                SecondPoint = new PersonPoint()
                {
                    place = new Point(925,340),
                    swipe = new Point(925,700),
                },
                ThirdPoint = new PersonPoint()
                {
                    place = new Point(1200,540),
                    swipe = new Point(800,560)
                }
            },
        };
        //静态表 存储其他时间
        OtherEvent[] otherevents = new OtherEvent[5]
        {
            new OtherEvent("商店"),
            new OtherEvent("不期而遇"),
            new OtherEvent("地区委托"),
            new OtherEvent("兴趣盎然1"),
            new OtherEvent("得偿所愿"),
        };

        string add = Address.res + "\\pic" + "\\ShuiYueRL\\";
        #region
        Person mountain = new Person()
        {
            Name = "山",
            Pics = new string[]
            {
                "山-头像.png",
                "皮肤山-头像.png",
                "读者山-头像.png"
            },
            PicsInHelp = new string[]
            {
                "无皮肤山-助战卡片.png"
                ,"皮肤山-助战卡片.png",
                "读者山-助战卡片.png"
            }
        };
        Person ansel = new Person()
        {
            Name = "安塞尔",
            Pics = new string[]
            {
                "无皮肤安塞尔-头像.png",
                "珊瑚海岸皮肤安塞尔-头像.png",
            }
        };
        Person caster = new Person()
        {
            Name = "预备干员-术士",
            Pics = new string[]
                {
                    "预备干员术士-头像.png",
                }
        };
        #endregion
        public RogueLike()
        {
            InitializeComponent();
            report.Visibility = Visibility.Collapsed;
            help.Visibility = Visibility.Visible;
            //


        }

        private void start_button_Click(object sender, RoutedEventArgs e)
        {
            //UI设置
            report.Visibility = Visibility.Visible;
            help.Visibility = Visibility.Collapsed;
            start_button.Visibility = Visibility.Collapsed;
            waiting_processbar.Visibility = Visibility.Visible;


            //开始工作
            Task SCHT = Task.Run(() =>
            {
                //时间
                string starttime = DateTime.Now.ToString("g");
                long starttime_sec = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();

                //人物选择
                Person First = mountain;
                Person Second = caster;
                Person Third = ansel;
                #region
                Point Get(string img)
                {
                    img = Address.res + @"\pic\ShuiYueRL\" + img;
                    Point[] bb;
                    using (ADB.Screenshot aa = new ADB.Screenshot())
                    {
                        bb = aa.PicToPoint(img, 0.7, 16, 100);
                    }
                    if (bb.Length > 0) { return bb[0]; }
                    else { return new Point(); }
                }

                Point wait(string img, double timeout)
                {
                    double aa = timeout * 1000;
                    for (; ; )
                    {
                        Get(img);
                        Thread.Sleep((int)aa);
                    }
                }

                void sleep(double time)
                {
                    int aa = (int)(time * 1000);
                    Thread.Sleep(aa);
                }

                void touch(Point point)
                {
                    ADB.Tap(point);
                }
                void touchimg(string img)
                {
                    var aa = Get(img);
                    ADB.Tap(aa);
                }

                bool exists(string img)
                {
                    var aa = Get(img);
                    if (aa.X == 0 && aa.Y == 0)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }

                void cfqw()
                {
                    akhcmd("tap,1300,514", "出发前往", 1);
                }
                void ksxd()
                {
                    akhcmd("tap,1238,761", "开始行动");
                }
                bool IsBattle()
                {
                    using (ADB.Screenshot aa = new ADB.Screenshot())
                    {
                        if (aa.PicToPoint("敌方情报.png").Length == 0)
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }


                //滑动屏幕
                void swipe()
                {
                    ADB.Swipe(new Point(1100, 400), new Point(200, 400), 2);
                    sleep(1);
                }

                Point tryPanDuan(string img)
                {
                    try
                    {
                        Point result = wait(img, timeout: 1);
                        return result;
                    }
                    catch
                    {
                        return new Point();
                    }
                }

                #endregion
                void Shangdian()
                {
                    sleep(1.0);
                    touch(new Point(1290, 600));
                    sleep(1.0);
                    int touzicishu = 0;
                    using (ADB.Screenshot aa = new ADB.Screenshot())
                    {
                        var aaa = aa.PicToPoint(add + "前瞻性投资系统.png");
                        if (aaa.Length != 0)
                        {
                            ADB.Tap(aaa[0]);
                            sleep(1.0);
                            using (ADB.Screenshot ab = new ADB.Screenshot())
                            {
                                var bbb = ab.PicToPoint(add + "投资入口.png");
                                ADB.Tap(bbb[0]);
                                sleep(1);
                                while (touzicishu < 5)
                                {
                                    using (ADB.Screenshot nnn = new ADB.Screenshot())
                                    {
                                        if (nnn.PicToPoint(add + "投资受限.png").Length == 0)
                                        {
                                            for (int i = 0; i < 30; i++)
                                            {
                                                touch(new Point(1100, 560));
                                            }
                                            touzicishu++;
                                        }
                                        else
                                        {
                                            sleep(1);
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        else { }
                        sleep(1.0);
                    }
                }
                void Buqieryu()
                {
                    cfqw();
                    sleep(5.0);
                    touch(new Point(1190, 406));
                    sleep(3.0);
                    using (ADB.Screenshot aaaa = new ADB.Screenshot())
                    {
                        string[] strings = new string[4]
                        {
                            "事件离开按钮.png",
                            "事件按钮1.png",
                            "事件按钮2.png",
                            "事件按钮3.png"
                        };
                        foreach (string s in strings)
                        {
                            if (detect(s))
                            {
                                goto aaa;
                            }
                        }
                        ADB.Tap(new Point(1190, 406));

                        bool detect(string aa)
                        {
                            var bb = aaaa.PicToPoint(add + aa);
                            if (bb.Length != 0)
                            {
                                ADB.Tap(bb[0]);
                                return true;
                            }
                            else { return false; }
                        }

                    }
                aaa:;
                    sleep(2.0);
                    using (ADB.Screenshot aaaa = new ADB.Screenshot())
                    {
                        Point[] aa = aaaa.PicToPoint(add + "事件确认按钮.png");
                        ADB.Tap(aa[0]);
                    }
                    sleep(1.5);
                    for (int i = 0; i < 3; i++)
                    {
                        using (ADB.Screenshot aaaa = new ADB.Screenshot())
                        {
                            Point[] aa = aaaa.PicToPoint(add + "骰子白勾.png");
                            if (aa.Length != 0) ADB.Tap(aa[0]);
                            sleep(3);
                        }
                    }
                    sleep(1.0);
                    using (ADB.Screenshot aaaa = new ADB.Screenshot())
                    {
                        Point[] aa = aaaa.PicToPoint(add + "放弃.png");
                        if (aa.Length != 0) ADB.Tap(aa[0]);
                        sleep(1);
                    }
                    for (int i = 0; i < 2; i++)
                    {
                        using (ADB.Screenshot aaaa = new ADB.Screenshot())
                        {
                            Point[] aa = aaaa.PicToPoint(add + "黑色确认键.png");
                            if (aa.Length != 0) ADB.Tap(aa[0]);
                            sleep(1.5);
                        }
                    }
                    for (int i = 0; i < 2; i++)
                    {
                        using (ADB.Screenshot aaaa = new ADB.Screenshot())
                        {
                            Point[] aa = aaaa.PicToPoint(add + "结局走向变化确认键.png");
                            if (aa.Length != 0) ADB.Tap(aa[0]);
                            sleep(1.5);
                        }
                    }
                    using (ADB.Screenshot aaaa = new ADB.Screenshot())
                    {
                        Point[] aa = aaaa.PicToPoint(add + "黑色确认键.png");
                        if (aa.Length != 0) ADB.Tap(aa[0]);
                    }
                    using (ADB.Screenshot aaaa = new ADB.Screenshot())
                    {
                        Point[] aa = aaaa.PicToPoint(add + "结局走向变化确认键.png");
                        if (aa.Length != 0) ADB.Tap(aa[0]);
                    }
                }
                void Diquweituo()
                {

                }
                void Xingzhiangran()
                {

                }
                void DeChangSuoYuan()
                {
                    cfqw();
                    sleep(4.0);
                    touch(new Point(1190, 406));
                    sleep(3.0);
                    using (ADB.Screenshot aaaa = new ADB.Screenshot())
                    {
                        Point[] aa = aaaa.PicToPoint(add + "取走这条.png");
                        if (aa.Length != 0)
                            ADB.Tap(aa[0]);
                    }
                    sleep(2.0);
                    using (ADB.Screenshot aaaa = new ADB.Screenshot())
                    {
                        Point[] aa = aaaa.PicToPoint(add + "事件确认按钮.png");
                        ADB.Tap(aa[0]);
                    }
                    sleep(2.5);

                    using (ADB.Screenshot aaaa = new ADB.Screenshot())
                    {
                        Point[] aa = aaaa.PicToPoint(add + "骰子白勾.png");
                        if (aa.Length != 0) ADB.Tap(aa[0]);
                        sleep(1);
                    }
                    using (ADB.Screenshot aaaa = new ADB.Screenshot())
                    {
                        Point[] aa = aaaa.PicToPoint(add + "放弃.png");
                        ADB.Tap(aa[0]);
                        Point[] ab = aaaa.PicToPoint(add + "放弃招募勾勾.png");
                        ADB.Tap(ab[0]);

                        sleep(1);
                    }
                    for (int i = 0; i < 2; i++)
                    {
                        using (ADB.Screenshot aaaa = new ADB.Screenshot())
                        {
                            Point[] aa = aaaa.PicToPoint(add + "得偿所愿获得藏品的叉.png");
                            if (aa.Length != 0) ADB.Tap(aa[0]);
                            sleep(1.5);
                        }
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        using (ADB.Screenshot aaaa = new ADB.Screenshot())
                        {
                            Point[] aa = aaaa.PicToPoint(add + "黑色确认键.png");
                            if (aa.Length != 0) ADB.Tap(aa[0]);
                            sleep(1.5);
                        }
                    }
                    using (ADB.Screenshot aaaa = new ADB.Screenshot())
                    {
                        Point[] aa = aaaa.PicToPoint(add + "结局走向变化确认键.png");
                        if (aa.Length != 0) ADB.Tap(aa[0]);
                        sleep(1.5);
                    }


                }
                for (; ; )
                {
                    //开始工作
                    akhcmd("tap,1314,660", "开始探索", 2);
                    akhcmd("tap,1150,664", "指挥分队", 1, 2);
                    akhcmd("tap,883,658", "取长补短", 1, 2);
                    akhcmd("tap,392,600", "近卫招募", 2);
                    SelectPerson(First);
                    akhcmd("tap,707,600", "辅助招募", 2);
                    SelectPerson(Second);
                    akhcmd("tap,1043,600", "医疗招募", 2);
                    SelectPerson(Third);

                    akhcmd("tap,1353,393", "探索海洋", 5);

                    akhcmd("tap,1238,768", "编队", 2);
                    akhcmd("tap,1114,43", "快捷编队", 2);
                    akhcmd("tap,627,158", First.Name, 1);
                    akhcmd("tap,182,612", First.Name + "二技能", 1);
                    akhcmd("tap,623,305", Second.Name, 1);
                    akhcmd("tap,625,454", Third.Name, 1);
                    akhcmd("tap,1290,760", "确认", 1);
                    akhcmd("tap,88,41", "返回", 1);

                    akhcmd("tap,440,370", "第一关", 2);
                    bool end = false;

                    while (!end)
                    {
                        Point[] points = new Point[5]
                        {
                            new Point(900, 370),
                            new Point(900, 280),
                            new Point(900, 210),
                            new Point(900, 440),
                            new Point(900, 510)
                        };

                        sleep(1.0);
                        foreach (Point p in points)
                        {
                            ADB.Tap(p);
                            sleep(0.2);
                        }
                        void dinaji(string a)
                        {
                            using (ADB.Screenshot aaaa = new ADB.Screenshot())
                            {
                                Point[] aa = aaaa.PicToPoint(add + a);
                                if (aa.Length != 0) ADB.Tap(aa[0]);
                                sleep(1);
                            }
                        }
                        string[] strings = new string[4]
                        {
                            "得偿所愿小字.png",
                            "地区委托小字.png",
                            "兴趣盎然.png",
                            "不期而遇小字.png"
                        };
                        foreach(string s in strings)
                        {
                            dinaji(s);
                        }

                        if (IsBattle())
                        {
                            using (ADB.Screenshot aa = new ADB.Screenshot())
                            {
                                foreach (Cp cp in cps)
                                {
                                    Point[] aaa = aa.PicToPoint(cp.NamePic);
                                    if (aaa.Length > 0)
                                    {
                                        cfqw();
                                        ksxd();
                                        end = cp.Action();
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            using (ADB.Screenshot aa = new ADB.Screenshot())
                            {
                                foreach (Cp cp in cps)
                                {
                                    Point[] aaa = aa.PicToPoint(add + cp.NamePic);
                                    if (aaa.Length > 0)
                                    {
                                        if (cp.NamePic == "商店")
                                        {
                                            Shangdian();
                                        }
                                        if (cp.NamePic == "不期而遇")
                                        {
                                            Buqieryu();
                                        }
                                        if (cp.NamePic == "地区委托")
                                        {
                                            Diquweituo();
                                        }
                                        if (cp.NamePic == "兴趣盎然1")
                                        {
                                            Xingzhiangran();
                                        }
                                        if (cp.NamePic == "得偿所愿")
                                        {
                                            DeChangSuoYuan();
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    using (ADB.Screenshot aa = new ADB.Screenshot())
                    {
                        var aaa = aa.PicToPoint(add + "退出按钮.png");
                        if (aaa.Length > 0)
                        {
                            sleep(0.5);
                            ADB.Tap(711, 737);
                            sleep(2.5);
                            touch(new Point(720, 720));
                            sleep(2.5);
                            touch(new Point(720, 720));
                            sleep(2.5);
                            touch(new Point(45, 30));
                            sleep(1.0);
                            touch(new Point(1307, 347));
                            sleep(1);
                            touch(new Point(941, 556));
                            sleep(3);
                        }
                        else { }
                        fanhui();
                    };
                }


                void fanhui()
                {
                    Thread.Sleep(4000);
                    for (int a = 0; a == 4; a++)
                    {
                        touch(new Point(700, 720));
                        sleep(4.0);
                    }
                    sleep(1.0);
                }

                void SelectPerson(Person person)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        using (var aa = new ADB.Screenshot())
                        {
                            foreach (var ac in person.Pics)
                            {
                                Point[] ab = aa.PicToPoint(add + ac);
                                if (ab.Length != 0)
                                {
                                    ADB.Tap(ab[0]);
                                    akhcmd("null", "招募" + person.Name, 2);
                                    akhcmd("tap,1306,760", "确认招募" + person.Name, 2);
                                    goto over;
                                }
                            }
                        }
                        swipe();
                    }
                help:;
                    akhcmd("tap,1243,48", "选择助战", 1);
                    for (int a = 0; ; a++)
                    {
                        int waittm = 4;
                        using (var aa = new ADB.Screenshot())
                        {
                            foreach (var ac in person.PicsInHelp)
                            {
                                Point[] ab = aa.PicToPoint(add + ac);
                                if (ab.Length != 0)
                                {
                                    ADB.Tap(ab[0]);
                                    akhcmd("null", "招募" + person.Name, 2);
                                    akhcmd("tap,1013,581", "确认招募" + person.Name, 2);
                                    goto over;
                                }

                            }
                        }
                        if (a > 2) { waittm++; }
                        Thread.Sleep(waittm * 1000);
                    }
                over:;
                    //招募结束，点击一下
                    Thread.Sleep(5000);
                    ADB.Tap(766, 293);
                }
            });


        }

        //方法 AKH指令转换器 ver1.1向下兼容待合并
        // 指令格式：adb语句(####延迟时间#;)($$$$重复次数$;)(&&&&显示文本&;)
        // 方法格式：akh指令,延迟时间,重复次数,显示文本
        // （必填，默认0，默认1，默认null，其中延迟时间的单位是秒、允许有三位及以内的小数，null为string格式且不显示，文本有值时会以info形式打印到前端和log）
        // 冲突处理：akh指令限定的参数优先
        void akhcmd(string text, string show = "null", int wait = 0, int repeat = 1)
        {
            if (text == "null") { }
            else
            {
                //解析
                if (text.Contains("####") && text.Contains("#;")) { wait = Convert.ToInt32(text.Substring(text.IndexOf("####") + 4, text.IndexOf("#;") - text.IndexOf("####") - 4)); }
                if (text.Contains("$$$$") && text.Contains("$;")) { repeat = Convert.ToInt32(text.Substring(text.IndexOf("$$$$") + 4, text.IndexOf("$;") - text.IndexOf("$$$$") - 4)); }
                if (text.Contains("&&&&") && text.Contains("&;")) { show = text.Substring(text.IndexOf("&&&&") + 4, text.IndexOf("&;") - text.IndexOf("&&&&") - 4); }
                string cmd = text.Replace("####" + wait + "#;", "").Replace("$$$$" + repeat + "$;", "").Replace("&&&&" + repeat + "&;", "");
                if (cmd.Contains(","))
                {
                    string[] _a = cmd.Split(',');
                    switch (_a[0])
                    {
                        case "tap":
                            cmd = "shell input tap " + _a[1] + " " + _a[2];
                            break;
                        case "swipe":
                            cmd = "shell input swipe " + _a[1] + " " + _a[2] + " " + _a[3] + " " + _a[4];
                            break;
                    }
                }

                for (int i = 1; i <= repeat; i++)
                {
                    if (cmd != "null")
                    {
                        ADB.CMD(cmd);
                    }
                    if (show == "null") { } else { info(show); }
                    Thread.Sleep(wait * 1000);
                }
            }
        }
        //方法 表达 + Output.Log
        void info(string content, bool show = true, Output.InfoKind infokind = Output.InfoKind.Infomational)
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
                    listbox.Items.Add(loglist_item);
                });
            }
        }


    }
}
