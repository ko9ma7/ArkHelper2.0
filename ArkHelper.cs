using ArkHelper.Pages.OtherList;
using MaterialDesignThemes.Wpf;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Win32;
using OpenCvSharp;
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
using System.Security.Policy;
using System.Security.RightsManagement;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Markup;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Appointments;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Point = System.Drawing.Point;

namespace ArkHelper
{
    /// <summary>
    /// 版本信息
    /// </summary>
    public static class Version
    {
        public class Data
        {
            public enum VersionType
            {
                realese,
                beta,
            }
            public string Version { get; }
            public int VersionNumber { get; }
            public string URL { get; set; }
            public bool Necessary { get; }
            public VersionType Type { get; }
            public Data(string ver, string url, bool necessary, VersionType type)
            {
                Version = ver;
                URL = url;
                Necessary = necessary;
                Type = type;

                string[] strings = ver.Split('.');
                int getInt(int n)
                {
                    return Convert.ToInt16(strings[n]);
                }

                VersionNumber = 1000 * getInt(0) + 100 * getInt(1) + 10 * getInt(2) + 1 * getInt(3);
            }
        }

        public static Data Current = new Data("2.0.0.0", "local", false, Data.VersionType.realese);

        /// <summary>
        /// 更新
        /// </summary>
        public class Update
        {
            /// <summary>
            /// 查找最新的发行版
            /// </summary>
            /// <returns></returns>
            public static Data SearchNewestRelease()
            {
                try
                {
                    //API
                    var NewVersionData = Net.GetFromApi("https://api.github.com/repos/ArkHelper/ArkHelper2.0/releases/latest");
                    var versionNum = NewVersionData.GetProperty("tag_name").GetString();
                    var assets = NewVersionData.GetProperty("assets").EnumerateArray();
                    var necessary = NewVersionData.GetProperty("body").GetString().Contains("[NECESSARY]");
                    string search(string name, bool fuzzy)
                    {
                        foreach (var asset in assets)
                        {
                            string assetName = asset.GetProperty("name").GetString();
                            string assetURL = asset.GetProperty("browser_download_url").GetString();

                            if (assetName == name && !fuzzy)
                            {
                                return assetURL;
                            }
                            if (assetName.Contains("DownloaderComments") && fuzzy)
                            {

                                return assetURL;
                            }
                        }
                        return "null";
                    }

                    var url = search("ArkHelper.zip", false);

                    if (search("DownloaderComments", true) != "null")
                    {
                        string add = Address.Cache.update + "\\config.json";
                        Net.DownloadFile(search("DownloaderComments", true), add);
                        var dc = JsonSerializer.Deserialize<JsonElement>(File.ReadAllText(Address.config));
                        File.Delete(add);
                        if (dc.TryGetProperty("redirectURL", out var red))
                        {
                            url = red.GetString();
                        }
                    }

                    var ret = new Data(versionNum, "", necessary, Data.VersionType.realese);
                    ret.URL = url;
                    return ret;
                }
                catch
                {
                    return null;
                }

                /*new ToastContentBuilder()
                    .AddArgument("kind", "Update")
                    .AddArgument("UpdateIsNecessary", necessary.ToString())
                    .AddArgument("url", url)
                    .AddText("提示：ArkHelper有更新")
                    .AddText("版本：" + ver)
                    //.AddText(necessary ? "正在更新中" : "点击本消息下载更新")
                    .AddText("点击获取更新")
                    .Show(); //通知*/
            }

            public static void Apply(string url)
            {
                string address = Address.akh + "\\ArkHelper.zip";
                Net.DownloadFile(url, address);//下载更新包
                new ToastContentBuilder()
                        .AddArgument("kind", "UpdateMessage")
                        .AddText("提示")
                        .AddText("ArkHelper更新完成")
                        .AddText("正在启动中")
                        .Show(); //通知*/
                App.ExitApp();
            }
        }
    }

    /// <summary>
    /// ArkHelper数据
    /// </summary>
    public static class ArkHelperDataStandard
    {
        /// <summary>
        /// 截图名称获取
        /// </summary>
        /// <returns>四位数年+两位数月+两位数日+两位数时分秒+三位毫秒+“.png”</returns>
        public static string Screenshot => DateTime.Now.ToString("yyyyMMddHHmmssfff") + @".png";
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
            official_communication,
            neteaseMusic
        }

        /// <summary>
        /// ArkHelperArg
        /// </summary>
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
            /// <summary>
            /// ArkHelperArg类型
            /// </summary>
            public enum ArgKind
            {
                Navigate,
                ActiveFunc,
                Shut,
                none
            }
        }

        /// <summary>
        /// 获取星期在中文中对应的下标
        /// </summary>
        /// <param name="week"></param>
        /// <returns>对于星期日，返回6；否则返回x（其中x为“星期”后的数字）</returns>
        public static int GetWeekSubInChinese(System.DayOfWeek week)
        {
            var _wek = (int)week - 1;
            if (_wek < 0) { _wek += 7; }

            return _wek;
        }

        /// <summary>
        /// 根据日期和时间获取datetime
        /// </summary>
        public static DateTime GetDateTimeFromDateAndTime(DateTime date, DateTime time)
        {
            return new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second);
        }

        #region 配置数据
        public class Data
        {
            public Simulator simulator { get; set; } = new Simulator();
            public class Simulator
            {
                public Custom custom { get; set; } = new Custom();
                public class Custom
                {
                    public bool status { get; set; } = false;
                    public int port { get; set; } = 0;
                    public string im { get; set; } = "";
                }
            }

            public SCHT scht { get; set; } = new SCHT();
            public class SCHT
            {
                public bool status { get; set; } = false;
                public SCHTData data { get; set; } = new SCHTData();
                public class SCHTData
                {
                    public Cp first { get; set; } = new Cp();
                    public Cp second { get; set; } = new Cp();
                    public class Cp
                    {
                        public string unit { get; set; } = "LS";
                    }

                    public Ann ann { get; set; } = new Ann();
                    public class Ann
                    {
                        public bool status { get; set; } = false;
                        public string select { get; set; } = "TT";
                        public bool customTime { get; set; } = false;
                        public int[] time { get; set; } = new int[7] { 0, 0, 0, 0, 0, 0, 0 }; //周一为每周的第一天
                    }

                    public Server server { get; set; } = new Server();
                    public class Server
                    {
                        public string id { get; set; } = "CO";
                    }
                }
                public bool showHelper { get; set; } = false;
                public bool showGuide { get; set; } = true;
                public Ct ct { get; set; } = new Ct();
                public class Ct
                {
                    public bool status { get; set; } = false;
                    public bool[] weekFliter { get; set; } = new bool[7] { true, true, true, true, true, true, true };
                    public List<DateTime> times { get; set; } = new List<DateTime>()
                    {
                        new DateTime(2000, 1, 1, 7, 58, 0),
                        new DateTime(2000, 1, 1, 19, 58, 0)
                    };
                    public List<DateTime> forceTimes { get; set; } = new List<DateTime>();
                }
            }

            public Message message { get; set; } = new Message();
            public class Message
            {
                public bool status { get; set; } = false;
            }

            public ArkHelper arkHelper { get; set; } = new ArkHelper();
            public class ArkHelper
            {
                public bool pure { get; set; } = false;
                public bool debug { get; set; } = false;
            }
        }
        #endregion
    }

    /// <summary>
    /// Akhcmd
    /// </summary>
    public class AKHcmd
    {
        public string ADBcmd { get; set; } = null;
        private string Discribe { get; set; } = "";
        public string OutputText { get; } = "";
        public int WaitTime { get; set; } = 0;
        public int ForTimes { get; set; } = 0;

        public static Dictionary<string, AKHcmd> FormatAKHcmd = new Dictionary<string, AKHcmd>
            {
                {"zhongduan",new AKHcmd(1090,185,waitTime:2) },
                {"zhongduan_menu_zhongduan",new AKHcmd(90,757,waitTime:1) },
                {"ziyuanshouji",new AKHcmd(806,756,waitTime:1) },
                {"menu",new AKHcmd(300,42,waitTime:1) },
                {"menu_home",new AKHcmd(103,194,waitTime:2) }
            };

        public AKHcmd(string body, string outputText = "", int waitTime = 0, int forTimes = 1)
        {
            if (body != null)
            {
                //解析
                if (body.Contains("####") && body.Contains("#;")) { waitTime = Convert.ToInt32(body.Substring(body.IndexOf("####") + 4, body.IndexOf("#;") - body.IndexOf("####") - 4)); }
                if (body.Contains("$$$$") && body.Contains("$;")) { forTimes = Convert.ToInt32(body.Substring(body.IndexOf("$$$$") + 4, body.IndexOf("$;") - body.IndexOf("$$$$") - 4)); }
                if (body.Contains("&&&&") && body.Contains("&;")) { outputText = body.Substring(body.IndexOf("&&&&") + 4, body.IndexOf("&;") - body.IndexOf("&&&&") - 4); }
                ADBcmd = body.Replace("####" + waitTime + "#;", "").Replace("$$$$" + forTimes + "$;", "").Replace("&&&&" + outputText + "&;", "");
            }
            OutputText = outputText;
            WaitTime = waitTime;
            ForTimes = forTimes;
        }
        public AKHcmd(int x, int y, string outputText = "", int waitTime = 0, int forTimes = 1)
        {
            ADBcmd = "shell input tap " + x + " " + y;

            OutputText = outputText;
            WaitTime = waitTime;
            ForTimes = forTimes;
        }

        public void RunCmd()
        {
            for (int i = 1; i <= ForTimes; i++)
            {
                if (ADBcmd != null) ADB.CMD(ADBcmd);
                Thread.Sleep(WaitTime * 1000);
            }
        }

        public override string ToString()
        {
            string _ret = ADBcmd;
            if (WaitTime != 0) _ret = _ret + "####" + WaitTime + "#;";
            if (ForTimes > 0) _ret = _ret + "$$$$" + ForTimes + "$;";
            if (OutputText != "") _ret = _ret + "&&&&" + OutputText + "&;";
            return _ret;
        }

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
            //cmd
            process.StartInfo.Arguments = cmd;
            if (true) Output.Log(cmd, "ADB");

            //启动命令并读取结果
            string end = "";
            if (!cmd.Contains("connect") && ConnectedInfo == null)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    //WithSystem.Message("模拟器连接断开", "请启动或重启模拟器");
                });
                end = "[Bad Connect]";
                if (true) Output.Log("=>" + end, "ADB");
                throw new BadConnectException();
            }
            else
            {
                process.Start();
                end = process.StandardOutput.ReadToEnd();
                //log结果
                if (true) Output.Log("=>" + end.Replace("\n", "[linebreak]").Replace("\r", ""), "ADB");
                //等待退出
                process.WaitForExit();
                //log退出
                if (true) Output.Log("=>" + "Exited", "ADB");
            }

            //返回结果
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
                {
                    ConnectedInfo = null;
                    Output.Log("Simulator Lost Connection");
                }
                else
                    return;
            }

            //尝试遍历寻找在线的模拟器
            PinnedData.Simulator.SimuInfo ConnectThis = new PinnedData.Simulator.SimuInfo();
            if (App.Data.simulator.custom.status)
            {
                if (Process.GetProcessesByName(App.Data.simulator.custom.im).Length != 0)
                    ConnectThis = new PinnedData.Simulator.SimuInfo("custom", "自定义", App.Data.simulator.custom.port, App.Data.simulator.custom.im);
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
            Tap(point.X, point.Y);
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
        /// 简单截图
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

        /// <summary>
        /// 获取当前运行的游戏类型
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentGameKind()
        {
            string r = ADB.CMD("shell \"dumpsys window | grep mCurrentFocus\"");
            string server = "CO";
            if (r.Contains("com.hypergryph.arknights")) { server = "CO"; }
            if (r.Contains("com.hypergryph.arknights.bilibili")) { server = "CB"; }
            if (r.Contains("com.YoStarJP.Arknights")) { server = "JP"; }
            if (r.Contains("com.YoStarEN.Arknights")) { server = "EN"; }
            if (r.Contains("com.YoStarKR.Arknights")) { server = "KR"; }
            if (r.Contains("tw.txwy.and.arknights")) { server = "TW"; }
            return server;
        }

        /// <summary>
        /// 获取游戏类型对应的包名
        /// </summary>
        /// <returns></returns>
        public static string GetGamePackageName(string server)
        {
            return PinnedData.Server.dataSheet.Select("id = '" + server + "'")[0][3].ToString();
        }

        /// <summary>
        /// 安装应用
        /// </summary>
        /// <param name="packAddress">apk包的地址</param>
        public static void Install(string packAddress)
        {
            ADB.CMD("install " + packAddress);
        }

        /// <summary>
        /// 从内存卡安装应用
        /// </summary>
        /// <param name="packAddress">apk包的地址</param>
        public static void InstallFromLocal(string packAddress)
        {
            ADB.CMD("shell pm install -r " + packAddress);
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="url">url</param>
        /// <param name="address">绝对路径</param>
        public static void DownloadFile(string url, string address)
        {
            ADB.CMD("shell wget " + url + " -O " + address);
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="address">绝对路径</param>
        public static void DeleteFile(string address)
        {
            ADB.CMD("shell rm " + address);
        }

        /// <summary>
        /// 等待模拟器运行
        /// </summary>
        public static void WaitingSimulator()
        {
            for (; ; )
            {
                while (ADB.ConnectedInfo == null)
                    Thread.Sleep(4000);
                try
                {
                    using (var testOK = new ADB.Screenshot())
                    {
                        testOK.CheckIsAvailable();
                    }
                }
                catch
                {
                    continue;
                }
                Thread.Sleep(1000);
                break;
            }
        }

        public class Screenshot : IDisposable
        {
            public class ScreenshotNotAvailableException : NullReferenceException
            {
            }
            private string Location { get; set; }
            private Bitmap ImgBitmap { get; set; }
            public Screenshot()
            {
                string name = ArkHelperDataStandard.Screenshot;
                string address = Address.Cache.main;
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
                return ColorPick(point.X, point.Y);
            }
            #endregion

            public List<Point> PicToPoint(string smallimg, double errorCon = 0.7, int errorRange = 16, int num = 50, double opencv_errorCon = 0.8)
            {
                if (!File.Exists(smallimg)) { return new List<Point>(); }

                /*//初始化图像类
                InitBitmap();
                var smallBM = new Bitmap(smallimg);
                return PictureProcess.PicToPoint.GetPointUsingNative(this.ImgBitmap, smallBM, errorCon, errorRange, num);*/
                return PictureProcess.PicToPoint.GetPointUsingOpenCV(this.Location, smallimg, errorCon: opencv_errorCon);
            }
            private void InitBitmap()
            {
                if (ImgBitmap == null)
                    ImgBitmap = new Bitmap(Location);
                else
                    return;
            }

            public void CheckIsAvailable()
            {
                if (File.Exists(Location)) { return; } else { throw new ScreenshotNotAvailableException(); }
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
            /// <summary>
            /// 执行与释放或重置非托管资源关联的应用程序定义的任务。
            /// </summary>
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

        #region 报故
        public class BadConnectException : Exception
        {
            public BadConnectException() : base("丢失模拟器连接") { }
        }
        #endregion
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
            string _name = ADB.GetScreenshot(Address.Cache.main, ArkHelperDataStandard.Screenshot);
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
            /// <returns>所有匹配矩形的中心点在大图中的坐标</returns>
            public static List<Point> GetPointUsingNative(Bitmap bigBM, Bitmap smallBM, double errorCon = 0.6, int errorRange = 15, int num = 100)
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
                    List<Point> _points = new List<Point>();
                    foreach (Point _point in FinallyPoints)
                    {
                        _points.Add(_point);
                    }
                    bigBM.Dispose(); smallBM.Dispose();
                    return _points;
                }
                else
                {
                    bigBM.Dispose(); smallBM.Dispose();
                    return new List<Point>();
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
            public static List<Point> GetPointUsingNative(string bigpic, string smallpic, double errorCon = 0.6, int errorRange = 15, int num = 100)
            {
                if (!File.Exists(bigpic)) { return new List<Point>(); }
                if (!File.Exists(smallpic)) { return new List<Point>(); }

                //初始化图像类
                var bigBM = new Bitmap(bigpic);
                var smallBM = new Bitmap(smallpic);

                return GetPointUsingNative(bigBM, smallBM, errorCon, errorRange, num);
            }

            /// <summary>
            /// 获取
            /// </summary>
            /// <param name="bigPicLocation">大图地址</param>
            /// <param name="smallPicLocation">小图地址</param>
            /// <returns></returns>
            public static List<Point> GetPointUsingOpenCV(string bigPicLocation, string smallPicLocation, double errorCon = 0.8)
            {
                List<Point> @return = new List<Point>();

                using (Mat bigPic = new Mat(bigPicLocation))//大图
                using (Mat smallPic = new Mat(smallPicLocation))//小图
                using (Mat res = new Mat(bigPic.Rows - smallPic.Rows + 1, bigPic.Cols - smallPic.Cols + 1, MatType.CV_32FC1))
                {
                    //灰度化
                    Mat gref = bigPic.CvtColor(ColorConversionCodes.BGR2GRAY);
                    Mat gtpl = smallPic.CvtColor(ColorConversionCodes.BGR2GRAY);

                    Cv2.MatchTemplate(gref, gtpl, res, TemplateMatchModes.CCoeffNormed);
                    Cv2.Threshold(res, res, 0.8, 1.0, ThresholdTypes.Tozero);

                    while (true)
                    {
                        double minval, maxval, threshold = errorCon;
                        OpenCvSharp.Point minloc, maxloc;
                        Cv2.MinMaxLoc(res, out minval, out maxval, out minloc, out maxloc);

                        if (maxval >= threshold)
                        {
                            Point newPoint = new Point(maxloc.X + smallPic.Width / 2, maxloc.Y + smallPic.Height / 2);
                            @return.Add(newPoint);

                            //去重
                            OpenCvSharp.Rect outRect;
                            Cv2.FloodFill(res, maxloc, new Scalar(0), out outRect, new Scalar(0.1), new Scalar(1.0));
                        }
                        else
                            break;
                    }
                }
                return @return;
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
        /// 升级包文件
        /// </summary>
        public readonly static string update = data + @"\updatePack";
        public static class Cache
        {
            /// <summary>
            /// cache根目录
            /// </summary>
            public readonly static string main = programData + @"\cache";
            public readonly static string message = main + @"\message";
            public readonly static string update = main + @"\update";
        }
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
            void CD(string add)
            {
                try
                {
                    Directory.CreateDirectory(add);
                }
                catch
                {

                }
            }
            //创建目录
            CD(programData);

            CD(data);
            CD(log);
            CD(dataExternal);
            CD(Cache.main);

            CD(Screenshot.main);
            CD(Screenshot.MB);
            CD(Screenshot.SCHT);

            CD(Cache.message);
            CD(Cache.update);
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

        private static void Text(string content, string file)
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
            if (!App.Data.arkHelper.debug) return;
            Text(DateTime.Now.ToString("s")
                + " "
                + "[" + module + "]"
                + " "
                + "[" + infokind.ToString() + "]"
                + " "
                + content
                , ArkHelperDataStandard.Log
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
    public static class Net
    {
        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="url">下载地址</param>
        /// <param name="address">下载位置（绝对路径）</param>
        public static FileInfo DownloadFile(string url, string address)
        {
            string cache = address + @".cache";
            if (!File.Exists(address))
            {
                using (var web = new WebClient())
                {
                    web.DownloadFile(url, cache);
                }
                Directory.Move(cache, address);

                if (File.Exists(cache)) { File.Delete(cache); }
            }
            return new FileInfo(address);
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