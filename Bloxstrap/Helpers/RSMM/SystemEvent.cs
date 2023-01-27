// https://github.com/MaximumADHD/Roblox-Studio-Mod-Manager/blob/main/ProjectSrc/Utility/SystemEvent.cs

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bloxstrap.Helpers.RSMM
{
    public class SystemEvent : EventWaitHandle
    {
        public string Name { get; private set; }

        public SystemEvent(string name, bool init = false, EventResetMode mode = EventResetMode.AutoReset) : base(init, mode, name)
        {
            if (init)
                Reset();
            else
                Set();

            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }

        public Task<bool> WaitForEvent()
        {
            return Task.Run(WaitOne);
        }

        public Task<bool> WaitForEvent(TimeSpan timeout, bool exitContext = false)
        {
            return Task.Run(() => WaitOne(timeout, exitContext));
        }

        public Task<bool> WaitForEvent(int millisecondsTimeout, bool exitContext = false)
        {
            return Task.Run(() => WaitOne(millisecondsTimeout, exitContext));
        }
    }
}
