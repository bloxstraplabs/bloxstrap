using DiscordRPC;

namespace Bloxstrap.Integrations
{
    public class DiscordRichPresence : IDisposable
    {
        private readonly DiscordRpcClient _rpcClient = new("1005469189907173486");
        private readonly ActivityWatcher _activityWatcher;
        
        private DiscordRPC.RichPresence? _currentPresence;
        private DiscordRPC.RichPresence? _currentPresenceCopy;
        private Message? _stashedRPCMessage;

        private bool _visible = true;
        private long _currentUniverseId;
        private DateTime? _timeStartedUniverse;

        public DiscordRichPresence(ActivityWatcher activityWatcher)
        {
            const string LOG_IDENT = "DiscordRichPresence::DiscordRichPresence";

            _activityWatcher = activityWatcher;

            _activityWatcher.OnGameJoin += (_, _) => Task.Run(() => SetCurrentGame());
            _activityWatcher.OnGameLeave += (_, _) => Task.Run(() => SetCurrentGame());
            _activityWatcher.OnRPCMessage += (_, message) => ProcessRPCMessage(message);

            _rpcClient.OnReady += (_, e) =>
                App.Logger.WriteLine(LOG_IDENT, $"Received ready from user {e.User} ({e.User.ID})");

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

        public void ProcessRPCMessage(Message message)
        {
            const string LOG_IDENT = "DiscordRichPresence::ProcessRPCMessage";

            if (message.Command != "SetRichPresence")
                return;

            if (_currentPresence is null || _currentPresenceCopy is null)
            {
                if (_activityWatcher.ActivityInGame)
                {
                    App.Logger.WriteLine(LOG_IDENT, "Presence is not yet set, but is currently in game, stashing presence set request");
                    _stashedRPCMessage = message;
                    return;
                }

                App.Logger.WriteLine(LOG_IDENT, "Presence is not set, aborting");
                return;
            }

            Models.BloxstrapRPC.RichPresence? presenceData;
            
            // a lot of repeated code here, could this somehow be cleaned up?

            try
            {
                presenceData = message.Data.Deserialize<Models.BloxstrapRPC.RichPresence>();
            }
            catch (Exception)
            {
                App.Logger.WriteLine(LOG_IDENT, "Failed to parse message! (JSON deserialization threw an exception)");
                return;
            }

            if (presenceData is null)
            {
                App.Logger.WriteLine(LOG_IDENT, "Failed to parse message! (JSON deserialization returned null)");
                return;
            }

            if (presenceData.Details is not null)
            {
                if (presenceData.Details.Length > 128)
                    App.Logger.WriteLine(LOG_IDENT, $"Details cannot be longer than 128 characters");
                else if (presenceData.Details == "<reset>")
                    _currentPresence.Details = _currentPresenceCopy.Details;
                else
                    _currentPresence.Details = presenceData.Details;
            }

            if (presenceData.State is not null)
            {
                if (presenceData.State.Length > 128)
                    App.Logger.WriteLine(LOG_IDENT, $"State cannot be longer than 128 characters");
                else if (presenceData.State == "<reset>")
                    _currentPresence.State = _currentPresenceCopy.State;
                else
                    _currentPresence.State = presenceData.State;
            }

            if (presenceData.TimestampStart == 0)
                _currentPresence.Timestamps.Start = null;
            else if (presenceData.TimestampStart is not null)
                _currentPresence.Timestamps.StartUnixMilliseconds = presenceData.TimestampStart * 1000;

            if (presenceData.TimestampEnd == 0)
                _currentPresence.Timestamps.End = null;
            else if (presenceData.TimestampEnd is not null)
                _currentPresence.Timestamps.EndUnixMilliseconds = presenceData.TimestampEnd * 1000;

            if (presenceData.SmallImage is not null)
            {
                if (presenceData.SmallImage.Clear)
                {
                    _currentPresence.Assets.SmallImageKey = "";
                }
                else if (presenceData.SmallImage.Reset)
                {
                    _currentPresence.Assets.SmallImageText = _currentPresenceCopy.Assets.SmallImageText;
                    _currentPresence.Assets.SmallImageKey = _currentPresenceCopy.Assets.SmallImageKey;
                }
                else
                {
                    if (presenceData.SmallImage.AssetId is not null)
                        _currentPresence.Assets.SmallImageKey = $"https://assetdelivery.roblox.com/v1/asset/?id={presenceData.SmallImage.AssetId}";

                    if (presenceData.SmallImage.HoverText is not null)
                        _currentPresence.Assets.SmallImageText = presenceData.SmallImage.HoverText;
                }
            }

            if (presenceData.LargeImage is not null)
            {
                if (presenceData.LargeImage.Clear)
                {
                    _currentPresence.Assets.LargeImageKey = "";
                }
                else if (presenceData.LargeImage.Reset)
                {
                    _currentPresence.Assets.LargeImageText = _currentPresenceCopy.Assets.LargeImageText;
                    _currentPresence.Assets.LargeImageKey = _currentPresenceCopy.Assets.LargeImageKey;
                }
                else
                {
                    if (presenceData.LargeImage.AssetId is not null)
                        _currentPresence.Assets.LargeImageKey = $"https://assetdelivery.roblox.com/v1/asset/?id={presenceData.LargeImage.AssetId}";

                    if (presenceData.LargeImage.HoverText is not null)
                        _currentPresence.Assets.LargeImageText = presenceData.LargeImage.HoverText;
                }
            }

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

                _currentPresence = _currentPresenceCopy =  null;
                _stashedRPCMessage = null;

                UpdatePresence();
                return true;
            }

            string icon = "roblox";
            string smallimagetext = "Roblox";
            string smallimage = "roblox";
            
            long placeId = _activityWatcher.ActivityPlaceId;
            string userId = _activityWatcher.ActivityUserId;
            
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

            //right now this is configured in a way so that it will only show your avatar and username if you have chosen to allow people to join you from their profile.
            if (!App.Settings.Prop.HideRPCButtons)
            {
                var userPfpResponse = await Http.GetJson<ApiArrayResponse<ThumbnailResponse>>($"https://thumbnails.roblox.com/v1/users/avatar-headshot?userIds={userId}&size=180x180&format=Png&isCircular=false"); //we can remove '-headshot' from the url if we want a full avatar picture
                if (userPfpResponse is null || !userPfpResponse.Data.Any())
                {
                    App.Logger.WriteLine(LOG_IDENT, "Could not get user thumbnail info!");
                }
                else
                {
                    smallimage = userPfpResponse.Data.ToArray()[0].ImageUrl;
                    App.Logger.WriteLine(LOG_IDENT, $"Got user thumbnail as {smallimage}");
                }
                
                var userInfoResponse = await Http.GetJson<UserInfoResponse>($"https://users.roblox.com/v1/users/{userId}");
                if (userInfoResponse is null)
                {
                    App.Logger.WriteLine(LOG_IDENT, "Could not get user info!");
                } else {
                    smallimagetext = userInfoResponse.DisplayName + $" (@{userInfoResponse.Username})"; //example: john doe (@johndoe)
                    App.Logger.WriteLine(LOG_IDENT, $"Got user info as {smallimagetext}");
                }
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

            if (universeDetails.Name.Length < 2)
                universeDetails.Name = $"{universeDetails.Name}\x2800\x2800\x2800";

            _currentPresence = new DiscordRPC.RichPresence
            {
                Details = $"Playing {universeDetails.Name}",
                State = status,
                Timestamps = new Timestamps { Start = _timeStartedUniverse },
                Buttons = buttons.ToArray(),
                Assets = new Assets
                {
                    LargeImageKey = icon,
                    LargeImageText = universeDetails.Name,
                    SmallImageKey = smallimage,
                    SmallImageText = smallimagetext
                }
            };

            // this is used for configuration from BloxstrapRPC
            _currentPresenceCopy = _currentPresence.Clone();

            if (_stashedRPCMessage is not null)
            {
                App.Logger.WriteLine(LOG_IDENT, "Found stashed RPC message, invoking presence set command now");
                ProcessRPCMessage(_stashedRPCMessage);
                _stashedRPCMessage = null;
            }
            else
            {
                UpdatePresence();
            }

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
