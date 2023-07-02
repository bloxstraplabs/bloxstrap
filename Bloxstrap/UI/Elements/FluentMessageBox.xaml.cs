using System;
using System.Configuration;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Bloxstrap.UI.Utility;
using Bloxstrap.Utility;

namespace Bloxstrap.UI.MessageBox
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
                    sound = SystemSounds.Asterisk;
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
                IntPtr hWnd = new WindowInteropHelper(this).Handle;
                NativeMethods.FlashWindow(hWnd, true);
            };
        }

        public void SetButton(Button button, MessageBoxResult result)
        {
            button.Visibility = Visibility.Visible;
            button.Content = result.ToString();
            button.Click += (_, _) =>
            {
                Result = result;
                Close();
            };
        }
    }
}
