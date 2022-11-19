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

        }

        void changeVis(bool edit = true)
        {
            if (edit)
            {
                file.Visibility = Visibility.Collapsed;
                editArea.Visibility = Visibility.Visible;
                savebtn.Visibility = Visibility.Visible;
            }
            else
            {
                file.Visibility = Visibility.Visible;
                editArea.Visibility = Visibility.Collapsed;
                savebtn.Visibility = Visibility.Collapsed;
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
        private void StartListening()
        {
            Process adb = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = Address.adb,
                    WindowStyle = ProcessWindowStyle.Normal,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = false,
                    Arguments = "shell getevent"
                }
            };
            adb.OutputDataReceived += Process_OutputDataReceived;
            adb.Start();
            adb.BeginOutputReadLine();
        }
        List<MessageFromADB> messages = new List<MessageFromADB>();
        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            string msg = e.Data as string;
            if (msg != "" && msg.Contains(@"/dev/input/event5:"))
            {
                messages.Add(new MessageFromADB(msg));
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
            cardList.Children.Add(new Style.Control.AKHcpiCard() { Num = 1 });
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
            List<string> list = new List<string>();

            foreach (var item in cardList.Children)
            {
                list.Add(item.ToString());
            }

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

        private void del(int num)
        {
            var target = cardList.Children[num - 1];
            //删除
            cardList.Children.Remove(target);
            int a = 1;
            foreach(var item in cardList.Children)
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
            if (up) cardList.Children.Insert(num-2, (target as AKHcpiCard));
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
    }
}
