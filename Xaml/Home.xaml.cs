using System;
using System.Windows;
using System.Windows.Threading;
using Page = System.Windows.Controls.Page;

namespace ArkHelper.Xaml
{
    /// <summary>
    /// Home.xaml 的交互逻辑
    /// </summary>
    public partial class Home : Page
    {
        DispatcherTimer dispatcherTimer = new DispatcherTimer()
        {
            Interval = new TimeSpan(0, 0, 0, 1),
        };
        public Home()
        {
            InitializeComponent();

            //启动计时委托器
            dispatcherTimer.Tick += new EventHandler(_UpdateTime);
            dispatcherTimer.Start();

            UpdateTime();
            time_welcome.Text = DateTime.Now.ToString("tt");
            Widget1.Navigate(new Uri(@"\Xaml\Widget\" + "SCHTStatus" + ".xaml", UriKind.RelativeOrAbsolute));
            Widget2.Navigate(new Uri(@"\Xaml\Widget\" + "ArkHelperSuggestion" + ".xaml", UriKind.RelativeOrAbsolute));

            string a;
            DateTime dateTime = DateTime.Now;
            if (dateTime.Hour < 4) { dateTime = DateTime.Now.AddDays(-1); PushNewMessage("数据将于凌晨 4:00刷新", "Refresh"); }
            a = dateTime.ToString("ddd");
            switch (a)
            {
                case "周一":
                    chip_notify.ContentStringFormat = "医疗/重装/术师/狙击";
                    material_notify.ContentStringFormat = "作战记录/采购凭证/碳";
                    break;
                case "周二":
                    chip_notify.ContentStringFormat = "术师/狙击/特种/近卫";
                    material_notify.ContentStringFormat = "作战记录/龙门币/技巧概要";
                    break;
                case "周三":
                    chip_notify.ContentStringFormat = "辅助/先锋/特种/近卫";
                    material_notify.ContentStringFormat = "作战记录/碳/技巧概要";
                    break;
                case "周四":
                    chip_notify.ContentStringFormat = "医疗/重装/辅助/先锋";
                    material_notify.ContentStringFormat = "作战记录/龙门币/采购凭证";
                    break;
                case "周五":
                    chip_notify.ContentStringFormat = "医疗/重装/术师/狙击";
                    material_notify.ContentStringFormat = "作战记录/碳/技巧概要";
                    break;
                case "周六":
                    chip_notify.ContentStringFormat = "术师/狙击/辅助/先锋/特种/近卫";
                    material_notify.ContentStringFormat = "作战记录/龙门币/采购凭证/碳";
                    break;
                case "周日":
                    chip_notify.ContentStringFormat = "医疗/重装/辅助/先锋/特种/近卫";
                    material_notify.ContentStringFormat = "作战记录/龙门币/采购凭证/技巧概要";
                    PushNewMessage("距本周剿灭奖励重置已不足一天","CurrencyUsd");
                    break;
            }
            
        }

        private void _UpdateTime(object sender, EventArgs e)
        {
            UpdateTime();
        }
        private void UpdateTime()
        {
            if (DateTime.Now.ToString("hh") == "03" || (DateTime.Now.ToString("HH") == "19" && App.Data.scht.fcm.status &&Convert.ToInt32(DateTime.Now.ToString("mm")) > 57))
            {
                time_notify.Text = DateTime.Now.ToString("tt h:mm:ss");
            }
            else
            {
                time_notify.Text = DateTime.Now.ToString("tt h:mm");
            }
        }

        public void PushNewMessage(string content, string icon_kind = "Message")
        {
            notif_box.Items.Add(new System.Windows.Controls.ListBoxItem()
            {
                ContentStringFormat = content,
                Style = (System.Windows.Style)FindResource("message_listbox_item"),
                Tag = icon_kind,
            });
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            dispatcherTimer.Stop();
        }
    }
}
