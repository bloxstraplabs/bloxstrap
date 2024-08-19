namespace Bloxstrap.Models.SettingTasks.Base
{
    public abstract class EnumBaseTask<T> : BaseTask where T : struct, Enum
    {
        private T _originalState = default!;

        private T _newState = default!;

        public virtual T OriginalState
        {
            get => _originalState;

            set
            {
                _originalState = value;
                _newState = value;
            }
        }

        public virtual T NewState
        {
            get => _newState;

            set
            {
                App.PendingSettingTasks[Name] = this;
                _newState = value;
            }
        }

        public override bool Changed => !NewState.Equals(OriginalState);

        public IEnumerable<T> Selections { get; private set; } 
            = Enum.GetValues(typeof(T)).Cast<T>().OrderBy(x =>
                {
                    var attributes = x.GetType().GetMember(x.ToString())[0].GetCustomAttributes(typeof(EnumSortAttribute), false);

                    if (attributes.Length > 0)
                    {
                        var attribute = (EnumSortAttribute)attributes[0];
                        return attribute.Order;
                    }

                    return 0;
                });

        public EnumBaseTask(string prefix, string name) : base(prefix, name) { }
    }
}
