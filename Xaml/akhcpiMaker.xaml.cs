using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.Windows;

namespace ArkHelper.Xaml
{
    /// <summary>
    /// akhcpiMaker.xaml 的交互逻辑
    /// </summary>
    public partial class akhcpiMaker : Window
    {
        public akhcpiMaker()
        {
            InitializeComponent();
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
    }
}
