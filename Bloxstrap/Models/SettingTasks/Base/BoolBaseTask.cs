using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bloxstrap.Models.SettingTasks.Base
{
    public abstract class BoolBaseTask : BaseTask
    {
        private bool _originalState;

        private bool _newState;

        public virtual bool OriginalState
        {
            get => _originalState;

            set
            {
                _originalState = value;
                _newState = value;
            }
        }

        public virtual bool NewState
        {
            get => _newState;

            set
            {
                App.PendingSettingTasks[Name] = this;
                _newState = value;
            }
        }

        public override bool Changed => NewState != OriginalState;

        public BoolBaseTask(string prefix, string name) : base(prefix, name) { }
    }
}
