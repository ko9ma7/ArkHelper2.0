using Microsoft.Toolkit.Uwp.Notifications;
using RestSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq.Expressions;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Border = System.Windows.Controls.Border;
using Color = System.Windows.Media.Color;
using Image = System.Windows.Controls.Image;
using Point = System.Windows.Point;

namespace ArkHelper.Pages
{
    /// <summary>
    /// Message.xaml 的交互逻辑
    /// </summary>
    public partial class Message : Page
    {
        #region class
        public class User
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public string Avatar { get; set; }
            public ArkHelperDataStandard.MessageSource Source { get; set; }
            public string UID { get; set; }

            public User(ArkHelperDataStandard.MessageSource source, string uid)
            {
                Source = source;
                UID = uid;
                ID = Source + UID;
                switch (Source)
                {
                    // 微博
                    case ArkHelperDataStandard.MessageSource.weibo:
                        JsonElement __userinfo = Net.GetFromApi("https://m.weibo.cn/api/container/getIndex?type=uid&value=" + UID);
                    st:;
                        JsonElement _userinfo;
                        try
                        {
                            _userinfo = __userinfo.GetProperty("data").GetProperty("userInfo");
                        }
                        catch{ goto st; }

                        Avatar = _userinfo.GetProperty("profile_image_url").GetString();
                        Name = _userinfo.GetProperty("screen_name").GetString();
                        break;
                    case ArkHelperDataStandard.MessageSource.official_communication:
                        Avatar = null;//
                        Name = "制作组通讯";
                        break;
                }
                if (Source == ArkHelperDataStandard.MessageSource.neteaseMusic)
                {
                    var res = Net.GetFromApi("http://music.163.com/api/artist/albums/" + uid);
                    Avatar = res.GetProperty("artist").GetProperty("picUrl").GetString();
                    Name = res.GetProperty("artist").GetProperty("name").GetString();

                }
                if (Avatar != null)
                {
                    string[] _ava = Directory.GetFiles(Address.Cache.message, ID + "_avatar.*");
                    if (_ava.Length == 0)
                    {
                        Net.DownloadFile(Avatar, Address.Cache.message + "\\" + ID + "_avatar.jpg");
                    }
                    Avatar = Address.Cache.message + "\\" + ID + "_avatar.jpg";
                }
                GC.Collect();
            }

            /// <summary>
            /// 更新消息列表
            /// </summary>
            public List<ArkHelperMessage> UpdateMessage()
            {
                List<ArkHelperMessage> back = new List<ArkHelperMessage>();
                switch (Source)
                {
                    case ArkHelperDataStandard.MessageSource.weibo:
                        for (int page = 1; ; page++)
                        {
                            var _getresult = Net.GetFromApi("https://m.weibo.cn/api/container/getIndex?type=uid&value=" + UID + @"&containerid=107603" + UID + "&page=" + page);

                            if (_getresult.GetProperty("ok").GetInt16() == 0) break; //返回错误就停
                            var cards = _getresult.GetProperty("data").GetProperty("cards"); //提取卡片

                            foreach (var card in cards.EnumerateArray())
                            {
                                bool _top = false;
                                if (card.GetProperty("profile_type_id").GetString().Contains("top")) { _top = true; }
                                var json = card.GetProperty("mblog");

                                //交给构造函数解析
                                var message = new ArkHelperMessage(Source, json)
                                {
                                    IsTop = _top,
                                    /*
                                    User = this*/
                                };

                                back.Add(message);
                            }
                            if (page == 1) { break; }
                        }
                        break;
                    case ArkHelperDataStandard.MessageSource.official_communication:
                        var _aa = new RestClient("https://ak.hypergryph.com/news");
                        string _ab = "";
                    start:;
                        try
                        {
                            _ab = _aa.Get(new RestRequest { Method = Method.Get }).Content;
                        }
                        catch
                        {
                            goto start;
                        }

                        string _aaa = _ab.ToString();
                        var _aaaa = new ArkHelperMessage(Source, _aaa)
                        {
                            User = this
                        };
                        back.Add(_aaaa);
                        break;
                }
                if (Source == ArkHelperDataStandard.MessageSource.neteaseMusic)
                {
                    var res = Net.GetFromApi("http://music.163.com/api/artist/albums/32540734");
                    foreach (var item in res.GetProperty("hotAlbums").EnumerateArray())
                    {
                        var _aaaa = new ArkHelperMessage(Source, item)
                        {
                            User = this
                        };
                        back.Add(_aaaa);
                    }
                }
                GC.Collect();
                return back;
            }
        }
        public class ArkHelperMessage : IComparable<ArkHelperMessage>
        {
            public string ID { get; set; } //消息唯一识别号，由来源和ID构成
            public User User { get; set; }
            public DateTime CreateAt { get; set; } //发布时间
            public bool IsTop { get; set; } = false; //是否是置顶
            public ArkHelperMessage Repost { get; set; } //是否是转发消息，若是则包含转发地址，若否则为null
            public string Text { get; set; } //消息正文
            public List<string> Links { get; set; } = new List<string>();//包含的链接
            public List<Media> Medias { get; set; } = new List<Media>(); //包含的媒体
            public class Media
            {
                public string ID { set; get; }
                public string Small { get; set; }
                public string Link { get; set; }
                public MediaType Type { get; set; }
                public enum MediaType
                {
                    photo,
                    video
                }
                public Media(string viewPic, string link, MediaType type)
                {
                    Small = viewPic;
                    Link = link;
                    Type = type;
                }
                public Media() { }
            }

            //重写的CompareTo方法，时间排序
            public int CompareTo(ArkHelperMessage other)
            {
                if (null == other)
                {
                    return 1;
                }
                return other.CreateAt.CompareTo(this.CreateAt);//降序
            }

            /// <summary>
            /// 构造函数，解析json
            /// </summary>
            /// <param name="source">来源</param>
            /// <param name="json">消息</param>
            public ArkHelperMessage(ArkHelperDataStandard.MessageSource source, object content)
            {
                if (source == ArkHelperDataStandard.MessageSource.weibo)
                {
                    var json = (JsonElement)content;
                    if (json.GetProperty("isLongText").GetBoolean()
                        //||true
                        )
                    {
                        string response;
                    start:;
                        try
                        {
                            var client = new RestClient(@"https://m.weibo.cn/detail/" + json.GetProperty("id").GetString());
                            var request = new RestRequest { Method = Method.Get };
                            response = client.Get(request).Content;
                            int _a = response.IndexOf(@"""status""");
                            int _b = response.IndexOf(@"""call"":");
                            response = response.Substring(_a, _b - _a);
                            int _c = response.IndexOf(@"""status""");
                            response = response.Substring(_c, response.LastIndexOf(",") - _c);
                            response = "{" + response + "}";
                        }
                        catch
                        {
                            goto start;
                        }
                        json = JsonSerializer.Deserialize<JsonElement>(response).GetProperty("status");
                    }
                    //解析作者
                    var userID = json.GetProperty("user").GetProperty("id").GetDouble();
                    foreach (User user in UserList)
                    {
                        if (userID.ToString() == user.UID)
                        {
                            User = user;
                            goto userFindEnd;
                        }
                    }
                    User = new User(source, userID.ToString());
                userFindEnd:;
                    //解析时间
                    CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("en-US");
                    CreateAt = DateTime.ParseExact(json.GetProperty("created_at").GetString(), "ddd MMM d HH:mm:ss zz00 yyyy", cultureInfo);
                    //ID
                    ID = source.ToString() + json.GetProperty("id").GetString();

                    //获取文字
                    Text = json.GetProperty("text").GetString();

                    //链接
                    while (Text.Contains(@"href="""))
                    {
                        int _herfAddress = Text.IndexOf(@"href=""");
                        string link = Text.Substring(_herfAddress + 6, Text.IndexOf(@"""", _herfAddress + 10) - _herfAddress - 6);

                        if (!link.Contains(@"m.weibo.cn/p")
                            && !link.Contains(@"m.weibo.cn/search"))
                        {
                            var _decodeResult = HttpUtility.UrlDecode(link.Replace("https://weibo.cn/sinaurl?u=", ""));
                            Links.Add(_decodeResult);
                        }
                        Text = Text.Replace(@"href=""" + link + @"""", "");
                    }

                    //文字处理（html转段落）//future解决误杀正常文字
                    while (Text.Contains("<") && Text.Contains(">"))
                    {
                        Text = Text.Replace("<br />", "\n");
                        int _lef = Text.IndexOf("<");
                        if(_lef != -1)
                            Text = Text.Remove(_lef, Text.IndexOf(">") - _lef + 1); //换行
                        
                    }
                    Text = System.Net.WebUtility.HtmlDecode(Text);//从html反转义

                    //转发状态
                    if (json.TryGetProperty("retweeted_status", out var _ret))
                    {
                        var _resu = _ret.GetProperty("id").GetString();
                        foreach (var aa in Messages)
                        {
                            if (aa.ID == "weibo" + _resu)
                            {
                                Repost = aa;
                                goto repend;
                            }
                        }
                        Repost = new ArkHelperMessage(ArkHelperDataStandard.MessageSource.weibo, _ret);
                    repend:;
                    }

                    //图片/视频获取
                    if (json.TryGetProperty("pics", out var _pics))
                    {
                        var pics = _pics.EnumerateArray();
                        foreach (var item in pics)
                        {
                            var _large = item.GetProperty("large");
                            /*var _size = _large.GetProperty("geo");
                            int _hei = 0;
                            int _wid = 0;
                            var hej = _size.GetProperty("height");
                            var wij = _size.GetProperty("width");
                            try
                            {
                                _hei = Convert.ToInt32(hej.GetString());
                                _wid = Convert.ToInt32(wij.GetString());
                            }
                            catch
                            {
                                _hei = hej.GetInt32();
                                _wid = wij.GetInt32();
                            }*/
                            string url = _large.GetProperty("url").GetString();
                            Medias.Add(new Media(null, url, Media.MediaType.photo));
                        }
                    }
                    else
                    {
                        if (json.TryGetProperty("page_info", out var pageInfo) && Repost == null)
                        {
                            if (pageInfo.TryGetProperty("media_info", out var mediaInfo))
                            {
                                Media media = new Media()
                                {
                                    Type = Media.MediaType.video,
                                };
                                if (pageInfo.TryGetProperty("page_pic", out var pagePic))
                                {
                                    if (pagePic.TryGetProperty("url", out var _url))
                                    {
                                        string url = _url.GetString();
                                        media.Small = url;
                                    }
                                }
                                if (mediaInfo.TryGetProperty("stream_url_hd", out var streamUrl))
                                {
                                    media.Link = pageInfo.GetProperty("page_url").GetString();
                                }
                                Medias.Add(media);
                            }
                        }
                    }
                }
                if (source == ArkHelperDataStandard.MessageSource.official_communication)
                {
                    IsTop = true;

                    string origin = (string)content;

                    int _txadd = origin.IndexOf("制作组通讯#");

                    int num = Convert.ToInt32(origin.Substring(_txadd + 6, 2));
                    ID = source.ToString() + num;

                    int _herfadd = origin.Substring(0, _txadd).LastIndexOf(@"href=""");
                    string _html = origin.Substring(_herfadd + 6, origin.IndexOf(@"""", _herfadd + 10) - _herfadd - 6);
                    Links.Add(@"https://ak.hypergryph.com" + _html);

                    string _timearea = origin.Substring(_txadd - 120, 80);
                    int _classadd = _timearea.LastIndexOf(@"articleItemDate"">");
                    int _classendadd = _timearea.IndexOf(@"</span>");
                    string _createat = _timearea.Substring(_classadd + 17, _classendadd - _classadd - 17);
                    var _dt = DateTime.ParseExact(_createat, "yyyy-MM-dd", CultureInfo.CreateSpecificCulture("en-US"));
                    CreateAt = new DateTime(_dt.Year, _dt.Month, _dt.Day, (DateTime.Now.Hour > 16) ? 16 : 10, 0, 0);

                    Text = "第" + num + "期制作组通讯已经发布。";
                }
                if (source == ArkHelperDataStandard.MessageSource.neteaseMusic)
                {

                    JsonElement origin = (JsonElement)content;

                    var id = origin.GetProperty("id").GetInt64();
                    ID = source.ToString() + id;
                    Links.Add(@"https://music.163.com/#/album?id=" + id);
                    var name = origin.GetProperty("name").GetString();
                    Text = "发布了新的专辑" + "《" + name + "》。";

                    var time = origin.GetProperty("publishTime").GetInt64();
                    System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                    CreateAt = dtDateTime.AddMilliseconds(time).ToLocalTime();
                }

                //处理图片确定缩略图
                foreach (Media media in Medias)
                {
                    //视频缩略图即为封面图
                    if (media.Type == Media.MediaType.video)
                    {
                        string filename = media.Small.Substring(media.Small.LastIndexOf(@"/") + 1);

                        string smallfile = Address.Cache.message + "\\" + "small_" + filename;
                        if (!File.Exists(smallfile))
                        {
                            Net.DownloadFile(media.Small, smallfile);
                        }
                        media.Small = smallfile;
                    }

                    //图片缩略图判断
                    if (media.Type == Media.MediaType.photo)
                    {
                        string filename = media.Link.Substring(media.Link.LastIndexOf(@"/") + 1);

                        string file = Address.Cache.message + "\\" + filename;
                        if (!File.Exists(file))
                        {
                            Net.DownloadFile(media.Link, file);
                        }
                        media.Link = file;

                        string smallfile = Address.Cache.message + "\\" + "small_" + filename;
                        if (!File.Exists(smallfile))
                        {
                            CreateViewPic(file, smallfile);
                        }
                        media.Small = smallfile;
                    }

                    //制作缩略图
                    void CreateViewPic(string inputAddress, string outputAddress)
                    {
                        int size = 200;
                        using (Bitmap input = new Bitmap(inputAddress))
                        using (Bitmap output = new Bitmap(size, size))
                        {
                            int a = input.Width > input.Height ? input.Height : input.Width;
                            var area = input.Width > input.Height ?
                                new Rectangle((input.Width - a) / 2, 0, a, a)
                                : new Rectangle(0, (input.Height - a) / 2, a, a);
                            using (Graphics g = Graphics.FromImage(output))
                            {
                                g.DrawImage(input, new Rectangle(0, 0, size, size),
                                      area,
                                      GraphicsUnit.Pixel);
                            };
                            output.Save(outputAddress);
                        }
                        WithSystem.GarbageCollect();
                    }
                }
            }
            public ArkHelperMessage() { }
        }
        #endregion

        #region 更新消息
        static ArrayList UserList;
        static bool firstUpdate = true;
        public static Task MessageInit = new Task(() =>
        {
            UserList = new ArrayList
            {
                new User(ArkHelperDataStandard.MessageSource.weibo, "7745672941"),//END
                new User(ArkHelperDataStandard.MessageSource.weibo, "6441489862"),//CHO
                new User(ArkHelperDataStandard.MessageSource.weibo, "7499841383"),//TER
                new User(ArkHelperDataStandard.MessageSource.weibo, "7506039414"),//MOU
                new User(ArkHelperDataStandard.MessageSource.weibo, "7461423907"),//HYP
                new User(ArkHelperDataStandard.MessageSource.weibo, "6279793937"),//ARK
                new User(ArkHelperDataStandard.MessageSource.weibo, "7753678921"),//GAW
                new User(ArkHelperDataStandard.MessageSource.weibo, "2954409082"),//PLG
                new User(ArkHelperDataStandard.MessageSource.official_communication,""), //COM
                new User(ArkHelperDataStandard.MessageSource.neteaseMusic,"32540734"), //MSR
                //new Pages.Message.User(ArkHelperDataStandard.MessageSource.weibo, "7784464307") //test
            };

            for (; ; Thread.Sleep(20000))
            {
                var createat = DateTime.Now;
                if (!firstUpdate) createat = Messages[0].CreateAt;

                foreach (User user in UserList)
                {
                    foreach (ArkHelperMessage message in user.UpdateMessage())
                    {
                        //加入消息池
                        if (!Messages.Exists(mes => mes.ID == message.ID))
                            if (!message.Text.Contains("对本次抽奖进行监督，结果公正有效。公示链接："))
                                if ((DateTime.Now - message.CreateAt) < new TimeSpan(60, 0, 0, 0, 0))
                                {
                                    Messages.Add(message);

                                    //更新通知
                                    if (!firstUpdate)
                                    {
                                        ToastContentBuilder Toast = new ToastContentBuilder();
                                        Toast.AddArgument("kind", "Message");
                                        Toast.AddText(user.Name + "发布了新的动态");
                                        Toast.AddText(message.Text);
                                        Toast.AddCustomTimeStamp(message.CreateAt);

                                        if (message.Medias.Count > 0)
                                        {
                                            var me = message.Medias[0];
                                            switch (me.Type)
                                            {
                                                case ArkHelperMessage.Media.MediaType.photo:
                                                    Toast.AddHeroImage(new Uri(me.Link));
                                                    break;
                                                case ArkHelperMessage.Media.MediaType.video:
                                                    Toast.AddHeroImage(new Uri(me.Small));
                                                    break;
                                            }
                                        }

                                        Toast.Show(tag =>
                                        {
                                            tag.Tag = "Message";
                                        });
                                    }
                                }
                    }
                }
                Messages.Sort();
                ////if (messages.Count > 20) { messages.RemoveRange(19, messages.Count - 19); }

                try { MessageInited(); } catch { }

                firstUpdate = false;
            }
        });
        static List<ArkHelperMessage> Messages = new List<ArkHelperMessage>();
        public delegate void MessageInitPointer();
        public static event MessageInitPointer MessageInited;
        #endregion

        #region 页面更新
        private bool ableToInit = true;
        public Message()
        {
            InitializeComponent();
            ReadyToInitFromBlank();
            if (!firstUpdate) InitFromList();
        }
        public int AlreadyInitedCards = 0;
        public List<DateTime> DTList = new List<DateTime>();
        private void ReadyToInitFromBlank()
        {
            if (App.Data.message.status)
            {
                pgb.Visibility = Visibility.Visible;
                off.Visibility = Visibility.Collapsed;
                cancel.Visibility = Visibility.Visible;
            }
            else
            {
                pgb.Visibility = Visibility.Collapsed;
                off.Visibility = Visibility.Visible;
                cancel.Visibility = Visibility.Collapsed;
            }
        }
        private void InitCard(int num = 3)
        {
            for (int i = 0; i < num; i++)
            {
                //pgb.Value = i / num;
                if (AlreadyInitedCards >= Messages.Count) break;
                ArkHelperMessage message = Messages[AlreadyInitedCards];
                var messageCard = MakeMessageCard(message);

                DockPanel.SetDock(messageCard, Dock.Top);
                MessageListDock.Children.Add(messageCard);

                bool _exist = false;
                foreach (DateTime dt in DTList)
                {
                    if (dt.Day == message.CreateAt.Day && dt.Month == message.CreateAt.Month)
                    {
                        _exist = true;
                        break;
                    }
                }

                if (!_exist)
                {
                    DTList.Add(message.CreateAt);
                    RadioButton radioButton = new RadioButton
                    {
                        Width = 220,
                        Tag = messageCard,
                        ContentStringFormat = ((message.CreateAt.Year != DateTime.Now.Year) ? (message.CreateAt.Year + "年") : "") + message.CreateAt.Month + "月" + message.CreateAt.Day + "日",
                        Style = (System.Windows.Style)FindResource("RadioButtonDock")
                    };
                    radioButton.Click += Btn_Click;
                    if (AlreadyInitedCards == 0) { radioButton.IsChecked = true; };
                    UserListXaml.Children.Add(radioButton);
                }
                AlreadyInitedCards++;
            }
        }
        /// <summary>
        /// 构建卡片
        /// </summary>
        /// <param name="message"></param>
        Border MakeMessageCard(ArkHelperMessage message)
        {
            StackPanel CreateMessageWppel(ArkHelperMessage _message)
            {
                //用户头像
                var userAvatar = GetUserAvatar(_message.User);

                //链接
                var _linkListBox = new ListBox();
                foreach (string link in _message.Links)
                {
                    var linkListBoxItem = new ListBoxItem()
                    {
                        Content = link,
                        Tag = link
                    };
                    linkListBoxItem.MouseLeftButtonUp += LinkShow;
                    _linkListBox.Items.Add(linkListBoxItem);
                }
                var linkBox = new MaterialDesignThemes.Wpf.PopupBox
                {
                    Width = 40,
                    Height = 40,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    StaysOpen = false,
                    PopupContent = _linkListBox
                };
                if (_message.Links.Count == 0) linkBox.Visibility = Visibility.Collapsed;

                //标头
                var head = new Grid()
                {
                    Children =
                    {
                        linkBox,
                        new WrapPanel()
                        {
                            Children =
                            {
                                //用户头像
                                userAvatar,
                                //标头文字
                                new TextBlock()
                                {
                                    Margin = new Thickness(10,0,0,0),
                                    VerticalAlignment = VerticalAlignment.Center,
                                    Inlines =
                                    {
                                        new Run()
                                        {
                                            Text = _message.User.Name,
                                            FontSize = 15,
                                        },
                                        new LineBreak(),
                                        new Run()
                                        {
                                            FontSize = 12,
                                            Foreground = new SolidColorBrush(Color.FromRgb(128,128,128)),
                                            Text = _message.CreateAt.ToString("M")+_message.CreateAt.ToString("HH:mm")
                                        }
                                    }
                                },
                            }
                        },
                    }
                };

                //图片
                var imageWrapPanel = new WrapPanel()
                {
                    Margin = new Thickness(0, 5, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Left
                };

                if (_message.Medias.Count > 3)
                {
                    imageWrapPanel.MaxWidth = 315;
                }
                else
                {
                    imageWrapPanel.MaxWidth = 470;
                }

                foreach (var media in _message.Medias)
                {
                    Image image = new Image()
                    {
                        Tag = media.Link,
                        Margin = new Thickness(0, 0, 5, 5),
                        Cursor = System.Windows.Input.Cursors.Hand,
                    };

                    bool isUsingBigImage = false;
                    var bitimg = GetBitmapImage(media.Small);

                    if (media.Type == ArkHelperMessage.Media.MediaType.video)
                    {
                        isUsingBigImage = true;
                    }
                    else
                    {
                        if (_message.Medias.Count == 1)
                        {
                            var _bitimg = GetBitmapImage(media.Link);
                            if (_bitimg.Height <= _bitimg.Width)
                            {
                                bitimg = _bitimg;
                                isUsingBigImage = true;
                                if (_bitimg.Height == _bitimg.Width)
                                {
                                    image.MaxWidth = 300;
                                }
                            }
                        }
                    }

                    if (!isUsingBigImage)
                    {
                        image.Width = image.Height = 100;
                    }

                    image.Source = bitimg;

                    image.MouseLeftButtonUp += PictureShow;
                    imageWrapPanel.Children.Add(image);

                    WithSystem.GarbageCollect();
                }

                //单消息卡片
                var _wr = new StackPanel()
                {
                    Orientation = Orientation.Vertical,
                    Margin = new Thickness(20, 12, 20, 15),
                    Children =
                    {
                        //标头
                        head,
                        
                        //主体
                        new WrapPanel()
                        {
                            Margin=new Thickness(0,10,0,0),
                            Orientation = Orientation.Vertical,
                            Children =
                            {
                                new TextBlock()
                                {
                                    TextWrapping = TextWrapping.Wrap,
                                    Text = _message.Text,
                                    FontSize = 14
                                },
                                imageWrapPanel
                            }
                        }
                    }
                };
                return _wr;
            }

            StackPanel mainMessageWrapPanel = CreateMessageWppel(message);

            //转发生成卡片并加入
            if (message.Repost != null)
            {
                StackPanel repostMessageWrapPanel = CreateMessageWppel(message.Repost);
                DockPanel repostArea = new DockPanel() { LastChildFill = true };
                Border repostLine = new Border()
                {
                    Width = 2,
                    //Margin = new Thickness(0, 5, 0, 5),
                    Background = new SolidColorBrush(Color.FromRgb(211, 211, 211))
                };
                DockPanel.SetDock(repostLine, Dock.Left);
                repostArea.Children.Add(repostLine);
                DockPanel.SetDock(repostMessageWrapPanel, Dock.Left);
                repostArea.Children.Add(repostMessageWrapPanel);
                mainMessageWrapPanel.Children.Add(repostArea);
            }

            //最后包装
            var endBorder = new Border
            {
                Margin = new Thickness(0, 0, 0, 10),
                Background = new SolidColorBrush(Color.FromRgb(240, 248, 255)),
                Child = mainMessageWrapPanel,
                Style = (System.Windows.Style)FindResource("card")
            };

            return endBorder;
        }
        private void InitFromList()
        {
            if (!App.Data.message.status || !ableToInit) return;
            ableToInit = false;
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageListDock.Children.Clear();
                pgb.IsIndeterminate = false;
                InitCard(3);
                pgb.Visibility = Visibility.Collapsed;
            });
        }
        #endregion

        #region 图片
        private List<BitmapImage> UserAvatarList = new List<BitmapImage>();
        Image GetUserAvatar(User user)
        {
            BitmapImage bitImage;
            foreach (BitmapImage avatarInList in UserAvatarList)
                if (avatarInList.UriSource.AbsoluteUri == user.Avatar)
                {
                    bitImage = avatarInList;
                    goto bitmapImageInited;//如果列表已有则直接生成Image
                }
            //如果列表没有则主动生成
            string bitImageUri = user.Avatar == null ? Address.res + "\\normal_avatar.jpg" : user.Avatar;
            bitImage = new BitmapImage(new Uri(bitImageUri));
            bitImage.Freeze();
            UserAvatarList.Add(bitImage);

        bitmapImageInited:;
            Image avatar = new Image()
            {
                Height = 36,
                Width = 36,
                VerticalAlignment = VerticalAlignment.Center,
                Tag = user,
                Source = bitImage,
                Clip = new RectangleGeometry()
                {
                    RadiusY = 18,
                    RadiusX = 18,
                    Rect = new Rect(0, 0, 36, 36)
                }
            };

            return avatar;
        }

        private List<BitmapImage> BitmapImageList = new List<BitmapImage>();
        BitmapImage GetBitmapImage(string absoluteUri)
        {
            foreach (BitmapImage bitImageInList in BitmapImageList)
                if (bitImageInList.UriSource.AbsoluteUri == absoluteUri)
                {
                    return bitImageInList;
                }

            var bitImage = new BitmapImage(new Uri(absoluteUri));
            bitImage.Freeze();
            BitmapImageList.Add(bitImage);

            return bitImage;
        }

        #endregion
        #region 页面响应
        /// <summary>
        /// 按时间导航
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            Border messageCard = (Border)((RadioButton)sender).Tag;
            var scrollViewerOffset = sc.VerticalOffset;
            var point = new Point(0, scrollViewerOffset);
            var tarPos = messageCard.TransformToVisual(sc).Transform(point);
            sc.ScrollToVerticalOffset(tarPos.Y);
        }
        /// <summary>
        /// 点击图片展示大图/点击视频打开浏览器播放
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PictureShow(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            System.Windows.Controls.Image img = (System.Windows.Controls.Image)sender;
            Process.Start(img.Tag.ToString());
        }
        /// <summary>
        /// 点击链接打开浏览器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void LinkShow(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ListBoxItem img = (ListBoxItem)sender;
            Process.Start(img.Tag.ToString());
        }
        /// <summary>
        /// 滑动到底部更新卡片
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            double dVer = sc.VerticalOffset;
            double vViewport = sc.ViewportHeight;
            double eextent = sc.ExtentHeight;
            bool isBottom;
            if (dVer != 0)
            {
                if (dVer + vViewport == eextent)
                {
                    isBottom = true;
                }
                else
                {
                    isBottom = false;
                }
            }
            else
            {
                isBottom = false;
            }
            if (isBottom) { InitCard(); }
        }
        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageInit.Start();
            }
            catch { }
            App.Data.message.status = true;
            ReadyToInitFromBlank();
            if (!firstUpdate) InitFromList();
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            App.Data.message.status = false;
            WithSystem.Message("功能已禁用", "ArkHelper下次启动时生效");
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            MessageInited += InitFromList;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            MessageInited -= InitFromList;
        }
        #endregion
    }
}