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
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui.Common;

namespace Bloxstrap.UI.Elements.Controls
{
  /// <summary>
  /// Interaction logic for Expander.xaml
  /// </summary>
  [ContentProperty(nameof(InnerContent))]
  public partial class Expander : UserControl
  {
    public static readonly DependencyProperty HeaderIconProperty =
        DependencyProperty.Register(nameof(HeaderIcon), typeof(SymbolRegular), typeof(Expander));

    public static readonly DependencyProperty HeaderTextProperty =
        DependencyProperty.Register(nameof(HeaderText), typeof(string), typeof(Expander));

    public static readonly DependencyProperty InnerContentProperty =
        DependencyProperty.Register(nameof(InnerContent), typeof(object), typeof(Expander));

    public string HeaderText
    {
      get { return (string)GetValue(HeaderTextProperty); }
      set { SetValue(HeaderTextProperty, value); }
    }

    public SymbolRegular HeaderIcon
    {
      get { return (SymbolRegular)GetValue(HeaderIconProperty); }
      set { SetValue(HeaderTextProperty, value); }
    }

    public object InnerContent
    {
      get { return GetValue(InnerContentProperty); }
      set { SetValue(InnerContentProperty, value); }
    }

    public Expander()
    {
      InitializeComponent();
    }
  }
}
