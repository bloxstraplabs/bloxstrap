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
            const string LOG_IDENT = "DiscordRichPresence::<construct>";

            _activityWatcher = activityWatcher;

            _activityWatcher.OnGameJoin += (_, _) => Task.Run(() => SetCurrentGame());
            _activityWatcher.OnGameLeave += (_, _) => Task.Run(() => SetCurrentGame());
            _activityWatcher.OnGameMessage += (_, message) => OnGameMessage(message);

            _rpcClient.OnReady += (_, e) =>
                App.Logger.WriteLine(LOG_IDENT, $"Received ready from user {e.User.Username} ({e.User.ID})");

            _rpcClient.OnPresenceUpdate += (_, e) =>
                App.Logger.WriteLine(LOG_IDENT, "Presence updated");

            _rpcClient.OnError += (_, e) =>
                App.Logger.WriteLine(LOG_IDENT, $"An RPC error occurred - {e.Message}");

            _rpcClient.OnConnectionEstablished += (_, e) =>
                App.Logger.WriteLine(LOG_IDENT, "Established connection with Discord RPC");

            //spams log as it tries to connect every ~15 sec when discord is closed so not now
            //_rpcClient.OnConnectionFailed += (_, e) =>
            //    App.Logger.WriteLine(LOG_IDENT, "Failed to establish connection with Discord RPC");

            _rpcClient.OnClose += (_, e) =>
                App.Logger.WriteLine(LOG_IDENT, $"Lost connection to Discord RPC - {e.Reason} ({e.Code})");

            _rpcClient.Initialize();
        }

        public void OnGameMessage(GameMessage message)
        {
            if (message.Command == "SetPresenceStatus")
                SetStatus(message.Data);
        }

        public void SetStatus(string status)
        {
            const string LOG_IDENT = "DiscordRichPresence::SetStatus";
            
            App.Logger.WriteLine(LOG_IDENT, $"Setting status to '{status}'");

            if (_currentPresence is null)
            {
                App.Logger.WriteLine(LOG_IDENT, $"Presence is not set, aborting");
                return;
            }

            if (status.Length > 128)
            {
                App.Logger.WriteLine(LOG_IDENT, $"Status cannot be longer than 128 characters, aborting");
                return;
            }

            if (_initialStatus is null)
                _initialStatus = _currentPresence.State;

            string finalStatus;

            if (string.IsNullOrEmpty(status))
            {
                App.Logger.WriteLine(LOG_IDENT, $"Status is empty, reverting to initial status");
                finalStatus = _initialStatus;
            }
            else
            {
                finalStatus = status;
            }

            if (_currentPresence.State == finalStatus)
            {
                App.Logger.WriteLine(LOG_IDENT, $"Status is unchanged, aborting");
                return;
            }

            _currentPresence.State = finalStatus;
            UpdatePresence();
        }

        public void SetVisibility(bool visible)
        {
            App.Logger.WriteLine("DiscordRichPresence::SetVisibility", $"Setting presence visibility ({visible})");

            _visible = visible;

            if (_visible)
                UpdatePresence();
            else
                _rpcClient.ClearPresence();
        }

        public async Task<bool> SetCurrentGame()
        {
            const string LOG_IDENT = "DiscordRichPresence::SetCurrentGame";
            
            if (!_activityWatcher.ActivityInGame)
            {
                App.Logger.WriteLine(LOG_IDENT, "Not in game, clearing presence");
                _currentPresence = null;
                _initialStatus = null;
                UpdatePresence();
                return true;
            }

            string icon = "roblox";
            long placeId = _activityWatcher.ActivityPlaceId;

            App.Logger.WriteLine(LOG_IDENT, $"Setting presence for Place ID {placeId}");

            var universeIdResponse = await Http.GetJson<UniverseIdResponse>($"https://apis.roblox.com/universes/v1/places/{placeId}/universe");
            if (universeIdResponse is null)
            {
                App.Logger.WriteLine(LOG_IDENT, "Could not get Universe ID!");
                return false;
            }

            long universeId = universeIdResponse.UniverseId;
            App.Logger.WriteLine(LOG_IDENT, $"Got Universe ID as {universeId}");

            // preserve time spent playing if we're teleporting between places in the same universe
            if (_timeStartedUniverse is null || !_activityWatcher.ActivityIsTeleport || universeId != _currentUniverseId)
                _timeStartedUniverse = DateTime.UtcNow;

            _currentUniverseId = universeId;

            var gameDetailResponse = await Http.GetJson<ApiArrayResponse<GameDetailResponse>>($"https://games.roblox.com/v1/games?universeIds={universeId}");
            if (gameDetailResponse is null || !gameDetailResponse.Data.Any())
            {
                App.Logger.WriteLine(LOG_IDENT, "Could not get Universe info!");
                return false;
            }

            GameDetailResponse universeDetails = gameDetailResponse.Data.ToArray()[0];
            App.Logger.WriteLine(LOG_IDENT, "Got Universe details");

            var universeThumbnailResponse = await Http.GetJson<ApiArrayResponse<ThumbnailResponse>>($"https://thumbnails.roblox.com/v1/games/icons?universeIds={universeId}&returnPolicy=PlaceHolder&size=512x512&format=Png&isCircular=false");
            if (universeThumbnailResponse is null || !universeThumbnailResponse.Data.Any())
            {
                App.Logger.WriteLine(LOG_IDENT, "Could not get Universe thumbnail info!");
            }
            else
            {
                icon = universeThumbnailResponse.Data.ToArray()[0].ImageUrl;
                App.Logger.WriteLine(LOG_IDENT, $"Got Universe thumbnail as {icon}");
            }

            List<Button> buttons = new();

            if (!App.Settings.Prop.HideRPCButtons && _activityWatcher.ActivityServerType == ServerType.Public)
            {
                buttons.Add(new Button
                {
                    Label = "Join server",
                    Url = $"roblox://experiences/start?placeId={placeId}&gameInstanceId={_activityWatcher.ActivityJobId}"
                });
            }

            buttons.Add(new Button
            {
                Label = "See game page",
                Url = $"https://www.roblox.com/games/{placeId}"
            });

            if (!_activityWatcher.ActivityInGame || placeId != _activityWatcher.ActivityPlaceId)
            {
                App.Logger.WriteLine(LOG_IDENT, "Aborting presence set because game activity has changed");
                return false;
            }

            string status = _activityWatcher.ActivityServerType switch
            {
                ServerType.Private => "In a private server",
                ServerType.Reserved => "In a reserved server",
                _ => $"by {universeDetails.Creator.Name}" + (universeDetails.Creator.HasVerifiedBadge ? " ☑️" : ""),
            };

            _currentPresence = new RichPresence
            {
                Details = $"Playing {universeDetails.Name}",
                State = status,
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
            const string LOG_IDENT = "DiscordRichPresence::UpdatePresence";
            
            if (_currentPresence is null)
            {
                App.Logger.WriteLine(LOG_IDENT, $"Presence is empty, clearing");
                _rpcClient.ClearPresence();
                return;
            }

            App.Logger.WriteLine(LOG_IDENT, $"Updating presence");

            if (_visible)
                _rpcClient.SetPresence(_currentPresence);
        }

        public void Dispose()
        {
            App.Logger.WriteLine("DiscordRichPresence::Dispose", "Cleaning up Discord RPC and Presence");
            _rpcClient.ClearPresence();
            _rpcClient.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
