using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bloxstrap.Models.Attributes
{
    public class LaunchFlagAttribute : Attribute
    {
        public string? Name { get; private set; }
        public string[]? Names { get; private set; }

        public LaunchFlagAttribute(string name)
        {
            Name = name;
        }

        public LaunchFlagAttribute(string[] names)
        {
            Names = names;
        }
    }
}
