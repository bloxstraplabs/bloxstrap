using Bloxstrap.Models.Entities;
using Bloxstrap.Models.SettingTasks.Base;

namespace Bloxstrap.Models.SettingTasks
{
    public class ModPresetTask : BoolBaseTask
    {
        private Dictionary<string, ModPresetFileData> _fileDataMap = new();
        
        private Dictionary<string, string> _pathMap;

        public ModPresetTask(string name, string path, string resource) : this(name, new() {{ path, resource }}) { }

        public ModPresetTask(string name, Dictionary<string, string> pathMap) : base("ModPreset", name)
        {
            _pathMap = pathMap;

            foreach (var pair in _pathMap)
            {
                var data = new ModPresetFileData(pair.Key, pair.Value);

                if (data.HashMatches() && !OriginalState)
                    OriginalState = true;

                _fileDataMap[pair.Key] = data;
            }
        }

        public override void Execute()
        {
            if (NewState == OriginalState)
                return;

            foreach (var pair in _fileDataMap)
            {
                var data = pair.Value;
                bool hashMatches = data.HashMatches();

                if (NewState && !hashMatches)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(data.FullFilePath)!);

                    using var resourceStream = data.ResourceStream;
                    using var memoryStream = new MemoryStream();
                    resourceStream.CopyTo(memoryStream);

                    Filesystem.AssertReadOnly(data.FullFilePath);
                    File.WriteAllBytes(data.FullFilePath, memoryStream.ToArray());
                }
                else if (!NewState && hashMatches)
                {
                    Filesystem.AssertReadOnly(data.FullFilePath);
                    File.Delete(data.FullFilePath);
                }
            }

            OriginalState = NewState;
        }
    }
}
