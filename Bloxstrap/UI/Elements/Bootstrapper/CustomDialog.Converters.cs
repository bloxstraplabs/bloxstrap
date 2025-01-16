using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
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
                throw new Exception($"{xmlElement.Name} has invalid {attributeName}: {ex.Message}", ex);
            }
        }

        private static ThicknessConverter? _thicknessConverter = null;
        private static ThicknessConverter ThicknessConverter { get => _thicknessConverter ??= new ThicknessConverter(); }

        private static object? GetThicknessFromXElement(XElement xmlElement, string attributeName) => GetTypeFromXElement(ThicknessConverter, xmlElement, attributeName);

        private static GeometryConverter? _geometryConverter = null;
        private static GeometryConverter GeometryConverter { get => _geometryConverter ??= new GeometryConverter(); }

        private static object? GetGeometryFromXElement(XElement xmlElement, string attributeName) => GetTypeFromXElement(GeometryConverter, xmlElement, attributeName);

        private static RectConverter? _rectConverter = null;
        public static RectConverter RectConverter { get => _rectConverter ??= new RectConverter(); }

        private static object? GetRectFromXElement(XElement xmlElement, string attributeName) => GetTypeFromXElement(RectConverter, xmlElement, attributeName);

        private static ColorConverter? _colorConverter = null;
        public static ColorConverter ColorConverter { get => _colorConverter ??= new ColorConverter(); }

        private static object? GetColorFromXElement(XElement xmlElement, string attributeName) => GetTypeFromXElement(ColorConverter, xmlElement, attributeName);

        private static PointConverter? _pointConverter = null;
        public static PointConverter PointConverter { get => _pointConverter ??= new PointConverter(); }

        private static object? GetPointFromXElement(XElement xmlElement, string attributeName) => GetTypeFromXElement(PointConverter, xmlElement, attributeName);

        private static CornerRadiusConverter? _cornerRadiusConverter = null;
        public static CornerRadiusConverter CornerRadiusConverter { get => _cornerRadiusConverter ??= new CornerRadiusConverter(); }

        private static object? GetCornerRadiusFromXElement(XElement xmlElement, string attributeName) => GetTypeFromXElement(CornerRadiusConverter, xmlElement, attributeName);


        private static BrushConverter? _brushConverter = null;
        private static BrushConverter BrushConverter { get => _brushConverter ??= new BrushConverter(); }

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
                throw new Exception($"{element.Name} has invalid {attributeName}: {ex.Message}", ex);
            }
        }
    }
}
