using System.Windows.Controls;

namespace ArkHelper.Style.Control
{
    /// <summary>
    /// Title.xaml 的交互逻辑
    /// </summary>
    public partial class OK : UserControl
    {
        public OK()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public string Text { get; set; }

    }
}
