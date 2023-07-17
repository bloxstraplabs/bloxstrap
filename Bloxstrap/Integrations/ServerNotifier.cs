using System.Windows;

namespace Bloxstrap.Integrations
{
    public class ServerNotifier
    {
        private readonly RobloxActivity _activityWatcher;

        public ServerNotifier(RobloxActivity activityWatcher)
        {
            _activityWatcher = activityWatcher;
            _activityWatcher.OnGameJoin += (_, _) => Task.Run(() => Notify());
        }

        public async void Notify()
        {
            string machineAddress = _activityWatcher.ActivityMachineAddress;
            string message = "";

            App.Logger.WriteLine($"[ServerNotifier::Notify] Getting server information for {machineAddress}");

            // basically nobody has a free public access geolocation api that's accurate,
            // the ones that do require an api key which isn't suitable for a client-side application like this
            // so, hopefully this is reliable enough?
            string locationCity = await App.HttpClient.GetStringAsync($"https://ipinfo.io/{machineAddress}/city");
            string locationRegion = await App.HttpClient.GetStringAsync($"https://ipinfo.io/{machineAddress}/region");
            string locationCountry = await App.HttpClient.GetStringAsync($"https://ipinfo.io/{machineAddress}/country");

            locationCity = locationCity.ReplaceLineEndings("");
            locationRegion = locationRegion.ReplaceLineEndings("");
            locationCountry = locationCountry.ReplaceLineEndings("");

            if (String.IsNullOrEmpty(locationCountry))
                message = "Location: N/A";
            else if (locationCity == locationRegion)
                message = $"Location: {locationRegion}, {locationCountry}";
            else
                message = $"Location: {locationCity}, {locationRegion}, {locationCountry}";

            message += "\nClick to copy Instance ID";

            App.Logger.WriteLine($"[ServerNotifier::Notify] {message.ReplaceLineEndings("\\n")}");

            EventHandler JobIDCopier = new((_, _) => Clipboard.SetText(_activityWatcher.ActivityJobId));

            App.Notification.BalloonTipTitle = "Connected to server";
			App.Notification.BalloonTipText = message;
            App.Notification.BalloonTipClicked += JobIDCopier;
            App.Notification.ShowBalloonTip(10);

            await Task.Delay(10000);
            App.Notification.BalloonTipClicked -= JobIDCopier;
		}
    }
}
