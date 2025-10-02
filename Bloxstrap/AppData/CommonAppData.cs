using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bloxstrap.AppData
{
    public abstract class CommonAppData
    {
        public virtual string ExecutableName { get; } = null!;

        public string Directory => Path.Combine(Paths.Versions, String.IsNullOrEmpty(State.VersionGuid) ? "" : State.VersionGuid);

        public string ExecutablePath => Path.Combine(Directory, ExecutableName);

        public virtual AppState State { get; } = null!;
    }
}
