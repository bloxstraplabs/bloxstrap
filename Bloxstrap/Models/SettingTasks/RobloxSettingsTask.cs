using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

using Bloxstrap.Models.SettingTasks.Base;

namespace Bloxstrap.Models.SettingTasks
{
    public class RobloxSettingsTask : BaseTask
    {
        public static string FileLocation => Path.Combine(Paths.LocalAppData, "Roblox", "GlobalBasicSettings_13.xml");

        private readonly XDocument? _document;

        private bool _changed;

        private bool _lockFile;

        private bool _originalLock;

        public bool Exists => _document is not null;

        public override bool Changed => _changed || _lockFile != _originalLock;

        private XElement? Properties => _document?.XPathSelectElement("//Item[@class='UserGameSettings']/Properties");

        public RobloxSettingsTask() : base("RobloxSettings")
        {
            const string LOG_IDENT = "RobloxSettingsTask";

            if (!File.Exists(FileLocation))
                return;

            try
            {
                _document = XDocument.Load(FileLocation, LoadOptions.PreserveWhitespace);
                _originalLock = _lockFile = File.GetAttributes(FileLocation).HasFlag(FileAttributes.ReadOnly);
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, "Failed to load Roblox settings");
                App.Logger.WriteException(LOG_IDENT, ex);
                _document = null;
            }
        }

        public bool LockFile
        {
            get => _lockFile;
            set
            {
                _lockFile = value;
                UpdatePending();
            }
        }

        private void UpdatePending()
        {
            if (Changed)
                App.PendingSettingTasks[Name] = this;
            else
                App.PendingSettingTasks.Remove(Name);
        }

        private XElement? Find(string name) =>
            Properties?.Elements().FirstOrDefault(x => x.Attribute("name")?.Value == name);

        public string? Get(string name) => Find(name)?.Value;

        public void Set(string type, string name, string value)
        {
            if (Properties is null || Get(name) == value)
                return;

            var element = Find(name);

            if (element is null)
                Properties.Add(new XElement(type, new XAttribute("name", name), value));
            else
                element.Value = value;

            _changed = true;
            UpdatePending();
        }

        public override void Execute()
        {
            const string LOG_IDENT = "RobloxSettingsTask::Execute";

            if (_document is null)
                return;

            try
            {
                File.SetAttributes(FileLocation, File.GetAttributes(FileLocation) & ~FileAttributes.ReadOnly);

                if (_changed)
                {
                    var settings = new XmlWriterSettings { OmitXmlDeclaration = true, Encoding = new UTF8Encoding(false) };

                    using (var writer = XmlWriter.Create(FileLocation, settings))
                        _document.Save(writer);
                }

                if (_lockFile)
                    File.SetAttributes(FileLocation, File.GetAttributes(FileLocation) | FileAttributes.ReadOnly);
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, "Failed to save Roblox settings");
                App.Logger.WriteException(LOG_IDENT, ex);
                Frontend.ShowMessageBox(ex.Message, System.Windows.MessageBoxImage.Error);
            }

            _changed = false;
            _originalLock = _lockFile;
        }
    }
}
