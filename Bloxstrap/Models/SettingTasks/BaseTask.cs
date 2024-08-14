using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bloxstrap.Models.SettingTasks
{
    public class BaseTask : ISettingTask
    {
        private bool _originalState;
        
        private bool _newState;

        public string Name { get; set; } = "";

        public bool OriginalState 
        {
            get
            {
                return _originalState;
            }

            set 
            {
                _originalState = value;
                _newState = value;
            }
        }

        public bool NewState 
        {
            get
            {
                return _newState;
            }

            set
            {
                App.PendingSettingTasks[Name] = this;
                _newState = value;
            }
        }

        public virtual void Execute() => throw new NotImplementedException();
    }
}
