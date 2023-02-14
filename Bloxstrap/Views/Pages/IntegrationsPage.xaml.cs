using System;
using System.Windows;
using System.Windows.Controls;
using Bloxstrap.Models;
using Bloxstrap.ViewModels;

namespace Bloxstrap.Views.Pages
{
    /// <summary>
    /// Interaction logic for IntegrationsPage.xaml
    /// </summary>
    public partial class IntegrationsPage
    {
        public IntegrationsPage()
        {
            DataContext = new IntegrationsViewModel(this);
            InitializeComponent();

            // rbxfpsunlocker does not have 64 bit support
            if (!Environment.Is64BitOperatingSystem)
                this.RbxFpsUnlockerOptions.Visibility = Visibility.Collapsed;
        }

        public void CustomIntegrationSelection(object sender, SelectionChangedEventArgs e)
        {
            IntegrationsViewModel viewModel = (IntegrationsViewModel)DataContext;
            viewModel.SelectedCustomIntegration = (CustomIntegration)((ListBox)sender).SelectedItem;
            viewModel.OnPropertyChanged(nameof(viewModel.SelectedCustomIntegration));
        }
    }
}
