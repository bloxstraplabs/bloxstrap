using Bloxstrap.Helpers.RSMM;

namespace Bloxstrap.Dialogs.BootstrapperStyles
{
    interface IBootstrapperStyle
    {
        Bootstrapper? Bootstrapper { get; set; }

        string Message { get; set; }
        ProgressBarStyle ProgressStyle { get; set; }
        int ProgressValue { get; set; }
        bool CancelEnabled { get; set; }

        void RunBootstrapper();
        void ShowError(string message);
        void ShowSuccess(object sender, ChangeEventArgs<string> e);
        void CloseDialog(object? sender, EventArgs e);
        void PromptShutdown(object? sender, EventArgs e);

        void MessageChanged(object sender, ChangeEventArgs<string> e);
        void ProgressBarValueChanged(object sender, ChangeEventArgs<int> e);
        void ProgressBarStyleChanged(object sender, ChangeEventArgs<ProgressBarStyle> e);
        void CancelEnabledChanged(object sender, ChangeEventArgs<bool> e);

        void ButtonCancel_Click(object sender, EventArgs e);
    }
}
