using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

using Bloxstrap.UI.ViewModels.Menu;

namespace Bloxstrap.UI.Menu.Pages
{
    /// <summary>
    /// Interaction logic for BehaviourPage.xaml
    /// </summary>
    public partial class BehaviourPage
    {
        public BehaviourPage()
        {
            DataContext = new BehaviourViewModel();
            InitializeComponent();
        }

        // https://stackoverflow.com/a/13289118/11852173
        // yes this doesnt fully conform to xaml but whatever
        private void ComboBox_KeyEnterUpdate(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ComboBox box = (ComboBox)sender;
                DependencyProperty prop = ComboBox.TextProperty;

                BindingExpression binding = BindingOperations.GetBindingExpression(box, prop);

                if (binding is not null)
                    binding.UpdateSource();
            }
        }
    }
}
