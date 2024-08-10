namespace Bloxstrap.UI.ViewModels.Installer
{
    // TODO: administrator check?
    public class WelcomeViewModel : NotifyPropertyChangedViewModel
    {
        // formatting is done here instead of in xaml, it's just a bit easier
        public string MainText => String.Format(
            Resources.Strings.Installer_Welcome_MainText,
            "[github.com/pizzaboxer/bloxstrap](https://github.com/pizzaboxer/bloxstrap)",
            "[bloxstrap.pizzaboxer.xyz](https://bloxstrap.pizzaboxer.xyz)"
        );

        public string VersionNotice { get; private set; } = "";

        public bool CanContinue { get; set; } = false;

        public event EventHandler? CanContinueEvent;

        // called by codebehind on page load
        public async void DoChecks()
        {
            const string LOG_IDENT = "WelcomeViewModel::DoChecks";

            // TODO: move into unified function that bootstrapper can use too
            GithubRelease? releaseInfo = null;

            try
            {
                releaseInfo = await Http.GetJson<GithubRelease>($"https://api.github.com/repos/{App.ProjectRepository}/releases/latest");

                if (releaseInfo is null || releaseInfo.Assets is null)
                {
                    App.Logger.WriteLine(LOG_IDENT, $"Encountered invalid data when fetching GitHub releases");
                }
                else
                {
                    if (Utilities.CompareVersions(App.Version, releaseInfo.TagName) == VersionComparison.LessThan)
                    {
                        VersionNotice = String.Format(Resources.Strings.Installer_Welcome_UpdateNotice, App.Version, releaseInfo.TagName.Replace("v", ""));
                        OnPropertyChanged(nameof(VersionNotice));
                    }
                }
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, $"Error occurred when fetching GitHub releases");
                App.Logger.WriteException(LOG_IDENT, ex);
            }

            CanContinue = true;
            OnPropertyChanged(nameof(CanContinue));

            CanContinueEvent?.Invoke(this, new EventArgs());
        }
    }
}
