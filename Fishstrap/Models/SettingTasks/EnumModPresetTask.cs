using Bloxstrap.Models.Entities;
using Bloxstrap.Models.SettingTasks.Base;

namespace Bloxstrap.Models.SettingTasks
{
    public class EnumModPresetTask<T> : EnumBaseTask<T> where T : struct, Enum
    {
        private readonly Dictionary<T, Dictionary<string, ModPresetFileData>> _fileDataMap = new();

        private readonly Dictionary<T, Dictionary<string, string>> _map;

        public EnumModPresetTask(string name, Dictionary<T, Dictionary<string, string>> map) : base("ModPreset", name)
        {
            _map = map;

            foreach (var enumPair in _map)
            {
                var dataMap = new Dictionary<string, ModPresetFileData>();

                foreach (var resourcePair in enumPair.Value)
                {
                    var data = new ModPresetFileData(resourcePair.Key, resourcePair.Value);

                    if (data.HashMatches() && OriginalState.Equals(default(T)))
                        OriginalState = enumPair.Key;

                    dataMap[resourcePair.Key] = data;
                }

                _fileDataMap[enumPair.Key] = dataMap;
            }
        }

        public override void Execute()
        {
            if (!NewState.Equals(default(T)))
            {
                var resourceMap = _fileDataMap[NewState];

                foreach (var resourcePair in resourceMap)
                {
                    var data = resourcePair.Value;

                    if (!data.HashMatches())
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(data.FullFilePath)!);

                        using var resourceStream = data.ResourceStream;
                        using var memoryStream = new MemoryStream();
                        data.ResourceStream.CopyTo(memoryStream);

                        Filesystem.AssertReadOnly(data.FullFilePath);
                        File.WriteAllBytes(data.FullFilePath, memoryStream.ToArray());
                    }
                }
            }
            else
            {
                foreach (var dataPair in _fileDataMap.First().Value)
                {
                    Filesystem.AssertReadOnly(dataPair.Value.FullFilePath);
                    File.Delete(dataPair.Value.FullFilePath);
                }
            }

            OriginalState = NewState;
        }
    }
}
