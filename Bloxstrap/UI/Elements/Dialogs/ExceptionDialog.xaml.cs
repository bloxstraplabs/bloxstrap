using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

using Windows.Win32;
using Windows.Win32.Foundation;

using Bloxstrap.Resources;

namespace Bloxstrap.UI.Elements.Dialogs
{
    // hmm... do i use MVVM for this?
    // this is entirely static, so i think im fine without it, and this way is just so much more efficient

    /// <summary>
    /// Interaction logic for ExceptionDialog.xaml
    /// </summary>
    public partial class ExceptionDialog
    {
        public ExceptionDialog(Exception exception)
        {
            InitializeComponent();
            AddException(exception);

            if (!App.Logger.Initialized)
                LocateLogFileButton.Content = Strings.Dialog_Exception_CopyLogContents;

            string helpMessage = String.Format(Strings.Dialog_Exception_Info_2, "https://github.com/pizzaboxer/bloxstrap/wiki", "https://github.com/pizzaboxer/bloxstrap/issues/new?template=bug_report.yaml");

            if (String.IsNullOrEmpty(App.BuildMetadata.CommitHash))
                helpMessage = String.Format(Strings.Dialog_Exception_Info_2_Alt, "https://github.com/pizzaboxer/bloxstrap/wiki");

            HelpMessageMDTextBlock.MarkdownText = helpMessage;

            LocateLogFileButton.Click += delegate
            {
                if (App.Logger.Initialized)
                    Process.Start("explorer.exe", $"/select,\"{App.Logger.FileLocation}\"");
                else
                    Clipboard.SetDataObject(String.Join("\r\n", App.Logger.Backlog));
            };

            CloseButton.Click += delegate
            {
                Close();
            };

            SystemSounds.Hand.Play();

            Loaded += delegate
            {
                IntPtr hWnd = new WindowInteropHelper(this).Handle;
                PInvoke.FlashWindow((HWND)hWnd, true);
            };
        }

        private void AddException(Exception exception, bool inner = false)
        {
            if (!inner)
                ErrorRichTextBox.Selection.Text = $"{exception.GetType()}: {exception.Message}";

            if (exception.InnerException is null)
                return;

            ErrorRichTextBox.Selection.Text += $"\n\n[Inner Exception]\n{exception.InnerException.GetType()}: {exception.InnerException.Message}";

            AddException(exception.InnerException, true);
        }
    }
}
