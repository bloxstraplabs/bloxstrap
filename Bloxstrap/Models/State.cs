using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bloxstrap.Models
{
    public class State
    {
        public string VersionGuid { get; set; } = "";
        public string ReShadeConfigVersion { get; set; } = "";
        public string ExtraviReShadePresetsVersion { get; set; } = "";
        public List<string> ModManifest { get; set; } = new();
    }
}
