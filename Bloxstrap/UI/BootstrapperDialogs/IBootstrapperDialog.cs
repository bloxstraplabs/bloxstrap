using System;
using System.Windows.Forms;

namespace Bloxstrap.UI.BootstrapperDialogs
{
    public interface IBootstrapperDialog
    {
        public Bootstrapper? Bootstrapper { get; set; }

        string Message { get; set; }
        ProgressBarStyle ProgressStyle { get; set; }
        int ProgressValue { get; set; }
        bool CancelEnabled { get; set; }

        void ShowBootstrapper();
        void CloseBootstrapper();
        void ShowSuccess(string message, Action? callback = null);
    }
}
