namespace Bloxstrap.Models.Manifest
{
    public class ManifestFile
    {
        public string Name { get; set; } = "";
        public string Signature { get; set; } = "";

        public override string ToString()
        {
            return $"[{Signature}] {Name}";
        }
    }
}
