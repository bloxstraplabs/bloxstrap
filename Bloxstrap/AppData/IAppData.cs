namespace Bloxstrap.AppData
{
    internal interface IAppData
    {
        string ProductName { get; }

        string BinaryType { get; }

        string RegistryName { get; }

        string ExecutableName { get; }

        string Directory { get; }

        string ExecutablePath { get; }

        string RobloxInstallerExecutableName { get; }

        AppState State { get; }

        IReadOnlyDictionary<string, string> PackageDirectoryMap { get; set; }
    }
}
