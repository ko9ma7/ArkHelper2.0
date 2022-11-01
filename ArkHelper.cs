using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Remoting.Channels;
using System.Security.RightsManagement;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static ArkHelper.Data.SCHT;
using Application = System.Windows.Application;
using JsonSerializer = System.Text.Json.JsonSerializer;
using MessageBox = System.Windows.MessageBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Point = System.Drawing.Point;
using ScrollViewer = System.Windows.Controls.ScrollViewer;

namespace ArkHelper
{
    /// <summary>
    /// 版本信息
    /// </summary>
    public class Version
    {
        /// <summary>
        /// 版本号
        /// </summary>
        public readonly static string tag = "v2.0.0.0";
        /// <summary>
        /// 版本种类
        /// </summary>
        public readonly static Kind kind = Kind.alpha;
        /// <summary>
        /// ArkHelperConfig版本
        /// </summary>
        public readonly static double ArkHelperConfig = 1.0;
        public enum Kind
        {
            alpha,
            beta,
            release,
        }
    }

    /// <summary>
    /// 数据传输
    /// </summary>
    public static class UniData
    {
        /// <summary>
        /// 从文件中读取json
        /// </summary>
        /// <param name="address">文件地址</param>
        /// <returns></returns>
        public static JsonElement ReadJson(string address)
        {
            if (!File.Exists(address)) { return new JsonElement(); }
            var _text = File.ReadAllText(address);
            var _result = JsonSerializer.Deserialize<JsonElement>(_text);
            return _result;
        }
        /// <summary>
        /// 截图名称获取
        /// </summary>
        /// <returns>四位数年+两位数月+两位数日+两位数时分秒+三位毫秒+“.png”</returns>
        public static string Screenshot => DateTime.Now.ToString("yyyyMMdd") + DateTime.Now.ToString("HHmmssfff") + @".png";

        /// <summary>
        /// log名称获取
        /// </summary>
        /// <returns></returns>
        public static string Log => Address.log + @"\" + DateTime.Now.ToString("yyyyMMdd") + @".log";
        /// <summary>
        /// 消息来源类型
        /// </summary>
        public enum MessageSource
        {
            weibo,
            bilibili,
            zhihu_person,
            zhihu_company,
            twitter,
            facebook,
            web,
            official_communication
        }
        public enum ArgKind
        {
            Navigate,
            ActiveFunc,
            Shut,
            none
        }

        public class ArkHelperArg
        {
            //public static string Creator { get; set; }
            public string Discribe { get; set; } = "null";
            public ArgKind Arg { get; set; } = ArgKind.none;
            public string ArgContent { get; set; } = "null";
            public string Target { get; set; } = "all";
            public ArkHelperArg() { }
            public ArkHelperArg(ArgKind arg, string argContent, string target)
            {
                Arg = arg;
                ArgContent = argContent;
                Target = target;
            }
            public void Dispose()
            {
                Discribe = "null";
                Arg = ArgKind.none;
                ArgContent = "null";
                Target = "all";
            }

            public override string ToString()
            {
                return Arg + ":" + ArgContent + "(" + Discribe + ")," + Target;
            }
        }
    }

    /// <summary>
    /// 参数
    /// </summary>
    public static class Param
    {
        /// <summary>
        /// 参数
        /// </summary>
        public readonly static string start_page = "Home";
        public readonly static bool logAdvanceOutput = true;
    }

    /// <summary>
    /// ADB
    /// </summary>
    public class ADB
    {
        public static PinnedData.Simulator.SimuInfo ConnectedInfo = null;//= new PinnedData.Simulator.SimuInfo();

        /// <summary>
        /// process
        /// </summary>
        public static Process process = new Process()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = Address.adb,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
            }
        };

        /// <summary>
        /// 指令
        /// </summary>
        /// <param name="cmd">指令</param>
        /// <returns>adb的返回结果</returns>
        public static string CMD(string cmd)
        {
            while (!cmd.Contains("connect") && ConnectedInfo == null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    WithSystem.Message("模拟器连接断开", "请启动或重启模拟器");
                });
                Thread.Sleep(3000);
            }

            process.StartInfo.Arguments = cmd;

            if (Param.logAdvanceOutput) Output.Log(cmd, "ADB");

            process.Start();
            var end = process.StandardOutput.ReadToEnd();

            if (Param.logAdvanceOutput) Output.Log("=>" + end.Replace("\n", "[linebreak]").Replace("\r", ""), "ADB");

            process.WaitForExit();

            if (Param.logAdvanceOutput) Output.Log("=>" + "Exited", "ADB");

            return end;
        }

        /// <summary>
        /// 连接模拟器
        /// </summary>
        public static void Connect()
        {
            //判断标示的模拟器是否运行，若否 则清空连接信息重连
            if (ConnectedInfo != null)
            {
                if (Process.GetProcessesByName(ConnectedInfo.IM).Length == 0)
                    ConnectedInfo = null;
                else
                    return;
            }

            //尝试遍历寻找在线的模拟器
            PinnedData.Simulator.SimuInfo ConnectThis = new PinnedData.Simulator.SimuInfo();
            if (Data.simulator.custom.status)
            {
                if (Process.GetProcessesByName(Data.simulator.custom.im).Length != 0)
                    ConnectThis = new PinnedData.Simulator.SimuInfo("custom", "自定义", Data.simulator.custom.port, Data.simulator.custom.im);
                else return;
            }
            else
            {
                foreach (PinnedData.Simulator.SimuInfo info in PinnedData.Simulator.Support)
                    if (Process.GetProcessesByName(info.IM).Length != 0)
                    {
                        ConnectThis = info;
                        break;
                    }
                    else return;
            }

            //等待模拟器端守护进程响应连接
            if (CMD("connect " + "127.0.0.1:" + ConnectThis.Port).Contains("connected"))
                ConnectedInfo = ConnectThis;
        }

        #region 点击
        /// <summary>
        /// adb点击
        /// </summary>
        /// <param name="point">点击坐标</param>
        public static void Tap(Point point)
        {
            Tap(point.X,point.Y);
        }
        /// <summary>
        /// adb点击
        /// </summary>
        /// <param name="x">横坐标</param>
        /// <param name="y">纵坐标</param>
        public static void Tap(int x, int y)
        {
            CMD("shell input tap " + x + " " + y);
        }
        #endregion

        #region 滑动
        /// <summary>
        /// adb滑动
        /// </summary>
        /// <param name="point">起始点</param>
        /// <param name="point1">终止点</param>
        /// <param name="time">滑动时间（单位：毫秒）</param>
        public static void Swipe(Point point, Point point1, int time = 1000)
        {
            Swipe(point.X, point.Y, point1.X, point.Y, time);
        }
        /// <summary>
        /// adb滑动
        /// </summary>
        /// <param name="x1">起始点横坐标</param>
        /// <param name="y1">起始点纵坐标</param>
        /// <param name="x2">终止点横坐标</param>
        /// <param name="y2">终止点纵坐标</param>
        /// <param name="time">滑动时间（单位：毫秒）</param>
        public static void Swipe(int x1, int y1, int x2, int y2, int time = 1000)
        {
            CMD("shell input swipe " + x1 + " " + y1 + " " + x2 + " " + y2 + time);
        }
        #endregion

        /// <summary>
        /// 截图
        /// </summary>
        /// <param name="address">截图输出地址</param>
        /// <param name="name">截图名称</param>
        /// <returns>所获截图的绝对地址</returns>
        public static string GetScreenshot(string address, string name)
        {
            CMD(@"shell screencap -p /sdcard/DCIM/" + name); //截图
            CMD(@"pull /sdcard/DCIM/" + name + " " + address); //pull
            CMD(@"shell rm -f /sdcard/DCIM/" + name); //删除
            //输出
            return address + "\\" + name;
        }

        public class Screenshot : IDisposable
        {
            private string Location { get; set; }
            private Bitmap ImgBitmap { get; set; }
            public Screenshot()
            {
                string name = UniData.Screenshot;
                string address = Address.cache;
                CMD(@"shell screencap -p /sdcard/DCIM/" + name); //截图
                CMD(@"pull /sdcard/DCIM/" + name + " " + address); //pull
                CMD(@"shell rm -f /sdcard/DCIM/" + name); //删除
                Location = address + @"\" + name;
                //DisposeAfterTime();
            }
            public void Save(string address, string name)
            {
                File.Copy(Location, address + @"\" + name);
            }

            #region 取色
            /// <summary>
            /// 取色
            /// </summary>
            /// <param name="x">像素点横坐标</param>
            /// <param name="y">像素点纵坐标</param>
            /// <returns>该像素点的16进制颜色</returns>
            public string ColorPick(int x, int y)
            {
                InitBitmap();
                return ColorTranslator.ToHtml(ImgBitmap.GetPixel(x, y));
            }
            /// <summary>
            /// 取色
            /// </summary>
            /// <param name="point">像素点横坐标</param>
            /// <returns>该像素点的16进制颜色</returns>
            public string ColorPick(Point point)
            {
                return ColorPick(point.X,point.Y);
            }
            #endregion

            public Point[] PicToPoint(string smallimg, double errorCon = 0.7, int errorRange = 16, int num = 50)
            {
                if (!File.Exists(smallimg)) { return new Point[0]; }

                //初始化图像类
                InitBitmap();
                var smallBM = new Bitmap(smallimg);

                return PictureProcess.PicToPoint.GetPoint(this.ImgBitmap, smallBM, errorCon, errorRange, num);
            }
            private void InitBitmap()
            {
                if (ImgBitmap == null)
                    ImgBitmap = new Bitmap(Location);
                else
                    return;
            }

            #region dispose
            private void DisposeAfterTime()
            {
                Task.Run(() =>
                {
                    Thread.Sleep(20000);
                    Dispose();
                });
            }
            /// <summary>
            /// 释放标记
            /// </summary>
            private bool disposed;
            /// <summary>
            /// 为了防止忘记显式的调用Dispose方法
            /// </summary>
            ~Screenshot()
            {
                //必须为false
                Dispose(false);
            }
            /// <summary>执行与释放或重置非托管资源关联的应用程序定义的任务。</summary>
            public void Dispose()
            {
                //必须为true
                Dispose(true);
                //通知垃圾回收器不再调用终结器
                GC.SuppressFinalize(this);
            }
            /// <summary>
            /// 非密封类可重写的Dispose方法，方便子类继承时可重写
            /// </summary>
            /// <param name="disposing"></param>
            protected virtual void Dispose(bool disposing)
            {
                if (disposed)
                {
                    return;
                }
                //清理托管资源
                if (disposing)
                {
                    if (ImgBitmap != null) { ImgBitmap.Dispose(); ImgBitmap = null; }
                    try
                    {
                        File.Delete(Location);
                    }
                    catch { }


                }
                //清理非托管资源

                //告诉自己已经被释放
                disposed = true;
            }
            #endregion

        }
    }

    /// <summary>
    /// 图片处理
    /// </summary>
    public static class PictureProcess
    {
        /// <summary>
        /// 图片取色
        /// </summary>
        /// <param name="img">图像地址</param>
        /// <param name="param">点坐标</param>
        /// <returns>16进制点颜色</returns>
        public static string[] ColorPick(string img, params int[] param)
        {
            if (param.Length % 2 != 0)
            {
                return new string[0];
            }

            string[] _strings = new string[param.Length / 2];
            Bitmap _bitmap = new Bitmap(img);
            for (int i = 0; i < (param.Length / 2); i++)
            {
                _strings[i] =
                    ColorTranslator.ToHtml(
                    _bitmap.GetPixel(param[i * 2], param[i * 2 + 1])
                    );
            }

            _bitmap.Dispose();
            return _strings;
        }

        /// <summary>
        /// 颜色判断
        /// </summary>
        /// <param name="x1">第一点纵坐标</param>
        /// <param name="y1">第一点横坐标</param>
        /// <param name="c1">第一点期望颜色值</param>
        /// <param name="x2">第二点纵坐标</param>
        /// <param name="y2">第二点横坐标</param>
        /// <param name="c2">第二点期望颜色值</param>
        /// <returns>如果两点都符合对应的期望颜色值，返回<see langword="true"/>，否则返回<see langword="false"/>。</returns>
        public static bool ColorCheck(int x1, int y1, string c1, int x2, int y2, string c2)
        {
            string _name = ADB.GetScreenshot(Address.cache, UniData.Screenshot);
            string[] color = ColorPick(
                _name, x1, y1, x2, y2);
            File.Delete(_name);
            if (color[0] == c1 || color[1] == c2) { return true; }
            else { return false; }
        }

        /// <summary>
        /// 图像识别检出图像位置
        /// </summary>
        public static class PicToPoint
        {
            /// <summary>
            /// 获取
            /// </summary>
            /// <param name="bigBM">大图Bitmap</param>
            /// <param name="smallBM">小图Bitmap</param>
            /// <param name="errorCon">点容差（0~1），越大越难以匹配</param>
            /// <param name="errorRange">色容差（0~255），越大越容易匹配</param>
            /// <param name="num">精确度，越大越准确</param>
            /// <returns>点数组</returns>
            public static Point[] GetPoint(Bitmap bigBM,Bitmap smallBM,double errorCon=0.6,int errorRange = 15,int num = 100)
            {
                //声明数组，写入随机坐标
                Point[] pointSmall = new Point[num];
                Random rand = new Random();
                for (int i = 0; i < num; i++)
                {
                repeat:;
                    Point aa = new Point(rand.Next(0, smallBM.Width), rand.Next(0, smallBM.Height));
                    if (pointSmall.Contains(aa)) { goto repeat; }
                    else { pointSmall[i] = aa; }
                }

                Point[] pointBig = new Point[num]; //声明数组，稍后写入以上随机坐标在大图中的对应坐标
                Point[] pointDistance = new Point[num]; //反映其他点与第一点的位置关系
                Color[] pointColor = new Color[num]; //获取点的颜色
                for (int i = 0; i < num; i++)
                {
                    pointBig[i] = new Point(0, 0);
                    pointDistance[i] = PointSub(pointSmall[0], pointSmall[i]);
                    pointColor[i] = smallBM.GetPixel(pointSmall[i].X, pointSmall[i].Y);
                }

                // 声明数组，用于写入pointBig[]
                ArrayList pointBigArray = new ArrayList();

                //在大图中检索第一个点
                for (int y = 0; y < bigBM.Height; y++)
                {
                    for (int x = 0; x < bigBM.Width; x++)
                    {
                        //第一点颜色匹配
                        if (IsColor(bigBM.GetPixel(x, y), pointColor[0]))
                        {
                            //测试第一点
                            pointBig[0] = new Point(x, y);

                            int a = 0; //容差计算1
                            for (int i = 1; i < num; i++)
                            {
                                //算出对应点的坐标
                                pointBig[i] = PointSub(pointBig[0], pointDistance[i]);

                                //跳过错误点
                                if (pointBig[i].X < 0
                                    || pointBig[i].Y < 0
                                    || pointBig[i].X >= bigBM.Width
                                    || pointBig[i].Y >= bigBM.Height)
                                { break; }

                                //容差计算2 推测出其他点 判断对应颜色是否符合
                                if (IsColor(bigBM.GetPixel(pointBig[i].X, pointBig[i].Y), pointColor[i])) { a += 1; }
                            }
                            if (a >= (num * errorCon))
                            {
                                //必须强制手动转换一次（以下）
                                //否则ArrayList会直接从内存中寻址
                                //造成数据重复
                                Point[] _points = new Point[num];
                                for (int i = 0; i < num; i++)
                                {
                                    _points[i] = pointBig[i];
                                }
                                pointBigArray.Add(_points);
                            }
                        }
                    }
                }

                //遍历全部结束

                if (pointBigArray.Count > 0)
                //存在图片 语句
                {
                    //提取中心点
                    ArrayList FinallyPoints = new ArrayList();
                    foreach (Point[] _pointBig in pointBigArray)
                    {
                        //计算中心点
                        Point leftTopPointSmall = PointSub(_pointBig[0], pointSmall[0]);
                        Point _point = new Point(leftTopPointSmall.X + smallBM.Width / 2, leftTopPointSmall.Y + smallBM.Height / 2);
                        //中心点去重
                        if (FinallyPoints.Count == 0)
                        {
                            FinallyPoints.Add(_point);
                        }
                        else
                        {
                            bool _check = true;
                            foreach (Point __point in FinallyPoints)
                            {
                                Point ___point = PointSub(_point, __point);
                                if (Math.Abs(___point.X) < smallBM.Width
                                    && Math.Abs(___point.Y) < smallBM.Height)
                                {
                                    _check = false;
                                }
                            }
                            if (_check)
                            {
                                FinallyPoints.Add(_point);
                            }
                        }
                    }
                    //存储
                    Point[] _points = new Point[FinallyPoints.Count];
                    for (int i = 0; i < FinallyPoints.Count; i++)
                    {
                        _points[i] = (Point)FinallyPoints[i];
                    }
                    bigBM.Dispose(); smallBM.Dispose();
                    return _points;
                }
                else
                {
                    bigBM.Dispose(); smallBM.Dispose();
                    return new Point[0];
                }

                //坐标相减
                Point PointSub(Point point1, Point point2)
                {
                    Point point = new Point(point1.X - point2.X, point1.Y - point2.Y);
                    return point;
                }
                //色差计算
                bool IsColor(Color colorA, Color colorB)
                {
                    return colorA.A <= colorB.A + errorRange && colorA.A >= colorB.A - errorRange &&
                           colorA.R <= colorB.R + errorRange && colorA.R >= colorB.R - errorRange &&
                           colorA.G <= colorB.G + errorRange && colorA.G >= colorB.G - errorRange &&
                           colorA.B <= colorB.B + errorRange && colorA.B >= colorB.B - errorRange;
                }
            }
            /// <summary>
            /// 获取
            /// </summary>
            /// <param name="bigpic">大图地址</param>
            /// <param name="smallpic">小图地址</param>
            /// <param name="errorCon">点容差（0~1），越大越难以匹配</param>
            /// <param name="errorRange">色容差（0~255），越大越容易匹配</param>
            /// <param name="num">精确度，越大越准确</param>
            /// <returns>点数组</returns>
            public static Point[] GetPoint(string bigpic, string smallpic, double errorCon = 0.6, int errorRange = 15, int num = 100)
            {
                if (!File.Exists(bigpic)) { return new Point[0]; }
                if (!File.Exists(smallpic)) { return new Point[0]; }

                //初始化图像类
                var bigBM = new Bitmap(bigpic);
                var smallBM = new Bitmap(smallpic);

                return GetPoint(bigBM, smallBM, errorCon,errorRange,num);
            }
        }
    }

    /// <summary>
    /// ArkHelper地址
    /// </summary>
    public static class Address
    {
        /// <summary>
        /// 数据存储（ProgramData）
        /// </summary>
        public readonly static string programData = Environment.GetEnvironmentVariable("ProgramData") + @"\ArkHelper";
        /// <summary>
        /// data地址
        /// </summary>
        public readonly static string data = programData + @"\data";
        /// <summary>
        /// ArkHelper根目录
        /// </summary>
        public readonly static string akh = Directory.GetCurrentDirectory();
        /// <summary>
        /// ArkHelper根目录下的附加文件
        /// </summary>
        public readonly static string akhExternal = akh + @"\external";
        /// <summary>
        /// 数据存储地址下的附加文件
        /// </summary>
        public readonly static string dataExternal = programData + @"\external";
        /// <summary>
        /// 资源文件
        /// </summary>
        public readonly static string res = akhExternal + @"\res";
        /// <summary>
        /// Cache
        /// </summary>
        public readonly static string cache = programData + @"\cache";
        public static class Screenshot
        {
            /// <summary>
            /// 截图根目录
            /// </summary>
            public readonly static string main = data + @"\screenshot";
            /// <summary>
            /// MB截图
            /// </summary>
            public readonly static string MB = main + @"\MB";
            /// <summary>
            /// scht截图
            /// </summary>
            public readonly static string SCHT = main + @"\SCHT";
        }
        /// <summary>
        /// AKH设置文件
        /// </summary>
        public readonly static string config = programData + @"\data\config.json";
        /// <summary>
        /// log
        /// </summary>
        public readonly static string log = programData + @"\log";
        /// <summary>
        /// cmd
        /// </summary>
        public readonly static string cmd = akhExternal + "\\cmd.lnk";
        /// <summary>
        /// adb
        /// </summary>
        public readonly static string adb = akhExternal + @"\adb.exe";

        //静态字段 地址
        public readonly static string github = @"https://github.com/ArkHelper/ArkHelper2.0";
        public readonly static string tg = @"https://t.me/ArkHelper";
        public readonly static string email = @"ArkHelper@proton.me";

        //静态字段 地址
        public readonly static string EULA = github + "/blob/master/Doc/EULA.md";
        public readonly static string PrivatePolicy = github + "/blob/master/Doc/ArkHelper隐私政策.md";

        public static void Create()
        {
            //创建目录
            Directory.CreateDirectory(programData);

            Directory.CreateDirectory(data);
            Directory.CreateDirectory(log);
            Directory.CreateDirectory(cache);

            //Directory.CreateDirectory(cache + @"\message");

            Directory.CreateDirectory(dataExternal);

            Directory.CreateDirectory(Screenshot.main);
            Directory.CreateDirectory(Screenshot.MB);
            Directory.CreateDirectory(Screenshot.SCHT);
        }
    }

    /// <summary>
    /// 运行数据
    /// </summary>
    public static class Data
    {
        public static class simulator
        {
            public static class custom
            {
                public static bool status = false;
                public static int port = 0;
                public static string im = "";
            }
        }
        public static class SCHT
        {
            public static bool status = false;
            public static class first
            {
                public static string unit = "LS";
                public static string cp = "1";
            }
            public static class second
            {
                public static string unit = "LS";
                public static string cp = "1";
            }
            public static class ann
            {
                public static bool status = false;
                public static string select = "TT";
                public static class time
                {
                    public static bool custom = true;
                    public static int Mon = 0;
                    public static int Tue = 0;
                    public static int Wed = 0;
                    public static int Thu = 0;
                    public static int Fri = 0;
                    public static int Sat = 0;
                    public static int Sun = 0;
                }
            }
            public static class server
            {
                public static string id = "CO";
            }
            public static class fcm
            {
                public static bool status = true;
            }
        }
        public static class ArkHelper
        {
            public static bool pure = false;
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        public static void Load()
        {
            if (File.Exists(Address.config))
            {
                //读取配置
                JObject config = (JObject)JToken.ReadFrom(
                    new JsonTextReader(
                        File.OpenText(Address.config)
                        )
                    );

                //模拟器
                JObject _simulator = (JObject)config["simulator"]["custom"];
                simulator.custom.status = (bool)_simulator["status"];
                if (simulator.custom.status)
                {
                    simulator.custom.port = Convert.ToInt32(_simulator["port"]);
                    simulator.custom.im = _simulator["im"].ToString();
                }
                //akh
                JObject _akh = (JObject)config["ArkHelper"];
                ArkHelper.pure = (bool)_akh["pure"];
                //SCHT
                JObject _SCHT = (JObject)config["SCHT"];
                status = (bool)_SCHT["status"];
                if (status)
                {
                    first.unit = _SCHT["first"]["unit"].ToString();
                    first.cp = _SCHT["first"]["cp"].ToString();

                    if (first.unit != "LS" && first.unit != "custom")
                    {
                        second.unit = _SCHT["second"]["unit"].ToString();
                        second.cp = _SCHT["second"]["cp"].ToString();
                    }

                    ann.status = (bool)_SCHT["ann"]["status"];
                    if (ann.status)
                    {
                        ann.select = _SCHT["ann"]["select"].ToString();
                        JObject _time = (JObject)_SCHT["ann"]["time"];
                        ann.time.custom = (bool)_time["custom"];
                        if (ann.time.custom)
                        {
                            ann.time.Mon = Convert.ToInt32(_time["Mon"].ToString());
                            ann.time.Tue = Convert.ToInt32(_time["Tue"].ToString());
                            ann.time.Wed = Convert.ToInt32(_time["Wed"].ToString());
                            ann.time.Thu = Convert.ToInt32(_time["Thu"].ToString());
                            ann.time.Fri = Convert.ToInt32(_time["Fri"].ToString());
                            ann.time.Sat = Convert.ToInt32(_time["Sat"].ToString());
                            ann.time.Sun = Convert.ToInt32(_time["Sun"].ToString());
                        }
                    }
                    server.id = _SCHT["server"]["id"].ToString();
                    fcm.status = (bool)_SCHT["fcm"]["status"];
                }
            }
        }

        /// <summary>
        /// 保存数据
        /// </summary>
        public static void Save()
        {
            if (!File.Exists(Address.config))
            {
                File.Create(Address.config).Close();
            }

            JObject config = new JObject();
            JObject akh = new JObject()
            {
                {"pure",ArkHelper.pure }
            };
            config.Add("ArkHelper", akh);

            JObject simu = new JObject();
            JObject custom = new JObject();
            custom.Add("status", simulator.custom.status);
            if (simulator.custom.status)
            {
                custom.Add("im", simulator.custom.im);
                custom.Add("port", simulator.custom.port);
            }
            simu.Add("custom", custom);
            config.Add("simulator", simu);

            JObject SCHT = new JObject();
            SCHT.Add("status", status);
            if (status)
            {
                JObject _first = new JObject()
                {
                    {"unit",first.unit },
                    {"cp",first.cp }
                };
                SCHT.Add("first", _first);

                if (first.unit != "custom" && first.unit != "LS")
                {
                    JObject _second = new JObject()
                    {
                        {"unit",second.unit },
                        {"cp",second.cp }
                    };
                    SCHT.Add("second", _second);
                }

                //剿灭
                JObject _ann = new JObject()
                {
                    {"status",ann.status }
                };
                if (ann.status)
                {
                    _ann.Add("select", ann.select);
                    JObject _time = new JObject()
                    {
                        {"custom",ann.time.custom }
                    };
                    if (ann.time.custom)
                    {
                        _time.Add("Mon", ann.time.Mon);
                        _time.Add("Tue", ann.time.Tue);
                        _time.Add("Wed", ann.time.Wed);
                        _time.Add("Thu", ann.time.Thu);
                        _time.Add("Fri", ann.time.Fri);
                        _time.Add("Sat", ann.time.Sat);
                        _time.Add("Sun", ann.time.Sun);
                    }
                    _ann.Add("time", _time);
                }
                SCHT.Add("ann", _ann);

                //服务器
                JObject _server = new JObject()
                {
                    {"id",server.id }
                };
                SCHT.Add("server", _server);

                //防沉迷
                JObject _fcm = new JObject()
                {
                    {"status",fcm.status }
                };
                SCHT.Add("fcm", _fcm);
            }
            config.Add("SCHT", SCHT);
            //保存
            File.WriteAllText(Address.config, JsonConvert.SerializeObject(config, Formatting.Indented));
        }
    }

    /// <summary>
    /// 输出
    /// </summary>
    public static class Output
    {
        /// <summary>
        /// 枚举 info种类
        /// </summary>
        public enum InfoKind
        {
            Debug,
            Infomational,
            Warning,
            Error,
            Emergency
        }

        /// <summary>
        /// 文字输出到文件
        /// </summary>
        /// <param name="content">内容</param>
        /// <param name="file">文件路径</param>
        public static void Text(string content, string file)
        {
            if (File.Exists(file)) { } else { File.Create(file).Close(); } //检查有无该文件，无就创建
            StreamWriter output_stream = new StreamWriter(file, true) { AutoFlush = true }; //启动写入流
            output_stream.WriteLine(content);
            output_stream.Close();
        }

        //方法 日志输出
        // 参数：输出内容，操作模块，日志级别
        public static void Log(string content, string module = "ArkHelper", InfoKind infokind = InfoKind.Infomational)
        {
            Text(DateTime.Now.ToString("s")
                + " "
                + "[" + module + "]"
                + " "
                + "[" + infokind.ToString() + "]"
                + " "
                + content
                , UniData.Log
                );
        }
    }

    /// <summary>
    /// 固定数据
    /// </summary>
    public static class PinnedData
    {
        /// <summary>
        /// 模拟器
        /// </summary>
        public class Simulator
        {
            public static List<SimuInfo> Support = new List<SimuInfo>
            {
                new SimuInfo("MuMu", "MuMu模拟器", 7555, "NemuPlayer"),
                new SimuInfo("BlueStacks", "蓝叠模拟器", 5555, "HD-Player"),
                new SimuInfo("LDplayer", "雷电模拟器", 5555, "dnplayer"),
                new SimuInfo("MEMU", "逍遥模拟器", 21503, "MEmu"),
                new SimuInfo("NOX", "夜神模拟器", 62001, "Nox"),
                new SimuInfo("WSA", "WSA", 58526, "WSAClient"),
            };
            public class SimuInfo
            {
                public string ID { get; set; }
                public string Name { get; set; }
                public int Port { get; set; }
                public string IM { get; set; }
                public SimuInfo(string id, string name, int port, string im)
                {
                    ID = id;
                    Name = name;
                    Port = port;
                    IM = im;
                }
                public SimuInfo() { }

                public override string ToString()
                {
                    return ID + Name + Port + " " + IM;
                }
            }
        }
        /// <summary>
        /// 游戏服务器
        /// </summary>
        public static class Server
        {
            /// <summary>
            /// 服务器信息表
            /// </summary>
            /// 
            public static DataTable dataSheet = new DataTable();
            public static void Load()
            {
                dataSheet.Columns.Add(new DataColumn("id", typeof(string)));
                dataSheet.Columns.Add(new DataColumn("shortname", typeof(string)));
                dataSheet.Columns.Add(new DataColumn("name", typeof(string)));
                dataSheet.Columns.Add(new DataColumn("package", typeof(string)));
                dataSheet.Rows.Add("CO", "明日方舟", "明日方舟（国服，CN，鹰角网络）", "com.hypergryph.arknights");
                dataSheet.Rows.Add("CB", "明日方舟", "明日方舟（国服，CN，bilibili）", "com.hypergryph.arknights.bilibili");
                dataSheet.Rows.Add("JP", "アークナイツ", "アークナイツ（日服，JP，悠星网络）", "com.YoStarJP.Arknights");
                dataSheet.Rows.Add("EN", "Arknights", "Arknights（英服，EN，悠星网络）", "com.YoStarEN.Arknights");
                dataSheet.Rows.Add("KR", "명일방주", "명일방주（韩服，KR，悠星网络）", "com.YoStarKR.Arknights");
                dataSheet.Rows.Add("TW", "明日方舟", "明日方舟（台服，TW，龙成网络）", "tw.txwy.and.arknights");
            }
        }
    }

    /// <summary>
    /// 系统交互
    /// </summary>
    public static class WithSystem
    {
        /// <summary>
        /// 锁定计算机
        /// </summary>
        [System.Runtime.InteropServices.DllImport("user32")]
        public static extern void LockWorkStation();

        /// <summary>
        /// cmd
        /// </summary>
        /// <param name="command">命令</param>
        public static void Cmd(params string[] command)
        {
            Process cmd = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = @"cmd.exe",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardInput = true
                }
            };
            cmd.Start();
            foreach (string s in command)
            {
                cmd.StandardInput.WriteLine(s);
            }
            cmd.Close();
        }

        /// <summary>
        /// 打开文件
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="filter">过滤器</param>
        /// <returns>文件绝对地址</returns>
        public static string OpenFile(string title = "选择文件", string filter = "文件(*.*)|*.*")
        {
            OpenFileDialog file = new OpenFileDialog
            {
                Title = title,
                Filter = filter
            };
            file.ShowDialog();
            return file.FileName;
        }

        /// <summary>
        /// 释放内存
        /// </summary>
        public static void GarbageCollect()
        {
            //挂个线程
            Task task = Task.Run(() =>
            {
                Thread.Sleep(3000);
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            });
        }

        /// <summary>
        /// 关闭模拟器进程
        /// </summary>
        public static void KillSimulator()
        {
            Process[] _pr = Process.GetProcessesByName(ADB.ConnectedInfo.IM);
            var pr = _pr[0];
            pr.Kill();
            ADB.ConnectedInfo = null;
        }

        /// <summary>
        /// 显示弹窗消息
        /// </summary>
        /// <param name="message">消息主体</param>
        /// <param name="guide">输出给用户的建议</param>
        public static void Message(string message, string guide = "")
        {
            if (guide != "")
            {
                message = message + "\n" + "/" + guide;
            }
            MessageBox.Show("/// " + message, "ArkHelper");
        }

        /// <summary>
        /// 关机
        /// </summary>
        public static void Shutdown(int waittime = 1)
        {
            WithSystem.Cmd("shutdown /s /t " + waittime);
        }

        /// <summary>
        /// 睡眠
        /// </summary>
        public static void Sleep()
        {
            WithSystem.Cmd("rundll32.exe powrprof.dll,SetSuspendState 0,1,0");
        }
    }

    /// <summary>
    /// 网络交互
    /// </summary>
    public static class WithNet
    {
        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="url">下载地址</param>
        /// <param name="address">下载位置（绝对路径）</param>
        public static void DownloadFile(string url, string address)
        {
            string cache = address + @".cache";
            if (File.Exists(cache)) { File.Delete(cache); }
            if (!File.Exists(address))
            {
                var web = new WebClient();
                web.DownloadFile(url, cache);
                Directory.Move(cache, address);
            }
        }

        /// <summary>
        /// 调用api
        /// </summary>
        /// <param name="url">api地址</param>
        /// <returns>json</returns>
        public static JsonElement GetFromApi(string url)
        {
        start:;
            try
            {
                //API
                var client = new RestClient(url);
                var request = new RestRequest { Method = Method.Get };
                var response = client.Get(request);
                var _result = JsonSerializer.Deserialize<JsonElement>(response.Content);
                return _result;
            }
            catch
            {
                goto start;
            }
        }
    }

    /// <summary>
    /// ArkHelper更新
    /// </summary>
    public static class Update
    {
        public static void Search()
        {
            Task updateTask = Task.Run(() =>
            {
                //调用API 查找版本信息
                var client = new RestClient("https://api.github.com/repos/ArkHelper/ArkHelper2.0/releases/latest");
                var request = new RestRequest { Method = Method.Get };
                var response = client.Execute(request);
                var _result = JsonConvert.DeserializeObject<JObject>(response.Content);

                //在json中取得最新版本号
                int tag = Convert.ToInt32(_result["tag_name"].ToString().Replace("v", "").Replace(".", ""));
                if (tag > Convert.ToInt32(Version.tag.Replace("v", "").Replace(".", "")))
                {
                    //下载版本更新描述
                    for (int i = 0; ; i++)
                    {
                        if (_result["assets"][i]["name"].ToString() == "config.json")
                        {

                            break;
                        }
                    }
                }


                //var URL = SearchURL(result, "test.txt");
                //MessageBox.Show(.ToString());
            });

            //在GitHub release中找到指定文件名的browser_download_url
            string SearchURL(JObject keys, string name)
            {
                for (int i = 0; ; i++)
                {
                    try
                    {
                        if (keys["assets"][i]["name"].ToString() == name)
                        {
                            return keys["assets"][i]["browser_download_url"].ToString();
                        }
                    }
                    catch (System.NullReferenceException)
                    {
                        return null;
                    }
                }
            }


        }
    }

    #region
    namespace thing
    {
        /// <summary>
        /// 杂项
        /// </summary>
        public static class Thing
        {

        }

        /// <summary>
        /// 值转换器：string→MD2packiconkind
        /// </summary>
        public class TagtoPackIconkindConverter : System.Windows.Data.IValueConverter
        {
            private PackIconKind packIconKind = PackIconKind.Clock;
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (Enum.TryParse(value.ToString(), out PackIconKind pack))
                {
                    return pack;
                }

                return packIconKind;
            }
            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        /*/// <summary>
        /// 
        /// </summary>
        public class TouchScrolling : DependencyObject
        {
            public static bool GetIsEnabled(DependencyObject obj)
            {
                return (bool)obj.GetValue(IsEnabledProperty);
            }

            public static void SetIsEnabled(DependencyObject obj, bool value)
            {
                obj.SetValue(IsEnabledProperty, value);
            }

            public bool IsEnabled
            {
                get { return (bool)GetValue(IsEnabledProperty); }
                set { SetValue(IsEnabledProperty, value); }
            }

            public static readonly DependencyProperty IsEnabledProperty =
                DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(TouchScrolling), new UIPropertyMetadata(false, IsEnabledChanged));

            static Dictionary<object, MouseCapture> _captures = new Dictionary<object, MouseCapture>();

            static void IsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            {
                if (!(d is ScrollViewer target)) return;

                if ((bool)e.NewValue)
                {
                    target.Loaded += target_Loaded;
                }
                else
                {
                    target_Unloaded(target, new RoutedEventArgs());
                }
            }

            static void target_Unloaded(object sender, RoutedEventArgs e)
            {
                System.Diagnostics.Debug.WriteLine("Target Unloaded");

                var target = sender as ScrollViewer;
                if (target == null) return;

                _captures.Remove(sender);

                target.Loaded -= target_Loaded;
                target.Unloaded -= target_Unloaded;
                target.PreviewMouseLeftButtonDown -= target_PreviewMouseLeftButtonDown;
                target.PreviewMouseMove -= target_PreviewMouseMove;

                target.PreviewMouseLeftButtonUp -= target_PreviewMouseLeftButtonUp;
            }

            static void target_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
            {
                var target = sender as ScrollViewer;
                if (target == null) return;

                _captures[sender] = new MouseCapture
                {
                    VerticalOffset = target.VerticalOffset,
                    HorticalOffset = target.HorizontalOffset,
                    Point = e.GetPosition(target),
                };
            }

            static void target_Loaded(object sender, RoutedEventArgs e)
            {
                var target = sender as ScrollViewer;
                if (target == null) return;

                System.Diagnostics.Debug.WriteLine("Target Loaded");

                target.Unloaded += target_Unloaded;
                target.PreviewMouseLeftButtonDown += target_PreviewMouseLeftButtonDown;
                target.PreviewMouseMove += target_PreviewMouseMove;

                target.PreviewMouseLeftButtonUp += target_PreviewMouseLeftButtonUp;
            }

            static void target_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
            {
                var target = sender as ScrollViewer;
                if (target == null) return;

                target.ReleaseMouseCapture();
            }

            static void target_PreviewMouseMove(object sender, MouseEventArgs e)
            {
                if (!_captures.ContainsKey(sender)) return;

                if (e.LeftButton != MouseButtonState.Pressed)
                {
                    _captures.Remove(sender);
                    return;
                }

                var target = sender as ScrollViewer;
                if (target == null) return;

                var capture = _captures[sender];

                var point = e.GetPosition(target);

                var dy = point.Y - capture.Point.Y;
                var dx = point.X - capture.Point.X;

                if (Math.Abs(dy) > 5)
                {
                    target.CaptureMouse();
                }

                if (Math.Abs(dx) > 5)
                {
                    target.CaptureMouse();
                }

                target.ScrollToVerticalOffset(capture.VerticalOffset - dy);
                target.ScrollToHorizontalOffset(capture.HorticalOffset - dx);
            }

            internal class MouseCapture
            {
                public Double VerticalOffset { get; set; }
                public Double HorticalOffset { get; set; }


                public System.Windows.Point Point { get; set; }
            }
        }*/

    }
    #endregion
}