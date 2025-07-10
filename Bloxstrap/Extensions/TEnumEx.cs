using System.ComponentModel;
using System.Reflection;

namespace Bloxstrap.Extensions
{
    internal static class TEnumEx
    {
        public static string? GetDescription<TEnum>(this TEnum e)
        {
            string? enumName = e?.ToString();
            if (enumName == null)
                return null;

            FieldInfo? field = e?.GetType().GetField(enumName);
            if (field == null)
                return null;

            DescriptionAttribute? attribute = field.GetCustomAttribute<DescriptionAttribute>();
            return attribute?.Description;
        }
    }
}
