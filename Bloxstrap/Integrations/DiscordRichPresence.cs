using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DiscordRPC;

using Bloxstrap.Models.RobloxApi;

namespace Bloxstrap.Integrations
{
    public class DiscordRichPresence : IDisposable
    {
        private readonly DiscordRpcClient _rpcClient = new("1005469189907173486");
        private readonly GameActivityWatcher _activityWatcher;
        
        private long _currentUniverseId;
        private DateTime? _timeStartedUniverse;

        public DiscordRichPresence(GameActivityWatcher activityWatcher)
        {
            _activityWatcher = activityWatcher;

            _activityWatcher.OnGameJoin += (_, _) => Task.Run(() => SetPresence());
            _activityWatcher.OnGameLeave += (_, _) => Task.Run(() => SetPresence());

            _rpcClient.OnReady += (_, e) =>
                App.Logger.WriteLine($"[DiscordRichPresence::DiscordRichPresence] Received ready from user {e.User.Username} ({e.User.ID})");

            _rpcClient.OnPresenceUpdate += (_, e) =>
                App.Logger.WriteLine("[DiscordRichPresence::DiscordRichPresence] Updated presence");

            _rpcClient.OnConnectionEstablished += (_, e) =>
                App.Logger.WriteLine("[DiscordRichPresence::DiscordRichPresence] Established connection with Discord RPC");

            //spams log as it tries to connect every ~15 sec when discord is closed so not now
            //_rpcClient.OnConnectionFailed += (_, e) =>
            //    App.Logger.WriteLine("[DiscordRichPresence::DiscordRichPresence] Failed to establish connection with Discord RPC");

            _rpcClient.OnClose += (_, e) =>
                App.Logger.WriteLine($"[DiscordRichPresence::DiscordRichPresence] Lost connection to Discord RPC - {e.Reason} ({e.Code})");

            _rpcClient.Initialize();
        }

        public async Task<bool> SetPresence()
        {
            if (!_activityWatcher.ActivityInGame)
            {
                App.Logger.WriteLine($"[DiscordRichPresence::SetPresence] Clearing presence");
                _rpcClient.ClearPresence();
                return true;
            }

            string icon = "roblox";

            App.Logger.WriteLine($"[DiscordRichPresence::SetPresence] Setting presence for Place ID {_activityWatcher.ActivityPlaceId}");

            var universeIdResponse = await Utilities.GetJson<UniverseIdResponse>($"https://apis.roblox.com/universes/v1/places/{_activityWatcher.ActivityPlaceId}/universe");
            if (universeIdResponse is null)
            {
                App.Logger.WriteLine($"[DiscordRichPresence::SetPresence] Could not get Universe ID!");
                return false;
            }

            long universeId = universeIdResponse.UniverseId;
            App.Logger.WriteLine($"[DiscordRichPresence::SetPresence] Got Universe ID as {universeId}");

            // preserve time spent playing if we're teleporting between places in the same universe
            if (_timeStartedUniverse is null || !_activityWatcher.ActivityIsTeleport || universeId != _currentUniverseId)
                _timeStartedUniverse = DateTime.UtcNow;

            _activityWatcher.ActivityIsTeleport = false;
            _currentUniverseId = universeId;

            var gameDetailResponse = await Utilities.GetJson<ApiArrayResponse<GameDetailResponse>>($"https://games.roblox.com/v1/games?universeIds={universeId}");
            if (gameDetailResponse is null || !gameDetailResponse.Data.Any())
            {
                App.Logger.WriteLine($"[DiscordRichPresence::SetPresence] Could not get Universe info!");
                return false;
            }

            GameDetailResponse universeDetails = gameDetailResponse.Data.ToArray()[0];
            App.Logger.WriteLine($"[DiscordRichPresence::SetPresence] Got Universe details");

            var universeThumbnailResponse = await Utilities.GetJson<ApiArrayResponse<ThumbnailResponse>>($"https://thumbnails.roblox.com/v1/games/icons?universeIds={universeId}&returnPolicy=PlaceHolder&size=512x512&format=Png&isCircular=false");
            if (universeThumbnailResponse is null || !universeThumbnailResponse.Data.Any())
            {
                App.Logger.WriteLine($"[DiscordRichPresence::SetPresence] Could not get Universe thumbnail info!");
            }
            else
            {
                icon = universeThumbnailResponse.Data.ToArray()[0].ImageUrl;
                App.Logger.WriteLine($"[DiscordRichPresence::SetPresence] Got Universe thumbnail as {icon}");
            }

            List<Button> buttons = new()
            {
                new Button
                {
                    Label = "See Details",
                    Url = $"https://www.roblox.com/games/{_activityWatcher.ActivityPlaceId}"
                }
            };

            if (!App.Settings.Prop.HideRPCButtons)
            {
                buttons.Insert(0, new Button
                {
                    Label = "Join",
                    Url = $"roblox://experiences/start?placeId={_activityWatcher.ActivityPlaceId}&gameInstanceId={_activityWatcher.ActivityJobId}"
                });
            }

            _rpcClient.SetPresence(new RichPresence
            {
                Details = universeDetails.Name,
                State = $"by {universeDetails.Creator.Name}" + (universeDetails.Creator.HasVerifiedBadge ? " ☑️" : ""),
                Timestamps = new Timestamps { Start = _timeStartedUniverse },
                Buttons = buttons.ToArray(),
                Assets = new Assets
                {
                    LargeImageKey = icon,
                    LargeImageText = universeDetails.Name,
                    SmallImageKey = "roblox",
                    SmallImageText = "Roblox"
                }
            });

            return true;
        }

        public void Dispose()
        {
            App.Logger.WriteLine("[DiscordRichPresence::Dispose] Cleaning up Discord RPC and Presence");
            _rpcClient.ClearPresence();
            _rpcClient.Dispose();
        }
    }
}
