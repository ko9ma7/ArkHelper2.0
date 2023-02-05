using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
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
using MaterialDesignThemes.Wpf;

namespace ArkHelper.Modules.Connect.XAML
{
    /// <summary>
    /// ConnectionController.xaml 的交互逻辑
    /// </summary>
    public partial class ConnectionController : Page
    {
        public ConnectionController()
        {
            InitializeComponent();
            FreshDeviceList();
            ConnectionInfo.DevicesChanged += (s, e) =>
            {
                FreshDeviceList();
            };
            //fu
            ConnectionInfo.DeviceChanged += (s, e) =>
            {
                if (!DeviceIsChangingByParent)
                {
                    DeviceIsChangingByEvent = true;
                    try
                    {
                        
                        Application.Current.Dispatcher.Invoke(delegate
                        {
                            foreach (var a in DeviceList.Items)
                            {
                                if (((a as ListBoxItem).Content as string) == e) (a as ListBoxItem).IsSelected = true;
                            }
                        });

                    }
                    catch
                    {

                    }
                    DeviceIsChangingByEvent = false;
                }
                    
            };
            foreach (var simu in ConnectionInfo.Connections.Keys) InitSimuCard(simu);
        }

        private void FreshDeviceList()
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                DeviceList.Items.Clear();
                foreach (var a in ConnectionInfo.Devices)
                {
                    var b = new ListBoxItem()
                    {
                        Content = a,
                    };
                    if (a == ConnectionInfo.Device) b.IsSelected = true;else b.IsSelected = false;

                    DeviceList.Items.Add(b);
                }
            });
        }

        private void InitSimuCard(ConnectionInfo.SimuInfo simu)
        {
            var card = new Connect.XAML.ConnectionCard(simu)
            {
                Margin = new Thickness()
                {
                    Bottom = 8
                }
            };
            card.Setting += (s, e) =>
            {
                Setting(e);
                EditingCard = s as ConnectionCard;
            };
            card.Delete += (s, e) =>
            {
                Delete(s, e);
            };
            if (simu.ReadOnly) UISimulatorsList.Children.Add(card);
            else UISimulatorsCustomList.Children.Add(card);
        }
        private void new_custom_btn_click(object sender, RoutedEventArgs e)
        {
            var newSimu = new ConnectionInfo.SimuInfo();

            Setting(newSimu, true);
        }

        ConnectionCard EditingCard { get; set; }
        ConnectionInfo.SimuInfo DialogEditingInfo { get; set; }
        bool DialogIsCreating { get; set; }
        private void Setting(ConnectionInfo.SimuInfo simuInfo, bool create = false)
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                DialogEditingInfo = simuInfo;
                DialogIsCreating = create;

                Dialog_simuEdit.IsEnabled = !simuInfo.ReadOnly;
                Dialog_name.Text = simuInfo.Name;
                Dialog_im.Text = simuInfo.IM;
                Dialog_port.Text = simuInfo.Port.ToString();
                dialog.IsOpen = true;
            });
        }
        private void DialogClose()
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                dialog.IsOpen = false;
            });
        }
        private void DialogSave()
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                DialogEditingInfo.Name = Dialog_name.Text;
                DialogEditingInfo.IM = Dialog_im.Text;
                dialog_save_pgb.Visibility = Visibility.Visible;
                dialog_save_btn.Visibility = Visibility.Collapsed;
                dialog_close_btn.Visibility = Visibility.Hidden;
                try
                {
                    DialogEditingInfo.Port = Convert.ToInt32(Dialog_port.Text);
                }
                catch { }

                if (DialogIsCreating)
                {
                    ConnectionInfo.ChangeSimulator(DialogEditingInfo, true, () =>
                    {
                        Application.Current.Dispatcher.Invoke(delegate
                        {
                            dialog_save_pgb.Visibility = Visibility.Collapsed;
                            dialog_save_btn.Visibility = Visibility.Visible;
                            dialog_close_btn.Visibility = Visibility.Visible;

                            InitSimuCard(DialogEditingInfo);
                            DialogClose();
                        });
                    });
                    App.Data.simulator.customs.Add(DialogEditingInfo);

                }
                else
                {
                    EditingCard.UpdateInfo();
                    DialogClose();
                }


            });
        }
        private void dialog_close_btn_click(object sender, RoutedEventArgs e)
        {
            DialogClose();
        }
        private void dialog_save_btn_click(object sender, RoutedEventArgs e)
        {
            DialogSave();
        }
        private void Delete(object sender, Connect.ConnectionInfo.SimuInfo e)
        {
            ConnectionInfo.ChangeSimulator(e, false, () =>
            {
                Application.Current.Dispatcher.Invoke(delegate
                {
                    UISimulatorsCustomList.Children.Remove(sender as ConnectionCard);
                });
            });
            App.Data.simulator.customs.Remove(e);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DeviceList.Items.Clear();
        }

        bool DeviceIsChangingByParent = false;
        bool DeviceIsChangingByEvent = false;
        private void DeviceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DeviceIsChangingByParent = true;
            try
            {
                if(!DeviceIsChangingByEvent) ConnectionInfo.Device = (DeviceList.SelectedItem as ListBoxItem).Content.ToString();
            }
            catch
            {

            }
            DeviceIsChangingByParent = false;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (ConnectionInfo.Device == null) return;
            if (MessageBox.Show("点击确定后，ArkHelper会拉起对应设备上的设置界面。\n这可以用来确定ArkHelper实际控制的是哪台设备。", "ArkHelper", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                ADB.CMD("shell am start -n " + "com.android.settings" + "/com.android.settings.Settings");
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (ConnectionInfo.Device == null) return;
            var cmd = "";
            var win = new Window()
            {
                Title = "请输入指令",
                Height = 79,
                Width = 281
            };
            WrapPanel grid = new WrapPanel();
            TextBox textBox = new TextBox()
            {
                Width = 200,
                Text = ""
            };
            Button button = new Button()
            {
                Content = "确定"
            };
            button.Click += (s, ea) =>
            {
                cmd = textBox.Text;
                win.Close();
            };
            
            grid.Children.Add(textBox);
            grid.Children.Add(button);

            win.Content = grid;

            win.ShowDialog();

            Task.Run(() =>
            {
                MessageBox.Show(ADB.CMD(cmd), "返回的结果");
            });
        }
    }
}
