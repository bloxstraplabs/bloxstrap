using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bloxstrap.Models.SettingTasks.Base
{
    public abstract class StringBaseTask : BaseTask
    {
        private string _originalState = "";

        private string _newState = "";

        public virtual string OriginalState
        {
            get => _originalState;

            set
            {
                _originalState = value;
                _newState = value;
            }
        }

        public virtual string NewState
        {
            get => _newState;

            set
            {
                App.PendingSettingTasks[Name] = this;
                _newState = value;
            }
        }

        public override bool Changed => NewState != OriginalState;

        public StringBaseTask(string prefix, string name) : base(prefix, name) { }
    }
}
