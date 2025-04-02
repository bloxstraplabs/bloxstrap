﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Bloxstrap.UI.ViewModels.Settings;

namespace Bloxstrap.UI.Elements.Settings.Pages
{
    /// <summary>
    /// Interaction logic for BootstrapperPage.xaml
    /// </summary>
    public partial class BootstrapperPage
    {
        public BootstrapperPage()
        {
            DataContext = new BootstrapperViewModel();
            InitializeComponent();
        }

        private void ToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
