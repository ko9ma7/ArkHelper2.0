using ArkHelper.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Windows.Services.Maps;

namespace ArkHelper.Xaml.Widget
{
    /// <summary>
    /// UnreadMessage.xaml 的交互逻辑
    /// </summary>
    public partial class UnreadMessage : Page
    {
        public UnreadMessage()
        {
            InitializeComponent();
            var a = Message.GetAllUnreadMessages();
            if (a.Count == 0)
            {
                NoUnreadMessage.Visibility = Visibility.Visible;
            }
            else
            {
                foreach(var m in a)
                {
                    var b = new Xaml.Control.RoundImageWithTwoTexts(m.Key.Avatar, m.Key.Name, "更新了" + m.Value.Count + "条新消息")
                    {
                        Margin = new Thickness(5),
                    };
                    unreadList.Children.Add(b);
                }
                UnreadMessagesShow.Visibility = Visibility.Visible;
            }
        }
    }
}
