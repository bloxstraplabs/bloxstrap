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

        AppState State { get; }
    }
}
