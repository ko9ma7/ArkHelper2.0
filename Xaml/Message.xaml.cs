using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
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
        public int AlreadyInitedCards = 0;
        public List<DateTime> DTList = new List<DateTime>();
        public List<ArkHelperMessage> Messages = new List<ArkHelperMessage>();
        public Message()
        {
            InitializeComponent();
            InitFromBlank();
        }

        private void InitFromBlank()
        {
            Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    pgb.Visibility = Visibility.Visible;
                    MessageListDock.Children.Clear();
                    UserListXaml.Children.Clear();
                });
                while (!App.isMessageInited)
                {
                    Thread.Sleep(2000);
                }
                foreach (ArkHelperMessage akms in App.messages)
                {
                    Messages.Add(akms);
                }
                Messages.Sort();

                Thread.Sleep(300);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    pgb.IsIndeterminate = false;
                    InitCard(3);
                    pgb.Visibility = Visibility.Collapsed;
                });
            });
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
                        ContentStringFormat = message.CreateAt.Month + "月" + message.CreateAt.Day + "日",
                        Style = (System.Windows.Style)FindResource("RadioButtonDock")
                    };
                    radioButton.Click += Btn_Click;
                    if (AlreadyInitedCards == 0) { radioButton.IsChecked = true; };
                    UserListXaml.Children.Add(radioButton);
                }
                AlreadyInitedCards++;
            }
        }

        #region 模板
        public class User
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public string Avatar { get; set; }
            public UniData.MessageSource Source { get; set; }
            public string UID { get; set; }

            public User(UniData.MessageSource source, string uid)
            {
                Source = source;
                UID = uid;
                ID = Source + UID;
                switch (Source)
                {
                    // 微博
                    case UniData.MessageSource.weibo:
                        JsonElement _userinfo = WithNet.GetFromApi("https://m.weibo.cn/api/container/getIndex?type=uid&value=" + UID).GetProperty("data").GetProperty("userInfo");
                        Avatar = _userinfo.GetProperty("profile_image_url").GetString();
                        Name = _userinfo.GetProperty("screen_name").GetString();
                        break;
                    case UniData.MessageSource.official_communication:

                        Avatar = null;//
                        Name = "制作组通讯";
                        break;
                }
                if (Avatar != null)
                {
                    string[] _ava = Directory.GetFiles(Address.Cache.message, ID + "_avatar.*");
                    if (_ava.Length == 0)
                    {
                        WithNet.DownloadFile(Avatar, Address.Cache.message + "\\" + ID + "_avatar.jpg");
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
                    case UniData.MessageSource.weibo:
                        for (int page = 1; ; page++)
                        {
                            var _getresult = WithNet.GetFromApi("https://m.weibo.cn/api/container/getIndex?type=uid&value=" + UID + @"&containerid=107603" + UID + "&page=" + page);

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
                    case UniData.MessageSource.official_communication:
                        var _aa = new RestClient("https://ak.hypergryph.com/news");
                        var _ab = _aa.Get(new RestRequest { Method = Method.Get });
                        string _aaa = _ab.Content.ToString();
                        var _aaaa = new ArkHelperMessage(Source, _aaa)
                        {
                            User = this
                        };
                        back.Add(_aaaa);
                        break;

                    default:
                        break;
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
                public int Height { get; set; }
                public int Width { get; set; }
                public enum MediaType
                {
                    photo,
                    video
                }
                public Media(string viewPic, string link, MediaType type, int height, int width)
                {
                    Small = viewPic;
                    Link = link;
                    Type = type;
                    Height = height;
                    Width = width;
                }
                public Media() { }
            }

            //重写的CompareTo方法，时间排序
            public int CompareTo(ArkHelperMessage other)
            {
                if (null == other)
                {
                    return 1;//空值比较大，返回1
                }
                //return this.Id.CompareTo(other.Id);//升序
                return other.CreateAt.CompareTo(this.CreateAt);//降序
            }

            /// <summary>
            /// 构造函数，解析json
            /// </summary>
            /// <param name="source">来源</param>
            /// <param name="json">消息</param>
            public ArkHelperMessage(UniData.MessageSource source, object content)
            {
                if (source == UniData.MessageSource.weibo)
                {
                    var json = (JsonElement)content;
                    if (json.GetProperty("isLongText").GetBoolean())
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
                    var _au = json.GetProperty("user");
                    var _aua = _au.GetProperty("id");
                    var _aub = _aua.GetDouble();
                    foreach (User user in App.UserList)
                    {
                        if (_aub.ToString() == user.UID)
                        {
                            User = user;
                            goto auend;
                        }
                    }
                    User = new User(source, _aub.ToString());
                auend:;
                    //解析时间
                    CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("en-US");
                    CreateAt = DateTime.ParseExact(json.GetProperty("created_at").GetString(), "ddd MMM d HH:mm:ss zz00 yyyy", cultureInfo);
                    //ID
                    ID = source.ToString() + json.GetProperty("id").GetString();

                    //获取文字
                    Text = json.GetProperty("text").GetString();

                    var links = new List<string>();
                    while (Text.Contains(@"href="""))
                    {
                        int _herfadd = Text.IndexOf(@"href=""");
                        string _html = Text.Substring(_herfadd + 6, Text.IndexOf(@"""", _herfadd + 10) - _herfadd - 6);

                        links.Add(_html);
                        Text = Text.Replace(@"href=""" + _html + @"""", "");
                    }
                    foreach (var a in links)
                    {
                        if (a.Contains(@"m.weibo.cn/p")
                            || a.Contains(@"m.weibo.cn/search"))
                        {

                        }
                        else
                        {
                            var ab = a.Replace("https://weibo.cn/sinaurl?u=", "").Replace("%3A", ":").Replace("%2F", "/");
                            Links.Add(ab);
                        }
                    }
                    //文字处理（html转段落）//future解决误杀正常文字
                    while (Text.Contains("<") && Text.Contains(">"))
                    {
                        Text = Text.Replace("<br />", "\n");
                        int _lef = Text.IndexOf("<");
                        try
                        {
                            Text = Text.Remove(_lef, Text.IndexOf(">") - _lef + 1); //换行
                        }
                        catch { }
                    }
                    Text = Text.Replace("&lt;", "<").Replace("&gt;", ">").Replace("&quot;", @"""").Replace("&amp;", "&");//从html反转义 // future
                    //转发状态
                    if (json.TryGetProperty("retweeted_status", out var _ret))
                    {
                        var _resu = _ret.GetProperty("id").GetString();
                        foreach (var aa in App.messages)
                        {
                            if (aa.ID == "weibo" + _resu)
                            {
                                Repost = aa;
                                goto repend;
                            }
                        }
                        Repost = new ArkHelperMessage(UniData.MessageSource.weibo, _ret);
                    repend:;
                    }
                    //图片/视频获取
                    if (json.TryGetProperty("pics", out var _pics))
                    {
                        var pics = _pics.EnumerateArray();
                        foreach (var item in pics)
                        {
                            var _large = item.GetProperty("large");
                            var _size = _large.GetProperty("geo");
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
                            }
                            string url = _large.GetProperty("url").GetString();
                            Medias.Add(new Media(null, url, Media.MediaType.photo, _hei, _wid));
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
                                    media.Link = streamUrl.GetString();
                                }
                                Medias.Add(media);
                            }
                        }
                    }
                    goto end;
                }
                if (source == UniData.MessageSource.official_communication)
                {
                    int num = 0;

                    ID = source.ToString() + num;
                    IsTop = true;

                    string origin = (string)content;

                    int _txadd = origin.IndexOf("制作组通讯#");

                    num = Convert.ToInt32(origin.Substring(_txadd + 6, 2));

                    int _herfadd = origin.Substring(0, _txadd).LastIndexOf(@"href=""");
                    string _html = origin.Substring(_herfadd + 6, origin.IndexOf(@"""", _herfadd + 10) - _herfadd - 6);
                    Links.Add(@"https://ak.hypergryph.com" + _html);

                    string _timearea = origin.Substring(_txadd - 120, 80);
                    int _classadd = _timearea.LastIndexOf(@"articleItemDate"">");
                    int _classendadd = _timearea.IndexOf(@"</span>");
                    string _createat = _timearea.Substring(_classadd + 17, _classendadd - _classadd - 17);
                    var _dt = DateTime.ParseExact(_createat, "yyyy-MM-dd", CultureInfo.CreateSpecificCulture("en-US"));
                    CreateAt = new DateTime(_dt.Year, _dt.Month, _dt.Day, 16, 0, 0);

                    Text = "第" + num + "期制作组通讯已经发布。";

                    goto end;
                }

            end:;
                //处理图片确定缩略图
                foreach (Media media1 in Medias)
                {
                    switch (media1.Type)
                    {
                        //视频缩略图即为封面图
                        case Media.MediaType.video:
                            string filename0 = media1.Small.Substring(media1.Small.LastIndexOf(@"/") + 1);

                            string smallfile0 = Address.Cache.message + "\\" + "small_" + filename0;
                            if (!File.Exists(smallfile0))
                            {
                                WithNet.DownloadFile(media1.Small, smallfile0);
                            }
                            media1.Small = smallfile0;

                            break;

                        //图片缩略图
                        case Media.MediaType.photo:
                            string filename1 = media1.Link.Substring(media1.Link.LastIndexOf(@"/") + 1);

                            string file1 = Address.Cache.message + "\\" + filename1;
                            if (!File.Exists(file1))
                            {
                                WithNet.DownloadFile(media1.Link, file1);
                            }
                            media1.Link = file1;

                            string smallfile1 = Address.Cache.message + "\\" + "small_" + filename1;
                            if (!File.Exists(smallfile1))
                            {
                                CreateViewPic(file1, smallfile1);
                            }
                            bool _longpic = (media1.Height > 400 && media1.Width < media1.Height);
                            media1.Small = (Medias.Count == 1 && !_longpic) ? file1 : smallfile1;

                            break;
                    }
                }
                void CreateViewPic(string inputAddress, string outputAddress)
                {
                    //制作缩略图
                    int size = 200;
                    Bitmap input = new Bitmap(inputAddress);
                    var output = new Bitmap(size, size);

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

                    input.Dispose();
                    output.Dispose();
                    WithSystem.GarbageCollect();
                }
            }
            public ArkHelperMessage() { }
        }
        #endregion

        /// <summary>
        /// 构建message卡片
        /// </summary>
        /// <param name="message"></param>
        Border MakeMessageCard(ArkHelperMessage message)
        {
            StackPanel CreateMessageWppel(ArkHelperMessage _message)
            {
                //图片
                var _pic = new WrapPanel()
                {
                    Margin = new Thickness(0, 5, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                foreach (var media in _message.Medias)
                {
                    System.Windows.Controls.Image imageControl = new System.Windows.Controls.Image()
                    {
                        Tag = media.Link,
                        Margin = new Thickness(0, 0, 5, 5),
                        Cursor = System.Windows.Input.Cursors.Hand,
                    };

                    var bitimg = new BitmapImage(new Uri(media.Small, UriKind.Absolute));
                    if (_message.Medias.Count == 1 && bitimg.Height > 200)
                    {
                        _pic.MaxWidth = 500;
                    }
                    else
                    {
                        imageControl.Height = imageControl.Width = 100;
                        _pic.MaxWidth = 315;
                    }
                    bitimg.Freeze();
                    imageControl.Source = bitimg;

                    imageControl.MouseLeftButtonUp += PictureShow;
                    _pic.Children.Add(imageControl);

                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                }

                //链接
                var lstb = new ListBox();
                foreach (string link in _message.Links)
                {
                    var _aa = new ListBoxItem()
                    {
                        Content = link,
                        Tag = link
                    };
                    _aa.MouseLeftButtonUp += LinkShow;
                    lstb.Items.Add(_aa);
                }
                var ppbox = new MaterialDesignThemes.Wpf.PopupBox
                {
                    Width = 40,
                    Height = 40,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    StaysOpen = false,
                    PopupContent = lstb
                };
                if (_message.Links.Count == 0) ppbox.Visibility = Visibility.Collapsed;

                //单消息卡片
                Image _image = new Image()
                {
                    Height = 36,
                    Width = 36,
                    VerticalAlignment = VerticalAlignment.Center,
                    Clip = new RectangleGeometry()
                    {
                        RadiusY = 18,
                        RadiusX = 18,
                        Rect = new Rect(0, 0, 36, 36)
                    }
                };
                if (_message.User.Avatar != null)
                {
                    _image.Source = new BitmapImage(new Uri(_message.User.Avatar));
                }
                else
                {
                    _image.Source = new BitmapImage(new Uri(Address.res + "\\normal_avatar.jpg"));
                }
                var _wr = new StackPanel()
                {
                    Orientation = Orientation.Vertical,
                    Margin = new Thickness(20, 12, 20, 15),
                    Children =
                    {
                        //标头区域
                        new Grid()
                        {
                            Children =
                            {
                                ppbox,
                                new WrapPanel()
                                {
                                    Children =
                                    {
                                        //用户头像
                                        _image,
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
                        },
                        
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
                                _pic
                            }
                        }
                    }
                };
                return _wr;
            }
            StackPanel _wrmain = CreateMessageWppel(message);

            //转发生成卡片并加入
            if (message.Repost != null)
            {
                StackPanel _wrpost = CreateMessageWppel(message.Repost);
                DockPanel dockPanel = new DockPanel() { LastChildFill = true };
                Border border = new Border()
                {
                    Width = 2,
                    Margin = new Thickness(0, 5, 0, 5),
                    Background = new SolidColorBrush(Color.FromRgb(211, 211, 211))
                };
                DockPanel.SetDock(border, Dock.Left);
                dockPanel.Children.Add(border);
                DockPanel.SetDock(_wrpost, Dock.Left);
                dockPanel.Children.Add(_wrpost);
                _wrmain.Children.Add(dockPanel);
            }

            //最后包装
            var _bo = new Border
            {
                Margin = new Thickness(0, 0, 0, 10),
                Background = new SolidColorBrush(Color.FromRgb(240, 248, 255)),
                Child = _wrmain,
                Style = (System.Windows.Style)FindResource("card")
            };

            return _bo;
        }

        #region 页面响应
        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            Border messageCard = (Border)((RadioButton)sender).Tag;
            var scrollViewerOffset = sc.VerticalOffset;
            var point = new Point(0, scrollViewerOffset);
            var tarPos = messageCard.TransformToVisual(sc).Transform(point);
            sc.ScrollToVerticalOffset(tarPos.Y);
        }
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            WithSystem.GarbageCollect();
        }
        void PictureShow(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            System.Windows.Controls.Image img = (System.Windows.Controls.Image)sender;
            Process.Start(img.Tag.ToString());
        }
        void LinkShow(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ListBoxItem img = (ListBoxItem)sender;
            Process.Start(img.Tag.ToString());
        }
        #endregion

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

        private void Border_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            InitFromBlank();
        }
    }

}
