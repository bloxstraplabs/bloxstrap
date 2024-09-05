namespace Bloxstrap.AppData
{
    internal interface IAppData
    {
        string ProductName { get; }

        string BinaryType { get; }

        string RegistryName { get; }

        string ExecutableName { get; }

        string StartEvent { get; }

        string FinalDirectory { get; }

        string StagingDirectory { get; }

        string ExecutablePath { get; }

        AppState State { get; }

        IReadOnlyDictionary<string, string> PackageDirectoryMap { get; set; }
    }
}
