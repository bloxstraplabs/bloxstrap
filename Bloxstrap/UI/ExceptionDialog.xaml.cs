using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

using Bloxstrap.Utility;

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
            InitializeComponent();

            Title = RootTitleBar.Title = $"{App.ProjectName} Exception";
            ErrorRichTextBox.Selection.Text = $"{exception.GetType()}: {exception.Message}";

            if (!App.Logger.Initialized)
                LocateLogFileButton.Content = "Copy log contents";

            LocateLogFileButton.Click += delegate
            {
                if (App.Logger.Initialized)
                    Process.Start("explorer.exe", $"/select,\"{App.Logger.Filename}\"");
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
        }
    }
}
