using System.Collections.ObjectModel;

namespace Bloxstrap.Models.Persistable
{
    public class Settings
    {
        // bloxstrap configuration
        public BootstrapperStyle BootstrapperStyle { get; set; } = BootstrapperStyle.FluentDialog;
        public BootstrapperIcon BootstrapperIcon { get; set; } = BootstrapperIcon.IconBloxstrap;
        public string BootstrapperTitle { get; set; } = App.ProjectName;
        public string BootstrapperIconCustomLocation { get; set; } = "";
        public Theme Theme { get; set; } = Theme.Default;
        public string AccentColor { get; set; } = "";
        public WindowBackdrop WindowBackdrop { get; set; } = WindowBackdrop.Mica;
        public string AppFont { get; set; } = "";
        public string BackgroundImage { get; set; } = "";
        public string BackgroundGradient { get; set; } = "";
        public double BackgroundOpacity { get; set; } = 0.3;
        public BackgroundStretch BackgroundStretch { get; set; } = BackgroundStretch.UniformToFill;
        public bool DisableAnimations { get; set; } = false;
        public bool CycleCustomThemes { get; set; } = false;
        public ThemeCycleInterval CycleCustomThemesInterval { get; set; } = ThemeCycleInterval.EveryLaunch;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool DeveloperMode { get; set; } = false;
        public bool CheckForUpdates { get; set; } = true;
        public bool ConfirmLaunches { get; set; } = false;
        public bool MultiInstanceLaunching { get; set; } = false;
        public bool BlockRobloxUpdates { get; set; } = false;
        public bool UseStaticVersionFolder { get; set; } = false;
        public string PlayerVersionOverride { get; set; } = "";
        public string StudioVersionOverride { get; set; } = "";
        public int DownloadThreads { get; set; } = 3;
        public ChannelChangeMode ChannelChangeMode { get; set; } = ChannelChangeMode.Automatic;
        public bool UpdateToPreReleases { get; set; } = false;
        public CleanerAge CleanerAge { get; set; } = CleanerAge.Never;
        public bool CleanRobloxLogs { get; set; } = true;
        public bool CleanRobloxCache { get; set; } = true;
        public bool CleanAppLogs { get; set; } = true;
        public bool CleanAppDownloads { get; set; } = true;
        public ProcessPriorityLevel RobloxPriority { get; set; } = ProcessPriorityLevel.Normal;
        public string RobloxWindowTitle { get; set; } = "";
        public bool RobloxBorderless { get; set; } = false;
        public bool RobloxGameTitle { get; set; } = false;
        public bool RobloxGameTitlePlayerCount { get; set; } = false;
        public bool RobloxGameIcon { get; set; } = false;
        public string RobloxIconPath { get; set; } = "";
        public RobloxWindowIcon RobloxWindowIcon { get; set; } = RobloxWindowIcon.Default;
        public RobloxWindowIcon StudioWindowIcon { get; set; } = RobloxWindowIcon.Default;
        public string StudioIconPath { get; set; } = "";
        public bool MemoryTrimmer { get; set; } = false;
        public int MemoryTrimThresholdMb { get; set; } = 2000;
        public int MemoryTrimIntervalSeconds { get; set; } = 60;
        public bool CloseCrashHandler { get; set; } = false;
        public string RobloxCpuAffinity { get; set; } = "";
        public RobloxAppTheme RobloxAppTheme { get; set; } = RobloxAppTheme.Default;
        public bool RobloxBackgroundApp { get; set; } = false;
        public bool ForceRobloxLanguage { get; set; } = false;
        public bool BetterMatchmaking { get; set; } = false;
        public string PreferredRegion { get; set; } = "";
        public bool RandomizeMatchmaking { get; set; } = false;
        public bool WindowControlEnabled { get; set; } = false;
        public bool WindowControlMove { get; set; } = true;
        public bool WindowControlTitle { get; set; } = true;
        public bool WindowControlTransparency { get; set; } = true;
        public bool WindowControlColor { get; set; } = true;
        public bool WindowControlWallpaper { get; set; } = false;
        public bool WindowControlDesktop { get; set; } = false;
        public bool WindowControlAllGames { get; set; } = false;
        public WindowMonitorMode WindowControlMonitor { get; set; } = WindowMonitorMode.Single;
        public int WindowControlFps { get; set; } = 60;
        public bool CrosshairEnabled { get; set; } = false;
        public CrosshairStyle CrosshairStyle { get; set; } = CrosshairStyle.Cross;
        public string CrosshairColor { get; set; } = "#00FF00";
        public double CrosshairSize { get; set; } = 8;
        public double CrosshairThickness { get; set; } = 2;
        public double CrosshairGap { get; set; } = 3;
        public ObservableCollection<GameProfile> GameProfiles { get; set; } = new();
        public bool ThrottleInBackground { get; set; } = false;
        public bool SocdEnabled { get; set; } = false;
        public SocdLayout SocdLayout { get; set; } = SocdLayout.WASD;
        public SocdMode SocdMode { get; set; } = SocdMode.LastInput;
        public int BreakReminderMinutes { get; set; } = 0;
        public bool SessionRecap { get; set; } = true;
        public bool PerformanceLog { get; set; } = true;
        public string Locale { get; set; } = "nil";
        public bool UseFastFlagManager { get; set; } = true;
        public bool WPFSoftwareRender { get; set; } = false;
        public bool EnableAnalytics { get; set; } = true;
        public bool BackgroundUpdatesEnabled { get; set; } = false;
        public bool DebugDisableVersionPackageCleanup { get; set; } = false;
        public string? SelectedCustomTheme { get; set; } = null;
        public WebEnvironment WebEnvironment { get; set; } = WebEnvironment.Production;

        // integration configuration
        public bool EnableActivityTracking { get; set; } = true;
        public bool UseDiscordRichPresence { get; set; } = true;
        public bool HideRPCButtons { get; set; } = true;
        public bool ShowAccountOnRichPresence { get; set; } = false;
        public bool ShowServerDetails { get; set; } = false;
        public bool AutoRejoin { get; set; } = false;
        public bool RejoinOnIdle { get; set; } = true;
        public bool RejoinOnConnectionLost { get; set; } = true;
        public bool RejoinOnShutdown { get; set; } = true;
        public bool RejoinOnTeleportFailure { get; set; } = false;
        public int RejoinMaxAttempts { get; set; } = 3;
        public int RejoinDelaySeconds { get; set; } = 3;
        public bool PlaytimeCounter { get; set; } = true;
        public bool TrackGameStats { get; set; } = true;
        public bool UseCustomStatusDisplay { get; set; } = true;
        public bool StudioRichPresence { get; set; } = false;
        public bool StudioShowGameName { get; set; } = true;
        public bool StudioShowGameIcon { get; set; } = true;
        public bool StudioShowTesting { get; set; } = true;
        public bool StudioShowGameButton { get; set; } = false;
        public bool StudioShowScript { get; set; } = false;
        public bool ShowUsingReistrapInRichPresence { get; set; } = true;
        public bool ShowServerUptime { get; set; } = true;
        public TrayDoubleClickAction TrayDoubleClickAction { get; set; } = TrayDoubleClickAction.ServerDetails;
        public ObservableCollection<CustomIntegration> CustomIntegrations { get; set; } = new();

        // mod preset configuration
        public bool UseDisableAppPatch { get; set; } = false;
        public bool ApplyModsToPlayer { get; set; } = true;
        public bool ApplyModsToStudio { get; set; } = true;
    }
}
