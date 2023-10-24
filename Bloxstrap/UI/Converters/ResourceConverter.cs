using System.Windows.Data;

namespace Bloxstrap.UI.Converters
{
    internal class ResourceConverter : IValueConverter
    {
        // parameter is the resource namespace prefix
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is null)
                return "";

            if (parameter is null)
                return value.ToString()!;

            string valueStr = value.ToString()!;
            string parameterStr = parameter.ToString()!;
            string resourceName = parameterStr + valueStr;

            return Resources.Strings.ResourceManager.GetStringSafe(resourceName);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException(nameof(ConvertBack));
        }
    }
}
