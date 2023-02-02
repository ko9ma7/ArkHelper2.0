using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace ArkHelper.Xaml.Control
{
    /// <summary>
    /// SelectButton.xaml 的交互逻辑
    /// </summary>
    public partial class SelectButton : UserControl
    {
        /// <summary>
        /// 图标
        /// </summary>
        public MaterialDesignThemes.Wpf.PackIconKind Icon { get; set; }

        /// <summary>
        /// 文本
        /// </summary>
        public string Text { get; set; }
        #region 属性


        /// <summary>
        /// 是否在右侧显示ProgressBar
        /// </summary>
        public bool IsHaveProgressBar
        {
            get
            {
                return _isHaveProgressBar;
            }
            set
            {
                _isHaveProgressBar = value;
                if (value)
                {
                    UIProgressBar.Visibility = Visibility.Visible;
                }
                else
                {
                    UIProgressBar.Visibility = Visibility.Collapsed;
                }
            }
        }
        private bool _isHaveProgressBar = false;

        /// <summary>
        /// ProgressBar是否是静态的
        /// </summary>
        public bool IsProgressBarStatic
        {
            get
            {
                return _isProgressBarStatic;
            }
            set
            {
                _isProgressBarStatic = value;
                UIProgressBar.IsIndeterminate = !value;
            }
        }
        private bool _isProgressBarStatic = true;

        /// <summary>
        /// ProgressBar的值
        /// </summary>
        public int ProgressBarValue
        {
            get
            {
                return _ProgressBarValue;
            }
            set
            {
                if (value <= 100 && value >= 0)
                {
                    _ProgressBarValue = value;
                    UIProgressBar.Value = value;
                }
            }
        }
        private int _ProgressBarValue = 0;

        /// <summary>
        /// （手动继承RadioButton）IsChecked
        /// </summary>
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                _isChecked = value;
                ChangeStyle();
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(this.Parent); i++)
                {
                    var child = VisualTreeHelper.GetChild(this.Parent, i);
                    if (child != this)
                        if (child is SelectButton)
                            if ((child as SelectButton).IsChecked)
                                (child as SelectButton).IsChecked = false;
                }
            }
        }
        private bool _isChecked = false;

        /*
        public UIElement ChildWhenThisIsPressed
        {
            get { return _childWhenThisIsPressed; }
            set
            {
                _childWhenThisIsPressed = value;
                UIChildWhenThisIsPressed.Child = value;
            }
        }
        private UIElement _childWhenThisIsPressed;
        */

        #endregion

        #region （手动继承RadioButton）实现_事件_Click
        public event RoutedEventHandler Click;
        bool _isPressing = false;
        /// <summary>
        /// 在this构造时执行，注册点击事件
        /// </summary>
        void BeginListenClickEvent()
        {
            this.MouseLeftButtonDown += (o, e) =>
            {
                Task.Run(() =>
                {
                    Thread.Sleep(500);
                    _isPressing = false;
                });
                _isPressing = true;
            };
            this.MouseLeftButtonUp += (o, e) =>
            {
                if (_isPressing)
                {
                    try
                    {
                        Click(this, new RoutedEventArgs());
                    }
                    catch (NullReferenceException ex)
                    {

                    }
                    IsChecked = true;
                }
                _isPressing = false;
            };
        }
        #endregion

        #region 动画和外观
        /// <summary>
        /// 切换外观
        /// </summary>
        void ChangeStyle()
        {
            int style = 0;
            if (this.IsChecked && this.IsEnabled) style = 0;
            if (this.IsChecked && !this.IsEnabled) style = 1;
            if (!this.IsChecked && this.IsEnabled) style = 2;
            if (!this.IsChecked && !this.IsEnabled) style = 3;

            string _bgColor = "";
            string _contentColor = "";
            string _pgbColor = IsEnabled ? "#006493" : "#b5ceda";
            switch (style)
            {
                case 0:
                    _bgColor = "#CAE6FF";
                    _contentColor = "#006493";
                    break;
                case 1:
                    _bgColor = "#f1f9ff";
                    _contentColor = "#b5ceda";
                    break;
                case 2:
                    _bgColor = "#FFFFFF";
                    _contentColor = "#000000";
                    break;
                case 3:
                    _bgColor = "#FFFFFF";
                    _contentColor = "#999999";
                    break;
            }

            Color getBrush(string html)
            {
                var color = System.Drawing.ColorTranslator.FromHtml(html);
                var _mediaColor = new Color();
                _mediaColor.A = 255;
                _mediaColor.R = color.R;
                _mediaColor.G = color.G;
                _mediaColor.B = color.B;
                return _mediaColor;
            }

            UIBackBorder.Background = new SolidColorBrush(getBrush(_bgColor));
            UIIcon.Foreground
                = UIText.Foreground
                = new SolidColorBrush(getBrush(_contentColor));
            UIProgressBar.Foreground = new SolidColorBrush(getBrush(_pgbColor));
        }
        ColorAnimation MouseEnterAnim = new ColorAnimation()
        {
            //From = Color.FromRgb(0,0,0),
            DecelerationRatio = 0.8,
            To = Color.FromRgb(221, 227, 234),
            Duration = new TimeSpan(0, 0, 0, 0, 100)
        };
        ColorAnimation MouseLeaveAnim = new ColorAnimation()
        {
            //From = Color.FromRgb(0,0,0),
            DecelerationRatio = 0.8,
            To = Color.FromRgb(255, 255, 255),
            Duration = new TimeSpan(0, 0, 0, 0, 300)
        };
        void BeginListenAnimAndStyleEvent()
        {
            this.IsEnabledChanged += (s, e) =>
            {
                ChangeStyle();
            };
            this.MouseEnter += (s, e) =>
            {
                if (!IsChecked) (UIBackBorder.Background as SolidColorBrush).BeginAnimation(SolidColorBrush.ColorProperty, MouseEnterAnim);
            };
            this.MouseLeave += (s, e) =>
            {
                if (!IsChecked) (UIBackBorder.Background as SolidColorBrush).BeginAnimation(SolidColorBrush.ColorProperty, MouseLeaveAnim);
            };
        }
        #endregion

        public SelectButton()
        {
            InitializeComponent();

            DataContext = this;

            /*UIIcon.Kind = Icon;
            UIText.Text =
                Text == null ?
                Text :
                "";*/

            BeginListenClickEvent();
            BeginListenAnimAndStyleEvent();
            ChangeStyle();
        }
    }
}
