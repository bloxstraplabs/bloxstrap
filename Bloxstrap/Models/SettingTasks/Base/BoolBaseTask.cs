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
                _newState = value;
                
                if (Changed)
                    App.PendingSettingTasks[Name] = this;
                else
                    App.PendingSettingTasks.Remove(Name);
            }
        }

        public override bool Changed => _newState != OriginalState;

        public BoolBaseTask(string prefix, string name) : base(prefix, name) { }
    }
}
