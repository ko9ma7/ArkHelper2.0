using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace ArkHelper.Xaml.Control
{
    /// <summary>
    /// RoundImageWithTwoTexts.xaml 的交互逻辑
    /// </summary>
    public partial class RoundImageWithTwoTexts : UserControl
    {
        public RoundImageWithTwoTexts(string ImageUri,string texta,string textb)
        {
            InitializeComponent();
            image.Source = new BitmapImage(new Uri(ImageUri));
            text1.Text = texta;
            text2.Text = textb;
        }
    }
}
