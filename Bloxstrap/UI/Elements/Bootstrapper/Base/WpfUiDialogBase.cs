using Bloxstrap.UI.Elements.Base;
using Bloxstrap.UI.Utility;
using Bloxstrap.UI.ViewModels.Bootstrapper;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Shell;

namespace Bloxstrap.UI.Elements.Bootstrapper.Base
{
    public class WpfUiDialogBase : WpfUiWindow, IBootstrapperDialog
    {
        // Should hopefully be set by the other ctor
        protected BootstrapperDialogViewModel _viewModel = null!;

        public Bloxstrap.Bootstrapper? Bootstrapper { get; set; }

        private bool _isClosing;

        #region UI Elements
        public virtual string Message
        {
            get => _viewModel.Message;
            set
            {
                _viewModel.Message = value;
                _viewModel.OnPropertyChanged(nameof(_viewModel.Message));
            }
        }

        public virtual ProgressBarStyle ProgressStyle
        {
            get => _viewModel.ProgressIndeterminate ? ProgressBarStyle.Marquee : ProgressBarStyle.Continuous;
            set
            {
                _viewModel.ProgressIndeterminate = (value == ProgressBarStyle.Marquee);
                _viewModel.OnPropertyChanged(nameof(_viewModel.ProgressIndeterminate));
            }
        }

        public virtual int ProgressMaximum
        {
            get => _viewModel.ProgressMaximum;
            set
            {
                _viewModel.ProgressMaximum = value;
                _viewModel.OnPropertyChanged(nameof(_viewModel.ProgressMaximum));
            }
        }

        public virtual int ProgressValue
        {
            get => _viewModel.ProgressValue;
            set
            {
                _viewModel.ProgressValue = value;
                _viewModel.OnPropertyChanged(nameof(_viewModel.ProgressValue));
            }
        }

        public virtual TaskbarItemProgressState TaskbarProgressState
        {
            get => _viewModel.TaskbarProgressState;
            set
            {
                _viewModel.TaskbarProgressState = value;

                if (Handle != IntPtr.Zero)
                    TaskbarProgress.SetProgressState(Handle, value);

                _viewModel.OnPropertyChanged(nameof(_viewModel.TaskbarProgressState));
            }
        }

        public virtual double TaskbarProgressValue
        {
            get => _viewModel.TaskbarProgressValue;
            set
            {
                _viewModel.TaskbarProgressValue = value;

                if (Handle != IntPtr.Zero)
                    TaskbarProgress.SetProgressValue(Handle, (int)value, App.TaskbarProgressMaximum);

                _viewModel.OnPropertyChanged(nameof(_viewModel.TaskbarProgressValue));
            }
        }

        public virtual bool CancelEnabled
        {
            get => _viewModel.CancelEnabled;
            set
            {
                _viewModel.CancelEnabled = value;

                _viewModel.OnPropertyChanged(nameof(_viewModel.CancelButtonVisibility));
                _viewModel.OnPropertyChanged(nameof(_viewModel.CancelEnabled));
            }
        }
        #endregion

        protected WpfUiDialogBase()
        {
            Title = App.Settings.Prop.BootstrapperTitle;
            Icon = App.Settings.Prop.BootstrapperIcon.GetIcon().GetImageSource();
        }

        #region IBootstrapperDialog Methods
        public void ShowBootstrapper()
        {
            this.ShowDialog();
        }

        public void CloseBootstrapper()
        {
            _isClosing = true;
            Dispatcher.BeginInvoke(this.Close);
        }

        public void ShowSuccess(string message, Action? callback) => BaseFunctions.ShowSuccess(message, callback);
        #endregion

        #region Overrides
        protected override void OnContentRendered(EventArgs e)
        {
            TaskbarProgress.SetProgressState(Handle, _viewModel.TaskbarProgressState);
            if (_viewModel.TaskbarProgressState != TaskbarItemProgressState.None && _viewModel.TaskbarProgressState != TaskbarItemProgressState.Indeterminate)
                TaskbarProgress.SetProgressValue(Handle, (int)_viewModel.TaskbarProgressValue, App.TaskbarProgressMaximum);

            base.OnContentRendered(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            if (!_isClosing)
                Bootstrapper?.Cancel();

            base.OnClosed(e);
        }
        #endregion
    }
}
