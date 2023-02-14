using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using static ArkHelper.Modules.Connect.ConnectionInfo;

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
            Output.Log("ADB server killed");
        }
        public static void StartServer()
        {
            ADBInteraction.GetOutput("start-server");
            Output.Log("ADB server started");
        }
    }

    /// <summary>
    /// 对本地回环和端口发起连接请求。
    /// </summary>
    static class Connector
    {
        public static event EventHandler<ConnectStatus> IPConnectionChange;
        private static int loopInterval;
        private static readonly Thread thread;
        public static bool Looping { get; set; } = false;
        static Connector()
        {
            thread = new Thread(new ThreadStart(Connect));
        }
        public static void Connect()
        {
            while (true)
            {
                Looping = true;
                if (!ADB.CheckIfADBUsing())
                {
                    foreach (var info in ConnectionInfo.Connections.Keys.ToArray())
                        if (Process.GetProcessesByName(info.IM).Length != 0 && ConnectionInfo.Connections[info].Auto)
                        {
                            var connectResult = false;
                            for (int i = 0; i < 2; i++)
                            {
                                connectResult = connectResult || ADBInteraction.GetOutput("connect 127.0.0.1:" + info.Port, 2).Contains("connected");
                            }
                            if (ConnectionInfo.Connections[info].Connected != connectResult)
                            {
                                ConnectionInfo.Connections[info].Connected = connectResult;
                                IPConnectionChange?.Invoke(info, Connections[info]);

                                Output.Log("Connection freshed:" + info.ToString() + ",turned into " + connectResult, "ADB");
                            }
                        }
                        else
                        {
                            var result = false;
                            if (ConnectionInfo.Connections[info].Connected != result)
                            {
                                ConnectionInfo.Connections[info].Connected = result;
                                IPConnectionChange?.Invoke(info, Connections[info]);

                                Output.Log("Connection freshed:" + info.ToString() + ",turned into " + result, "ADB");
                            }
                        }
                }
                Looping = false;
                Thread.Sleep(loopInterval);
            }
        }
        public static void DisConnect(SimuInfo info,Action action = null)
        {
            Task.Run(() =>
            {
                if (!Connections[info].Connected) return;
                if (ADBInteraction.GetOutput("disconnect 127.0.0.1:" + info.Port, 2).Contains("disconnected"))
                    IPConnectionChange?.Invoke(info, Connections[info]);
                if (action != null)
                {
                    action();
                }
            });
            
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

        public static bool Looping { get; set; } = false;

        static DevicesWatcher()
        {
            thread = new Thread(new ThreadStart(Refresh));
        }

        public static void Refresh()
        {
            while (true)
            {
                Looping = true;
                if (/*ConnectionInfo.Connections.Values.Count(t => t.Connected) != 0
                    && */!ADB.CheckIfADBUsing())
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
                Looping = false;
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
                if (_device != null)
                {
                    if (_device != value)
                        set();
                }
                else
                    set();
                void set()
                {
                    _device = value;
                    DeviceChanged?.Invoke(null, value);
                    Output.Log("Device changed to:" + (value ?? "null"), "ADB");
                }
            }
        }

        private static List<string> _devices = new List<string>();
        public static List<string> Devices
        {
            get
            {
                return _devices;
            }
            set
            {
                _devices = value;
                DevicesChanged?.Invoke(null, value);
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

        public static Dictionary<SimuInfo, ConnectStatus> Connections { get; set; } = new Dictionary<SimuInfo, ConnectStatus> ()
        {
            { new SimuInfo("MuMu", "MuMu模拟器", 7555, "NemuPlayer"){ReadOnly = true},new ConnectStatus() },
            { new SimuInfo("LDPlayer", "雷电模拟器", 5555, "dnplayer"){ReadOnly = true},new ConnectStatus() },
            { new SimuInfo("BlueStacks", "蓝叠模拟器", 5555, "HD-Player"){ReadOnly = true},new ConnectStatus() },
            { new SimuInfo("MEMU", "逍遥模拟器", 21503, "MEmu"){ReadOnly = true},new ConnectStatus() },
            { new SimuInfo("NOX", "夜神模拟器", 62001, "Nox"){ReadOnly = true},new ConnectStatus() },
            { new SimuInfo("WSA", "WSA", 58526, "WSAClient"){ReadOnly = true},new ConnectStatus() },
        };

        public class SimuInfo
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public int Port { get; set; }
            public string IM { get; set; }
            [JsonIgnore]
            public bool ReadOnly
            {
                get;
                set;
            } = false;
            /*[JsonIgnore]
            public bool IsCreateOK { get; set; }*/
            public SimuInfo(string id, string name, int port, string im)
            {
                ID = id;
                Name = name;
                Port = port;
                IM = im;
            }
            public SimuInfo() 
            {
                ID = "";
                Name = "";
                Port = 0;
                IM = "";
            }

            public override string ToString()
            {
                return ID + ":" + Name + "(" + IM + "," + Port + ")";
            }
        }
        public class ConnectStatus
        {
            public bool Auto { get; set; }
            public bool Connected { get; set; }
            public ConnectStatus()
            {
                Auto = true; Connected = false;
            }
        }
        static ConnectionInfo()
        {

        }
        public static void ChangeSimulator(SimuInfo simuInfo,bool add,Action action = null)
        {
            Task.Run(() =>
            {
                while (true)
                {
                    if (Connector.Looping)
                    {
                        Thread.Sleep(2000);
                    }
                    else
                    {
                        try
                        {
                            if (!add)
                                Connections.Remove(simuInfo);
                            else
                                Connections.Add(simuInfo, new ConnectStatus());
                        }
                        catch
                        {

                        }
                        break;
                    }
                }
                if (action != null) action();
            });
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
