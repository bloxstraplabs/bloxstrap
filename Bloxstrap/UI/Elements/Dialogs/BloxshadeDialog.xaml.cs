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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Bloxstrap.UI.ViewModels.Dialogs;
using Bloxstrap.UI.ViewModels.Installer;
using Wpf.Ui.Mvvm.Interfaces;

namespace Bloxstrap.UI.Elements.Dialogs
{
    /// <summary>
    /// Interaction logic for BloxshadeDialog.xaml
    /// </summary>
    public partial class BloxshadeDialog
    {
        public NextAction CloseAction = NextAction.Terminate;

        public BloxshadeDialog()
        {
            InitializeComponent();
        }

        public void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
