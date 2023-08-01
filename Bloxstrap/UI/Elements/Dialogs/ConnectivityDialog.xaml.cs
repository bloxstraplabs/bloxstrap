using System.Media;
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
    public partial class ConnectivityDialog
    {
        public ConnectivityDialog(string targetName, string description, Exception exception)
        {
            Exception? innerException = exception.InnerException;

            InitializeComponent();

            TitleTextBlock.Text = $"{App.ProjectName} is unable to connect to {targetName}";
            DescriptionTextBlock.Text = description;

            ErrorRichTextBox.Selection.Text = $"{exception.GetType()}: {exception.Message}";

            if (innerException is not null)
                ErrorRichTextBox.Selection.Text += $"\n\n===== Inner Exception =====\n{innerException.GetType()}: {innerException.Message}";

            CloseButton.Click += delegate
            {
                Close();
            };

            SystemSounds.Hand.Play();

            Loaded += delegate
            {
                var hWnd = new WindowInteropHelper(this).Handle;
                PInvoke.FlashWindow((HWND)hWnd, true);
            };
        }
    }
}
