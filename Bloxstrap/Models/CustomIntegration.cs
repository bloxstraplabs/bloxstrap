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
        public string Name { get; set; } = "";
        public string Location { get; set; } = "";
        public string LaunchArgs { get; set; } = "";
        public bool AutoClose { get; set; } = true;
    }
}
