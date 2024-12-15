using System.Windows.Forms;
using System.Windows.Shell;

namespace Bloxstrap.UI
{
    public interface IBootstrapperDialog
    {
        public Bootstrapper? Bootstrapper { get; set; }

        string Message { get; set; }
        ProgressBarStyle ProgressStyle { get; set; }
        int ProgressValue { get; set; }
        int ProgressMaximum { get; set; }
        TaskbarItemProgressState TaskbarProgressState { get; set; }
        double TaskbarProgressValue { get; set; }
        bool CancelEnabled { get; set; }

        void ShowBootstrapper();
        void CloseBootstrapper();
        void ShowSuccess(string message, Action? callback = null);
    }
}
