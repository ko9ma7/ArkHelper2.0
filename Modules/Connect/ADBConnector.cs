using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Forms.VisualStyles;
using ArkHelper.Modules.MB;
using OpenCvSharp;
using OpenCvSharp.Internal.Vectors;

namespace ArkHelper.Modules.Connect
{
    /// <summary>
    /// 负责监控ADB服务。
    /// </summary>
    static class ADBServerStatus
    {
        public static event EventHandler<string> ServerStarted;
        public static event EventHandler<string> ServerKilled;
        public static void KillServer()
        {
            ADBInteraction.GetOutput("kill-server");
        }
        public static void StartServer()
        {
            ADBInteraction.GetOutput("start-server");
        }
    }

    /// <summary>
    /// 对本地回环和端口发起连接请求。
    /// </summary>
    static class Connector
    {
        public static event EventHandler<bool> IPConnectionChange;
        private static int loopInterval;
        private static readonly Thread thread;
        static Connector()
        {
            thread = new Thread(new ThreadStart(Connect));
        }
        public static void Connect()
        {
            while (true)
            {
                if (!ADB.CheckIfADBUsing())
                {
                    foreach (var info in ConnectionInfo.Connections.Keys.ToArray())
                        if (Process.GetProcessesByName(info.IM).Length != 0)
                        {
                            var connectResult = false;
                            for (int i = 0; i < 2; i++)
                            {
                                connectResult = connectResult || ADBInteraction.GetOutput("connect 127.0.0.1:" + info.Port, 2).Contains("connected");
                            }
                            if (ConnectionInfo.Connections[info] != connectResult)
                            {
                                ConnectionInfo.Connections[info] = connectResult;
                                IPConnectionChange?.Invoke(info, true);

                                Output.Log("Connection freshed:" + info.ToString() + ",turned into " + connectResult, "ADB");
                            }
                        }
                        else
                        {
                            var result = false;
                            if (ConnectionInfo.Connections[info] != result)
                            {
                                ConnectionInfo.Connections[info] = result;
                                IPConnectionChange?.Invoke(info, false);

                                Output.Log("Connection freshed:" + info.ToString() + ",turned into " + result, "ADB");
                            }
                        }
                }
                Thread.Sleep(loopInterval);
            }
        }
        public static void Start(int interval = 2000)
        {
            loopInterval = interval;
            thread.Start();
            Output.Log($"Connector thread{thread.ManagedThreadId} started", "ADB");
        }
    }

    /// <summary>
    /// 负责监视ADB连接设备改变。
    /// </summary>
    static class DevicesWatcher
    {
        private static string oldOutput = "";

        private static readonly Thread thread;

        private static int refreshInterval;

        static DevicesWatcher()
        {
            thread = new Thread(new ThreadStart(Refresh));
        }

        public static void Refresh()
        {
            while (true)
            {
                if (ConnectionInfo.Connections.Values.Contains(true)
                    && !ADB.CheckIfADBUsing())
                {
                    string output = ADBInteraction.GetOutput("devices");

                    if (output != oldOutput)
                    {
                        List<string> list = new List<string>();

                        oldOutput = output;

                        foreach (string str in output.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            if (str.StartsWith("List") || !str.Contains("device"))
                            {
                                continue;
                            }

                            list.Add(str.Substring(0, str.IndexOf("\t")));
                        }

                        ConnectionInfo.Devices = list;
                    }
                }
                Thread.Sleep(refreshInterval);
            }
        }

        public static void Start(int interval = 2000)
        {
            refreshInterval = interval;
            thread.Start();
            Output.Log($"DeviceWatcher thread{thread.ManagedThreadId} started", "ADB");
        }
    }

    /// <summary>
    /// 连接信息和配置类
    /// </summary>
    public static class ConnectionInfo
    {
        public static event EventHandler<List<string>> DevicesChanged;
        public static event EventHandler<string> DeviceChanged;

        public static bool IsServerRunning { get; set; }

        private static string _device;
        public static string Device
        {
            get
            {
                return _device;
            }
            set
            {
                _device = value;
                DeviceChanged?.Invoke(null, value);
                Output.Log("Device changed to:" + (value ?? "null"), "ADB");
            }
        }

        private static List<string> _devices;
        public static List<string> Devices
        {
            get
            {
                return _devices;
            }
            set
            {
                DevicesChanged?.Invoke(null, value);
                _devices = value;
                Output.Log("Devices list changed:" + string.Join(" ", value), "ADB");

                if (value.Count == 0)
                {
                    ConnectionInfo.Device = null;
                }
                else
                {
                    if (!value.Contains(ConnectionInfo.Device) || Device == null)
                    {
                        ConnectionInfo.Device = ConnectionInfo.Devices[0];
                    }
                }
            }
        }

        public static Dictionary<SimuInfo, bool> Connections { get; set; } = new Dictionary<SimuInfo, bool>()
        {
            { new SimuInfo("MuMu", "MuMu模拟器", 7555, "NemuPlayer"),false },
            { new SimuInfo("LDPlayer", "雷电模拟器", 5555, "dnplayer"),false }
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
                return ID + ":" + Name + "(" + IM + "," + Port + ")";
            }
        }
        static ConnectionInfo()
        {

        }
    }

    /// <summary>
    /// 对外端口，负责点火启动ADB。
    /// </summary>
    static class ADBStarter
    {
        public static void Start()
        {
            ADBServerStatus.StartServer();
            Connector.Start();
            DevicesWatcher.Start();
        }
    }
}
