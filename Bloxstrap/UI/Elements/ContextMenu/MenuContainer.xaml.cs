using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Bloxstrap.UI.ViewModels;

namespace Bloxstrap.UI.Elements.ContextMenu
{
    /// <summary>
    /// Interaction logic for NotifyIconMenu.xaml
    /// </summary>
    public partial class MenuContainer
    {
        // i wouldve gladly done this as mvvm but turns out that data binding just does not work with menuitems for some reason so idk this sucks

        public MenuContainer()
        {
            InitializeComponent();

            VersionMenuItem.Header = $"{App.ProjectName} v{App.Version}";
        }

        private void Window_Loaded(object? sender, RoutedEventArgs e)
        {
            // this is an awful hack lmao im so sorry to anyone who reads this
            // this is done to register the context menu wrapper as a tool window so it doesnt appear in the alt+tab switcher
            // https://stackoverflow.com/a/551847/11852173

            var wndHelper = new WindowInteropHelper(this);
            long exStyle = NativeMethods.GetWindowLongPtr(wndHelper.Handle, NativeMethods.GWL_EXSTYLE).ToInt64();
            exStyle |= NativeMethods.WS_EX_TOOLWINDOW;
            NativeMethods.SetWindowLongPtr(wndHelper.Handle, NativeMethods.GWL_EXSTYLE, (IntPtr)exStyle);
        }

        private void RichPresenceMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Controls.ShowMessageBox($"hi how u doing i am {RichPresenceMenuItem.IsChecked}", MessageBoxImage.Warning);
        }

        private void ServerDetailsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Controls.ShowMessageBox($"hi how u doing i am {RichPresenceMenuItem.IsChecked}", MessageBoxImage.Warning);
        }

        private void TestMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Controls.ShowMessageBox($"hi how u doing i am {TestMenuItem.IsChecked}", MessageBoxImage.Warning);
        }
    }
}
