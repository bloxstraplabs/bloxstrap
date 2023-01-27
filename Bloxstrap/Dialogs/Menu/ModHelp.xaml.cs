using System.Windows;

using Bloxstrap.Enums;
using System;

namespace Bloxstrap.Dialogs.Menu
{
    /// <summary>
    /// Interaction logic for ModHelp.xaml
    /// </summary>
    public partial class ModHelp : Window
    {
        public ModHelp()
        {
            InitializeComponent();
            SetTheme();
        }

        public void SetTheme()
        {
            string theme = "Light";

            if (App.Settings.Theme.GetFinal() == Theme.Dark)
                theme = "ColourfulDark";

            this.Resources.MergedDictionaries[0] = new ResourceDictionary() { Source = new Uri($"Dialogs/Menu/Themes/{theme}Theme.xaml", UriKind.Relative) };
        }

        private void ButtonClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
