using System.Text.RegularExpressions;

using DiscordRPC;

namespace Bloxstrap.Integrations
{
    public class StudioRichPresence : IDisposable
    {
        private const string PlaceOpenEntry  = "[FLog::PlaceManager] Start to open place";
        private const string PlaceCloseEntry = "[FLog::PlaceManager] PlaceManager::closeCurrentPlayDoc";
        private const string TestStartEntry  = "[FLog::Network] NetworkServer:Start";
        private const string TestEndEntry    = "[FLog::StudioKeyEvents] end play test";

        private static readonly Regex PlaceIdPattern = new(@"open place \(identifier = (\d+)\)", RegexOptions.Compiled);
        private static readonly Regex UniverseIdPattern = new(@"universeId[ =](\d+)", RegexOptions.Compiled);

        private readonly DiscordRpcClient _rpcClient = new("1005469189907173486");

        private readonly DateTime _studioOpened = DateTime.UtcNow;

        private DateTime? _placeOpened;

        private DateTime? _testStarted;

        private long _placeId;

        private long _universeId;

        private UniverseDetails? _universeDetails;

        private readonly object _lock = new();

        private HttpListener? _listener;

        private string? _scriptName;

        private string? _scriptKind;

        private int _scriptLines;

        private class ScriptMessage
        {
            [JsonPropertyName("script")]
            public string? Script { get; set; }

            [JsonPropertyName("kind")]
            public string? Kind { get; set; }

            [JsonPropertyName("lines")]
            public int Lines { get; set; }
        }

        public StudioRichPresence(ActivityWatcher activityWatcher)
        {
            const string LOG_IDENT = "StudioRichPresence";

            activityWatcher.OnLogEntry += (_, entry) => ReadLogEntry(entry);

            _rpcClient.OnError += (_, e) =>
                App.Logger.WriteLine(LOG_IDENT, $"An RPC error occurred - {e.Message}");

            _rpcClient.Initialize();

            if (App.Settings.Prop.StudioShowScript)
                StartListener();

            UpdatePresence();
        }

        private void StartListener()
        {
            const string LOG_IDENT = "StudioRichPresence::StartListener";

            try
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add($"http://localhost:{StudioPlugin.Port}/");
                _listener.Start();

                Task.Run(Listen);

                App.Logger.WriteLine(LOG_IDENT, $"Listening for the Studio plugin on port {StudioPlugin.Port}");
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, "Could not listen for the Studio plugin");
                App.Logger.WriteException(LOG_IDENT, ex);

                _listener = null;
            }
        }

        private async Task Listen()
        {
            while (_listener is { IsListening: true } listener)
            {
                HttpListenerContext context;

                try
                {
                    context = await listener.GetContextAsync();
                }
                catch (Exception)
                {
                    return;
                }

                HandleRequest(context);
            }
        }

        private void HandleRequest(HttpListenerContext context)
        {
            using var response = context.Response;

            try
            {
                if (context.Request.HttpMethod != "POST" || context.Request.Url?.AbsolutePath != "/studio")
                {
                    response.StatusCode = 404;
                    return;
                }

                using var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8);

                char[] buffer = new char[4096];
                int length = reader.ReadBlock(buffer, 0, buffer.Length);

                var message = JsonSerializer.Deserialize<ScriptMessage>(new string(buffer, 0, length));

                if (message is null)
                {
                    response.StatusCode = 400;
                    return;
                }

                SetScript(message);
                response.StatusCode = 200;
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("StudioRichPresence::HandleRequest", ex);
                response.StatusCode = 400;
            }
        }

        private void SetScript(ScriptMessage message)
        {
            string? name = String.IsNullOrWhiteSpace(message.Script) ? null : message.Script.Trim();
            string? kind = message.Kind is "server" or "client" or "module" ? message.Kind : null;
            int lines = Math.Max(message.Lines, 0);

            if (name is not null && name.Length > 80)
                name = name[..80];

            lock (_lock)
            {
                if (name == _scriptName && kind == _scriptKind && lines == _scriptLines)
                    return;

                _scriptName = name;
                _scriptKind = name is null ? null : kind;
                _scriptLines = lines;
            }

            UpdatePresence();
        }

        private static string GetScriptLabel(string? kind) => kind switch
        {
            "client" => "Local script",
            "module" => "Module script",
            _ => "Server script"
        };

        private static string GetScriptIcon(string? kind) =>
            $"https://raw.githubusercontent.com/{App.ProjectRepository}/main/Images/StudioScripts/{kind ?? "server"}.png";

        private void ReadLogEntry(string entry)
        {
            int logMessageIdx = entry.IndexOf(' ');

            if (logMessageIdx == -1)
                return;

            string logMessage = entry[(logMessageIdx + 1)..];

            if (_placeOpened is null && logMessage.StartsWith(PlaceOpenEntry, StringComparison.Ordinal))
            {
                App.Logger.WriteLine("StudioRichPresence::ReadLogEntry", "Place opened");
                _placeOpened = DateTime.UtcNow;
                UpdatePresence();
            }
            else if (_placeOpened is not null && logMessage.StartsWith(PlaceCloseEntry, StringComparison.Ordinal))
            {
                App.Logger.WriteLine("StudioRichPresence::ReadLogEntry", "Place closed");
                _placeOpened = null;
                _testStarted = null;
                _placeId = 0;
                _universeId = 0;
                _universeDetails = null;

                lock (_lock)
                {
                    _scriptName = null;
                    _scriptKind = null;
                    _scriptLines = 0;
                }

                UpdatePresence();
            }
            else if (_placeOpened is not null && _placeId == 0 && PlaceIdPattern.Match(logMessage) is { Success: true } placeMatch)
            {
                _placeId = long.Parse(placeMatch.Groups[1].Value);
            }
            else if (_placeOpened is not null && _universeId == 0 && UniverseIdPattern.Match(logMessage) is { Success: true } universeMatch)
            {
                _universeId = long.Parse(universeMatch.Groups[1].Value);
                Task.Run(LoadUniverseDetails);
            }
            else if (_placeOpened is not null && _testStarted is null && logMessage.StartsWith(TestStartEntry, StringComparison.Ordinal))
            {
                _testStarted = DateTime.UtcNow;
                UpdatePresence();
            }
            else if (_testStarted is not null && logMessage.StartsWith(TestEndEntry, StringComparison.Ordinal))
            {
                _testStarted = null;
                UpdatePresence();
            }
        }

        private async Task LoadUniverseDetails()
        {
            long universeId = _universeId;

            try
            {
                await UniverseDetails.FetchSingle(universeId);
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("StudioRichPresence::LoadUniverseDetails", ex);
                return;
            }

            if (universeId != _universeId)
                return;

            _universeDetails = UniverseDetails.LoadFromCache(universeId);
            UpdatePresence();
        }

        private void UpdatePresence()
        {
            lock (_lock)
                SetPresence();
        }

        private void SetPresence()
        {
            var settings = App.Settings.Prop;

            bool editing = _placeOpened is not null;
            bool testing = editing && _testStarted is not null && settings.StudioShowTesting;

            string? gameName = settings.StudioShowGameName ? _universeDetails?.Data.Name : null;

            bool showScript = editing && !testing && settings.StudioShowScript && _scriptName is not null;

            string state;

            if (!editing)
                state = "Idle";
            else if (showScript)
                state = $"Editing {_scriptName} ({_scriptLines:N0} {(_scriptLines == 1 ? "line" : "lines")})";
            else if (testing)
                state = gameName is null ? "Testing a place" : $"Testing {gameName}";
            else
                state = gameName is null ? "Editing a place" : $"Editing {gameName}";

            if (state.Length > 128)
                state = state[..128];

            string? icon = settings.StudioShowGameIcon ? _universeDetails?.Thumbnail.ImageUrl : null;

            var buttons = new List<Button>();

            if (settings.StudioShowGameButton && editing && _placeId != 0 && _universeDetails is not null)
                buttons.Add(new Button { Label = "See game page", Url = $"https://www.roblox.com/games/{_placeId}" });

            _rpcClient.SetPresence(new DiscordRPC.RichPresence
            {
                Details = "Roblox Studio",
                State = state,
                StatusDisplay = StatusDisplayType.Details,
                Timestamps = new Timestamps { Start = testing ? _testStarted : editing ? _placeOpened : _studioOpened },
                Buttons = buttons.ToArray(),
                Assets = new Assets
                {
                    LargeImageKey = icon ?? "roblox",
                    LargeImageText = gameName ?? "Roblox Studio",
                    SmallImageKey = showScript ? GetScriptIcon(_scriptKind) : icon is null ? null : "roblox",
                    SmallImageText = showScript ? GetScriptLabel(_scriptKind) : icon is null ? null : "Roblox Studio"
                }
            });
        }

        public void Dispose()
        {
            try
            {
                _listener?.Close();
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("StudioRichPresence::Dispose", ex);
            }

            _listener = null;

            _rpcClient.ClearPresence();
            _rpcClient.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
