using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bloxstrap.Models
{
    public class FastFlag
    {
        public bool Enabled { get; set; }
        public string Name { get; set; } = null!;
        public string Value { get; set; } = null!;
    }
}
