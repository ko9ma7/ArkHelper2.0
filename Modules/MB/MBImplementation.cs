using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Services.Store;
using static ArkHelper.MB;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using static ArkHelper.Output;
using System.Drawing;
using System.IO;

namespace ArkHelper.Modules.MB
{
    public static class Tool
    {

    }

    public class MBImplementationResult
    {
        protected MBImplementationResult()
        {

        }
    }
    public class MBImplementation
    {
        public MBImplementation() { }

        public event ArkHelperDataStandard.ArkHelperMessage MessageUpdated;
        protected virtual void OnUpdate(string text, Output.InfoKind infoKind)
        {
            MessageUpdated?.Invoke(text, infoKind);
        }

        public class NextArg
        {
            public int AlreadyTime { get; set; }
        }
        public delegate void NextHandler(NextArg arg);
        public event NextHandler MoveToNext;
        protected virtual void OnMoveToNext(NextArg arg)
        {
            MoveToNext.Invoke(arg);
        }
    }

    public class MBCoreResult : MBImplementationResult
    {
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
        public int Time { get; }
        public ResultType Type { get; }
        public MBCoreResult(ResultType resultType, int time = 0)
        {
            void log(string content, InfoKind infoKind = InfoKind.Infomational)
            {
                Output.Log(content, "MBCore", infoKind);
            }
            Time = time;
            Type = resultType;
            log("Code=" + resultType
                , resultType == ResultType.Succeed ?
                InfoKind.Infomational :
                InfoKind.Error);
            log("--- END ---");
        }
    }
    public class MBCore : MBImplementation
    {
        #region 接口
        protected override void OnUpdate(string text, Output.InfoKind infoKind)
        {
            base.OnUpdate(text, infoKind);
        }
        protected override void OnMoveToNext(NextArg arg)
        {
            base.OnMoveToNext(arg);
        }
        /// <summary>
        /// MBCore模式
        /// </summary>
        public enum ModeType
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
        #endregion

        #region tool
        /// <summary>
        /// MB日志输出
        /// </summary>
        /// <param name="content">日志内容</param>
        /// <param name="infoKind">日志重要程度</param>
        private static void Logger(string content, Output.InfoKind infoKind = Output.InfoKind.Infomational)
        {
            Output.Log(content, "MBCore", infoKind);
        }
        private void Info(string text, Output.InfoKind infoKind = InfoKind.Infomational) => OnUpdate(text, infoKind);
        private void Next() => OnMoveToNext(new NextArg() { AlreadyTime = alreadyTime });
        #endregion

        #region 逻辑
        private ModeType Mode { get; }
        private int alreadyTime;
        private int Time { get; }
        private bool AllowToRecoverSantiy { get; }
        private int Ann_cardToUse { get; }
        public MBCoreResult Run()
        {
            //ann_cardToUse = 1;//debug
            int battleKind = 0;//0：普通，1：剿灭

            int firstSleepTime = 35000;//进入作战到开始检测等待时间

            alreadyTime = 0;

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
            Logger("mode=" + Mode + "," + "time=" + Time + "," + "ann_cardToUse=" + Ann_cardToUse);
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
                        return new MBCoreResult(MBCoreResult.ResultType.Error_NotDetectACheckpoint);
                    }
                }
                Logger("battleKind=" + battleKind);

                //检测代理指挥是否可用
                if (screenshot.PicToPoint(Address.res + "\\pic\\UI\\AutoDeployLocked.png").Count != 0)
                {
                    return new MBCoreResult(MBCoreResult.ResultType.Error_AutoDeployNotAvailable);
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
            if (Mode == ModeType.time && alreadyTime >= Time) { goto MBend; } //检查次数
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
                        if (ann_cardAlreadyUsed < Ann_cardToUse)
                        {
                            Info("指令：启用全权委托");
                            ADB.Tap(1146, 666);
                            WithSystem.Wait(500);
                            UseCard();
                        }
                        else
                        {
                            NotUseCard();
                        }
                        break;
                    case 1:
                        if (ann_cardAlreadyUsed >= Ann_cardToUse)
                        {
                            Info("指令：禁用全权委托");
                            ADB.Tap(1146, 666);
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
            Logger("(" + (alreadyTime + 1) + "/" + Time + ")");

        //开始行动
        beginTask:;
            Info("指令：开始行动");
            ADB.Tap(1266, 753);
            WithSystem.Wait(3500);

            using (ADB.Screenshot screenshot = new ADB.Screenshot())
            {
                //检查是否有回理智界面
                if (screenshot.PicToPoint(Address.res + "\\pic\\UI\\RecoverSanitySymbol.png").Count != 0)
                {
                    Info("剩余理智不足以指挥本次作战");
                    //理智模式直接退出
                    if (Mode == ModeType.san)
                    {
                        ADB.Tap(871, 651); //点叉
                        goto MBend;
                    }
                    //次数模式恢复理智
                    if (Mode == ModeType.time)
                    {
                        if (!AllowToRecoverSantiy) goto MBend;
                        Info("指令：使用理智恢复物恢复理智");
                        ADB.Tap(1224, 648);//点对号
                        WithSystem.Wait(3000);

                        goto beginTask;
                    }
                }
            }
            Info("指令：开始行动");
            ADB.Tap(1258, 717);//开始行动（红）

            //已进本，等待出本
            Info("代理指挥作战运行中");
            WithSystem.Wait(firstSleepTime);
            //循环检查是否在本里
            for (; ; )
            {
                WithSystem.Wait(5000);
                using (var sc = new ADB.Screenshot())
                {
                    if (sc.ColorPick(77, 70) != "#8C8C8C" && sc.ColorPick(1341, 62) != "#FFFFFF")
                    {
                        break;
                    }
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
                    Info("指令：退出作战");
                    ADB.Tap(1204, 290); //点击空白
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
            return new MBCoreResult(MBCoreResult.ResultType.Succeed, time: alreadyTime);
        }
        #endregion

        #region 构造
        public MBCore(ModeType mode, int time = -1, bool allowToRecoverSantiy = false, int ann_cardToUse = 0)
        {
            Mode = mode;
            Time = time;
            AllowToRecoverSantiy = allowToRecoverSantiy;
            Ann_cardToUse = ann_cardToUse;
        }
        #endregion
    }

    public class SXYSResult : MBImplementationResult
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
            /// 上次未结束
            /// </summary>
            Error_LastTimeNotEnd,
            /// <summary>
            /// 未知错误
            /// </summary>
            Error_UndefinedError
        }
        public ResultType Type { get; }
        public int Time { get; }

        public SXYSResult(ResultType resultType, int time = 0)
        {
            void log(string content, InfoKind infoKind = InfoKind.Infomational)
            {
                Output.Log(content, "SXYS", infoKind);
            }
            Time = time;
            Type = resultType;
            log("Code=" + resultType
                , resultType == ResultType.Succeed ?
                InfoKind.Infomational :
                InfoKind.Error);
            log("--- END ---");
        }
    }
    public class SXYS : MBImplementation
    {
        #region 接口
        protected override void OnUpdate(string text, Output.InfoKind infoKind)
        {
            base.OnUpdate(text, infoKind);
        }
        protected override void OnMoveToNext(NextArg arg)
        {
            base.OnMoveToNext(arg);
        }
        #endregion

        #region tool
        /// <summary>
        /// 日志输出
        /// </summary>
        /// <param name="content">日志内容</param>
        /// <param name="infoKind">日志重要程度</param>
        private static void Logger(string content, Output.InfoKind infoKind = Output.InfoKind.Infomational)
        {
            Output.Log(content, "SXYS", infoKind);
        }
        private void Info(string text, Output.InfoKind infoKind = InfoKind.Infomational) => OnUpdate(text, infoKind);
        private void Next() => OnMoveToNext(new NextArg() { AlreadyTime = exetimes });
        #endregion

        #region 逻辑
        private int exetimes;
        private int Time { get; }
        public SXYSResult Run()
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


            Logger("--- 生息演算 START ---");
            Info("连续生息演算开始");

            var scs = new ADB.Screenshot();
            if (!isHave(address("availableSymbol"), screenshot: scs)) return new SXYSResult(SXYSResult.ResultType.Error_NotDetectACheckpoint);
            if (isHave(address("cancel"), screenshot: scs)) return new SXYSResult(SXYSResult.ResultType.Error_LastTimeNotEnd);
            scs.Dispose();
            exetimes = 0;

        SXYS_start:;

            ADB.Tap(1358, 740);
            Info("开始演算");
            sleep(2);

            ADB.Tap(1301, 54);
            Info("选择干员");
            sleep(2);


            ADB.Tap(1301, 54);
            Info("快捷编队");
            sleep(1);

            ADB.Tap(530, 769);
            Info("清空选择");
            sleep(1);

            ADB.Tap(661, 149);
            Info("干员1");
            sleep(1);

            ADB.Tap(1143, 762);
            Info("确认");
            sleep(2);

            ADB.Tap(1143, 762);
            Info("补充");
            sleep(4);

            ADB.Tap(1143, 762);
            Info("确认");
            sleep(10);

            for (; ; ) //天数循环
            {
                if (isHave(address("news"))) sleep(18);

                ADB.Tap(1369, 761);
                Info("关闭日报");
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
                    Info("缩小地图");
                    sleep(2);

                    var portPoint = getPoint(Address.res + "\\pic\\SXYS\\port.png", 0.9);
                    portPoint.RemoveAll(t => t.Y < 200);

                    foreach (var port in portPoint)//port循环
                    {
                        ADB.Tap(44, 759);
                        Info("缩小地图");
                        sleep(2);

                        ADB.Tap(port);
                        Info("点击节点");
                        sleep(3);

                        var a = ADB.WaitOnePicture(new List<string>()
                        {
                            address("huntPlace"),
                            address("resourcePlace"),
                        }, 2, 0.7);
                        if (!a.IsEmpty)
                        {
                            ADB.Tap(a);
                            Info("点击节点");
                            sleep(2.5);

                            break;
                        }
                    }

                    ADB.Tap(1279, 695);
                    Info("开始行动");
                    sleep(2.5);

                    judge++;

                    ADB.Tap(1263, 744);
                    Info("准备");
                    sleep(2);

                    ADB.Tap(1277, 356);
                    Info("开始行动");
                    sleep(3);

                    ADB.Tap(1411, 550);
                    Info("确认");
                    sleep(10);

                    ADB.WaitPicture(address("pack"), 30, 0.7);
                    sleep(2);

                    ADB.Tap(84, 58);
                    Info("退出");
                    sleep(2);

                    ADB.Tap(1161, 473);
                    Info("确认离开");
                    sleep(6);

                    ADB.Tap(1258, 283);
                    Info("");
                    sleep(5);
                }

                ADB.Tap(1313, 60);
                Info("进入下一天");
                sleep(12);
            }

            ADB.Tap(42, 46);
            Info("结束");
            sleep(2);

            ADB.Tap(938, 743);
            Info("放弃");
            sleep(1);

            ADB.Tap(977, 562);
            Info("确定");
            sleep(3.5);

            ADB.Tap(1302, 701);
            Info("确定");
            sleep(1.5);

            ADB.Tap(1302, 701);
            Info("确定");
            sleep(1.5);

            ADB.Tap(1302, 701);
            Info("确定");
            sleep(3);

            exetimes++;
            Next();
            if (exetimes != Time) goto SXYS_start;

            return new SXYSResult(SXYSResult.ResultType.Succeed);
        }
        #endregion

        #region 构造
        public SXYS(int time = -1)
        {
            Time = time;
        }
        #endregion
    }
}
