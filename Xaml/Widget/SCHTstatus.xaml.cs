using System;
using System.Windows.Controls;
using static ArkHelper.Data.SCHT;

namespace ArkHelper.Xaml.Widget
{
    public partial class SCHTstatus : Page
    {
        public SCHTstatus()
        {
            InitializeComponent();
            if (status)
            {
                SCHT_status.Text = "正常运行中";
                if (fcm.status)
                {
                    //防沉迷
                    DateTime dateTime = DateTime.Now;

                    switch (dateTime.DayOfWeek)
                    {
                        case DayOfWeek.Friday:
                        case DayOfWeek.Saturday:
                            if (dateTime.Hour < 20)
                            {
                                time_NextSCHT.Text = dateTime.ToString("M") + " 下午 7:58";
                            }
                            else
                            {
                                time_NextSCHT.Text = dateTime.AddDays(1).ToString("M") + " 下午 7:58";
                            }
                            break;
                        default:
                            if (dateTime.DayOfWeek == DayOfWeek.Sunday && dateTime.Hour < 20)
                            {
                                time_NextSCHT.Text = time_NextSCHT.Text = dateTime.ToString("M") + " 下午 7:58";
                            }
                            else
                            {
                                for (int a = 0;
                                    dateTime.AddDays(a).DayOfWeek <= DayOfWeek.Friday;
                                    a += 1)
                                {
                                    time_NextSCHT.Text = dateTime.AddDays(a).ToString("M") + " 下午 7:58";
                                }
                            }
                            break;
                    }
                }
                else
                {
                    //非防沉迷
                    int a = Convert.ToInt32(DateTime.Now.ToString("HH"));
                    if (a < 8) { time_NextSCHT.Text = DateTime.Now.ToString("M") + " 上午 7:58"; }
                    else { time_NextSCHT.Text = DateTime.Now.ToString("M") + " 下午 7:58"; }
                }
            }
            else
            {
                SCHT_status.Text = "已禁用";
                time_NextSCHT.Text = "不会运行";
            }
        }
    }
}
