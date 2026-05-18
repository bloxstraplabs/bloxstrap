using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

using Windows.Win32.Foundation;
using Windows.Win32;

using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Services;

namespace Bloxstrap.UI.Elements.Base
{
    public abstract class WpfUiWindow : UiWindow
    {
        #region Drag Variables
        private bool _isManualDrag;
        private System.Drawing.Point _dragStartMousePos;
        private System.Drawing.Point _dragStartWindowPos;
        private DateTime _hitTime = DateTime.Now;
        private readonly int _dragDelay = 10;
        #endregion

        private readonly IThemeService _themeService = new ThemeService();

        public WpfUiWindow()
        {
            ApplyTheme();
        }

        public void ApplyTheme(bool useAcrylic = true)
        {
            const int customThemeIndex = 2; // index for CustomTheme merged dictionary

            _themeService.SetTheme(App.Settings.Prop.Theme.GetFinal() == Enums.Theme.Dark ? ThemeType.Dark : ThemeType.Light);
            _themeService.SetSystemAccent();

            // there doesn't seem to be a way to query the name for merged dictionaries
            var dict = new ResourceDictionary { Source = new Uri($"pack://application:,,,/UI/Style/{Enum.GetName(App.Settings.Prop.Theme.GetFinal())}.xaml") };

            if (App.Settings.Prop.UseAcrylicBackground && useAcrylic)
            {
                this.WindowStyle = WindowStyle.None;

                if (!AllowsTransparency)
                    this.AllowsTransparency = true;

                this.ExtendsContentIntoTitleBar = true;
                this.WindowBackdropType = BackgroundType.Acrylic;

                byte opacity = App.Settings.Prop.AcrylicBackgroundOpacity;

                if (App.Settings.Prop.Theme.GetFinal() == Enums.Theme.Light)
                    this.Resources["MainWindowBackgroundBrush"] = new SolidColorBrush(Color.FromArgb(opacity, 250, 250, 250));
                else
                    this.Resources["MainWindowBackgroundBrush"] = new SolidColorBrush(Color.FromArgb(opacity, 32, 32, 32));
            }
            else
            {
                this.ExtendsContentIntoTitleBar = true;
                this.WindowBackdropType = BackgroundType.Mica;

                if (App.Settings.Prop.Theme.GetFinal() == Enums.Theme.Light)
                    this.Resources["MainWindowBackgroundBrush"] = new SolidColorBrush(Color.FromArgb(255, 250, 250, 250));
                else
                    this.Resources["MainWindowBackgroundBrush"] = new SolidColorBrush(Color.FromArgb(255, 32, 32, 32));
            }

            Application.Current.Resources.MergedDictionaries[customThemeIndex] = dict;

#if QA_BUILD
            this.BorderBrush = System.Windows.Media.Brushes.Red;
            this.BorderThickness = new Thickness(4);
#endif
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            if (App.Settings.Prop.WPFSoftwareRender || App.LaunchSettings.NoGPUFlag.Active)
            {
                if (PresentationSource.FromVisual(this) is HwndSource hwndSource)
                    hwndSource.CompositionTarget.RenderMode = RenderMode.SoftwareOnly;
            }

            base.OnSourceInitialized(e);
        }

        #region Acrylic Drag Logic
        // basically, the default acrylic implementation is horrible as it causes a crap ton of lag (on windows 10) when the window is moved
        // the reason afaik is due to the window being redrawn every single time it moves, which is a no no
        // so, we're going to do the window moving ourselves.
        // the drag delay is controlled by the _dragDelay variable (wow). any variable between 5 and 15 should work good.
        // 5 should not cause that much lag but i'm going to keep it at that for now
        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            // skip if acrylic is not on
            if (!App.Settings.Prop.UseAcrylicBackground)
            {
                base.OnPreviewMouseLeftButtonDown(e);
                return;
            }

            if (e.ClickCount > 1)
            {
                base.OnPreviewMouseLeftButtonDown(e);
                return;
            }

            var clickedElement = e.OriginalSource as DependencyObject;
            bool isTitleBarClick = false;

            while (clickedElement != null)
            {
                if (clickedElement is System.Windows.Controls.Button || clickedElement is Button)
                {
                    base.OnPreviewMouseLeftButtonDown(e);
                    return;
                }

                if (clickedElement is TitleBar)
                {
                    isTitleBarClick = true;
                    break;
                }
                clickedElement = VisualTreeHelper.GetParent(clickedElement);
            }

            if (isTitleBarClick)
            {
                _isManualDrag = true;

                PInvoke.GetCursorPos(out _dragStartMousePos);

                IntPtr hwnd = new WindowInteropHelper(this).Handle;
                PInvoke.GetWindowRect((HWND)hwnd, out RECT rect);
                _dragStartWindowPos = new System.Drawing.Point { X = rect.left, Y = rect.top };

                this.CaptureMouse();
                e.Handled = true;
                return;
            }

            base.OnPreviewMouseLeftButtonDown(e);
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (_isManualDrag)
            {
                _isManualDrag = false;
                this.ReleaseMouseCapture();
                e.Handled = true;
            }
            base.OnPreviewMouseLeftButtonUp(e);
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            if (_isManualDrag && this.IsMouseCaptured)
            {
                if ((DateTime.Now - _hitTime).TotalMilliseconds < _dragDelay)
                    return;

                _hitTime = DateTime.Now;

                PInvoke.GetCursorPos(out System.Drawing.Point pt);

                int deltaX = pt.X - _dragStartMousePos.X;
                int deltaY = pt.Y - _dragStartMousePos.Y;

                IntPtr hwnd = new WindowInteropHelper(this).Handle;
                PInvoke.GetWindowRect((HWND)hwnd, out Windows.Win32.Foundation.RECT rect);

                int width = rect.right - rect.left;
                int height = rect.bottom - rect.top;

                PInvoke.MoveWindow((HWND)hwnd, _dragStartWindowPos.X + deltaX, _dragStartWindowPos.Y + deltaY, width, height, true);
            }
            base.OnPreviewMouseMove(e);
        }
        #endregion
    }
}
