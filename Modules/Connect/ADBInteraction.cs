using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.TextFormatting;

namespace ArkHelper.Modules.Connect
{
    /// <summary>
    /// 使用命令行与ADB进行交互。
    /// </summary>
    static class ADBInteraction
    {
        private static readonly int defaultCodePage = CultureInfo.CurrentCulture.TextInfo.OEMCodePage;

        static ADBInteraction()
        {
            Commandline = new Process();

            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = "cmd",
                Arguments = "/K prompt $g ",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                StandardOutputEncoding = Encoding.GetEncoding(defaultCodePage),
                StandardErrorEncoding = Encoding.GetEncoding(defaultCodePage)
            };
            Commandline.EnableRaisingEvents = true;
            Commandline.StartInfo = startInfo;
            Commandline.Start();

            string oldValue = Environment.GetEnvironmentVariable("PATH");
            Environment.SetEnvironmentVariable("PATH", oldValue + ";" + Address.ADBEnvironment);
        }

        public static Process Commandline { get; set; } = new Process();

        public static List<int> GetChildProcesses()
        {
            List<int> lst = new List<int>();

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Process WHERE ParentProcessID=" + Commandline.Id + " AND Caption != 'conhost.exe'"))
            {
                ManagementObjectCollection managementObjectCollection = searcher.Get();
                foreach (ManagementObject managementObject in managementObjectCollection)
                {
                    lst.Add(Convert.ToInt32(managementObject["ProcessID"]));
                }
            }

            return lst;
        }

        public static void KillChildProcesses()
        {
            foreach (int pid in GetChildProcesses())
            {
                Debug.WriteLine($"Killing {pid})");
                Process.GetProcessById(pid).Kill();
            }
        }

        public static void KillChildProcessesAsync()
        {
            Task.Run(() => { KillChildProcesses(); });
        }

        public static void KillChildProcessesWithShell()
        {
            string input = "taskkill /F ";

            foreach (int pid in GetChildProcesses())
            {
                input += $"/PID {pid} ";
            }

            if (input == "taskkill /F ") return;

            Process cmd = new Process();

            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = "cmd",
                Arguments = "/c " + input,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            cmd.StartInfo = startInfo;

            Debug.WriteLine("Executing: cmd /c " + input);

            cmd.Start();
        }

        public static void KillAllAdbProcessesWithShell()
        {
            string input = "taskkill /F ";

            foreach (Process process in Process.GetProcessesByName("adb"))
            {
                input += $"/PID {process.Id} ";
            }

            if (input == "taskkill /F ") return;

            Process cmd = new Process();

            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = "cmd",
                Arguments = "/c " + input,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            cmd.StartInfo = startInfo;

            Debug.WriteLine("Executing: cmd /c " + input);

            cmd.Start();

        }

        public static void Execute(string command)
        {
            Commandline.StandardInput.WriteLine(".\\adb.exe " + command);
        }

        public static string GetOutput(string command,double MaxWaitSecond = -1)
        {
            Process cmd = new Process();

            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = "adb.exe",
                Arguments = command,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                StandardOutputEncoding = Encoding.GetEncoding(defaultCodePage),
                StandardErrorEncoding = Encoding.GetEncoding(defaultCodePage)
            };

            cmd.EnableRaisingEvents = true;

            cmd.StartInfo = startInfo;

            cmd.Start();

            string result = "";
            bool haveReaded = false;
            bool needToRead = true;

            Thread readout = new Thread(()=>{
                result = cmd.StandardOutput.ReadToEnd();
                if (!needToRead) return;
                haveReaded= true;
            });

            readout.Start();

            if (MaxWaitSecond != -1)
            {
                Thread.Sleep((int)(MaxWaitSecond * 1000));
            }
            else
            {
                while (true)
                {
                    if (haveReaded) break;
                    Thread.Sleep(1000);
                }
            }

            needToRead = false;
            return result;
        }
        public static void ExecuteClogViaCLI(string command)
        {
            Process cmd = new Process();

            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = "cmd.exe",
                Arguments = "cmd /k \"" + Address.ADB + " " + command + "\"&exit",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                StandardOutputEncoding = Encoding.GetEncoding(defaultCodePage),
                StandardErrorEncoding = Encoding.GetEncoding(defaultCodePage)
            };

            cmd.EnableRaisingEvents = true;

            cmd.StartInfo = startInfo;

            cmd.Start();

            cmd.WaitForExit();
        }

        public static void StopWithShell()
        {
            KillChildProcessesWithShell();
            Commandline.Kill();
        }
    }
}
