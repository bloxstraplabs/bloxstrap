using System;
using System.Windows;

namespace Bloxstrap.Dialogs.Menu
{
    /// <summary>
    /// Interaction logic for ReShadeHelp.xaml
    /// </summary>
    public partial class ReShadeHelp : Window
    {
        public ReShadeHelp()
        {
            InitializeComponent();
        }

        private void ButtonClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
