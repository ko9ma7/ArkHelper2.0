using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
using ArkHelper.Modules.Connect;

namespace ArkHelper.Modules.Connect.XAML
{
    /// <summary>
    /// ConnectionCard.xaml 的交互逻辑
    /// </summary>
    public partial class ConnectionCard : UserControl
    {
        private bool _isconnected;
        public bool IsConnected
        {
            get
            {
                return _isconnected;
            }
            set
            {
                _isconnected = value;
                if (value)
                {
                    MainConIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.LanConnect;
                    BtnConIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.LanDisconnect;
                    ConnectStatus.Text = "已连接";
                }
                else
                {
                    MainConIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.LanDisconnect;
                    BtnConIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.LanConnect;
                    ConnectStatus.Text = "未连接";
                }
            }
        }

        public ConnectionInfo.SimuInfo ThisSimulator { get; set; }
        public ConnectionCard(ConnectionInfo.SimuInfo simuInfo)
        {
            InitializeComponent();

            ThisSimulator = simuInfo;
            UpdateInfo();

            Connector.IPConnectionChange += (s, e) =>
            {
                var ChangeSender = s as ConnectionInfo.SimuInfo;
                if (ChangeSender == ThisSimulator)
                    Application.Current.Dispatcher.Invoke(delegate
                    {
                        IsConnected = e.Connected;
                        Connect_btn.Visibility = Visibility.Visible;
                        Connect_pgb.Visibility = Visibility.Collapsed;
                    });
            };
        }

        /// <summary>
        /// 切换连接状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (ConnectionInfo.Connections[ThisSimulator].Auto && IsConnected)
            {
                Connect_btn.Visibility = Visibility.Collapsed;
                Connect_pgb.Visibility = Visibility.Visible;
                ConnectionInfo.Connections[ThisSimulator].Auto = false;
                Connector.DisConnect(ThisSimulator, () =>
                {
                    Application.Current.Dispatcher.Invoke(delegate
                    {
                        IsConnected = false;
                        Connect_btn.Visibility = Visibility.Visible;
                        Connect_pgb.Visibility = Visibility.Collapsed;
                    });
                });
            }
            else
            {
                ConnectionInfo.Connections[ThisSimulator].Auto = true;
                Connect_btn.Visibility = Visibility.Collapsed;
                Connect_pgb.Visibility = Visibility.Visible;
            }
        }

        public event EventHandler<ConnectionInfo.SimuInfo> Setting;
        public event EventHandler<ConnectionInfo.SimuInfo> Delete;

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Setting?.Invoke(this, this.ThisSimulator);
        }

        public void UpdateInfo()
        {
            Delete_btn.IsEnabled = !ThisSimulator.ReadOnly;
            IsConnected = ConnectionInfo.Connections[ThisSimulator].Connected;
            IP.Text = "127.0.0.1:" + ThisSimulator.Port;
            Name.Text = ThisSimulator.Name;
        }

        private void Delete_btn_Click(object sender, RoutedEventArgs e)
        {
            Delete_btn.Visibility = Visibility.Collapsed;
            Delete_pgb.Visibility = Visibility.Visible;
            Delete?.Invoke(this, this.ThisSimulator);
        }
    }
}
