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
    /// Interaction logic for UninstallerDialog.xaml
    /// </summary>
    public partial class UninstallerDialog
    {
        public bool Confirmed { get; private set; } = false;

        public bool KeepData { get; private set; } = true;

        public UninstallerDialog()
        {
            var viewModel = new UninstallerViewModel();
            viewModel.ConfirmUninstallRequest += (_, _) =>
            {
                Confirmed = true;
                KeepData = viewModel.KeepData;
                Close();
            };

            DataContext = viewModel;

            InitializeComponent();
        }
    }
}
