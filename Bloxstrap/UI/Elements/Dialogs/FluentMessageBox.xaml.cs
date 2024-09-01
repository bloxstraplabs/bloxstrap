using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

using Windows.Win32;
using Windows.Win32.Foundation;

using Bloxstrap.UI.Utility;

namespace Bloxstrap.UI.Elements.Dialogs
{
    // wpfui does have its own messagebox control but it SUCKS so heres this instead

    /// <summary>
    /// Interaction logic for FluentMessageBox.xaml
    /// </summary>
    public partial class FluentMessageBox
    {
        public MessageBoxResult Result = MessageBoxResult.None;

        public FluentMessageBox(string message, MessageBoxImage image, MessageBoxButton buttons)
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

            Title = App.ProjectName;
            MessageTextBlock.Text = message;
            ButtonOne.Visibility = Visibility.Collapsed;
            ButtonTwo.Visibility = Visibility.Collapsed;
            ButtonThree.Visibility = Visibility.Collapsed;

            switch (buttons)
            {
                case MessageBoxButton.YesNo:
                    SetButton(ButtonOne, MessageBoxResult.Yes);
                    SetButton(ButtonTwo, MessageBoxResult.No);
                    break;

                case MessageBoxButton.YesNoCancel:
                    SetButton(ButtonOne, MessageBoxResult.Yes);
                    SetButton(ButtonTwo, MessageBoxResult.No);
                    SetButton(ButtonThree, MessageBoxResult.Cancel);
                    break;

                case MessageBoxButton.OKCancel:
                    SetButton(ButtonOne, MessageBoxResult.OK);
                    SetButton(ButtonTwo, MessageBoxResult.Cancel);
                    break;

                case MessageBoxButton.OK:
                default:
                    SetButton(ButtonOne, MessageBoxResult.OK);
                    break;
            }

            // we're doing the width manually for this because ye

            if (ButtonThree.Visibility == Visibility.Visible)
                Width = 356;
            else if (ButtonTwo.Visibility == Visibility.Visible)
                Width = 245;

            double textWidth = Math.Ceiling(Rendering.GetTextWidth(MessageTextBlock));

            // offset to account for box size
            textWidth += 40;

            // offset to account for icon
            if (image != MessageBoxImage.None)
                textWidth += 50;

            if (textWidth > MaxWidth)
                Width = MaxWidth;
            else if (textWidth > Width)
                Width = textWidth;

            sound?.Play();

            Loaded += delegate
            {
                var hWnd = new WindowInteropHelper(this).Handle;
                PInvoke.FlashWindow((HWND)hWnd, true);
            };
        }

        // reuse existing strings
        private static string GetTextForResult(MessageBoxResult result)
        {
            switch (result)
            {
                case MessageBoxResult.OK:
                    return Strings.Common_OK;
                case MessageBoxResult.Cancel:
                    return Strings.Common_Cancel;
                case MessageBoxResult.Yes:
                    return Strings.Common_Yes;
                case MessageBoxResult.No:
                    return Strings.Common_No;
                default:
                    Debug.Assert(false);
                    return result.ToString();
            }
        }

        public void SetButton(Button button, MessageBoxResult result)
        {
            button.Visibility = Visibility.Visible;
            button.Content = GetTextForResult(result);
            button.Click += (_, _) =>
            {
                Result = result;
                Close();
            };
        }
    }
}
