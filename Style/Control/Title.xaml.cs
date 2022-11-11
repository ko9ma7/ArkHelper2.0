using System.Windows.Controls;

namespace ArkHelper.Style.Control
{
    /// <summary>
    /// Title.xaml 的交互逻辑
    /// </summary>
    public partial class Title : UserControl
    {
        public Title()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public string Text { get; set; }

        public MaterialDesignThemes.Wpf.PackIconKind Icon { get; set; } = MaterialDesignThemes.Wpf.PackIconKind.About;
    }
}
