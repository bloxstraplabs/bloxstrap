using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows.Forms;

using Bloxstrap.Helpers;
using Bloxstrap.Properties;

namespace Bloxstrap.Integrations
{
    public class ServerNotifier
    {
        private readonly GameActivityWatcher _activityWatcher;

        public ServerNotifier(GameActivityWatcher activityWatcher)
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

            if (_activityWatcher.ActivityMachineUDMUX)
                message += "\nServer is UDMUX protected";

            App.Logger.WriteLine($"[ServerNotifier::Notify] {message.ReplaceLineEndings("\\n")}");

            NotifyIcon notification = new()
            {
                Icon = Resources.IconBloxstrap,
                Text = "Bloxstrap",
                Visible = true,
                BalloonTipTitle = "Connected to server",
                BalloonTipText = message
            };

            notification.ShowBalloonTip(10);
            await Task.Delay(10000);
            notification.Dispose();
        }
    }
}
