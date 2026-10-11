using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Bloxstrap.UI
{
    public static class SmoothScroll
    {
        private const double Damping = 6;

        private class ScrollState
        {
            public double Current;
            public double Target;
        }

        private static readonly Dictionary<ScrollViewer, ScrollState> _active = new();

        private static TimeSpan _lastFrame;

        private static bool _registered;

        private static bool _rendering;

        public static void Register()
        {
            if (_registered)
                return;

            _registered = true;
            EventManager.RegisterClassHandler(typeof(Window), UIElement.PreviewMouseWheelEvent, new MouseWheelEventHandler(OnPreviewMouseWheel));
        }

        private static bool Enabled => !App.Settings.Prop.DisableAnimations && SystemParameters.ClientAreaAnimation;

        private static void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Handled || e.Delta == 0 || Keyboard.Modifiers != ModifierKeys.None || !Enabled)
                return;

            var viewer = FindTarget(e.OriginalSource as DependencyObject, e.Delta);

            if (viewer is null)
                return;

            e.Handled = true;

            if (!_active.TryGetValue(viewer, out var state))
            {
                state = new ScrollState { Current = viewer.VerticalOffset, Target = viewer.VerticalOffset };
                _active[viewer] = state;
            }

            double distance = SystemParameters.WheelScrollLines < 0 ? viewer.ViewportHeight : SystemParameters.WheelScrollLines * 100 / 3.0;

            state.Target = Math.Clamp(state.Target - e.Delta / 120.0 * distance, 0, viewer.ScrollableHeight);

            Start();
        }

        private static ScrollViewer? FindTarget(DependencyObject? element, int delta)
        {
            while (element is not null and not Window)
            {
                if (element is ScrollViewer viewer && viewer.VerticalScrollBarVisibility != ScrollBarVisibility.Disabled && viewer.ScrollableHeight > 0)
                {
                    if (viewer.CanContentScroll)
                        return null;

                    double position = _active.TryGetValue(viewer, out var state) ? state.Target : viewer.VerticalOffset;

                    if (delta > 0 ? position > 0 : position < viewer.ScrollableHeight)
                        return viewer;
                }

                element = element is Visual or Visual3D ? VisualTreeHelper.GetParent(element) : LogicalTreeHelper.GetParent(element);
            }

            return null;
        }

        private static void Start()
        {
            if (_rendering)
                return;

            _rendering = true;
            _lastFrame = TimeSpan.Zero;
            CompositionTarget.Rendering += OnRendering;
        }

        private static void Stop()
        {
            _rendering = false;
            CompositionTarget.Rendering -= OnRendering;
        }

        private static void OnRendering(object? sender, EventArgs e)
        {
            var time = ((RenderingEventArgs)e).RenderingTime;
            double dt = _lastFrame == TimeSpan.Zero ? 1 / 60.0 : (time - _lastFrame).TotalSeconds;

            if (dt <= 0)
                return;

            _lastFrame = time;
            dt = Math.Min(dt, 0.1);

            foreach (var (viewer, state) in _active.ToList())
            {
                if (!viewer.IsLoaded || Math.Abs(viewer.VerticalOffset - state.Current) > 1)
                {
                    _active.Remove(viewer);
                    continue;
                }

                state.Target = Math.Clamp(state.Target, 0, viewer.ScrollableHeight);
                state.Current += (state.Target - state.Current) * (1 - Math.Exp(-Damping * dt));

                if (Math.Abs(state.Target - state.Current) < 0.5)
                {
                    state.Current = state.Target;
                    _active.Remove(viewer);
                }

                viewer.ScrollToVerticalOffset(state.Current);
            }

            if (!_active.Any())
                Stop();
        }
    }
}
