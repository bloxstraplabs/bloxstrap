using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bloxstrap.Models
{
    public class DeployInfo
    {
        public string Timestamp { get; set; } = null!;
        public string Version { get; set; } = null!;
        public string VersionGuid { get; set; } = null!;
    }
}
