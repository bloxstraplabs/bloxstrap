namespace Bloxstrap.Enums
{
    public enum LaunchMode
    {
        None,
        /// <summary>
        /// Launch mode will be determined inside the bootstrapper. Only works if the VersionFlag is set.
        /// </summary>
        Unknown,
        Player,
        Studio,
        StudioAuth
    }
}
