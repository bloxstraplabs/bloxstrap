namespace Bloxstrap.Resources
{
    public static class LumiFeatureStrings
    {
        private static string Get(string key)
            => Strings.ResourceManager.GetString(key, Strings.Culture) ?? key;

        public static string Menu_FastFlags_ClientPreset_Title => Get("Menu.FastFlags.ClientPreset.Title");
        public static string Menu_FastFlags_ClientPreset_Description => Get("Menu.FastFlags.ClientPreset.Description");
        public static string Menu_FastFlags_ClientPreset_Performance => Get("Menu.FastFlags.ClientPreset.Performance");
        public static string Menu_FastFlags_ClientPreset_Quality => Get("Menu.FastFlags.ClientPreset.Quality");
        public static string Menu_FastFlags_ClientPreset_Current => Get("Menu.FastFlags.ClientPreset.Current");
        public static string Menu_FastFlags_ClientPreset_Custom => Get("Menu.FastFlags.ClientPreset.Custom");
        public static string Menu_FastFlags_ClientPreset_ApplyPrompt => Get("Menu.FastFlags.ClientPreset.ApplyPrompt");
        public static string Menu_FastFlags_ClientPreset_Applied => Get("Menu.FastFlags.ClientPreset.Applied");
        public static string Menu_FastFlags_ClientPreset_ApplyFailed => Get("Menu.FastFlags.ClientPreset.ApplyFailed");
        public static string Menu_Profiles_HistoryTitle => Get("Menu.Profiles.HistoryTitle");
        public static string Menu_Profiles_HistoryDescription => Get("Menu.Profiles.HistoryDescription");
        public static string Menu_Profiles_HistoryEntry => Get("Menu.Profiles.HistoryEntry");
        public static string Menu_Profiles_HistoryEmpty => Get("Menu.Profiles.HistoryEmpty");
        public static string Menu_Profiles_RestoreHistory => Get("Menu.Profiles.RestoreHistory");
        public static string Menu_Profiles_HistoryRestorePrompt => Get("Menu.Profiles.HistoryRestorePrompt");
        public static string Menu_Integrations_PrivacyMode_Title => Get("Menu.Integrations.PrivacyMode.Title");
        public static string Menu_Integrations_PrivacyMode_Description => Get("Menu.Integrations.PrivacyMode.Description");
        public static string Menu_LumiFix_PreflightTitle => Get("Menu.LumiFix.PreflightTitle");
        public static string Menu_LumiFix_PreflightDescription => Get("Menu.LumiFix.PreflightDescription");
        public static string Menu_LumiFix_PreflightRun => Get("Menu.LumiFix.PreflightRun");
        public static string Menu_LumiFix_PreflightChecking => Get("Menu.LumiFix.PreflightChecking");
        public static string Menu_LumiFix_PreflightReady => Get("Menu.LumiFix.PreflightReady");
        public static string Menu_LumiFix_PreflightOtherInstance => Get("Menu.LumiFix.PreflightOtherInstance");
        public static string Menu_LumiFix_PreflightRobloxRunning => Get("Menu.LumiFix.PreflightRobloxRunning");
        public static string Menu_LumiFix_PreflightNotInstalled => Get("Menu.LumiFix.PreflightNotInstalled");
        public static string Menu_LumiFix_PreflightExecutableMissing => Get("Menu.LumiFix.PreflightExecutableMissing");
        public static string Menu_LumiFix_PreflightFileIssues => Get("Menu.LumiFix.PreflightFileIssues");
        public static string Menu_LumiFix_PreflightFailed => Get("Menu.LumiFix.PreflightFailed");
        public static string Menu_Experiences_Title => Get("Menu.Experiences.Title");
        public static string Menu_Experiences_Description => Get("Menu.Experiences.Description");
        public static string Menu_Experiences_AddTitle => Get("Menu.Experiences.AddTitle");
        public static string Menu_Experiences_AddDescription => Get("Menu.Experiences.AddDescription");
        public static string Menu_Experiences_UrlPlaceholder => Get("Menu.Experiences.UrlPlaceholder");
        public static string Menu_Experiences_NamePlaceholder => Get("Menu.Experiences.NamePlaceholder");
        public static string Menu_Experiences_Add => Get("Menu.Experiences.Add");
        public static string Menu_Experiences_Added => Get("Menu.Experiences.Added");
        public static string Menu_Experiences_InvalidUrl => Get("Menu.Experiences.InvalidUrl");
        public static string Menu_Experiences_SaveFailed => Get("Menu.Experiences.SaveFailed");
        public static string Menu_Experiences_OpenFailed => Get("Menu.Experiences.OpenFailed");
        public static string Menu_Experiences_RemovePrompt => Get("Menu.Experiences.RemovePrompt");
        public static string Menu_Experiences_Open => Get("Menu.Experiences.Open");
        public static string Menu_Experiences_ToggleFavorite => Get("Menu.Experiences.ToggleFavorite");
        public static string Menu_Experiences_Remove => Get("Menu.Experiences.Remove");
        public static string Menu_Experiences_ListTitle => Get("Menu.Experiences.ListTitle");
        public static string Menu_Experiences_Empty => Get("Menu.Experiences.Empty");
        public static string Menu_Experiences_DefaultName => Get("Menu.Experiences.DefaultName");
        public static string Menu_Experiences_LastPlayed => Get("Menu.Experiences.LastPlayed");
        public static string Menu_Experiences_MarkFavorite => Get("Menu.Experiences.MarkFavorite");
        public static string Menu_Experiences_UnmarkFavorite => Get("Menu.Experiences.UnmarkFavorite");
        public static string Menu_Studio_Title => Get("Menu.Studio.Title");
        public static string Menu_Studio_Description => Get("Menu.Studio.Description");
        public static string Menu_Studio_StatusTitle => Get("Menu.Studio.StatusTitle");
        public static string Menu_Studio_Installed => Get("Menu.Studio.Installed");
        public static string Menu_Studio_NotInstalled => Get("Menu.Studio.NotInstalled");
        public static string Menu_Studio_Launch => Get("Menu.Studio.Launch");
        public static string Menu_Studio_LaunchFailed => Get("Menu.Studio.LaunchFailed");
        public static string Menu_Studio_FolderMissing => Get("Menu.Studio.FolderMissing");
        public static string Menu_Studio_OpenFolder => Get("Menu.Studio.OpenFolder");
        public static string Menu_Studio_FlagEditor => Get("Menu.Studio.FlagEditor");
        public static string Menu_Studio_FlagNotice => Get("Menu.Studio.FlagNotice");
        public static string Menu_Studio_ToolsTitle => Get("Menu.Studio.ToolsTitle");
        public static string Menu_Studio_ToolsDescription => Get("Menu.Studio.ToolsDescription");
        public static string Menu_Search_Experiences => Get("Menu.Search.Experiences");
        public static string Menu_Search_Studio => Get("Menu.Search.Studio");
    }
}
