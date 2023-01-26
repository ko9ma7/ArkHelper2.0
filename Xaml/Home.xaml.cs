using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Windows.Media.Audio;
using Page = System.Windows.Controls.Page;

namespace ArkHelper.Xaml
{
    /// <summary>
    /// Home.xaml 的交互逻辑
    /// </summary>
    public partial class Home : Page
    {
        public static bool FirstCheckUpdate = true;
        Dictionary<string, int[]> materialDic = new Dictionary<string, int[]>()
        {
            {"作战记录",new int[]{ 1,2,3,4,5,6,7 } },
            {"龙门币",new int[]{ 2,4,6,7 } },
            {"采购凭证",new int[]{ 1,4,6,7 } },
            {"技巧概要",new int[]{ 2,3,5,7 } },
            {"碳",new int[]{ 1,3,5,6 } },
        };
        Dictionary<string, int[]> chipDic = new Dictionary<string, int[]>()
        {
            {"医疗/重装",new int[]{ 1,4,5,7 } },
            {"术师/狙击",new int[]{ 1,2,5,6 } },
            {"辅助/先锋",new int[]{ 3,4,6,7 } },
            {"特种/近卫",new int[]{ 2,3,6,7 } }
        };
        DispatcherTimer dispatcherTimer = new DispatcherTimer()
        {
            Interval = new TimeSpan(0, 0, 0, 1, 0),
        };
        string handleUpdateURL = "";
        public Home()
        {
            InitializeComponent();

            //启动计时委托器
            dispatcherTimer.Tick += new EventHandler((object sender, EventArgs e) => UpdateTime());
            dispatcherTimer.Start();

            UpdateTime();
            time_welcome.Text = DateTime.Now.ToString("tt");
            Widget1.Navigate(new Uri(@"\Xaml\Widget\" + "SCHTStatus" + ".xaml", UriKind.RelativeOrAbsolute));
            Widget2.Navigate(new Uri(@"\Xaml\Widget\" + "UnreadMessage" + ".xaml", UriKind.RelativeOrAbsolute));

            DateTime dateTime = DateTime.Now;
            if (dateTime.Hour < 4) { dateTime = DateTime.Now.AddDays(-1); PushNewMessage("数据将于凌晨 4:00刷新", "Refresh"); }

            var week = dateTime.DayOfWeek;
            if (week == DayOfWeek.Sunday) PushNewMessage("距本周剿灭奖励重置已不足一天", "CurrencyUsd");
            var weekNum = ArkHelperDataStandard.GetWeekSubInChinese(week) + 1;

            chip_notify.ContentStringFormat = initList(chipDic);
            material_notify.ContentStringFormat = initList(materialDic);

            string initList(Dictionary<string, int[]> dic)
            {
                List<string> list = new List<string>();
                foreach (var _stage in dic)
                {
                    if (_stage.Value.Contains<int>(weekNum))
                    {
                        list.Add(_stage.Key);
                    }
                }
                string result = ""; int _a = 1;
                foreach (var _stage in list)
                {
                    result = result + _stage + (_a == list.Count ? "" : "/");
                    _a++;
                }
                return result;
            }

            if (App.Data.scht.showGuide)
            {
                PushNewMessage("开始使用SCHT系统","Message", null, "点击左侧“SCHT控制台”，了解更多信息。");
            }

            var supd = new Task(() =>
            {
                var a = Version.Update.SearchNewestRelease();
                if (a.VersionNumber > Version.Current.VersionNumber)
                {
                    handleUpdateURL = a.URL;
                    PushNewMessage("ArkHelper有更新可用", "Update", HandleUpdate);
                }
                if (a.Necessary)
                {
                    WithSystem.Message("ArkHelper有重要更新，点击确定跳转到更新地址下载更新，否则无法正常使用。");
                    Process.Start(handleUpdateURL);
                    App.ExitApp();
                }
                FirstCheckUpdate = false;
            });
            if (FirstCheckUpdate) supd.Start();
        }

        private void UpdateTime()
        {
            if (DateTime.Now.ToString("hh") == "03" || (DateTime.Now.ToString("HH") == "19" && Convert.ToInt32(DateTime.Now.ToString("mm")) > 57))
            {
                time_notify.Text = DateTime.Now.ToString("tt h:mm:ss");
            }
            else
            {
                time_notify.Text = DateTime.Now.ToString("tt h:mm");
            }
        }
        public void PushNewMessage(string content, string icon_kind = "Message", MouseButtonEventHandler funcA = null,string Tooltip = "")
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var a = new System.Windows.Controls.ListBoxItem()
                {
                    ContentStringFormat = content,
                    Style = (System.Windows.Style)FindResource("message_listbox_item"),
                    Tag = icon_kind,
                    ToolTip = Tooltip
                };
                if (funcA != null)
                {
                    a.Cursor = Cursors.Hand;
                    MouseLeftButtonUp += funcA;
                }
                notif_box.Items.Add(a);
            });
        }

        private void HandleUpdate(object sender, MouseButtonEventArgs e)
        {
            Process.Start(handleUpdateURL);
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            dispatcherTimer.Stop();
        }


    }
}
