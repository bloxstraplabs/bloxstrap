namespace Bloxstrap.Dialogs.BootstrapperDialogs
{
    public interface IBootstrapperDialog
    {
        Bootstrapper? Bootstrapper { get; set; }

        string Message { get; set; }
        ProgressBarStyle ProgressStyle { get; set; }
        int ProgressValue { get; set; }
        bool CancelEnabled { get; set; }

        void RunBootstrapper();
        void ShowSuccess(string message);
        void ShowError(string message);
        void CloseDialog();
        void PromptShutdown();
    }
}
