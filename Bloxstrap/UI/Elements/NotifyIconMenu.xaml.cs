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

namespace Bloxstrap.UI.Elements
{
    /// <summary>
    /// Interaction logic for NotifyIconMenu.xaml
    /// </summary>
    public partial class NotifyIconMenu
    {
        public NotifyIconMenu()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object? sender, RoutedEventArgs e)
        {
            // this is an awful hack lmao im so sorry to anyone who reads this
            // https://stackoverflow.com/questions/357076/best-way-to-hide-a-window-from-the-alt-tab-program-switcher#:~:text=ShowInTaskbar%20%3D%20false%3B%20Owner%20%3D%20form1,form%27s%20ShowInTaskbar%20property%20to%20false.
            var wndHelper = new WindowInteropHelper(this);
            long exStyle = NativeMethods.GetWindowLongPtr(wndHelper.Handle, NativeMethods.GWL_EXSTYLE).ToInt64();
            exStyle |= NativeMethods.WS_EX_TOOLWINDOW;
            NativeMethods.SetWindowLongPtr(wndHelper.Handle, NativeMethods.GWL_EXSTYLE, (IntPtr)exStyle);
        }

        // i tried to use a viewmodel but uhhhhhhh it just didnt work idk
        private void TestMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Controls.ShowMessageBox($"hi how u doing i am {TestMenuItem.IsChecked}", MessageBoxImage.Warning);
        }
    }
}
