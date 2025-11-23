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

        JsonManager<DistributionState> DistributionStateManager { get; }

        DistributionState DistributionState { get; }

        List<string> ModManifest { get; }

        IReadOnlyDictionary<string, string> PackageDirectoryMap { get; set; }
    }
}
