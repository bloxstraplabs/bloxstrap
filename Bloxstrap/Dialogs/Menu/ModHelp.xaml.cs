using System;
using System.Windows;

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
        }

        private void ButtonClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
