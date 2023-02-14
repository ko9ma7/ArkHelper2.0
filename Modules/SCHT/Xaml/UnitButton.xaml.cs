using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ArkHelper.Xaml.Control;

namespace ArkHelper.Modules.SCHT.Xaml
{
    /// <summary>
    /// UnitButton.xaml 的交互逻辑
    /// </summary>
    public partial class UnitButton : CustomRadioButton
    {
        public string Text { get; set; }
        public object BKG { get; set; }
        public UnitButton()
        {
            InitializeComponent();
            DataContext = this;

            ChangeThickBorderVisibility(IsChecked);
            InitWhenStart();
        }

        private BitmapImage bitImage;
        private void InitWhenStart()
        {
            this.IsCheckedChanged += (o, a) =>
            {
                ChangeThickBorderVisibility(a);
            };
        }
        private void ChangeThickBorderVisibility(bool a)
        {
            if (a) thickBorder.Visibility = Visibility.Visible;
            else thickBorder.Visibility = Visibility.Collapsed;
        }
        private void InitImage()
        {
            if (BKG is System.Drawing.Color)
            {
                var a = (System.Drawing.Color)BKG;
                bkgBorder.Background = new SolidColorBrush(new System.Windows.Media.Color()
                {
                    R = a.R,
                    G = a.G,
                    B = a.B,
                    A = a.A
                });
            }
            else
            {
                bitImage = new BitmapImage(new Uri((string)BKG, UriKind.Relative));
                bitImage.CacheOption = BitmapCacheOption.OnLoad;
                Image.Source = bitImage;
            }
        }
        private void DisposeImage()
        {
            if (BKG is string)
            {
                bitImage.Freeze();
                Image.Source = null;
                bitImage = null;
            }
        }

        private void rootElement_Loaded(object sender, RoutedEventArgs e)
        {
            InitImage();
        }

        private void rootElement_Unloaded(object sender, RoutedEventArgs e)
        {
            DisposeImage();
        }
    }
}
