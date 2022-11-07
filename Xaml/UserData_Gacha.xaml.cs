using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Globalization;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Page = System.Windows.Controls.Page;

namespace ArkHelper.Xaml
{
    /// <summary>
    /// UserData_Gacha.xaml 的交互逻辑
    /// </summary>
    public partial class UserData_Gacha : Page
    {
        public class GachaLog
        {
            public string Time { get; set; }
            public List<string> Operators { get; set; }
            public string Pool { get; set; }
            public GachaLog(JsonElement json)
            {
                long unixTimeStamp = json.GetProperty("ts").GetInt32();
                DateTime dttime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                Time = dttime.AddSeconds(unixTimeStamp).ToLocalTime().ToString("g");

                Pool = json.GetProperty("pool").GetString();
                Operators = new List<string>();
                foreach (JsonElement op in json.GetProperty("chars").EnumerateArray())
                {
                    Operators.Add(new Operator(op).ToString());
                }
            }
        }
        public class Operator
        {
            public string Name { get; set; }
            public int Rare { get; set; }
            public bool IsNew { get; set; }
            public Operator(JsonElement json)
            {
                Name = json.GetProperty("name").GetString();
                Rare = json.GetProperty("rarity").GetInt32();
                IsNew = json.GetProperty("isNew").GetBoolean();
            }
            public override string ToString()
            {
                string ret = "";
                ret += IsNew ? "[新]" : "";
                for (int i = 0; i > Rare; ret += "⭐") ;
                ret += Name;
                return ret;
            }
        }
        /// <summary>
        /// Token
        /// </summary>
        public string Token = null;
        /// <summary>
        /// 抽卡列表
        /// </summary>
        public List<GachaLog> Lists = new List<GachaLog>();

        #region List
        /// <summary>
        /// 获取单页抽卡列表
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        private JsonElement get(int page)
        {
            //获取结果
            var client = new RestClient("https://ak.hypergryph.com/user/api/inquiry/gacha?page=" + page + "&token=" + Token + "&channelId=1");
            var result = client.Get(new RestRequest());
            return JsonDocument.Parse(result.Content).RootElement;
        }
        /// <summary>
        /// 根据token获取抽卡列表
        /// </summary>
        private void GetResult()
        {
            Lists.Clear();
            int totalPage = get(1).GetProperty("data").GetProperty("pagination").GetProperty("total").GetInt32();
            if (totalPage == 0) return;
            else
            {
                for (int i = 1; i <= totalPage; i++)
                {
                    var aa = get(i);
                    foreach (var json in aa.GetProperty("data").GetProperty("list").EnumerateArray())
                    {
                        Lists.Add(new GachaLog(json));
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// 测试token是否可用 否返回无效报错
        /// </summary>
        /// <returns>错误类型</returns>
        public bool IsTokenUseful()
        {
            var userdata = WithNet.GetFromApi("https://as.hypergryph.com/user/info/v1/basic?token=" + Token);
            if (!userdata.TryGetProperty("error", out var error)) return false; //Token无效返回
            return true;
        }

        private void FreshData()
        {
            if (IsTokenUseful())
            {
                Oauth.Visibility = Visibility.Visible;
                return;
            }
            GetResult();
        }

        #region UI
        public UserData_Gacha()
        {
            InitializeComponent();
        }

        private void Fresh(object sender, RoutedEventArgs e)
        {
            FreshData();
        }

        #region
        private void FT_1(object sender, RoutedEventArgs e)
        {
            Process.Start("https://ak.hypergryph.com/user/login");
        }
        private void FT_2(object sender, RoutedEventArgs e)
        {
            Process.Start("https://web-api.hypergryph.com/account/info/hg");
        }
        #endregion
        private void FromTokenJson(object sender, RoutedEventArgs e)
        {
            void Error()
            {
                TokenOauthButton.Content = "输入错误，请重新输入......";
                Task.Run(() =>
                {
                    Thread.Sleep(2000);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (TokenOauthButton.Content.ToString() == "输入错误，请重新输入......")
                            TokenOauthButton.Content = "认证";
                    });
                });
            }
            try
            {
                var aa = JsonSerializer.Deserialize<JsonElement>(TokenJsonTextBox.Text);
                Token = aa.GetProperty("data").GetProperty("content").GetString();
                //FromToken.Visibility = Visibility.Collapsed;
                Oauth.Visibility = Visibility.Collapsed;
                //if (!IsTokenUseful()) { Error(); return; }
            }
            catch
            {
                Error();
                return;
            }
            pgb.Visibility = Visibility.Visible;
            Task.Run(() =>
            {
                GetResult();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Show();
                    pgb.Visibility = Visibility.Collapsed;
                });
            });

        }

        private void Show()
        {
            datagrid.ItemsSource = this.Lists;
            datagrid.Visibility = Visibility.Visible;
        }
        #endregion
    }
}
