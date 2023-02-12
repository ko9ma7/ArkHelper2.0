using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ArkHelper.Modules.MaterialCalc.Xaml
{
    /// <summary>
    /// MaterialUnit.xaml 的交互逻辑
    /// </summary>
    public partial class MaterialUnit : UserControl
    {
        private int _number = -1;
        public int Number
        {
            get
            {
                return _number;
            }
            set
            {
                if (_number == -1 && IsZeroEquelTextboxEmpty)
                {
                    UInumber.Text = "";
                }
                else
                {
                    UInumber.Text = value.ToString();
                }
                _number = value;

                CheckMask();
            }
        }
        public bool IsZeroEquelTextboxEmpty { get; set; } = false;
        public Material thisMaterial;

        private BitmapImage bitImage;
        public MaterialUnit(Material material, int startNumber)
        {
            InitializeComponent();
            //DataContext = this;
            UImaterialName.Text = material.name;
            Number = startNumber;
            thisMaterial = material;

            string res = "合成方案：\n";
            int i = 1;
            foreach(var pair in thisMaterial.equal)
            {
                res += pair.Key.name + "*" + pair.Value;
                if (i != thisMaterial.equal.Count) res += " + ";
                i++;
            }
            UIpic.ToolTip = res;
            CheckMask();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            UIpic.Source = null;
            bitImage = null;
        }

        void CheckMask(bool? disAuto = null)
        {
            if (disAuto == null)
            {
                if (_number == 0)
                {
                    UImask.Visibility = Visibility.Visible;
                }
                else
                {
                    UImask.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                if ((bool)disAuto)
                {
                    UImask.Visibility = Visibility.Visible;
                }
                else
                {
                    UImask.Visibility = Visibility.Collapsed;
                }
            }

        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var picURL = Address.res + "\\pic\\material\\" + thisMaterial.id + ".png";
            bitImage = new BitmapImage(new Uri(picURL));
            bitImage.CacheOption = BitmapCacheOption.OnLoad;
            UIpic.Source = bitImage;
            bitImage.Freeze();
        }

        private void UInumber_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                Number = Convert.ToInt16(UInumber.Text);
            }
            catch
            {
                Number = 0;
            }

            CheckMask();
        }

        private void UInumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                int num = Convert.ToInt16(UInumber.Text);
                if (num == 0)
                    CheckMask(true);
                else
                    CheckMask(false);
            }
            catch
            {

            }
        }
    }
}
