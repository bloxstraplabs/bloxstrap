using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bloxstrap.Models
{
    public class CustomIntegration
    {
        public string Name { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string LaunchArgs { get; set; } = null!;
        public bool AutoClose { get; set; } = false;

        public override string ToString()
        {
            return Name;
        }
    }
}
