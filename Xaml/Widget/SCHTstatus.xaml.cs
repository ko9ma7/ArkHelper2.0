using System;
using System.Windows.Controls;

namespace ArkHelper.Xaml.Widget
{
    public partial class SCHTstatus : Page
    {
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

            time_NextSCHT.Text = ArkHelper.Pages.OtherList.SCHT.GetNextRunTimeStringFormat();
        }
    }
}
