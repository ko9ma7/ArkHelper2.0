using System.Windows;
using RestSharp;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using RoutedEventArgs = System.Windows.RoutedEventArgs;
using Page = System.Windows.Controls.Page;
using DataFormat = RestSharp.DataFormat;
using System.Threading;
using Microsoft.Win32;
using System;
using Windows.ApplicationModel.Background;

namespace ArkHelper.Xaml
{
    /// <summary>
    /// Test.xaml 的交互逻辑
    /// </summary>
    public partial class Test : Page
    {
        enum ca
        {
            aaa,
            bbb,

        }
        List<UserModel> users;
        public Test()
        {
            InitializeComponent();
            users = new List<UserModel>()
            {
                new UserModel("ss"),
                new UserModel("ssss"),

            };
            datagrid.ItemsSource = users;
            //MessageBox.Show(ca.bbb.ToString());
            /*for (int i = 0; i < 100; i++)
            {
                pgb.Value++;
                Thread.Sleep(100);
            }*/
            if (ADB.ConnectedInfo != null)
                text.Text = ADB.ConnectedInfo.ToString();
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            /*Point[] points = AKH.PicToPoint.GetObjectLocation(AKH.PicToPoint.ArkObject.threeStarCp, @"C:\Users\SurFace\Documents\MuMu共享文件夹\MuMu20220908134730.png");
            string aa = "";
            foreach(Point point in points)
            {
                aa += point.ToString() + "\n";
            }
            MessageBox.Show(aa);*/
        }
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            string user = "xxx";
            string pswd = "xxx";
            //获取token
            var request = new RestRequest
            {
                Method = Method.Post,
                RequestFormat = DataFormat.Json
            };
            var json = new JObject()
            {
                {"phone",user },
                {"password",pswd }
            };
            request.AddParameter("", json, ParameterType.RequestBody);
            var client = new RestClient("https://as.hypergryph.com/user/auth/v1/token_by_phone_password");
            var result = client.Post(request);
            MessageBox.Show(result.Content.ToString());
            Clipboard.SetText(result.Content.ToString());
        }
        private void ListBoxItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

            string[] fileArr = Directory.GetFiles(Address.cache, "777.*");
            if (fileArr.Length > 0)
            {
                MessageBox.Show("exi");
            }
            else
            {
                MessageBox.Show("null");
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            new Xaml.akhcpiMaker().Show();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            /*RegistryKey registryKey = Registry.CurrentUser.OpenSubKey
                ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            registryKey.SetValue("ArkHelper", Application.Current);//"BaichuiMonitor"可以自定义*/
            MessageBox.Show(UniData.Screenshot);
        }
    }

    public class UserModel
    {
        public bool IsEnabled { get; set; }
        public string UserName { get; set; }
        public UserModel() { }
        public UserModel(string name)
        {
            UserName = name;
        }
    }
    /*public class UserDBModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }
        private ObservableCollection<UserModel> userdb = new ObservableCollection<UserModel>();
        public ObservableCollection<UserModel> UserDB
        {
            get { return userdb; }
            set
            {
                userdb = value;
                OnPropertyChanged(new PropertyChangedEventArgs("UserDB"));
            }
        }
    }*/
}
