using Bloxstrap.Helpers;
using Bloxstrap.Helpers.RSMM;

namespace Bloxstrap.Dialogs.BootstrapperStyles
{
    // example: https://youtu.be/h0_AL95Sc3o?t=48

    // i suppose a better name for this here would be "VistaDialog" rather than "TaskDialog"?
    // having this named as BootstrapperStyles.TaskDialog would conflict with Forms.TaskDialog
    // so naming it VistaDialog would let us drop the ~Style suffix on every style name

    // this currently doesn't work because c# is stupid
    // technically, task dialogs are treated as winforms controls, but they don't classify as winforms controls at all
    // all winforms controls have the ability to be invoked from another thread, but task dialogs don't
    // so we're just kind of stuck with this not working in multithreaded use
    // (unless we want the bootstrapper to freeze during package extraction)

    // for now, just stick to legacydialog and progressdialog

    public class TaskDialogStyle
    {
        private Bootstrapper Bootstrapper;
        private TaskDialogPage Dialog;

        public TaskDialogStyle(Bootstrapper bootstrapper)
        {
            Bootstrapper = bootstrapper;
            Bootstrapper.ShowSuccessEvent += new ChangeEventHandler<string>(ShowSuccess);
            Bootstrapper.MessageChanged += new ChangeEventHandler<string>(MessageChanged);
            Bootstrapper.ProgressBarValueChanged += new ChangeEventHandler<int>(ProgressBarValueChanged);
            Bootstrapper.ProgressBarStyleChanged += new ChangeEventHandler<ProgressBarStyle>(ProgressBarStyleChanged);

            Dialog = new TaskDialogPage()
            {
                Icon = new TaskDialogIcon(IconManager.GetIconResource()),
                Caption = Program.ProjectName,
                Heading = "Please wait...",

                Buttons = { TaskDialogButton.Cancel },
                ProgressBar = new TaskDialogProgressBar()
                {
                    State = TaskDialogProgressBarState.Marquee
                }
            };

            Task.Run(() => RunBootstrapper());
            TaskDialog.ShowDialog(Dialog);
        }

        public async void RunBootstrapper()
        {
            try
            {
                await Bootstrapper.Run();
            }
            catch (Exception ex)
            {
                // string message = String.Format("{0}: {1}", ex.GetType(), ex.Message);
                string message = ex.ToString();
                ShowError(message);

                Program.Exit();
            }
        }

        public void ShowError(string message)
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
            
            Dialog.Navigate(errorDialog);
            Dialog = errorDialog;
        }

        public void ShowSuccess(object sender, ChangeEventArgs<string> e)
        {
            TaskDialogPage successDialog = new()
            {
                Icon = TaskDialogIcon.ShieldSuccessGreenBar,
                Caption = Program.ProjectName,
                Heading = e.Value
            };

            Dialog.Navigate(successDialog);
            Dialog = successDialog;
        }

        private void MessageChanged(object sender, ChangeEventArgs<string> e)
        {
            if (Dialog is null)
                return;

            Dialog.Heading = e.Value;
        }

        private void ProgressBarValueChanged(object sender, ChangeEventArgs<int> e)
        {
            if (Dialog is null || Dialog.ProgressBar is null)
                return;

            Dialog.ProgressBar.Value = e.Value;
        }

        private void ProgressBarStyleChanged(object sender, ChangeEventArgs<ProgressBarStyle> e)
        {
            if (Dialog is null || Dialog.ProgressBar is null)
                return;

            switch (e.Value)
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
}
