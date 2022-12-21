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
using System.Windows.Input;
using System.Windows.Media;
using System.Diagnostics;
using Windows.ApplicationModel.Appointments;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices.ComTypes;
using MaterialDesignThemes.Wpf;
using System.Net;
using System.Security.Policy;

namespace ArkHelper.Xaml
{
    /// <summary>
    /// Test.xaml 的交互逻辑
    /// </summary>
    public partial class Test : Page
    {
        public Test()
        {
            InitializeComponent();
        }

        #region
        private void getToken()
        {
            //token
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
                { "phone",user },
                { "password",pswd }
            };*/
            request.AddParameter("", json, ParameterType.RequestBody);
            var client = new RestClient("https://as.hypergryph.com/user/auth/v1/token_by_phone_password");
            var result = client.Post(request);
            MessageBox.Show(result.Content.ToString());
            Clipboard.SetText(result.Content.ToString());
        }
        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ADB.Screenshot screenshot = new ADB.Screenshot();
            var aa = screenshot.PicToPoint(Address.res + "\\pic\\UI\\AutoDeployON.png",opencv_errorCon:0.95);
        }
    }
}