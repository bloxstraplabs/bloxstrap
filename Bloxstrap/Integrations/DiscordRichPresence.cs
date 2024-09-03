using DiscordRPC;

namespace Bloxstrap.Integrations
{
    public class DiscordRichPresence : IDisposable
    {
        private readonly DiscordRpcClient _rpcClient = new("1005469189907173486");
        private readonly ActivityWatcher _activityWatcher;
        
        private DiscordRPC.RichPresence? _currentPresence;
        private DiscordRPC.RichPresence? _currentPresenceCopy;
        private Queue<Message> _messageQueue = new();

        private bool _visible = true;

        public DiscordRichPresence(ActivityWatcher activityWatcher)
        {
            const string LOG_IDENT = "DiscordRichPresence";

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

        public void ProcessRPCMessage(Message message, bool implicitUpdate = true)
        {
            const string LOG_IDENT = "DiscordRichPresence::ProcessRPCMessage";

            if (message.Command != "SetRichPresence" && message.Command != "SetLaunchData")
                return;

            if (_currentPresence is null || _currentPresenceCopy is null)
            {
                App.Logger.WriteLine(LOG_IDENT, "Presence is not set, enqueuing message");
                _messageQueue.Enqueue(message);
                return;
            }

            // a lot of repeated code here, could this somehow be cleaned up?

            if (message.Command == "SetLaunchData")
            {
                var buttonQuery = _currentPresence.Buttons.Where(x => x.Label == "Join server");

                if (!buttonQuery.Any())
                    return;

                buttonQuery.First().Url = _activityWatcher.Data.GetInviteDeeplink();
            }
            else if (message.Command == "SetRichPresence")
            {
                Models.BloxstrapRPC.RichPresence? presenceData;

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
            }

            if (implicitUpdate)
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
            
            if (!_activityWatcher.InGame)
            {
                App.Logger.WriteLine(LOG_IDENT, "Not in game, clearing presence");

                _currentPresence = _currentPresenceCopy =  null;
                _messageQueue.Clear();

                UpdatePresence();
                return true;
            }

            string icon = "roblox";

            var activity = _activityWatcher.Data;
            long placeId = activity.PlaceId;

            App.Logger.WriteLine(LOG_IDENT, $"Setting presence for Place ID {placeId}");

            // preserve time spent playing if we're teleporting between places in the same universe
            var timeStarted = activity.TimeJoined;

            if (activity.RootActivity is not null)
                timeStarted = activity.RootActivity.TimeJoined;

            if (activity.UniverseDetails is null)
            {
                await UniverseDetails.FetchSingle(activity.UniverseId);
                activity.UniverseDetails = UniverseDetails.LoadFromCache(activity.UniverseId);
            }

            var universeDetails = activity.UniverseDetails;

            if (universeDetails is null)
            {
                Frontend.ShowMessageBox(Strings.ActivityTracker_RichPresenceLoadFailed, System.Windows.MessageBoxImage.Warning);
                return false;
            }

            icon = universeDetails.Thumbnail.ImageUrl;

            List<Button> buttons = new();

            if (!App.Settings.Prop.HideRPCButtons && activity.ServerType == ServerType.Public)
            {
                buttons.Add(new Button
                {
                    Label = "Join server",
                    Url = activity.GetInviteDeeplink()
                });
            }

            buttons.Add(new Button
            {
                Label = "See game page",
                Url = $"https://www.roblox.com/games/{placeId}"
            });

            if (!_activityWatcher.InGame || placeId != activity.PlaceId)
            {
                App.Logger.WriteLine(LOG_IDENT, "Aborting presence set because game activity has changed");
                return false;
            }

            string status = _activityWatcher.Data.ServerType switch
            {
                ServerType.Private => "In a private server",
                ServerType.Reserved => "In a reserved server",
                _ => $"by {universeDetails.Data.Creator.Name}" + (universeDetails.Data.Creator.HasVerifiedBadge ? " ☑️" : ""),
            };

            string universeName = universeDetails.Data.Name;

            if (universeName.Length < 2)
                universeName = $"{universeName}\x2800\x2800\x2800";

            _currentPresence = new DiscordRPC.RichPresence
            {
                Details = $"Playing {universeName}",
                State = status,
                Timestamps = new Timestamps { Start = timeStarted.ToUniversalTime() },
                Buttons = buttons.ToArray(),
                Assets = new Assets
                {
                    LargeImageKey = icon,
                    LargeImageText = universeName,
                    SmallImageKey = "roblox",
                    SmallImageText = "Roblox"
                }
            };

            // this is used for configuration from BloxstrapRPC
            _currentPresenceCopy = _currentPresence.Clone();

            if (_messageQueue.Any())
            {
                App.Logger.WriteLine(LOG_IDENT, "Processing queued messages");
                ProcessRPCMessage(_messageQueue.Dequeue(), false);
            }
            
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
