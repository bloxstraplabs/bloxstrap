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

namespace Bloxstrap.UI.Elements.Dialogs
{
    /// <summary>
    /// Interaction logic for LanguageSelectorDialog.xaml
    /// </summary>
    public partial class LanguageSelectorDialog
    {
        public LanguageSelectorDialog()
        {
            var viewModel = new LanguageSelectorViewModel();

            DataContext = viewModel;
            InitializeComponent();
            ApplyTheme();

            viewModel.CloseRequestEvent += (_, _) => Close();
        }
    }
}
