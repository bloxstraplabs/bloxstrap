// https://github.com/MaximumADHD/Roblox-Studio-Mod-Manager/blob/main/ProjectSrc/Utility/Package.cs

namespace Bloxstrap.Helpers.RSMM
{
    internal class Package
    {
        public string Name { get; set; } = "";
        public string Signature { get; set; } = "";
        public int PackedSize { get; set; }
        public int Size { get; set; }

        public override string ToString()
        {
            return $"[{Signature}] {Name}";
        }
    }
}
