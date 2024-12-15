using Bloxstrap.UI.ViewModels.About;

using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Bloxstrap.UI.Elements.About.Pages
{
    /// <summary>
    /// Interaction logic for AboutPage.xaml
    /// </summary>
    public partial class AboutPage
    {
        private readonly Queue<Key> _keys = new();

        private readonly List<Key> _expectedKeys = new() { Key.M, Key.A, Key.T, Key.T, Key.LeftShift, Key.D1 };

        private bool _triggered = false;

        public AboutPage()
        {
            DataContext = new AboutViewModel();
            InitializeComponent();
        }

        private void UiPage_KeyDown(object sender, KeyEventArgs e)
        {
            if (_triggered)
                return;

            if (_keys.Count >= 6)
                _keys.Dequeue();

            var key = e.Key;

            if (key == Key.RightShift)
                key = Key.LeftShift;

            _keys.Enqueue(key);

            if (_keys.SequenceEqual(_expectedKeys))
            {
                _triggered = true;
                var storyboard = Resources["EggStoryboard"] as Storyboard;
                storyboard!.Begin();
            }
        }
    }
}
