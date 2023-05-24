using System;
using System.Windows.Forms;

using Bloxstrap.Extensions;

namespace Bloxstrap.UI.BootstrapperDialogs.WinForms
{
    // https://youtu.be/h0_AL95Sc3o?t=48

    // a bit hacky, but this is actually a hidden form
    // since taskdialog is part of winforms, it can't really be properly used without a form
    // for example, cross-threaded calls to ui controls can't really be done outside of a form

    public partial class VistaDialog : DialogBase
    {
        private TaskDialogPage _dialogPage;

        protected sealed override string _message
        {
            get => _dialogPage.Heading ?? "";
            set => _dialogPage.Heading = value;
        }

        protected sealed override ProgressBarStyle _progressStyle
        {
            set
            {
                if (_dialogPage.ProgressBar is null)
                    return;

                _dialogPage.ProgressBar.State = value switch
                {
                    ProgressBarStyle.Continuous => TaskDialogProgressBarState.Normal,
                    ProgressBarStyle.Blocks => TaskDialogProgressBarState.Normal,
                    ProgressBarStyle.Marquee => TaskDialogProgressBarState.Marquee,
                    _ => _dialogPage.ProgressBar.State
                };
            }
        }

        protected sealed override int _progressValue
        {
            get => _dialogPage.ProgressBar?.Value ?? 0;
            set
            {
                if (_dialogPage.ProgressBar is null)
                    return;

                _dialogPage.ProgressBar.Value = value;
            }
        }

        protected sealed override bool _cancelEnabled
        {
            get => _dialogPage.Buttons[0].Enabled;
            set => _dialogPage.Buttons[0].Enabled = value;
        }

        public VistaDialog()
        {
            InitializeComponent();

            _dialogPage = new TaskDialogPage()
            {
                Icon = new TaskDialogIcon(App.Settings.Prop.BootstrapperIcon.GetIcon()),
                Caption = App.Settings.Prop.BootstrapperTitle,

                Buttons = { TaskDialogButton.Cancel },
                ProgressBar = new TaskDialogProgressBar()
                {
                    State = TaskDialogProgressBarState.Marquee
                }
            };

            _message = "Please wait...";
            _cancelEnabled = false;

            _dialogPage.Buttons[0].Click += ButtonCancel_Click;

            SetupDialog();
        }

        public override void ShowSuccess(string message, Action? callback)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(ShowSuccess, message, callback);
            }
            else
            {
                TaskDialogPage successDialog = new()
                {
                    Icon = TaskDialogIcon.ShieldSuccessGreenBar,
                    Caption = App.Settings.Prop.BootstrapperTitle,
                    Heading = message,
                    Buttons = { TaskDialogButton.OK }
                };

                successDialog.Buttons[0].Click += (_, _) =>
                {
                    if (callback is not null)
                        callback();

                    App.Terminate();
                };

                _dialogPage.Navigate(successDialog);
                _dialogPage = successDialog;
            }
        }

        public override void ShowError(string message)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(ShowError, message);
            }
            else
            {
                TaskDialogPage errorDialog = new()
                {
                    Icon = TaskDialogIcon.Error,
                    Caption = App.Settings.Prop.BootstrapperTitle,
                    Heading = "An error occurred while starting Roblox",
                    Buttons = { TaskDialogButton.Close },
                    Expander = new TaskDialogExpander()
                    {
                        Text = message,
                        CollapsedButtonText = "See details",
                        ExpandedButtonText = "Hide details",
                        Position = TaskDialogExpanderPosition.AfterText
                    }
                };

                errorDialog.Buttons[0].Click += (sender, e) => App.Terminate(Bootstrapper.ERROR_INSTALL_FAILURE);

                _dialogPage.Navigate(errorDialog);
                _dialogPage = errorDialog;
            }
        }

        public override void CloseBootstrapper()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(CloseBootstrapper);
            }
            else
            {
                _dialogPage.BoundDialog?.Close();
                base.CloseBootstrapper();
            }
        }


        private void VistaDialog_Load(object sender, EventArgs e) => TaskDialog.ShowDialog(_dialogPage);
    }
}
