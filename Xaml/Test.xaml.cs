using System.Windows;
using RestSharp;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;
using System.Text.Json;
using System;
using OpenCvSharp;
using Point = OpenCvSharp.Point;
using Rect = OpenCvSharp.Rect;
using Size = OpenCvSharp.Size;

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
                RequestFormat = RestSharp.DataFormat.Json
            };
            var json = new JsonElement();
            /*{
                {"phone",user },
                {"password",pswd }
            };*/
            request.AddParameter("", json, ParameterType.RequestBody);
            var client = new RestClient("https://as.hypergryph.com/user/auth/v1/token_by_phone_password");
            var result = client.Post(request);
            MessageBox.Show(result.Content.ToString());
            Clipboard.SetText(result.Content.ToString());
        }
        private void ListBoxItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

            string[] fileArr = Directory.GetFiles(Address.Cache.main, "777.*");
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



            /*
            var ac = JsonSerializer.Serialize(App.Data);
            var jsonString = @"{""Simulator"":{}}";

            ArkHelperDataStandard.Data bb =
                JsonSerializer.Deserialize<ArkHelperDataStandard.Data>(ac);
            bb.ToString();*/

            /*ArkHelperDataStandard.Data cc =
                JsonSerializer.Deserialize<ArkHelperDataStandard.Data>( new ArkHelper.ArkHelperDataStandard.AKHcpi());*/
            var aaaa = new ArkHelperDataStandard.AKHcpi();
            aaaa.adcmd.Add(new ArkHelperDataStandard.AKHcmd("shell input tap 66 66", "", 3, 3));
            var ac = JsonSerializer.Serialize(aaaa);
            string aba = "";
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            //var aa = GetSmallPicLocationUsingOpenCV(@"C:\Users\SurFace\Desktop\屏幕截图 2022-11-12 174043.png", @"C:\Users\SurFace\Desktop\MuMu20220906000022255201.png");
            /*foreach (var ab in aa)
            {
                MessageBox.Show(ab.ToString());
            }*/
        }
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
