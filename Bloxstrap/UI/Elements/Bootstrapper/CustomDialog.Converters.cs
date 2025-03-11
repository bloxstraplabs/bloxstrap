using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;

namespace Bloxstrap.UI.Elements.Bootstrapper
{
    public partial class CustomDialog
    {
        // https://stackoverflow.com/a/2961702
        private static T? ConvertValue<T>(string input) where T : struct
        {
            try
            {
                var converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter != null)
                {
                    return (T?)converter.ConvertFromInvariantString(input);
                }
                return default;
            }
            catch (NotSupportedException)
            {
                return default;
            }
        }

        private static object? GetTypeFromXElement(TypeConverter converter, XElement xmlElement, string attributeName)
        {
            string? attributeValue = xmlElement.Attribute(attributeName)?.Value?.ToString();
            if (attributeValue == null)
                return null;

            try
            {
                return converter.ConvertFromInvariantString(attributeValue);
            }
            catch (Exception ex)
            {
                throw new CustomThemeException(ex, "CustomTheme.ElementAttributeConversionError", xmlElement.Name, attributeName, ex.Message);
            }
        }

        private static ThicknessConverter ThicknessConverter { get; } = new ThicknessConverter();
        private static object? GetThicknessFromXElement(XElement xmlElement, string attributeName) => GetTypeFromXElement(ThicknessConverter, xmlElement, attributeName);

        private static RectConverter RectConverter { get; } = new RectConverter();
        private static object? GetRectFromXElement(XElement xmlElement, string attributeName) => GetTypeFromXElement(RectConverter, xmlElement, attributeName);

        private static ColorConverter ColorConverter { get; } = new ColorConverter();
        private static object? GetColorFromXElement(XElement xmlElement, string attributeName) => GetTypeFromXElement(ColorConverter, xmlElement, attributeName);

        private static PointConverter PointConverter { get; } = new PointConverter();
        private static object? GetPointFromXElement(XElement xmlElement, string attributeName) => GetTypeFromXElement(PointConverter, xmlElement, attributeName);

        private static CornerRadiusConverter CornerRadiusConverter { get; } = new CornerRadiusConverter();
        private static object? GetCornerRadiusFromXElement(XElement xmlElement, string attributeName) => GetTypeFromXElement(CornerRadiusConverter, xmlElement, attributeName);

        private static GridLengthConverter GridLengthConverter { get; } = new GridLengthConverter();
        private static object? GetGridLengthFromXElement(XElement xmlElement, string attributeName) => GetTypeFromXElement(GridLengthConverter, xmlElement, attributeName);


        private static BrushConverter BrushConverter { get; } = new BrushConverter();

        /// <summary>
        /// Return type of string = Name of DynamicResource
        /// Return type of brush = ... The Brush!!!
        /// </summary>
        private static object? GetBrushFromXElement(XElement element, string attributeName)
        {
            string? value = element.Attribute(attributeName)?.Value?.ToString();
            if (value == null)
                return null;

            // dynamic resource name
            if (value.StartsWith('{') && value.EndsWith('}'))
                return value[1..^1];

            try
            {
                return BrushConverter.ConvertFromInvariantString(value);
            }
            catch (Exception ex)
            {
                throw new CustomThemeException(ex, "CustomTheme.ElementAttributeConversionError", element.Name, attributeName, ex.Message);
            }
        }
    }
}
