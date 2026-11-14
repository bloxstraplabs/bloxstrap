using System.Windows;
using System.Windows.Controls;

namespace Bloxstrap.UI.Elements.Controls
{
    public partial class LumiPageHeader : UserControl
    {
        public static readonly DependencyProperty PageTitleProperty = DependencyProperty.Register(
            nameof(PageTitle), typeof(string), typeof(LumiPageHeader), new PropertyMetadata(String.Empty));

        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
            nameof(Description), typeof(string), typeof(LumiPageHeader), new PropertyMetadata(String.Empty));

        public string PageTitle
        {
            get => (string)GetValue(PageTitleProperty);
            set => SetValue(PageTitleProperty, value);
        }

        public string Description
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        public LumiPageHeader()
        {
            InitializeComponent();
        }
    }
}
