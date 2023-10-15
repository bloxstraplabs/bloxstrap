using System.Resources;

namespace Bloxstrap.Extensions
{
    static class ResourceManagerEx
    {
        /// <summary>
        /// Returns the value of the specified string resource. <br/>
        /// If the resource is not found, the resource name will be returned.
        /// </summary>
        public static string GetStringSafe(this ResourceManager manager, string name) => manager.GetStringSafe(name, null);

        /// <summary>
        /// Returns the value of the string resource localized for the specified culture. <br/>
        /// If the resource is not found, the resource name will be returned.
        /// </summary>
        public static string GetStringSafe(this ResourceManager manager, string name, CultureInfo? culture)
        {
            string? resourceValue = manager.GetString(name, culture);

            return resourceValue ?? name;
        }
    }
}
