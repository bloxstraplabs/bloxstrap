using System.Media;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

using Windows.Win32;
using Windows.Win32.Foundation;

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
            Exception? innerException = exception.InnerException;

            InitializeComponent();

            AddException(exception);

            if (!App.Logger.Initialized)
                LocateLogFileButton.Content = Bloxstrap.Resources.Strings.Dialog_Exception_CopyLogContents;

            LocateLogFileButton.Click += delegate
            {
                if (App.Logger.Initialized)
                {
                    Process.Start("explorer.exe", $"/select,\"{App.Logger.FileLocation}\"");
                }
                else
                {
                    try
                    {
                        Clipboard.SetText(String.Join("\r\n", App.Logger.Backlog));
                    }
                    catch (COMException ex)
                    {
                        Frontend.ShowMessageBox(string.Format(Bloxstrap.Resources.Strings.Bootstrapper_ClipboardCopyFailed, ex.Message), MessageBoxImage.Error);
                    }
                }
            };

            ReportOptions.DropDownClosed += (sender, e) =>
            {
                if (ReportOptions.SelectedItem is not ComboBoxItem comboBoxItem)
                    return;

                ReportOptions.SelectedIndex = 0;

                string? tag = comboBoxItem.Tag?.ToString();

                if (tag == "github")
                    Utilities.ShellExecute($"https://github.com/{App.ProjectRepository}/issues");
                else if (tag == "discord")
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
