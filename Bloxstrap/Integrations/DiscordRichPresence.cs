using DiscordRPC;

namespace Bloxstrap.Integrations
{
    public class DiscordRichPresence : IDisposable
    {
        private readonly DiscordRpcClient _rpcClient = new("1005469189907173486");
        private readonly RobloxActivity _activityWatcher;
        
        private RichPresence? _currentPresence;
        private bool _visible = true;
        private string? _initialStatus;
        private long _currentUniverseId;
        private DateTime? _timeStartedUniverse;

        public DiscordRichPresence(RobloxActivity activityWatcher)
        {
            _activityWatcher = activityWatcher;

            _activityWatcher.OnGameJoin += (_, _) => Task.Run(() => SetCurrentGame());
            _activityWatcher.OnGameLeave += (_, _) => Task.Run(() => SetCurrentGame());
            _activityWatcher.OnGameMessage += (_, message) => OnGameMessage(message);

            _rpcClient.OnReady += (_, e) =>
                App.Logger.WriteLine($"[DiscordRichPresence::DiscordRichPresence] Received ready from user {e.User.Username} ({e.User.ID})");

            _rpcClient.OnPresenceUpdate += (_, e) =>
                App.Logger.WriteLine("[DiscordRichPresence::DiscordRichPresence] Updated presence");

            _rpcClient.OnError += (_, e) =>
                App.Logger.WriteLine($"[DiscordRichPresence::DiscordRichPresence] An RPC error occurred - {e.Message}");

            _rpcClient.OnConnectionEstablished += (_, e) =>
                App.Logger.WriteLine("[DiscordRichPresence::DiscordRichPresence] Established connection with Discord RPC");

            //spams log as it tries to connect every ~15 sec when discord is closed so not now
            //_rpcClient.OnConnectionFailed += (_, e) =>
            //    App.Logger.WriteLine("[DiscordRichPresence::DiscordRichPresence] Failed to establish connection with Discord RPC");

            _rpcClient.OnClose += (_, e) =>
                App.Logger.WriteLine($"[DiscordRichPresence::DiscordRichPresence] Lost connection to Discord RPC - {e.Reason} ({e.Code})");

            _rpcClient.Initialize();
        }

        public void OnGameMessage(GameMessage message)
        {
            if (message.Command == "SetPresenceStatus")
                SetStatus(message.Data);
        }

        public void SetStatus(string status)
        {
            App.Logger.WriteLine($"[DiscordRichPresence::SetStatus] Setting status to '{status}'");

            if (_currentPresence is null)
            {
                App.Logger.WriteLine($"[DiscordRichPresence::SetStatus] Presence is not set, aborting");
                return;
            }

            if (status.Length > 128)
            {
                App.Logger.WriteLine($"[DiscordRichPresence::SetStatus] Status cannot be longer than 128 characters, aborting");
                return;
            }

            if (_initialStatus is null)
                _initialStatus = _currentPresence.State;

            string finalStatus;

            if (string.IsNullOrEmpty(status))
            {
                App.Logger.WriteLine($"[DiscordRichPresence::SetStatus] Status is empty, reverting to initial status");
                finalStatus = _initialStatus;
            }
            else
            {
                finalStatus = status;
            }

            if (_currentPresence.State == finalStatus)
            {
                App.Logger.WriteLine($"[DiscordRichPresence::SetStatus] Status is unchanged, aborting");
                return;
            }

            _currentPresence.State = finalStatus;
            UpdatePresence();
        }

        public void SetVisibility(bool visible)
        {
            App.Logger.WriteLine($"[DiscordRichPresence::SetVisibility] Setting presence visibility ({visible})");

            _visible = visible;

            if (_visible)
                UpdatePresence();
            else
                _rpcClient.ClearPresence();
        }

        public async Task<bool> SetCurrentGame()
        {
            if (!_activityWatcher.ActivityInGame)
            {
                App.Logger.WriteLine($"[DiscordRichPresence::SetCurrentGame] Not in game, clearing presence");
                _currentPresence = null;
                _initialStatus = null;
                UpdatePresence();
                return true;
            }

            string icon = "roblox";

            App.Logger.WriteLine($"[DiscordRichPresence::SetCurrentGame] Setting presence for Place ID {_activityWatcher.ActivityPlaceId}");

            var universeIdResponse = await Utility.Http.GetJson<UniverseIdResponse>($"https://apis.roblox.com/universes/v1/places/{_activityWatcher.ActivityPlaceId}/universe");
            if (universeIdResponse is null)
            {
                App.Logger.WriteLine($"[DiscordRichPresence::SetCurrentGame] Could not get Universe ID!");
                return false;
            }

            long universeId = universeIdResponse.UniverseId;
            App.Logger.WriteLine($"[DiscordRichPresence::SetCurrentGame] Got Universe ID as {universeId}");

            // preserve time spent playing if we're teleporting between places in the same universe
            if (_timeStartedUniverse is null || !_activityWatcher.ActivityIsTeleport || universeId != _currentUniverseId)
                _timeStartedUniverse = DateTime.UtcNow;

            _activityWatcher.ActivityIsTeleport = false;
            _currentUniverseId = universeId;

            var gameDetailResponse = await Utility.Http.GetJson<ApiArrayResponse<GameDetailResponse>>($"https://games.roblox.com/v1/games?universeIds={universeId}");
            if (gameDetailResponse is null || !gameDetailResponse.Data.Any())
            {
                App.Logger.WriteLine($"[DiscordRichPresence::SetCurrentGame] Could not get Universe info!");
                return false;
            }

            GameDetailResponse universeDetails = gameDetailResponse.Data.ToArray()[0];
            App.Logger.WriteLine($"[DiscordRichPresence::SetCurrentGame] Got Universe details");

            var universeThumbnailResponse = await Utility.Http.GetJson<ApiArrayResponse<ThumbnailResponse>>($"https://thumbnails.roblox.com/v1/games/icons?universeIds={universeId}&returnPolicy=PlaceHolder&size=512x512&format=Png&isCircular=false");
            if (universeThumbnailResponse is null || !universeThumbnailResponse.Data.Any())
            {
                App.Logger.WriteLine($"[DiscordRichPresence::SetCurrentGame] Could not get Universe thumbnail info!");
            }
            else
            {
                icon = universeThumbnailResponse.Data.ToArray()[0].ImageUrl;
                App.Logger.WriteLine($"[DiscordRichPresence::SetCurrentGame] Got Universe thumbnail as {icon}");
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

            // so turns out discord rejects the presence set request if the place name is less than 2 characters long lol
            if (universeDetails.Name.Length < 2)
                universeDetails.Name = $"{universeDetails.Name}\x2800\x2800\x2800";

            _currentPresence = new RichPresence
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
            };

            UpdatePresence();

            return true;
        }

        public void UpdatePresence()
        {
            App.Logger.WriteLine($"[DiscordRichPresence::UpdatePresence] Updating presence");

            if (_currentPresence is null)
            {
                App.Logger.WriteLine($"[DiscordRichPresence::UpdatePresence] Presence is empty, clearing");
                _rpcClient.ClearPresence();
                return;
            }

            if (_visible)
                _rpcClient.SetPresence(_currentPresence);
        }

        public void Dispose()
        {
            App.Logger.WriteLine("[DiscordRichPresence::Dispose] Cleaning up Discord RPC and Presence");
            _rpcClient.ClearPresence();
            _rpcClient.Dispose();
        }
    }
}
