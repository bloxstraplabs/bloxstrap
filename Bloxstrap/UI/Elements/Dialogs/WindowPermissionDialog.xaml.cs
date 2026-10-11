using System.Windows;

using Bloxstrap.UI.Elements.Base;

namespace Bloxstrap.UI.Elements.Dialogs
{
    public partial class WindowPermissionDialog : WpfUiWindow
    {
        public bool Allowed { get; private set; }

        public bool NeverAsk { get; private set; }

        public WindowPermissionDialog(string gameName)
        {
            InitializeComponent();

            GameText.Text = String.Format(Strings.Dialog_WindowPermission_Game, gameName);
        }

        private void AllowButton_Click(object sender, RoutedEventArgs e)
        {
            Allowed = true;
            Close();
        }

        private void DenyButton_Click(object sender, RoutedEventArgs e)
        {
            NeverAsk = NeverAskCheckBox.IsChecked == true;
            Close();
        }
    }
}
