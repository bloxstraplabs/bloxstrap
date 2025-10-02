using Bloxstrap.UI.ViewModels.Settings;

using System.Windows.Controls;

namespace Bloxstrap.UI.Elements.Settings.Pages
{
    public partial class FastFlagsDisabled
    {
        public FastFlagsDisabled()
        {
            DataContext = new FastFlagsDisabledViewModel(this);
            InitializeComponent();
        }
    }
}