using System;
using System.Windows.Controls;

namespace ArkHelper.Xaml.Widget
{
    public partial class SCHTstatus : Page
    {
        public static string GetTime()
        {
            string res = "";
            if (App.Data.scht.status)
            {
                if (true)
                {
                    //防沉迷
                    DateTime dateTime = DateTime.Now;

                    switch (dateTime.DayOfWeek)
                    {
                        case DayOfWeek.Friday:
                        case DayOfWeek.Saturday:
                            if (dateTime.Hour < 20)
                            {
                                res = dateTime.ToString("M") + " 下午 7:58";
                            }
                            else
                            {
                                res= dateTime.AddDays(1).ToString("M") + " 下午 7:58";
                            }
                            break;
                        default:
                            if (dateTime.DayOfWeek == DayOfWeek.Sunday && dateTime.Hour < 20)
                            {
                                res= dateTime.ToString("M") + " 下午 7:58";
                            }
                            else
                            {
                                for (int a = 0;
                                    dateTime.AddDays(a).DayOfWeek <= DayOfWeek.Friday;
                                    a += 1)
                                {
                                    res= dateTime.AddDays(a).ToString("M") + " 下午 7:58";
                                }
                            }
                            break;
                    }
                }
                else
                {
                    //非防沉迷
                    int a = Convert.ToInt32(DateTime.Now.ToString("HH"));
                    if (a < 8) { res= DateTime.Now.ToString("M") + " 上午 7:58"; }
                    else { res= DateTime.Now.ToString("M") + " 下午 7:58"; }
                }
            }
            else
            {
                res= "不会运行";
            }
            return res;
        }
        public SCHTstatus()
        {
            InitializeComponent();
            if (App.Data.scht.status)
            {
                SCHT_status.Text = "正常运行中";
            }
            else
            {
                SCHT_status.Text = "已禁用";
            }

            time_NextSCHT.Text = GetTime();
        }
    }
}
