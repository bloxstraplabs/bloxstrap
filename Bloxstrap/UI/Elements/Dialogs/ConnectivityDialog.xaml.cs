using System.Media;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

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
        public ConnectivityDialog(string title, string description, MessageBoxImage image, Exception exception)
        {
            InitializeComponent();

            string? iconFilename = null;
            SystemSound? sound = null;

            switch (image)
            {
                case MessageBoxImage.Error:
                    iconFilename = "Error";
                    sound = SystemSounds.Hand;
                    break;

                case MessageBoxImage.Question:
                    iconFilename = "Question";
                    sound = SystemSounds.Question;
                    break;

                case MessageBoxImage.Warning:
                    iconFilename = "Warning";
                    sound = SystemSounds.Exclamation;
                    break;

                case MessageBoxImage.Information:
                    iconFilename = "Information";
                    sound = SystemSounds.Asterisk;
                    break;
            }

            if (iconFilename is null)
                IconImage.Visibility = Visibility.Collapsed;
            else
                IconImage.Source = new BitmapImage(new Uri($"pack://application:,,,/Resources/MessageBox/{iconFilename}.png"));

            TitleTextBlock.Text = title;
            DescriptionTextBlock.MarkdownText = description;

            AddException(exception);

            CloseButton.Click += delegate
            {
                Close();
            };

            sound?.Play();

            Loaded += delegate
            {
                var hWnd = new WindowInteropHelper(this).Handle;
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
