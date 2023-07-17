using System.Media;
using System.Windows;
using System.Windows.Interop;

namespace Bloxstrap.UI
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
            Exception? innerException = exception.InnerException;

            InitializeComponent();

            Title = RootTitleBar.Title = $"{App.ProjectName} Exception";
            ErrorRichTextBox.Selection.Text = $"{exception.GetType()}: {exception.Message}";

            if (innerException is not null)
                ErrorRichTextBox.Selection.Text += $"\n\n===== Inner Exception =====\n{innerException.GetType()}: {innerException.Message}";

            if (!App.Logger.Initialized)
                LocateLogFileButton.Content = "Copy log contents";

            LocateLogFileButton.Click += delegate
            {
                if (App.Logger.Initialized)
                    Process.Start("explorer.exe", $"/select,\"{App.Logger.FileLocation}\"");
                else
                    Clipboard.SetText(String.Join("\r\n", App.Logger.Backlog));
            };

            ReportOptions.DropDownClosed += (sender, e) =>
            {
                string? selectionName = ReportOptions.SelectedItem.ToString();

                ReportOptions.SelectedIndex = 0;

                if (selectionName is null)
                    return;

                if (selectionName.EndsWith("GitHub"))
                    Utilities.ShellExecute($"https://github.com/{App.ProjectRepository}/issues");
                else if (selectionName.EndsWith("Discord"))
                    Utilities.ShellExecute("https://discord.gg/nKjV3mGq6R");
            };

            CloseButton.Click += delegate
            {
                Close();
            };

            SystemSounds.Hand.Play();

            Loaded += delegate
            {
                IntPtr hWnd = new WindowInteropHelper(this).Handle;
                NativeMethods.FlashWindow(hWnd, true);
            };
        }
    }
}
