using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Xml.Linq;

namespace Bloxstrap.UI.Converters
{
    class EnumNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // https://stackoverflow.com/a/28672015/11852173
    
            var enumVal = (Enum)value;
            var stringVal = enumVal.ToString();

            var type = enumVal.GetType();
            var typeName = type.FullName!;
            var attributes = type.GetMember(stringVal)[0].GetCustomAttributes(typeof(EnumNameAttribute), false);

            if (attributes.Length > 0)
            {
                var attribute = (EnumNameAttribute)attributes[0];

                if (attribute is not null)
                {
                    if (attribute.StaticName is not null)
                        return attribute.StaticName;

                    if (attribute.FromTranslation is not null)
                        return Strings.ResourceManager.GetStringSafe(attribute.FromTranslation);
                }
            }

            return Strings.ResourceManager.GetStringSafe(String.Format(
                "{0}.{1}",
                typeName.Substring(typeName.IndexOf('.', StringComparison.Ordinal) + 1),
                stringVal
            ));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
