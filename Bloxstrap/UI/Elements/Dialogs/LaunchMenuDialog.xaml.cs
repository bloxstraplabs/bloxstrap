using Bloxstrap.UI.ViewModels.Dialogs;
using Bloxstrap.UI.ViewModels.Installer;
using SimpleFeedReader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Wpf.Ui.Mvvm.Interfaces;

namespace Bloxstrap.UI.Elements.Dialogs
{
    /// <summary>
    /// Interaction logic for LaunchMenuDialog.xaml
    /// </summary>
    public partial class LaunchMenuDialog
    {
        public NextAction CloseAction = NextAction.Terminate;

        private LaunchMenuViewModel ViewModel = new(App.Settings.Prop.ShowNewsfeed);

        public LaunchMenuDialog()
        {
            ViewModel.CloseWindowRequest += (_, closeAction) =>
            {
                CloseAction = closeAction;
                Close();
            };

            if (App.Settings.Prop.ShowNewsfeed)
                LoadNews();

            DataContext = ViewModel;

            InitializeComponent();

            string logoFilename = "WordmarkDark.png";

            if (App.Settings.Prop.Theme.GetFinal() == Theme.Light)
                logoFilename = "WordmarkLight.png";

            Logo.Source = new BitmapImage(new Uri($"pack://application:,,,/Resources/{logoFilename}"));
        }

        public async void LoadNews()
        {
            try
            {
                var reader = new FeedReader();
                var rssItems = await reader.RetrieveFeedAsync(StaticContent.NewsfeedURL);
                var items = rssItems.Select(x => new NewsItem(x)).ToList();

                if (items.Any())
                    items[0].ThumbnailExpanded = true;

                ViewModel.Newsfeed = items;
                ViewModel.RaiseNewsLoaded();
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine("LaunchMenuDialog::LoadNews", "Failed to fetch news");
                App.Logger.WriteException("LaunchMenuDialog::LoadNews", ex);
            }
        }
    }
}

