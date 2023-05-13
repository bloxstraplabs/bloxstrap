using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Bloxstrap.Dialogs;
using Bloxstrap.Extensions;

namespace Bloxstrap.ViewModels
{
    public class ByfronDialogViewModel : FluentDialogViewModel, INotifyPropertyChanged
    {
        public string Version => $"Bloxstrap v{App.Version}";

        // Using dark theme for default values.
        public Thickness DialogBorder { get; set; } = new Thickness(0);
        public Brush Background { get; set; } = Brushes.Black;
        public Brush Foreground { get; set; } = new SolidColorBrush(Color.FromRgb(239, 239, 239));
        public Brush IconColor { get; set; } = new SolidColorBrush(Color.FromRgb(255, 255, 255));
        public Brush ProgressBarBackground { get; set; } = new SolidColorBrush(Color.FromRgb(86, 86, 86));
        public ImageSource ByfronLogo { get; set; } = new BitmapImage(new Uri("pack://application:,,,/Bloxstrap;component/Resources/ByfronLogo.png"));

        // When cancel button is visible, version number is hidden.
        public Visibility VersionNumberVisibility { get; set; } = Visibility.Collapsed;

        public ByfronDialogViewModel(IBootstrapperDialog dialog) : base(dialog)
        {
        }
    }
}
