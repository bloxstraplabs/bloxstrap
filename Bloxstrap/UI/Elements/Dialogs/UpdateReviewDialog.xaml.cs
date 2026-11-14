using System.Text.RegularExpressions;
using System.Windows;

namespace Bloxstrap.UI.Elements.Dialogs
{
    /// <summary>
    /// Shows the release notes and the changes LumiStrap will make before updating.
    /// </summary>
    public partial class UpdateReviewDialog
    {
        public bool UpdateNowSelected { get; private set; }

        public UpdateReviewDialog(string currentVersion, string newVersion, string? releaseName, string? releaseNotes)
        {
            InitializeComponent();

            CurrentVersionText.Text = String.Format(Strings.Dialog_UpdateReview_CurrentVersion, currentVersion);
            NewVersionText.Text = String.Format(Strings.Dialog_UpdateReview_NewVersion, newVersion);
            ReleaseNameText.Text = String.IsNullOrWhiteSpace(releaseName) || String.Equals(releaseName, newVersion, StringComparison.OrdinalIgnoreCase)
                ? String.Empty
                : releaseName;
            ReleaseNotesTextBox.Text = FormatReleaseNotes(releaseNotes);

            Title = App.ProjectName;
            RootTitleBar.Title = Strings.Dialog_UpdateReview_Title;

            LaterButton.Click += (_, _) => Close();
            UpdateNowButton.Click += (_, _) =>
            {
                UpdateNowSelected = true;
                Close();
            };
        }

        private static string FormatReleaseNotes(string? markdown)
        {
            if (String.IsNullOrWhiteSpace(markdown))
                return Strings.Dialog_UpdateReview_NoReleaseNotes;

            var lines = new List<string>();
            foreach (string sourceLine in markdown.Replace("\r\n", "\n").Split('\n'))
            {
                string line = sourceLine.TrimEnd();
                string trimmed = line.TrimStart();

                int headingLength = 0;
                while (headingLength < trimmed.Length && trimmed[headingLength] == '#')
                    headingLength++;

                if (headingLength > 0 && headingLength < trimmed.Length && Char.IsWhiteSpace(trimmed[headingLength]))
                    line = trimmed[(headingLength + 1)..];

                line = Regex.Replace(line, @"^\s*[-*+]\s+", "• ");
                line = Regex.Replace(line, @"!\[([^\]]*)\]\([^)]*\)", "$1");
                line = Regex.Replace(line, @"\[([^\]]+)\]\(([^)]+)\)", "$1 ($2)");
                line = Regex.Replace(line, @"`([^`]+)`", "$1");
                line = line.Replace("**", String.Empty).Replace("__", String.Empty);
                lines.Add(line);
            }

            string result = String.Join(Environment.NewLine, lines).Trim();
            return result.Length == 0 ? Strings.Dialog_UpdateReview_NoReleaseNotes : result;
        }
    }
}
