using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.Json;
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
using Windows.UI.Xaml.Controls;
using Page = System.Windows.Controls.Page;

namespace ArkHelper.Xaml
{
    /// <summary>
    /// UserData_Gacha.xaml 的交互逻辑
    /// </summary>
    public partial class UserData_Gacha : Page
    {
        public class ArknightsUser
        {
            public string Name { get; set; }
            public string UID { get; set; }
            public ArknightsUser()
            {

            }
        }
        /// <summary>
        /// Token
        /// </summary>
        public static string Token = null;
        /// <summary>
        /// 抽卡列表
        /// </summary>
        public static List<JsonElement> Lists = new List<JsonElement>();
        /// <summary>
        /// 报错
        /// </summary>
        public enum errorKind
        {
            token_ineffective,
            token_null,
            password_error,
            need_password,
            CD,
            succeed,
            need_cap
        }


        /// <summary>
        /// 是否处于CD（避免被和谐）
        /// </summary>
        private static bool ableToGet = true;
        /// <summary>
        /// 根据账号密码获取token
        /// </summary>
        private static errorKind _FromUserAndPassword()
        {
            //获取token
            var request = new RestRequest
            {
                Method = Method.Post,
                RequestFormat = RestSharp.DataFormat.Json
            };
            var json = new JObject()
                    {
                        {"phone",Data.UserData.User },
                        {"password",Data.UserData.Password }
                    };
            request.AddParameter("", json, ParameterType.RequestBody);
            var client = new RestClient("https://as.hypergryph.com/user/auth/v1/token_by_phone_password");
            var result = client.Post(request);
            var getJson = JsonDocument.Parse(result.Content).RootElement;
            switch (getJson.GetProperty("status").GetInt32())
            {
                case 1:
                    return errorKind.need_cap;
                case 100:
                    return errorKind.password_error;
                case 0:
                    Token = getJson.GetProperty("data").GetProperty("token").GetString();
                    break;
            }
            return errorKind.succeed;
        }

        /// <summary>
        /// 获取单页抽卡列表
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        private static JsonElement get(int page)
        {
            //获取结果
            var client = new RestClient("https://ak.hypergryph.com/user/api/inquiry/gacha?page=" + page + "&token=" + Token + "&channelId=1");
            var result = client.Get(new RestRequest());
            return JsonDocument.Parse(result.Content).RootElement;
        }

        /// <summary>
        /// 根据token获取抽卡列表
        /// </summary>
        private static void GetResult()
        {
            ableToGet = false;
            Task.Run(() =>
            {
                Thread.Sleep(60000);
                ableToGet = true;
            });
            int totalPage = get(1).GetProperty("data").GetProperty("pagination").GetProperty("total").GetInt32();
            if (totalPage == 0) return;
            else
            {
                for (int i = 1; i <= totalPage; i++)
                {
                    var aa = get(i);
                    foreach (var json in aa.GetProperty("data").GetProperty("list").EnumerateArray())
                    {
                        Lists.Add(json);
                    }
                }
            }
        }

        /// <summary>
        /// 预处理返回错误情况
        /// </summary>
        /// <returns>错误类型</returns>
        public static errorKind Get()
        {
            if (!ableToGet) return errorKind.CD;
            if (Token == null)
            {
                if (Data.UserData.User == "" || Data.UserData.Password == "")
                {
                    return errorKind.need_password;
                }
                else
                {
                    var aab = _FromUserAndPassword();
                    if (aab != errorKind.succeed) return aab;
                }
            }

            //测试token
            var userdata = WithNet.GetFromApi("https://as.hypergryph.com/user/info/v1/basic?token=" + Token);
            if (!userdata.TryGetProperty("error", out var error)) return errorKind.token_ineffective;

            GetResult();
            return errorKind.succeed;
        }

        #region UI
        public UserData_Gacha()
        {
            InitializeComponent();
        }
        private void Fresh(object sender, RoutedEventArgs e)
        {
            _Fresh();
        }
        private void _Fresh()
        {
            Unable.Visibility = ableToGet ? Visibility.Collapsed : Visibility.Visible;
            var aaa = Get();
        }
        private void FromUserAndPassword(object sender, RoutedEventArgs e)
        {
            Data.UserData.User = User.Text;
            Data.UserData.Password = Password.Password;
            var aa = _FromUserAndPassword();
            switch (aa)
            {
                case errorKind.succeed:
                    FromUser.Visibility = Visibility.Collapsed;
                    break;
                case errorKind.password_error:
                    WithSystem.Message("账号或密码错误", "请重新输入");
                    break;
                case errorKind.need_cap:
                    WithSystem.Message("单位时间查询次数已达到上限", "请稍等");
                    break;
            }
        }
        #endregion


    }
}
