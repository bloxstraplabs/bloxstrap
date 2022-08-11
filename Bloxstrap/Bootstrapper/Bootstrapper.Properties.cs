using Bloxstrap.Helpers.RSMM;

namespace Bloxstrap
{
    partial class Bootstrapper
    {
        private string? LaunchCommandLine;

        private string VersionGuid;
        private PackageManifest VersionPackageManifest;
        private FileManifest VersionFileManifest;
        private string VersionFolder;

        private readonly string DownloadsFolder;
        private readonly bool FreshInstall;

        private int ProgressIncrement;
        private bool CancelFired = false;

        private static readonly HttpClient Client = new();

        // in case a new package is added, you can find the corresponding directory
        // by opening the stock bootstrapper in a hex editor
        // TODO - there ideally should be a less static way to do this that's not hardcoded?
        private static readonly IReadOnlyDictionary<string, string> PackageDirectories = new Dictionary<string, string>()
        {
            { "RobloxApp.zip",                 @"" },
            { "shaders.zip",                   @"shaders\" },
            { "ssl.zip",                       @"ssl\" },

            { "content-avatar.zip",            @"content\avatar\" },
            { "content-configs.zip",           @"content\configs\" },
            { "content-fonts.zip",             @"content\fonts\" },
            { "content-sky.zip",               @"content\sky\" },
            { "content-sounds.zip",            @"content\sounds\" },
            { "content-textures2.zip",         @"content\textures\" },
            { "content-models.zip",            @"content\models\" },

            { "content-textures3.zip",         @"PlatformContent\pc\textures\" },
            { "content-terrain.zip",           @"PlatformContent\pc\terrain\" },
            { "content-platform-fonts.zip",    @"PlatformContent\pc\fonts\" },

            { "extracontent-luapackages.zip",  @"ExtraContent\LuaPackages\" },
            { "extracontent-translations.zip", @"ExtraContent\translations\" },
            { "extracontent-models.zip",       @"ExtraContent\models\" },
            { "extracontent-textures.zip",     @"ExtraContent\textures\" },
            { "extracontent-places.zip",       @"ExtraContent\places\" },
        };

        private static readonly string AppSettings =
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
            "<Settings>\n" +
            "	<ContentFolder>content</ContentFolder>\n" +
            "	<BaseUrl>http://www.roblox.com</BaseUrl>\n" +
            "</Settings>\n";

        public event EventHandler CloseDialogEvent;
        public event EventHandler PromptShutdownEvent;
        public event ChangeEventHandler<string> ShowSuccessEvent;
        public event ChangeEventHandler<string> MessageChanged;
        public event ChangeEventHandler<int> ProgressBarValueChanged;
        public event ChangeEventHandler<ProgressBarStyle> ProgressBarStyleChanged;
        public event ChangeEventHandler<bool> CancelEnabledChanged;

        private string _message;
        private int _progress = 0;
        private ProgressBarStyle _progressStyle = ProgressBarStyle.Marquee;
        private bool _cancelEnabled = false;

        public string Message
        {
            get => _message;

            private set
            {
                if (_message == value)
                    return;

                MessageChanged.Invoke(this, new ChangeEventArgs<string>(value));

                _message = value;
            }
        }

        public int Progress
        {
            get => _progress;

            private set
            {
                if (_progress == value)
                    return;

                ProgressBarValueChanged.Invoke(this, new ChangeEventArgs<int>(value));

                _progress = value;
            }
        }

        public ProgressBarStyle ProgressStyle
        {
            get => _progressStyle;

            private set
            {
                if (_progressStyle == value)
                    return;

                ProgressBarStyleChanged.Invoke(this, new ChangeEventArgs<ProgressBarStyle>(value));

                _progressStyle = value;
            }
        }

        public bool CancelEnabled
        {
            get => _cancelEnabled;

            private set
            {
                if (_cancelEnabled == value)
                    return;

                CancelEnabledChanged.Invoke(this, new ChangeEventArgs<bool>(value));

                _cancelEnabled = value;
            }
        }
    }
}
