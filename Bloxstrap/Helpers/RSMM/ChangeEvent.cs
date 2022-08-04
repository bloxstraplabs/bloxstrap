// https://github.com/MaximumADHD/Roblox-Studio-Mod-Manager/blob/main/ProjectSrc/Events/ChangeEvent.cs

namespace Bloxstrap.Helpers.RSMM
{
    public delegate void ChangeEventHandler<T>(object sender, ChangeEventArgs<T> e);

    public class ChangeEventArgs<T>
    {
        public T Value { get; }

        public ChangeEventArgs(T value)
        {
            Value = value;
        }
    }
}
