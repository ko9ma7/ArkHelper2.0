using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.Windows;
using ArkHelper.Pages.NewUserList;
using System.Drawing.Printing;
using System.Runtime.InteropServices.WindowsRuntime;
using System.IO;
using System.Text.Json;
using System.Linq;
using Windows.Data.Json;
using Newtonsoft.Json.Linq;
using System.Windows.Documents;
using Newtonsoft.Json;
using static ArkHelper.ArkHelperDataStandard;
using Windows.Media.Protection.PlayReady;
using ArkHelper.Style.Control;
using System.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using System.Threading.Tasks;

namespace ArkHelper.Xaml
{
    /// <summary>
    /// akhcpiMaker.xaml 的交互逻辑
    /// </summary>
    public partial class akhcpiMaker : Window
    {

        bool edit = false;
        string targetFile;

        public akhcpiMaker()
        {
            InitializeComponent();
            changeVis(false);
            exebtn.IsEnabled = false;
            listenBtn.IsEnabled = false;
            Task.Run(() =>
            {
                while (ADB.ConnectedInfo == null)
                    Thread.Sleep(2000);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    exebtn.IsEnabled = true;
                    listenBtn.IsEnabled = true;
                });
            });
        }

        void changeVis(bool edit = true)
        {
            if (edit)
            {
                file.Visibility = Visibility.Collapsed;
                editArea.Visibility = Visibility.Visible;
                savebtn.Visibility = Visibility.Visible;
                exebtn.Visibility = Visibility.Visible;
                listenBtn.Visibility = Visibility.Visible;
            }
            else
            {
                file.Visibility = Visibility.Visible;
                editArea.Visibility = Visibility.Collapsed;
                savebtn.Visibility = Visibility.Collapsed;
                exebtn.Visibility = Visibility.Collapsed;
                listenBtn.Visibility = Visibility.Collapsed;

            }
        }
        private class MessageFromADB
        {
            public DateTime time { get; set; }
            public string msg { get; set; }
            public MessageFromADB(string str)
            {
                time = DateTime.Now;
                msg = str;
            }
        }

        private void _load(object sender, DragEventArgs e)
        {
            LoadFromFile(
                ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString()
                );
        }
        private void _loadFromClick(object sender, RoutedEventArgs e)
        {
            string file = "";

            file = Pages.OtherList.SCHT.OpenFileAsAkhcpi();
            if (file == "")
                return;
            LoadFromFile(file);
        }
        private void LoadFromFile(string fileAddress)
        {
            edit = true;
            changeVis();
            targetFile = fileAddress;

            int num = 0;
            var aa = ReadFromAKHcpi(fileAddress);

            foreach (var a in aa)
            {
                num++;
                var ab = new Style.Control.AKHcpiCard(a) { Num = num };
                cardList.Children.Add(ab);
            }
        }

        public static List<string> ReadFromAKHcpi(string fileAddress)
        {
            var list = new List<string>();
            var aa = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(File.ReadAllText(fileAddress)).GetProperty("content");
            foreach (var a in aa.EnumerateArray())
            {
                list.Add(a.ToString());

            }
            return list;
        }

        private void _create(object sender, RoutedEventArgs e)
        {
            edit = false;
            //cardList.Children.Add(new Style.Control.AKHcpiCard() { Num = 1 });
            changeVis();
        }
        private void add(object sender, RoutedEventArgs e)
        {
            var aa = new Style.Control.AKHcpiCard();
            aa.Num = cardList.Children.Count + 1;
            cardList.Children.Add(aa);
        }

        private void save(object sender, RoutedEventArgs e)
        {
            JObject Jo = new JObject();
            List<string> list = getString();

            var liststring = Newtonsoft.Json.JsonConvert.SerializeObject(list);
            Jo.Add("content", JsonConvert.DeserializeObject<JArray>(liststring));

            if (!edit)
            {
                var dialog = new System.Windows.Forms.SaveFileDialog()
                {
                    Title = "选择关卡方案",
                    Filter = "ArkHelper关卡方案文件(*.akhcpi)|*.akhcpi",
                    DefaultExt = "*.akhcpi",
                    AddExtension = true
                };
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    targetFile = dialog.FileName;
                    if (!File.Exists(targetFile))
                    {
                        File.Create(targetFile).Dispose();
                    }

                }
                else
                {
                    return;
                }
            }

            File.WriteAllText(targetFile, Jo.ToString());
            this.Close();
        }

        private List<string> getString()
        {
            List<string> list = new List<string>();

            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var item in cardList.Children)
                {
                    list.Add(item.ToString());
                }
            });

            return list;
        }

        private void del(int num)
        {
            var target = cardList.Children[num - 1];
            //删除
            cardList.Children.Remove(target);
            int a = 1;
            foreach (var item in cardList.Children)
            {
                (item as AKHcpiCard).Num = a;
                a++;
            }
        }
        private void move(int num, bool up)
        {
            if (num == 1 && up == true) return;
            if (num == cardList.Children.Count && !up) return;
            //移动
            //var targetUp = cardList.Children[num - 2];
            var target = cardList.Children[num - 1];
            //var targetDown = cardList.Children[num];

            cardList.Children.Remove(target);
            if (up) cardList.Children.Insert(num - 2, (target as AKHcpiCard));
            if (!up) cardList.Children.Insert(num, target);

            int a = 1;
            foreach (var item in cardList.Children)
            {
                (item as AKHcpiCard).Num = a;
                a++;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            AKHcpiCard.delEvent += del;
            AKHcpiCard.moveEvent += move;
        }
        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            AKHcpiCard.delEvent -= del;
            AKHcpiCard.moveEvent -= move;
        }

        public static void Exe(List<string> strings)
        {
            foreach (var akhcmd in strings)
            {
                new AKHcmd(akhcmd).RunCmd();
            }
        }

        private void exebtn_Click(object sender, RoutedEventArgs e)
        {
            exebtn.IsEnabled = false;
            listenBtn.IsEnabled = false;
            new Thread(() =>
            {
                Exe(getString());
                Application.Current.Dispatcher.Invoke(() =>
                {
                    exebtn.IsEnabled = true;
                    listenBtn.IsEnabled = true;
                });
            }).Start();
        }

        #region 监听ADB
        Process process = new Process()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = Address.adb,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                Arguments = "shell getevent"
            }
        };
        List<List<string>> eventsForAll = new List<List<string>>();
        List<TimeSpan> timesDuring = new List<TimeSpan>();
        List<TimeSpan> timesSwipe = new List<TimeSpan>();
        bool listening = false;
        void StartListening()
        {
            listening = true;
            process.Start();
            process.BeginOutputReadLine();
            process.OutputDataReceived += Process_OutputDataReceived;

            recordIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Stop;
            exebtn.IsEnabled = false;
        }

        void EndListening()
        {
            listening = false;
            process.CancelOutputRead();
            process.Kill();

            recordIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Record;
            exebtn.IsEnabled = true;

            eventsForAll.RemoveAll(t => t.Count == 0);
            if (eventsForAll.Count == 0)
            {
                Clear(); return;
            }
            timesDuring.RemoveAt(0);
            List<AKHcmd> ret = new List<AKHcmd>();
            int indexInList = 0;
            foreach (var a in eventsForAll)
            {
                a.RemoveAll(
                    t =>
                    t.Contains("0000 0000 00000000")
                    || t == ""
                    || t.Contains("0003 0039 ffffffff"));

                var _ = a.Find(t => t.Contains("0035")).Split(' ');
                int x1 = Convert.ToInt32(_[_.Length - 1], 16) - 1;

                var __ = a.Find(t => t.Contains("0036")).Split(' ');
                int y1 = Convert.ToInt32(__[__.Length - 1], 16) - 1;

                var ___ = a.FindLast(t => t.Contains("0035")).Split(' ');
                int x2 = Convert.ToInt32(___[___.Length - 1], 16) - 1;

                var ____ = a.FindLast(t => t.Contains("0036")).Split(' ');
                int y2 = Convert.ToInt32(____[____.Length - 1], 16) - 1;


                int waittime = (indexInList == timesDuring.Count) ? 2 : (int)timesDuring[indexInList].TotalSeconds + 1;
                if (x1 == x2 && y1 == y2)
                {
                    ret.Add(
                        new AKHcmd("shell input tap " + x1 + " " + y1, waitTime: waittime)
                        );
                }
                else
                {
                    ret.Add(
                        new AKHcmd("shell input swipe " + x1 + " " + y1 + " " + x2 + " " + y2 + " " + (int)timesSwipe[indexInList].TotalMilliseconds, waitTime: waittime)
                        );
                }
                indexInList++;
            }

            int num = 1;
            foreach (var a in ret)
            {
                var c = new AKHcpiCard(a.ToString());
                c.Num = num;
                cardList.Children.Add(c);
                num++;
            }

            Clear();

            void Clear()
            {
                eventsForAll.Clear();
                timesDuring.Clear();
                timesSwipe.Clear();
            }
        }

        bool isPressed = false;
        List<string> _eventsDuringEveryTime = new List<string>();
        DateTime time = DateTime.Now;
        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!listening) return;
            var cmd = e.Data;
            if (cmd.Contains("0001 014a 00000001"))//keyDown
            {
                isPressed = true;
                timesDuring.Add(DateTime.Now - time);
                time = DateTime.Now;
            }
            else
            {
                if (cmd.Contains("0001 014a 00000000"))//keyUp
                {
                    isPressed = false;
                    timesSwipe.Add(DateTime.Now - time);
                    time = DateTime.Now;
                    var list = new List<string>();
                    list.AddRange(_eventsDuringEveryTime);
                    eventsForAll.Add(list);
                    _eventsDuringEveryTime.Clear();
                }
                else
                {
                    if (isPressed)
                    {
                        _eventsDuringEveryTime.Add(e.Data);
                    }
                }
            }
        }
        private void listenBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!listening) StartListening();
            else EndListening();
        }
        #endregion
    }
}
