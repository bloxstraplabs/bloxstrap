using System;
using System.Windows;
using System.Windows.Forms;

using Bloxstrap.Extensions;
using Bloxstrap.Utility;

namespace Bloxstrap.UI.BootstrapperDialogs.WinForms
{
    public class DialogBase : Form, IBootstrapperDialog
    {
        public Bootstrapper? Bootstrapper { get; set; }

        #region UI Elements
        protected virtual string _message { get; set; } = "Please wait...";
        protected virtual ProgressBarStyle _progressStyle { get; set; }
        protected virtual int _progressValue { get; set; }
        protected virtual bool _cancelEnabled { get; set; }

        public string Message
        {
            get => _message;
            set
            {
                if (InvokeRequired)
                    Invoke(() => _message = value);
                else
                    _message = value;
            }
        }

        public ProgressBarStyle ProgressStyle
        {
            get => _progressStyle;
            set
            {
                if (InvokeRequired)
                    Invoke(() => _progressStyle = value);
                else
                    _progressStyle = value;
            }
        }

        public int ProgressValue
        {
            get => _progressValue;
            set
            {
                if (InvokeRequired)
                    Invoke(() => _progressValue = value);
                else
                    _progressValue = value;
            }
        }

        public bool CancelEnabled
        {
            get => _cancelEnabled;
            set
            {
                if (InvokeRequired)
                    Invoke(() => _cancelEnabled = value);
                else
                    _cancelEnabled = value;
            }
        }
        #endregion

        public void ScaleWindow()
        {
            Size = MinimumSize = MaximumSize = WindowScaling.GetScaledSize(Size);

            foreach (Control control in Controls)
            {
                control.Size = WindowScaling.GetScaledSize(control.Size);
                control.Location = WindowScaling.GetScaledPoint(control.Location);
                control.Padding = WindowScaling.GetScaledPadding(control.Padding);
            }
        }

        public void SetupDialog()
        {
            Text = App.Settings.Prop.BootstrapperTitle;
            Icon = App.Settings.Prop.BootstrapperIcon.GetIcon();
        }

        public void ButtonCancel_Click(object? sender, EventArgs e)
        {
            Bootstrapper?.CancelInstall();
            Close();
        }

        #region IBootstrapperDialog Methods
        public void ShowBootstrapper() => ShowDialog();

        public virtual void CloseBootstrapper()
        {
            if (InvokeRequired)
                Invoke(CloseBootstrapper);
            else
                Close();
        }

        public virtual void ShowSuccess(string message, Action? callback)
        {
            App.ShowMessageBox(message, MessageBoxImage.Information);

            if (callback is not null)
                callback();

            App.Terminate();
        }

        public virtual void ShowError(string message)
        {
            App.ShowMessageBox($"An error occurred while starting Roblox\n\nDetails: {message}", MessageBoxImage.Error);
            App.Terminate(Bootstrapper.ERROR_INSTALL_FAILURE);
        }

        public void PromptShutdown()
        {
            MessageBoxResult result = App.ShowMessageBox(
                "Roblox is currently running, but needs to close. Would you like close Roblox now?",
                MessageBoxImage.Information,
                MessageBoxButton.OKCancel
            );

            if (result != MessageBoxResult.OK)
                Environment.Exit(Bootstrapper.ERROR_INSTALL_USEREXIT);
        }
        #endregion
    }
}
