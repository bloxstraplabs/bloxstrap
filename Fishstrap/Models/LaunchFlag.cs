using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bloxstrap.Models
{
    public class LaunchFlag
    {
        public string Identifiers { get; private set; }

        public bool Active = false;

        public string? Data;

        public LaunchFlag(string identifiers)
        {
            Identifiers = identifiers;
        }
    }
}
