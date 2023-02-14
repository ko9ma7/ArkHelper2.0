using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace ArkHelper.Xaml.Control
{
    public class CustomRadioButton : UserControl
    {
        #region（手动继承RadioButton）实现_属性_IsChecked
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    IsCheckedChanged?.Invoke(this, value);
                }
                
                if (value == true)
                {
                    if (this.Parent == null) return;
                    for (int i = 0; i < VisualTreeHelper.GetChildrenCount(this.Parent); i++)
                    {
                        var child = VisualTreeHelper.GetChild(this.Parent, i);
                        if (child != this)
                            if (child is CustomRadioButton)
                                if ((child as CustomRadioButton).IsChecked)
                                    (child as CustomRadioButton).IsChecked = false;
                    }
                }
            }
        }
        private bool _isChecked = false;
        #endregion

        #region（手动继承RadioButton）实现_事件_IsCheckedChanged
        /// <summary>
        /// IsChecked属性改变时间，Arg是改变后的值
        /// </summary>
        public event EventHandler<bool> IsCheckedChanged;
        #endregion

        #region （手动继承RadioButton）实现_事件_Click
        public event RoutedEventHandler Click;
        protected bool isPressed { get; set; } = false;
        /// <summary>
        /// 在this构造时执行，注册点击事件
        /// </summary>
        protected void BeginListenClickEvent()
        {
            this.MouseLeftButtonDown += (o, e) =>
            {
                Task.Run(() =>
                {
                    Thread.Sleep(500);
                    isPressed = false;
                });
                isPressed = true;
            };
            this.MouseLeftButtonUp += (o, e) =>
            {
                if (isPressed)
                {
                    IsChecked = true;
#pragma warning disable CS0168 // 声明了变量，但从未使用过
                    try
                    {
                        OnClick();
                    }
                    catch (NullReferenceException ex)
                    {

                    }
#pragma warning restore CS0168 // 声明了变量，但从未使用过
                }
                isPressed = false;
            };
        }
        public virtual void OnClick()
        {
            Click?.Invoke(this, new RoutedEventArgs());
        }
        #endregion

        public CustomRadioButton()
        {
            BeginListenClickEvent();
        }
    }
}
