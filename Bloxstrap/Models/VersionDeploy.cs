using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bloxstrap.Models
{
    public class VersionDeploy
    {
        public string VersionGuid { get; set; } = null!;
        public string FileVersion { get; set; } = null!;
        public DateTime Timestamp { get; set; }
    }
}
