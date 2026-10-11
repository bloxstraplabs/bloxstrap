using System.Xml.Linq;

namespace Bloxstrap
{
    public static class StudioPlugin
    {
        public const int Port = 47621;

        private static string ScriptName => $"{App.ProjectName}StudioPresence";

        public static string FileLocation => Path.Combine(Paths.LocalAppData, "Roblox", "Plugins", $"{ScriptName}.rbxmx");

        public static bool Wanted => App.Settings.Prop.StudioRichPresence && App.Settings.Prop.StudioShowScript;

        private static string Build()
        {
            string source;

            using (var reader = new StreamReader(Resource.GetStream("StudioPresencePlugin.lua")))
                source = reader.ReadToEnd().Replace("{PORT}", Port.ToString());

            XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";

            var document = new XDocument(
                new XElement("roblox",
                    new XAttribute(XNamespace.Xmlns + "xmime", "http://www.w3.org/2005/05/xmlmime"),
                    new XAttribute(XNamespace.Xmlns + "xsi", xsi),
                    new XAttribute(xsi + "noNamespaceSchemaLocation", "http://www.roblox.com/roblox.xsd"),
                    new XAttribute("version", "4"),
                    new XElement("Item",
                        new XAttribute("class", "Script"),
                        new XAttribute("referent", "RBX0"),
                        new XElement("Properties",
                            new XElement("string", new XAttribute("name", "Name"), ScriptName),
                            new XElement("ProtectedString", new XAttribute("name", "Source"), source)))));

            return document.ToString();
        }

        public static void Sync()
        {
            const string LOG_IDENT = "StudioPlugin::Sync";

            try
            {
                if (!Wanted)
                {
                    Remove();
                    return;
                }

                string contents = Build();

                if (File.Exists(FileLocation) && File.ReadAllText(FileLocation) == contents)
                    return;

                Directory.CreateDirectory(Path.GetDirectoryName(FileLocation)!);
                File.WriteAllText(FileLocation, contents);

                App.Logger.WriteLine(LOG_IDENT, $"Installed Studio plugin to {FileLocation}");
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
            }
        }

        public static void Remove()
        {
            if (!File.Exists(FileLocation))
                return;

            File.Delete(FileLocation);
            App.Logger.WriteLine("StudioPlugin::Remove", "Removed Studio plugin");
        }
    }
}
