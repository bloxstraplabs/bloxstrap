using Bloxstrap.Helpers;
using Bloxstrap.Helpers.RSMM;

namespace Bloxstrap.Dialogs.BootstrapperStyles
{
    // https://youtu.be/h0_AL95Sc3o?t=48

    // a bit hacky, but this is actually a hidden form
    // since taskdialog is part of winforms, it can't really be properly used without a form
    // for example, cross-threaded calls to ui controls can't really be done outside of a form

    public partial class VistaDialog : BootstrapperStyleForm
    {
        private TaskDialogPage Dialog;

        public override string Message
        {
            get => Dialog.Heading ?? "";
            set => Dialog.Heading = value;
        }

        public override ProgressBarStyle ProgressStyle
        {
            set
            {
                if (Dialog.ProgressBar is null)
                    return;

                switch (value)
                {
                    case ProgressBarStyle.Continuous:
                    case ProgressBarStyle.Blocks:
                        Dialog.ProgressBar.State = TaskDialogProgressBarState.Normal;
                        break;

                    case ProgressBarStyle.Marquee:
                        Dialog.ProgressBar.State = TaskDialogProgressBarState.Marquee;
                        break;
                }
            }
        }

        public override int ProgressValue
        {
            get => Dialog.ProgressBar is null ? 0 : Dialog.ProgressBar.Value;
            set
            {
                if (Dialog.ProgressBar is null)
                    return;

                Dialog.ProgressBar.Value = value;
            }
        }

        public override bool CancelEnabled
        {
            get => Dialog.Buttons[0].Enabled;
            set => Dialog.Buttons[0].Enabled = value;
        }

        public VistaDialog(Bootstrapper? bootstrapper = null)
        {
            InitializeComponent();

            Bootstrapper = bootstrapper;

            Dialog = new TaskDialogPage()
            {
                Icon = new TaskDialogIcon(IconManager.GetIconResource()),
                Caption = Program.ProjectName,

                Buttons = { TaskDialogButton.Cancel },
                ProgressBar = new TaskDialogProgressBar()
                {
                    State = TaskDialogProgressBarState.Marquee
                }
            };

            Message = "Please wait...";
            CancelEnabled = false;

            Dialog.Buttons[0].Click += (sender, e) => ButtonCancel_Click(sender, e);

            SetupDialog();
        }

        public override void ShowSuccess(object sender, ChangeEventArgs<string> e)
        {
            if (this.InvokeRequired)
            {
                ChangeEventHandler<string> handler = new(ShowSuccess);
                this.Invoke(handler, sender, e);
            }
            else
            {
                TaskDialogPage successDialog = new()
                {
                    Icon = TaskDialogIcon.ShieldSuccessGreenBar,
                    Caption = Program.ProjectName,
                    Heading = e.Value,
                    Buttons = { TaskDialogButton.OK }
                };

                successDialog.Buttons[0].Click += (sender, e) => Program.Exit();

                Dialog.Navigate(successDialog);
                Dialog = successDialog;
            }
        }

        private void InvokeShowError(object sender, ChangeEventArgs<string> e)
        {
            ShowError(e.Value);
        }

        public override void ShowError(string message)
        {
            if (this.InvokeRequired)
            {
                ChangeEventHandler<string> handler = new(InvokeShowError);
                this.Invoke(handler, this, new ChangeEventArgs<string>(message));
            }
            else
            {
                TaskDialogPage errorDialog = new()
                {
                    Icon = TaskDialogIcon.Error,
                    Caption = Program.ProjectName,
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

                errorDialog.Buttons[0].Click += (sender, e) => Program.Exit();

                Dialog.Navigate(errorDialog);
                Dialog = errorDialog;
            }
        }

        public override void CloseDialog(object? sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                EventHandler handler = new(CloseDialog);
                this.Invoke(handler, sender, e);
            }
            else
            {
                if (Dialog.BoundDialog is null)
                    return;
                
                Dialog.BoundDialog.Close();
            }
        }


        private void TestDialog_Load(object sender, EventArgs e)
        {
            TaskDialog.ShowDialog(Dialog);
        }
    }
}
