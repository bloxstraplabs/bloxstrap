using System.ComponentModel;
using System.Reflection;

namespace Bloxstrap.Extensions
{
    internal static class TEnumEx
    {
        public static string? GetDescription<TEnum>(this TEnum e)
        {
            DescriptionAttribute? attribute = e?.GetType().GetCustomAttribute<DescriptionAttribute>();
            return attribute?.Description;
        }
    }
}
