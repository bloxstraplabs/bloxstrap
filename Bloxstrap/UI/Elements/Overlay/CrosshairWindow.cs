using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;

using Bloxstrap.Integrations;

namespace Bloxstrap.UI.Elements.Overlay
{
    public class CrosshairWindow : Window
    {
        private const int GWL_EXSTYLE = -20;
        private const long WS_EX_TRANSPARENT = 0x00000020;
        private const long WS_EX_TOOLWINDOW = 0x00000080;
        private const long WS_EX_NOACTIVATE = 0x08000000;
        private const long WS_EX_LAYERED = 0x00080000;

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
        private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        private readonly Canvas _canvas = new();

        public CrosshairWindow()
        {
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            ShowInTaskbar = false;
            ShowActivated = false;
            Topmost = true;
            IsHitTestVisible = false;
            Focusable = false;
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.Manual;

            Content = _canvas;

            DrawCrosshair();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            var hWnd = new WindowInteropHelper(this).Handle;
            long style = GetWindowLongPtr(hWnd, GWL_EXSTYLE).ToInt64();
            SetWindowLongPtr(hWnd, GWL_EXSTYLE, new IntPtr(style | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE | WS_EX_LAYERED));
        }

        private void DrawCrosshair()
        {
            var settings = App.Settings.Prop;

            if (!ModGenerator.TryParseColor(settings.CrosshairColor, out var color))
                color = Colors.Lime;

            var brush = new SolidColorBrush(color);
            var outline = new SolidColorBrush(Color.FromArgb(160, 0, 0, 0));

            double size = Math.Clamp(settings.CrosshairSize, 2, 100);
            double thickness = Math.Clamp(settings.CrosshairThickness, 1, 20);
            double gap = Math.Clamp(settings.CrosshairGap, 0, 50);

            double extent = size + gap + thickness + 2;
            Width = Height = extent * 2;

            double center = extent;

            void AddLine(double x1, double y1, double x2, double y2)
            {
                _canvas.Children.Add(new Line { X1 = x1, Y1 = y1, X2 = x2, Y2 = y2, Stroke = outline, StrokeThickness = thickness + 2 });
                _canvas.Children.Add(new Line { X1 = x1, Y1 = y1, X2 = x2, Y2 = y2, Stroke = brush, StrokeThickness = thickness });
            }

            void AddDot()
            {
                double dot = thickness + 2;
                var outer = new Ellipse { Width = dot + 2, Height = dot + 2, Fill = outline };
                var inner = new Ellipse { Width = dot, Height = dot, Fill = brush };
                Canvas.SetLeft(outer, center - (dot + 2) / 2);
                Canvas.SetTop(outer, center - (dot + 2) / 2);
                Canvas.SetLeft(inner, center - dot / 2);
                Canvas.SetTop(inner, center - dot / 2);
                _canvas.Children.Add(outer);
                _canvas.Children.Add(inner);
            }

            switch (settings.CrosshairStyle)
            {
                case CrosshairStyle.Dot:
                    AddDot();
                    break;

                case CrosshairStyle.Circle:
                    var ring = new Ellipse { Width = size * 2, Height = size * 2, Stroke = brush, StrokeThickness = thickness };
                    Canvas.SetLeft(ring, center - size);
                    Canvas.SetTop(ring, center - size);
                    _canvas.Children.Add(ring);
                    AddDot();
                    break;

                default:
                    AddLine(center - gap - size, center, center - gap, center);
                    AddLine(center + gap, center, center + gap + size, center);
                    AddLine(center, center - gap - size, center, center - gap);
                    AddLine(center, center + gap, center, center + gap + size);

                    if (settings.CrosshairStyle == CrosshairStyle.CrossWithDot)
                        AddDot();
                    break;
            }
        }

        public void Follow(RobloxWindow window)
        {
            var rect = window.IsForeground ? window.GetClientScreenRect() : null;

            if (rect is null || rect.Value.Width <= 0)
            {
                if (IsVisible)
                    Hide();

                return;
            }

            if (!IsVisible)
                Show();

            var dpi = VisualTreeHelper.GetDpi(this);
            double centerX = (rect.Value.Left + rect.Value.Width / 2.0) / dpi.DpiScaleX;
            double centerY = (rect.Value.Top + rect.Value.Height / 2.0) / dpi.DpiScaleY;

            Left = centerX - Width / 2;
            Top = centerY - Height / 2;
        }
    }
}
