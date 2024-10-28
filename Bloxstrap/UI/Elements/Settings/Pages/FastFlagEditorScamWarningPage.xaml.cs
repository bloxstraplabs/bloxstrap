using Bloxstrap.UI.ViewModels.Settings;
using System.Windows;

namespace Bloxstrap.UI.Elements.Settings.Pages
{
    /// <summary>
    /// Interaction logic for FastFlagEditorScamWarningPage.xaml
    /// </summary>
    public partial class FastFlagEditorScamWarningPage
    {
        public FastFlagEditorScamWarningPage() : base(typeof(FastFlagEditorPage))
        {
            InitializeComponent();
        }

        protected override void ContinueCallback()
        {
            App.State.Prop.ShowFFlagEditorWarnings = false;
            App.State.Save(); // should we be force saving?
        }
    }
}
