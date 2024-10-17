﻿using System.Media;
using System.Web;
using System.Windows;
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
            InitializeComponent();
            AddException(exception);

            if (!App.Logger.Initialized)
                LocateLogFileButton.Content = Strings.Dialog_Exception_CopyLogContents;

            string repoUrl = $"https://github.com/{App.ProjectRepository}";
            string wikiUrl = $"{repoUrl}/wiki";

            string issueUrl = String.Format(
                "{0}/issues/new?template=bug_report.yaml&title={1}&log={2}",
                repoUrl,
                HttpUtility.UrlEncode($"[BUG] {exception.GetType()}: {exception.Message}"),
                HttpUtility.UrlEncode(String.Join('\n', App.Logger.History))
            );

            string helpMessage = String.Format(Strings.Dialog_Exception_Info_2, wikiUrl, issueUrl);

            if (!App.IsActionBuild && !App.BuildMetadata.Machine.Contains("pizzaboxer", StringComparison.Ordinal))
                helpMessage = String.Format(Strings.Dialog_Exception_Info_2_Alt, wikiUrl);

            HelpMessageMDTextBlock.MarkdownText = helpMessage;
            VersionText.Text = String.Format(Strings.Dialog_Exception_Version, App.Version);

            ReportExceptionButton.Click += (_, _) => Utilities.ShellExecute(issueUrl);

            LocateLogFileButton.Click += delegate
            {
                if (App.Logger.Initialized && !String.IsNullOrEmpty(App.Logger.FileLocation))
                    Utilities.ShellExecute(App.Logger.FileLocation);
                else
                    Clipboard.SetDataObject(String.Join("\r\n", App.Logger.History));
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
