using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Bloxstrap.UI.Converters
{
    public class StringFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string? valueStr = value as string;
            string? parameterStr = parameter as string;

            if (valueStr is null)
                return "";

            if (parameterStr is null)
                return valueStr;

            string[] args = parameterStr.Split(new char[] { '|' });

            return string.Format(valueStr, args);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException(nameof(ConvertBack));
        }
    }
}
