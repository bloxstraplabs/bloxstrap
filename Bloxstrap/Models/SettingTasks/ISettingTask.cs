using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bloxstrap.Models.SettingTasks
{
    public interface ISettingTask
    {
        public bool OriginalState { get; set; }

        public bool NewState { get; set; }

        public void Execute();
    }
}
