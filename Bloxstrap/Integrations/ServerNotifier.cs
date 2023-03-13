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

            if (locationCity == locationRegion)
                message = $"Location: {locationRegion}, {locationCountry}\n";
            else
                message = $"Location: {locationCity}, {locationRegion}, {locationCountry}\n";

            PingReply ping = await new Ping().SendPingAsync(machineAddress);

            // UDMUX protected servers reject ICMP packets and so the ping fails
            // we could get around this by doing a UDP ping but ehhhhhhhhhhhhh
            if (ping.Status == IPStatus.Success)
                message += $"Latency: ~{ping.RoundtripTime}ms";
            else
                message += "Latency: N/A (server may be UDMUX protected)";

            App.Logger.WriteLine($"[ServerNotifier::Notify] {message}");

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
