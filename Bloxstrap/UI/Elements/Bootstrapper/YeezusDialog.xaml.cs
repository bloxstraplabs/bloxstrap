using Bloxstrap.UI.Elements.Bootstrapper.Base;
using Bloxstrap.UI.ViewModels.Bootstrapper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Bloxstrap.UI.Elements.Bootstrapper {
    /// <summary>
    /// Interaction logic for YeezusDialog.xaml
    /// </summary>
    public partial class YeezusDialog : IBootstrapperDialog {
        private readonly YeezusDialogViewModel _viewModel;

        public Bloxstrap.Bootstrapper? Bootstrapper { get; set; }

        private bool _isClosing;

        #region UI Elements
        public string Message {
            get => _viewModel.Message;
            set {
                _viewModel.Message = value;
                _viewModel.OnPropertyChanged(nameof(_viewModel.Message));
            }
        }

        public ProgressBarStyle ProgressStyle {
            get => _viewModel.ProgressIndeterminate ? ProgressBarStyle.Marquee : ProgressBarStyle.Continuous;
            set {
                _viewModel.ProgressIndeterminate = (value == ProgressBarStyle.Marquee);
                _viewModel.OnPropertyChanged(nameof(_viewModel.ProgressIndeterminate));
            }
        }

        public int ProgressMaximum {
            get => _viewModel.ProgressMaximum;
            set {
                _viewModel.ProgressMaximum = value;
                _viewModel.OnPropertyChanged(nameof(_viewModel.ProgressMaximum));
            }
        }

        public int ProgressValue {
            get => _viewModel.ProgressValue;
            set {
                _viewModel.ProgressValue = value;
                _viewModel.OnPropertyChanged(nameof(_viewModel.ProgressValue));
            }
        }

        public bool CancelEnabled {
            get => _viewModel.CancelEnabled;
            set {
                _viewModel.CancelEnabled = value;

                _viewModel.OnPropertyChanged(nameof(_viewModel.CancelButtonVisibility));
                _viewModel.OnPropertyChanged(nameof(_viewModel.CancelEnabled));
            }
        }
        #endregion

        public YeezusDialog() {
            InitializeComponent();

            _viewModel = new YeezusDialogViewModel(this);
            DataContext = _viewModel;
            Title = App.Settings.Prop.BootstrapperTitle;
            InitializeComponent();
        }

        private void UiWindow_Closing(object sender, CancelEventArgs e) {
            if (!_isClosing)
                Bootstrapper?.CancelInstall();
        }

        #region IBootstrapperDialog Methods
        public void ShowBootstrapper() => this.ShowDialog();

        public void CloseBootstrapper() {
            _isClosing = true;
            Dispatcher.BeginInvoke(this.Close);
        }

        public void ShowSuccess(string message, Action? callback) => BaseFunctions.ShowSuccess(message, callback);
        #endregion
    }
}
