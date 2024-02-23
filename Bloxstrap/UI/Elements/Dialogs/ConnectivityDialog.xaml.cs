using System.Media;
using System.Windows.Interop;

#if !DEBUG_ROSLYN_PUBLISH
using Windows.Win32;
using Windows.Win32.Foundation;
#endif

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
            InitializeComponent();

            TitleTextBlock.Text = $"{App.ProjectName} is unable to connect to {targetName}";
            DescriptionTextBlock.Text = description;

            AddException(exception);

            CloseButton.Click += delegate
            {
                Close();
            };

            SystemSounds.Hand.Play();

            Loaded += delegate
            {
                var hWnd = new WindowInteropHelper(this).Handle;
#if !DEBUG_ROSLYN_PUBLISH
                PInvoke.FlashWindow((HWND)hWnd, true);
#endif
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
