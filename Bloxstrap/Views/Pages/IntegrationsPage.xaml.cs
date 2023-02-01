using System;
using System.Diagnostics;
using System.Windows;
using Wpf.Ui.Common.Interfaces;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Interfaces;

namespace Bloxstrap.Views.Pages
{
    /// <summary>
    /// Interaction logic for IntegrationsPage.xaml
    /// </summary>
    public partial class IntegrationsPage
    {
        public IntegrationsPage()
        {
            InitializeComponent();
        }

        private void NavigateReShadeHelp(object sender, EventArgs e)
        {
            ((INavigationWindow)Window.GetWindow(this)!).Navigate(typeof(ReShadeHelpPage));
        }
    }
}
