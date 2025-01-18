using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Xml.Linq;

using Wpf.Ui.Markup;
using Wpf.Ui.Appearance;

using Bloxstrap.UI.Elements.Controls;
using System.Windows.Media.Animation;

namespace Bloxstrap.UI.Elements.Bootstrapper
{
    public partial class CustomDialog
    {
        private class DummyFrameworkElement : FrameworkElement { }

        private const int MaxElements = 100;

        private bool _initialised = false;

        // prevent users from creating elements with the same name multiple times
        private List<string> UsedNames { get; } = new List<string>();

        private string ThemeDir { get; set; } = "";

        delegate object HandleXmlElementDelegate(CustomDialog dialog, XElement xmlElement);

        private static Dictionary<string, HandleXmlElementDelegate> _elementHandlerMap = new Dictionary<string, HandleXmlElementDelegate>()
        {
            ["BloxstrapCustomBootstrapper"] = HandleXmlElement_BloxstrapCustomBootstrapper_Fake,
            ["TitleBar"] = HandleXmlElement_TitleBar,
            ["Button"] = HandleXmlElement_Button,
            ["ProgressBar"] = HandleXmlElement_ProgressBar,
            ["ProgressRing"] = HandleXmlElement_ProgressRing,
            ["TextBlock"] = HandleXmlElement_TextBlock,
            ["MarkdownTextBlock"] = HandleXmlElement_MarkdownTextBlock,
            ["Image"] = HandleXmlElement_Image,
            ["MediaElement"] = HandleXmlElement_MediaElement,

            ["SolidColorBrush"] = HandleXmlElement_SolidColorBrush,
            ["ImageBrush"] = HandleXmlElement_ImageBrush,
            ["LinearGradientBrush"] = HandleXmlElement_LinearGradientBrush,
            ["RadialGradientBrush"] = HandleXmlElement_RadialGradientBrush,

            ["GradientStop"] = HandleXmlElement_GradientStop,

            //["PathFigure"] = HandleXmlElement_PathGeometry,

            ["ScaleTransform"] = HandleXmlElement_ScaleTransform,
            ["SkewTransform"] = HandleXmlElement_SkewTransform,
            ["RotateTransform"] = HandleXmlElement_RotateTransform,
            ["TranslateTransform"] = HandleXmlElement_TranslateTransform,

            ["BlurEffect"] = HandleXmlElement_BlurEffect,
            ["DropShadowEffect"] = HandleXmlElement_DropShadowEffect,

            ["Path"] = HandleXmlElement_Path,
            ["Ellipse"] = HandleXmlElement_Ellipse,
            ["Line"] = HandleXmlElement_Line,
            ["Rectangle"] = HandleXmlElement_Rectangle
        };


        #region Utilities
        private static string GetXmlAttribute(XElement element, string attributeName, string? defaultValue = null)
        {
            var attribute = element.Attribute(attributeName);

            if (attribute == null)
            {
                if (defaultValue != null)
                    return defaultValue;

                throw new Exception($"Element {element.Name} is missing the {attributeName} attribute");
            }

            return attribute.Value.ToString();
        }

        private static T ParseXmlAttribute<T>(XElement element, string attributeName, T? defaultValue = null) where T : struct
        {
            var attribute = element.Attribute(attributeName);

            if (attribute == null)
            {
                if (defaultValue != null)
                    return (T)defaultValue;

                throw new Exception($"Element {element.Name} is missing the {attributeName} attribute");
            }

            T? parsed = ConvertValue<T>(attribute.Value);
            if (parsed == null)
                throw new Exception($"{element.Name} {attributeName} is not a valid {typeof(T).Name}");

            return (T)parsed;
        }

        /// <summary>
        /// ParseXmlAttribute but the default value is always null
        /// </summary>
        private static T? ParseXmlAttributeNullable<T>(XElement element, string attributeName) where T : struct
        {
            var attribute = element.Attribute(attributeName);

            if (attribute == null)
                return null;

            T? parsed = ConvertValue<T>(attribute.Value);
            if (parsed == null)
                throw new Exception($"{element.Name} {attributeName} is not a valid {typeof(T).Name}");

            return (T)parsed;
        }

        private static void ValidateXmlElement(string elementName, string attributeName, int value, int? min = null, int? max = null)
        {
            if (min != null && value < min)
                throw new Exception($"{elementName} {attributeName} must be larger than {min}");
            if (max != null && value > max)
                throw new Exception($"{elementName} {attributeName} must be smaller than {max}");
        }

        private static void ValidateXmlElement(string elementName, string attributeName, double value, double? min = null, double? max = null)
        {
            if (min != null && value < min)
                throw new Exception($"{elementName} {attributeName} must be larger than {min}");
            if (max != null && value > max)
                throw new Exception($"{elementName} {attributeName} must be smaller than {max}");
        }

        // You can't do numeric only generics in .NET 6. The feature is exclusive to .NET 7+.
        private static int ParseXmlAttributeClamped(XElement element, string attributeName, int? defaultValue = null, int? min = null, int? max = null)
        {
            int value = ParseXmlAttribute<int>(element, attributeName, defaultValue);
            ValidateXmlElement(element.Name.ToString(), attributeName, value, min, max);
            return value;
        }

        private static FontWeight GetFontWeightFromXElement(XElement element)
        {
            string? value = element.Attribute("FontWeight")?.Value?.ToString();
            if (string.IsNullOrEmpty(value))
                value = "Normal";

            // bruh
            // https://learn.microsoft.com/en-us/dotnet/api/system.windows.fontweights?view=windowsdesktop-6.0
            switch (value)
            {
                case "Thin":
                    return FontWeights.Thin;

                case "ExtraLight":
                case "UltraLight":
                    return FontWeights.ExtraLight;

                case "Medium":
                    return FontWeights.Medium;

                case "Normal":
                case "Regular":
                    return FontWeights.Normal;

                case "DemiBold":
                case "SemiBold":
                    return FontWeights.DemiBold;

                case "Bold":
                    return FontWeights.Bold;

                case "ExtraBold":
                case "UltraBold":
                    return FontWeights.ExtraBold;

                case "Black":
                case "Heavy":
                    return FontWeights.Black;

                case "ExtraBlack":
                case "UltraBlack":
                    return FontWeights.UltraBlack;

                default:
                    throw new Exception($"{element.Name} Unknown FontWeight {value}");
            }
        }

        private static FontStyle GetFontStyleFromXElement(XElement element)
        {
            string? value = element.Attribute("FontStyle")?.Value?.ToString();
            if (string.IsNullOrEmpty(value))
                value = "Normal";

            switch (value)
            {
                case "Normal":
                    return FontStyles.Normal;

                case "Italic":
                    return FontStyles.Italic;

                case "Oblique":
                    return FontStyles.Oblique;

                default:
                    throw new Exception($"{element.Name} Unknown FontStyle {value}");
            }
        }

        private static TextDecorationCollection? GetTextDecorationsFromXElement(XElement element)
        {
            string? value = element.Attribute("TextDecorations")?.Value?.ToString();
            if (string.IsNullOrEmpty(value))
                return null;

            switch (value)
            {
                case "Baseline":
                    return TextDecorations.Baseline;

                case "OverLine":
                    return TextDecorations.OverLine;

                case "Strikethrough":
                    return TextDecorations.Strikethrough;

                case "Underline":
                    return TextDecorations.Underline;

                default:
                    throw new Exception($"{element.Name} Unknown TextDecorations {value}");
            }
        }

        private static string? GetTranslatedText(string? text)
        {
            if (text == null || !text.StartsWith('{') || !text.EndsWith('}'))
                return text; // can't be translated (not in the correct format)

            string resourceName = text[1..^1];
            return Strings.ResourceManager.GetStringSafe(resourceName);
        }

        private static string? GetFullPath(CustomDialog dialog, string? sourcePath)
        {
            if (sourcePath == null)
                return null;

            return sourcePath.Replace("theme://", $"{dialog.ThemeDir}\\");
        }

        struct GetImageSourceDataResult
        {
            public bool IsIcon = false;
            public Uri? Uri = null;

            public GetImageSourceDataResult()
            {
            }
        }

        private static GetImageSourceDataResult GetImageSourceData(CustomDialog dialog, string name, XElement xmlElement)
        {
            string path = GetXmlAttribute(xmlElement, name);

            if (path == "{Icon}")
                return new GetImageSourceDataResult { IsIcon = true };

            path = GetFullPath(dialog, path)!;

            if (!Uri.TryCreate(path, UriKind.RelativeOrAbsolute, out Uri? result))
                throw new Exception($"{xmlElement.Name} failed to parse {name} as Uri");

            if (result == null)
                throw new Exception($"{xmlElement.Name} {name} Uri is null");

            return new GetImageSourceDataResult { Uri = result };
        }

        private static Uri GetMediaSourceData(CustomDialog dialog, string name, XElement xmlElement)
        {
            string path = GetXmlAttribute(xmlElement, name);

            path = GetFullPath(dialog, path)!;

            if (!Uri.TryCreate(path, UriKind.RelativeOrAbsolute, out Uri? result))
                throw new Exception($"{xmlElement.Name} failed to parse Source as Uri");

            if (result == null)
                throw new Exception($"{xmlElement.Name} Source Uri is null");

            Uri? uri = result; //why does this work?

            return uri;
        }

        private static RepeatBehavior GetImageRepeatBehaviourData(XElement element)
        {
            string? value = element.Attribute("RepeatBehaviour")?.Value?.ToString();
            RepeatBehavior Behaviour = RepeatBehavior.Forever;

            // Repeat forever behaviour (default)
            if (string.IsNullOrEmpty(value) || value == "Forever")
                return Behaviour;

            // Patterns
            const string RepeatCountPattern = "([0-9]+)x";
            const string PlayTimePattern = "[0-9][0-9]:[0-9][0-9]:[0-9][0-9]";

            // RegExes
            Match RepeatCountRegEx = new Regex(RepeatCountPattern).Match(value);
            Match PlayTimeRegEx = new Regex(PlayTimePattern).Match(value);

            // Repeat count ex. 3x (repeats 3 times)
            if (RepeatCountRegEx.Success)
            {
                int? RepeatCount = int.TryParse(RepeatCountRegEx.Groups[1].Value, out int x) ? x : 0;
                Behaviour = new RepeatBehavior(x);
            }

            // Play time ex. 00:00:10 (plays for 10 seconds)
            if (PlayTimeRegEx.Success)
            {
                TimeSpan PlayTime = TimeSpan.Parse(value);
                Behaviour = new RepeatBehavior(PlayTime);
            }

            return Behaviour;
        }
        #endregion

        #region Transformation Elements
        private static Transform HandleXmlElement_ScaleTransform(CustomDialog dialog, XElement xmlElement)
        {
            var st = new ScaleTransform();

            st.ScaleX = ParseXmlAttribute<double>(xmlElement, "ScaleX", 1);
            st.ScaleY = ParseXmlAttribute<double>(xmlElement, "ScaleY", 1);
            st.CenterX = ParseXmlAttribute<double>(xmlElement, "CenterX", 0);
            st.CenterY = ParseXmlAttribute<double>(xmlElement, "CenterY", 0);

            return st;
        }

        private static Transform HandleXmlElement_SkewTransform(CustomDialog dialog, XElement xmlElement)
        {
            var st = new SkewTransform();

            st.AngleX = ParseXmlAttribute<double>(xmlElement, "AngleX", 0);
            st.AngleY = ParseXmlAttribute<double>(xmlElement, "AngleY", 0);
            st.CenterX = ParseXmlAttribute<double>(xmlElement, "CenterX", 0);
            st.CenterY = ParseXmlAttribute<double>(xmlElement, "CenterY", 0);

            return st;
        }

        private static Transform HandleXmlElement_RotateTransform(CustomDialog dialog, XElement xmlElement)
        {
            var rt = new RotateTransform();

            rt.Angle = ParseXmlAttribute<double>(xmlElement, "Angle", 0);
            rt.CenterX = ParseXmlAttribute<double>(xmlElement, "CenterX", 0);
            rt.CenterY = ParseXmlAttribute<double>(xmlElement, "CenterY", 0);

            return rt;
        }

        private static Transform HandleXmlElement_TranslateTransform(CustomDialog dialog, XElement xmlElement)
        {
            var tt = new TranslateTransform();

            tt.X = ParseXmlAttribute<double>(xmlElement, "X", 0);
            tt.Y = ParseXmlAttribute<double>(xmlElement, "Y", 0);

            return tt;
        }

        private static void ApplyTransformation_UIElement(CustomDialog dialog, string name, DependencyProperty property, UIElement uiElement, XElement xmlElement)
        {
            var transformElement = xmlElement.Element($"{xmlElement.Name}.{name}");

            if (transformElement == null)
                return;

            var tg = new TransformGroup();

            foreach (var child in transformElement.Elements())
            {
                Transform element = HandleXml<Transform>(dialog, child);
                tg.Children.Add(element);
            }

            uiElement.SetValue(property, tg);
        }

        private static void ApplyTransformations_UIElement(CustomDialog dialog, UIElement uiElement, XElement xmlElement)
        {
            ApplyTransformation_UIElement(dialog, "RenderTransform", FrameworkElement.RenderTransformProperty, uiElement, xmlElement);
            ApplyTransformation_UIElement(dialog, "LayoutTransform", FrameworkElement.LayoutTransformProperty, uiElement, xmlElement);
        }
        #endregion

        #region Effects
        private static BlurEffect HandleXmlElement_BlurEffect(CustomDialog dialog, XElement xmlElement)
        {
            var effect = new BlurEffect();

            effect.KernelType = ParseXmlAttribute<KernelType>(xmlElement, "KernelType", KernelType.Gaussian);
            effect.Radius = ParseXmlAttribute<double>(xmlElement, "Radius", 5);
            effect.RenderingBias = ParseXmlAttribute<RenderingBias>(xmlElement, "RenderingBias", RenderingBias.Performance);

            return effect;
        }

        private static DropShadowEffect HandleXmlElement_DropShadowEffect(CustomDialog dialog, XElement xmlElement)
        {
            var effect = new DropShadowEffect();

            effect.BlurRadius = ParseXmlAttribute<double>(xmlElement, "BlurRadius", 5);
            effect.Direction = ParseXmlAttribute<double>(xmlElement, "Direction", 315);
            effect.Opacity = ParseXmlAttribute<double>(xmlElement, "Opacity", 1);
            effect.ShadowDepth = ParseXmlAttribute<double>(xmlElement, "ShadowDepth", 5);
            effect.RenderingBias = ParseXmlAttribute<RenderingBias>(xmlElement, "RenderingBias", RenderingBias.Performance);

            var color = GetColorFromXElement(xmlElement, "Color");
            if (color is Color)
                effect.Color = (Color)color;

            return effect;
        }


        private static void ApplyEffects_UIElement(CustomDialog dialog, UIElement uiElement, XElement xmlElement)
        {
            var effectElement = xmlElement.Element($"{xmlElement.Name}.Effect");
            if (effectElement == null)
                return;

            var children = effectElement.Elements();
            if (children.Count() > 1)
                throw new Exception($"{xmlElement.Name}.Effect can only have one child");

            var child = children.FirstOrDefault();
            if (child == null)
                return;

            Effect effect = HandleXml<Effect>(dialog, child);
            uiElement.Effect = effect;
        }
        #endregion

        #region Brushes
        private static void HandleXml_Brush(Brush brush, XElement xmlElement)
        {
            brush.Opacity = ParseXmlAttribute<double>(xmlElement, "Opacity", 1.0);
        }

        private static Brush HandleXmlElement_SolidColorBrush(CustomDialog dialog, XElement xmlElement)
        {
            var brush = new SolidColorBrush();
            HandleXml_Brush(brush, xmlElement);

            object? color = GetColorFromXElement(xmlElement, "Color");
            if (color is Color)
                brush.Color = (Color)color;

            return brush;
        }

        private static Brush HandleXmlElement_ImageBrush(CustomDialog dialog, XElement xmlElement)
        {
            var imageBrush = new ImageBrush();
            HandleXml_Brush(imageBrush, xmlElement);

            imageBrush.AlignmentX = ParseXmlAttribute<AlignmentX>(xmlElement, "AlignmentX", AlignmentX.Center);
            imageBrush.AlignmentY = ParseXmlAttribute<AlignmentY>(xmlElement, "AlignmentY", AlignmentY.Center);

            imageBrush.Stretch = ParseXmlAttribute<Stretch>(xmlElement, "Stretch", Stretch.Fill);
            imageBrush.TileMode = ParseXmlAttribute<TileMode>(xmlElement, "TileMode", TileMode.None);

            imageBrush.ViewboxUnits = ParseXmlAttribute<BrushMappingMode>(xmlElement, "ViewboxUnits", BrushMappingMode.RelativeToBoundingBox);
            imageBrush.ViewportUnits = ParseXmlAttribute<BrushMappingMode>(xmlElement, "ViewportUnits", BrushMappingMode.RelativeToBoundingBox);

            var viewbox = GetRectFromXElement(xmlElement, "Viewbox");
            if (viewbox is Rect)
                imageBrush.Viewbox = (Rect)viewbox;

            var viewport = GetRectFromXElement(xmlElement, "Viewport");
            if (viewport is Rect)
                imageBrush.Viewport = (Rect)viewport;

            var sourceData = GetImageSourceData(dialog, "ImageSource", xmlElement);

            if (sourceData.IsIcon)
            {
                // bind the icon property
                Binding binding = new Binding("Icon") { Mode = BindingMode.OneWay };
                BindingOperations.SetBinding(imageBrush, ImageBrush.ImageSourceProperty, binding);
            }
            else
            {
                BitmapImage bitmapImage;
                try
                {
                    bitmapImage = new BitmapImage(sourceData.Uri!);
                }
                catch (Exception ex)
                {
                    throw new Exception($"ImageBrush Failed to create BitmapImage: {ex.Message}", ex);
                }

                imageBrush.ImageSource = bitmapImage;
            }

            return imageBrush;
        }

        private static GradientStop HandleXmlElement_GradientStop(CustomDialog dialog, XElement xmlElement)
        {
            var gs = new GradientStop();

            object? color = GetColorFromXElement(xmlElement, "Color");
            if (color is Color)
                gs.Color = (Color)color;

            gs.Offset = ParseXmlAttribute<double>(xmlElement, "Offset", 0.0);

            return gs;
        }

/*        private static PathGeometry HandleXmlElement_PathGeometry(CustomDialog dialog, XElement xmlElement) 
        {
            var pg = new PathGeometry();
            object? data = GetGeometryFromXElement(xmlElement, "Figures");
            if (data is Geometry)
                pg.Figures = (PathFigureCollection)data;

            pg.FillRule = ParseXmlAttribute<FillRule>(xmlElement, "FillRule", FillRule.EvenOdd);

            return pg;
        }*/

        private static Brush HandleXmlElement_LinearGradientBrush(CustomDialog dialog, XElement xmlElement)
        {
            var brush = new LinearGradientBrush();

            HandleXml_Brush(brush, xmlElement);

            object? startPoint = GetPointFromXElement(xmlElement, "StartPoint");
            if (startPoint is Point)
                brush.StartPoint = (Point)startPoint;

            object? endPoint = GetPointFromXElement(xmlElement, "EndPoint");
            if (endPoint is Point)
                brush.EndPoint = (Point)endPoint;

            brush.ColorInterpolationMode = ParseXmlAttribute<ColorInterpolationMode>(xmlElement, "ColorInterpolationMode", ColorInterpolationMode.SRgbLinearInterpolation);
            brush.MappingMode = ParseXmlAttribute<BrushMappingMode>(xmlElement, "MappingMode", BrushMappingMode.RelativeToBoundingBox);
            brush.SpreadMethod = ParseXmlAttribute<GradientSpreadMethod>(xmlElement, "SpreadMethod", GradientSpreadMethod.Pad);

            foreach (var child in xmlElement.Elements())
                brush.GradientStops.Add(HandleXml<GradientStop>(dialog, child));

            return brush;
        }

        private static Brush HandleXmlElement_RadialGradientBrush(CustomDialog dialog, XElement xmlElement) {
            var radialbrush = new RadialGradientBrush();

            HandleXml_Brush(radialbrush, xmlElement);

            object? startPoint = GetPointFromXElement(xmlElement, "GradientOrigin");
            if (startPoint is Point)
                radialbrush.GradientOrigin = (Point)startPoint;

            object? endPoint = GetPointFromXElement(xmlElement, "Center");
            if (endPoint is Point)
                radialbrush.Center = (Point)endPoint;

            radialbrush.ColorInterpolationMode = ParseXmlAttribute<ColorInterpolationMode>(xmlElement, "ColorInterpolationMode", ColorInterpolationMode.SRgbLinearInterpolation);
            radialbrush.MappingMode = ParseXmlAttribute<BrushMappingMode>(xmlElement, "MappingMode", BrushMappingMode.RelativeToBoundingBox);
            radialbrush.SpreadMethod = ParseXmlAttribute<GradientSpreadMethod>(xmlElement, "SpreadMethod", GradientSpreadMethod.Pad);

            foreach (var child in xmlElement.Elements())
                radialbrush.GradientStops.Add(HandleXml<GradientStop>(dialog, child));

            return radialbrush;
        }

        private static void ApplyBrush_UIElement(CustomDialog dialog, FrameworkElement uiElement, string name, DependencyProperty dependencyProperty, XElement xmlElement)
        {
            // check if attribute exists
            object? brushAttr = GetBrushFromXElement(xmlElement, name);
            if (brushAttr is Brush)
            {
                uiElement.SetValue(dependencyProperty, brushAttr);
                return;
            }
            else if (brushAttr is string)
            {
                uiElement.SetResourceReference(dependencyProperty, brushAttr);
                return;
            }

            // check if element exists
            var brushElement = xmlElement.Element($"{xmlElement.Name}.{name}");
            if (brushElement == null)
                return;

            var first = brushElement.FirstNode as XElement;
            if (first == null)
                throw new Exception($"{xmlElement.Name} {name} is missing the brush");

            var brush = HandleXml<Brush>(dialog, first);
            uiElement.SetValue(dependencyProperty, brush);
        }
        #endregion

        #region Shapes
        private static void HandleXmlElement_Shape(CustomDialog dialog, Shape shape, XElement xmlElement)
        {
            HandleXmlElement_FrameworkElement(dialog, shape, xmlElement);

            ApplyBrush_UIElement(dialog, shape, "Fill", Shape.FillProperty, xmlElement);
            ApplyBrush_UIElement(dialog, shape, "Stroke", Shape.StrokeProperty, xmlElement);

            shape.Stretch = ParseXmlAttribute<Stretch>(xmlElement, "Stretch", Stretch.Fill);

            shape.StrokeDashCap = ParseXmlAttribute<PenLineCap>(xmlElement, "StrokeDashCap", PenLineCap.Flat);
            shape.StrokeDashOffset = ParseXmlAttribute<double>(xmlElement, "StrokeDashOffset", 0);
            shape.StrokeEndLineCap = ParseXmlAttribute<PenLineCap>(xmlElement, "StrokeEndLineCap", PenLineCap.Flat);
            shape.StrokeLineJoin = ParseXmlAttribute<PenLineJoin>(xmlElement, "StrokeLineJoin", PenLineJoin.Miter);
            shape.StrokeMiterLimit = ParseXmlAttribute<double>(xmlElement, "StrokeMiterLimit", 10);
            shape.StrokeStartLineCap = ParseXmlAttribute<PenLineCap>(xmlElement, "StrokeStartLineCap", PenLineCap.Flat);
            shape.StrokeThickness = ParseXmlAttribute<double>(xmlElement, "StrokeThickness", 1);
        }
        private static System.Windows.Shapes.Path HandleXmlElement_Path(CustomDialog dialog, XElement xmlElement)
        {
            var path = new System.Windows.Shapes.Path();
            HandleXmlElement_Shape(dialog, path, xmlElement);

            object? data = GetGeometryFromXElement(xmlElement, "Data");
            if (data is Geometry)
                path.Data = (Geometry)data;

            return path;
        }
        private static Ellipse HandleXmlElement_Ellipse(CustomDialog dialog, XElement xmlElement)
        {
            var ellipse = new Ellipse();
            HandleXmlElement_Shape(dialog, ellipse, xmlElement);

            return ellipse;
        }

        private static Line HandleXmlElement_Line(CustomDialog dialog, XElement xmlElement)
        {
            var line = new Line();
            HandleXmlElement_Shape(dialog, line, xmlElement);

            line.X1 = ParseXmlAttribute<double>(xmlElement, "X1", 0);
            line.X2 = ParseXmlAttribute<double>(xmlElement, "X2", 0);
            line.Y1 = ParseXmlAttribute<double>(xmlElement, "Y1", 0);
            line.Y2 = ParseXmlAttribute<double>(xmlElement, "Y2", 0);

            return line;
        }

        private static Rectangle HandleXmlElement_Rectangle(CustomDialog dialog, XElement xmlElement)
        {
            var rectangle = new Rectangle();
            HandleXmlElement_Shape(dialog, rectangle, xmlElement);

            rectangle.RadiusX = ParseXmlAttribute<double>(xmlElement, "RadiusX", 0);
            rectangle.RadiusY = ParseXmlAttribute<double>(xmlElement, "RadiusY", 0);

            return rectangle;
        }

        #endregion

        #region Elements
        private static void HandleXmlElement_FrameworkElement(CustomDialog dialog, FrameworkElement uiElement, XElement xmlElement)
        {
            // prevent two elements from having the same name
            string? name = xmlElement.Attribute("Name")?.Value?.ToString();
            if (name != null)
            {
                if (dialog.UsedNames.Contains(name))
                    throw new Exception($"{xmlElement.Name} has duplicate name {name}");

                dialog.UsedNames.Add(name);
            }

            uiElement.Name = name;

            uiElement.Visibility = ParseXmlAttribute<Visibility>(xmlElement, "Visibility", Visibility.Visible);
            uiElement.IsEnabled = ParseXmlAttribute<bool>(xmlElement, "IsEnabled", true);

            object? margin = GetThicknessFromXElement(xmlElement, "Margin");
            if (margin != null)
                uiElement.Margin = (Thickness)margin;

            uiElement.Height = ParseXmlAttribute<double>(xmlElement, "Height", double.NaN);
            uiElement.Width = ParseXmlAttribute<double>(xmlElement, "Width", double.NaN);

            // default values of these were originally Stretch but that was no good
            uiElement.HorizontalAlignment = ParseXmlAttribute<HorizontalAlignment>(xmlElement, "HorizontalAlignment", HorizontalAlignment.Left);
            uiElement.VerticalAlignment = ParseXmlAttribute<VerticalAlignment>(xmlElement, "VerticalAlignment", VerticalAlignment.Top);

            uiElement.Opacity = ParseXmlAttribute<double>(xmlElement, "Opacity", 1);
            ApplyBrush_UIElement(dialog, uiElement, "OpacityMask", FrameworkElement.OpacityMaskProperty, xmlElement);

            object? renderTransformOrigin = GetPointFromXElement(xmlElement, "RenderTransformOrigin");
            if (renderTransformOrigin is Point)
                uiElement.RenderTransformOrigin = (Point)renderTransformOrigin;

            int zIndex = ParseXmlAttributeClamped(xmlElement, "ZIndex", defaultValue: 0, min: 0, max: 1000);
            Panel.SetZIndex(uiElement, zIndex);

            ApplyTransformations_UIElement(dialog, uiElement, xmlElement);
            ApplyEffects_UIElement(dialog, uiElement, xmlElement);
        }

        private static void HandleXmlElement_Control(CustomDialog dialog, Control uiElement, XElement xmlElement)
        {
            HandleXmlElement_FrameworkElement(dialog, uiElement, xmlElement);

            object? padding = GetThicknessFromXElement(xmlElement, "Padding");
            if (padding != null)
                uiElement.Padding = (Thickness)padding;

            object? borderThickness = GetThicknessFromXElement(xmlElement, "BorderThickness");
            if (borderThickness != null)
                uiElement.BorderThickness = (Thickness)borderThickness;

            ApplyBrush_UIElement(dialog, uiElement, "Foreground", Control.ForegroundProperty, xmlElement);

            ApplyBrush_UIElement(dialog, uiElement, "Background", Control.BackgroundProperty, xmlElement);

            ApplyBrush_UIElement(dialog, uiElement, "BorderBrush", Control.BorderBrushProperty, xmlElement);

            var fontSize = ParseXmlAttributeNullable<double>(xmlElement, "FontSize");
            if (fontSize is double)
                uiElement.FontSize = (double)fontSize;
            uiElement.FontWeight = GetFontWeightFromXElement(xmlElement);
            uiElement.FontStyle = GetFontStyleFromXElement(xmlElement);

            // NOTE: font family can both be the name of the font or a uri
            string? fontFamily = "file:///" + GetFullPath(dialog, xmlElement.Attribute("FontFamily")?.Value);
            if (fontFamily != null)
                uiElement.FontFamily = new System.Windows.Media.FontFamily(fontFamily);
        }

        private static UIElement HandleXmlElement_BloxstrapCustomBootstrapper(CustomDialog dialog, XElement xmlElement)
        {
            xmlElement.SetAttributeValue("Visibility", "Collapsed"); // don't show the bootstrapper yet!!!
            xmlElement.SetAttributeValue("IsEnabled", "True");
            HandleXmlElement_Control(dialog, dialog, xmlElement);

            dialog.AllowsTransparency = ParseXmlAttribute<bool>(xmlElement, "AllowsTransparency", true);
            dialog.WindowCornerPreference = ParseXmlAttribute<WindowCornerPreference>(xmlElement, "WindowCornerPreference", WindowCornerPreference.Default);
            dialog.WindowBackdropType = ParseXmlAttribute<BackgroundType>(xmlElement, "WindowBackdropType", BackgroundType.Disable);
            dialog.Opacity = ParseXmlAttribute<double>(xmlElement, "Opacity", 1);

            // transfer effect to element grid
            dialog.ElementGrid.RenderTransform = dialog.RenderTransform;
            dialog.RenderTransform = null;
            dialog.ElementGrid.LayoutTransform = dialog.LayoutTransform;
            dialog.LayoutTransform = null;

            dialog.ElementGrid.Effect = dialog.Effect;
            dialog.Effect = null;

            var theme = ParseXmlAttribute<Bloxstrap.Enums.Theme>(xmlElement, "Theme", Bloxstrap.Enums.Theme.Default);
            if (theme == Bloxstrap.Enums.Theme.Default)
                theme = App.Settings.Prop.Theme;

            var wpfUiTheme = theme.GetFinal() == Bloxstrap.Enums.Theme.Dark ? Wpf.Ui.Appearance.ThemeType.Dark : Wpf.Ui.Appearance.ThemeType.Light;

            dialog.Resources.MergedDictionaries.Clear();
            dialog.Resources.MergedDictionaries.Add(new ThemesDictionary() { Theme = wpfUiTheme });
            dialog.DefaultBorderThemeOverwrite = wpfUiTheme;

            // disable default window border if border is modified
            if (xmlElement.Attribute("BorderBrush") != null || xmlElement.Attribute("BorderThickness") != null)
                dialog.DefaultBorderEnabled = false;

            // set the margin & padding on the element grid
            dialog.ElementGrid.Margin = dialog.Margin;
            // TODO: put elementgrid inside a border?

            dialog.Margin = new Thickness(0, 0, 0, 0);
            dialog.Padding = new Thickness(0, 0, 0, 0);

            string? title = xmlElement.Attribute("Title")?.Value?.ToString() ?? "Bloxstrap";
            dialog.Title = title;

            return new DummyFrameworkElement();
        }

        private static UIElement HandleXmlElement_BloxstrapCustomBootstrapper_Fake(CustomDialog dialog, XElement xmlElement)
        {
            // this only exists to error out the theme if someone tries to use two BloxstrapCustomBootstrappers
            throw new Exception($"{xmlElement.Parent!.Name} cannot have a child of {xmlElement.Name}");
        }

        private static DummyFrameworkElement HandleXmlElement_TitleBar(CustomDialog dialog, XElement xmlElement)
        {
            xmlElement.SetAttributeValue("Name", "TitleBar"); // prevent two titlebars from existing
            xmlElement.SetAttributeValue("IsEnabled", "True");
            HandleXmlElement_Control(dialog, dialog.RootTitleBar, xmlElement);

            // get rid of all effects
            dialog.RootTitleBar.RenderTransform = null;
            dialog.RootTitleBar.LayoutTransform = null;

            dialog.RootTitleBar.Effect = null;

            Panel.SetZIndex(dialog.RootTitleBar, 1001); // always show above others

            // properties we dont want modifiable
            dialog.RootTitleBar.Height = double.NaN;
            dialog.RootTitleBar.Width = double.NaN;
            dialog.RootTitleBar.HorizontalAlignment = HorizontalAlignment.Stretch;
            dialog.RootTitleBar.Margin = new Thickness(0, 0, 0, 0);

            dialog.RootTitleBar.ShowMinimize = ParseXmlAttribute<bool>(xmlElement, "ShowMinimize", true);
            dialog.RootTitleBar.ShowClose = ParseXmlAttribute<bool>(xmlElement, "ShowClose", true);

            string? title = xmlElement.Attribute("Title")?.Value?.ToString() ?? "Fishstrap";
            dialog.RootTitleBar.Title = title;

            return new DummyFrameworkElement(); // dont add anything
        }

        private static object? GetContentFromXElement(CustomDialog dialog, XElement xmlElement)
        {
            var contentAttr = xmlElement.Attribute("Content");
            var contentElement = xmlElement.Element($"{xmlElement.Name}.Content");
            if (contentAttr != null && contentElement != null)
                throw new Exception($"{xmlElement.Name} can only have one Content defined");

            if (contentAttr != null)
                return GetTranslatedText(contentAttr.Value);

            if (contentElement == null)
                return null;

            var children = contentElement.Elements();
            if (children.Count() > 1)
                throw new Exception($"{xmlElement.Name}.Content can only have one child");

            var first = contentElement.FirstNode as XElement;
            if (first == null)
                throw new Exception($"{xmlElement.Name} Content is missing the content");

            var uiElement = HandleXml<UIElement>(dialog, first);
            return uiElement;
        }

        private static UIElement HandleXmlElement_Button(CustomDialog dialog, XElement xmlElement)
        {
            var button = new Button();
            HandleXmlElement_Control(dialog, button, xmlElement);

            button.Content = GetContentFromXElement(dialog, xmlElement);

            if (xmlElement.Attribute("Name")?.Value == "CancelButton")
            {
                Binding cancelEnabledBinding = new Binding("CancelEnabled") { Mode = BindingMode.OneWay };
                BindingOperations.SetBinding(button, Button.IsEnabledProperty, cancelEnabledBinding);

                Binding cancelCommandBinding = new Binding("CancelInstallCommand");
                BindingOperations.SetBinding(button, Button.CommandProperty, cancelCommandBinding);
            }

            return button;
        }

        private static void HandleXmlElement_RangeBase(CustomDialog dialog, RangeBase rangeBase, XElement xmlElement)
        {
            HandleXmlElement_Control(dialog, rangeBase, xmlElement);

            rangeBase.Value = ParseXmlAttribute<double>(xmlElement, "Value", 0);
            rangeBase.Maximum = ParseXmlAttribute<double>(xmlElement, "Maximum", 100);
        }

        private static UIElement HandleXmlElement_ProgressBar(CustomDialog dialog, XElement xmlElement)
        {
            var progressBar = new Wpf.Ui.Controls.ProgressBar();
            HandleXmlElement_RangeBase(dialog, progressBar, xmlElement);

            progressBar.IsIndeterminate = ParseXmlAttribute<bool>(xmlElement, "IsIndeterminate", false);

            object? cornerRadius = GetCornerRadiusFromXElement(xmlElement, "CornerRadius");
            if (cornerRadius != null)
                progressBar.CornerRadius = (CornerRadius)cornerRadius;

            object? indicatorCornerRadius = GetCornerRadiusFromXElement(xmlElement, "IndicatorCornerRadius");
            if (indicatorCornerRadius != null)
                progressBar.IndicatorCornerRadius = (CornerRadius)indicatorCornerRadius;

            if (xmlElement.Attribute("Name")?.Value == "PrimaryProgressBar")
            {
                Binding isIndeterminateBinding = new Binding("ProgressIndeterminate") { Mode = BindingMode.OneWay };
                BindingOperations.SetBinding(progressBar, ProgressBar.IsIndeterminateProperty, isIndeterminateBinding);

                Binding maximumBinding = new Binding("ProgressMaximum") { Mode = BindingMode.OneWay };
                BindingOperations.SetBinding(progressBar, ProgressBar.MaximumProperty, maximumBinding);

                Binding valueBinding = new Binding("ProgressValue") { Mode = BindingMode.OneWay };
                BindingOperations.SetBinding(progressBar, ProgressBar.ValueProperty, valueBinding);
            }

            return progressBar;
        }

        private static UIElement HandleXmlElement_ProgressRing(CustomDialog dialog, XElement xmlElement)
        {
            var progressBar = new Wpf.Ui.Controls.ProgressRing();
            HandleXmlElement_RangeBase(dialog, progressBar, xmlElement);

            progressBar.IsIndeterminate = ParseXmlAttribute<bool>(xmlElement, "IsIndeterminate", false);

            if (xmlElement.Attribute("Name")?.Value == "PrimaryProgressRing")
            {
                Binding isIndeterminateBinding = new Binding("ProgressIndeterminate") { Mode = BindingMode.OneWay };
                BindingOperations.SetBinding(progressBar, Wpf.Ui.Controls.ProgressRing.IsIndeterminateProperty, isIndeterminateBinding);

                Binding maximumBinding = new Binding("ProgressMaximum") { Mode = BindingMode.OneWay };
                BindingOperations.SetBinding(progressBar, Wpf.Ui.Controls.ProgressRing.MaximumProperty, maximumBinding);

                Binding valueBinding = new Binding("ProgressValue") { Mode = BindingMode.OneWay };
                BindingOperations.SetBinding(progressBar, Wpf.Ui.Controls.ProgressRing.ValueProperty, valueBinding);
            }

            return progressBar;
        }

        private static void HandleXmlElement_TextBlock_Base(CustomDialog dialog, TextBlock textBlock, XElement xmlElement)
        {
            HandleXmlElement_FrameworkElement(dialog, textBlock, xmlElement);

            textBlock.Text = GetTranslatedText(xmlElement.Attribute("Text")?.Value);

            ApplyBrush_UIElement(dialog, textBlock, "Foreground", TextBlock.ForegroundProperty, xmlElement);

            ApplyBrush_UIElement(dialog, textBlock, "Background", TextBlock.BackgroundProperty, xmlElement);

            var fontSize = ParseXmlAttributeNullable<double>(xmlElement, "FontSize");
            if (fontSize is double)
                textBlock.FontSize = (double)fontSize;
            textBlock.FontWeight = GetFontWeightFromXElement(xmlElement);
            textBlock.FontStyle = GetFontStyleFromXElement(xmlElement);

            textBlock.LineHeight = ParseXmlAttribute<double>(xmlElement, "LineHeight", double.NaN);
            textBlock.LineStackingStrategy = ParseXmlAttribute<LineStackingStrategy>(xmlElement, "LineStackingStrategy", LineStackingStrategy.MaxHeight);

            textBlock.TextAlignment = ParseXmlAttribute<TextAlignment>(xmlElement, "TextAlignment", TextAlignment.Center);
            textBlock.TextTrimming = ParseXmlAttribute<TextTrimming>(xmlElement, "TextTrimming", TextTrimming.None);
            textBlock.TextWrapping = ParseXmlAttribute<TextWrapping>(xmlElement, "TextWrapping", TextWrapping.NoWrap);
            textBlock.TextDecorations = GetTextDecorationsFromXElement(xmlElement);

            textBlock.IsHyphenationEnabled = ParseXmlAttribute<bool>(xmlElement, "IsHyphenationEnabled", false);
            textBlock.BaselineOffset = ParseXmlAttribute<double>(xmlElement, "BaselineOffset", double.NaN);

            // NOTE: font family can both be the name of the font or a uri
            string? fontFamily = "file:///" + GetFullPath(dialog, xmlElement.Attribute("FontFamily")?.Value);
            if (fontFamily != null)
                textBlock.FontFamily = new System.Windows.Media.FontFamily(fontFamily);

            object? padding = GetThicknessFromXElement(xmlElement, "Padding");
            if (padding != null)
                textBlock.Padding = (Thickness)padding;

            if (xmlElement.Attribute("Name")?.Value == "StatusText")
            {
                Binding textBinding = new Binding("Message") { Mode = BindingMode.OneWay };
                BindingOperations.SetBinding(textBlock, TextBlock.TextProperty, textBinding);
            }
        }

        private static UIElement HandleXmlElement_TextBlock(CustomDialog dialog, XElement xmlElement)
        {
            var textBlock = new TextBlock();
            HandleXmlElement_TextBlock_Base(dialog, textBlock, xmlElement);

            return textBlock;
        }

        private static UIElement HandleXmlElement_MarkdownTextBlock(CustomDialog dialog, XElement xmlElement)
        {
            var textBlock = new MarkdownTextBlock();
            HandleXmlElement_TextBlock_Base(dialog, textBlock, xmlElement);

            string? text = GetTranslatedText(xmlElement.Attribute("Text")?.Value);
            if (text != null)
                textBlock.MarkdownText = text;

            return textBlock;
        }

        private static UIElement HandleXmlElement_Image(CustomDialog dialog, XElement xmlElement)
        {
            var image = new Image();
            HandleXmlElement_FrameworkElement(dialog, image, xmlElement);

            image.Stretch = ParseXmlAttribute<Stretch>(xmlElement, "Stretch", Stretch.Uniform);
            image.StretchDirection = ParseXmlAttribute<StretchDirection>(xmlElement, "StretchDirection", StretchDirection.Both);

            RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.HighQuality); // should this be modifiable by the user?

            var sourceData = GetImageSourceData(dialog, "Source", xmlElement);
            

            if (sourceData.IsIcon)
            {
                // bind the icon property
                Binding binding = new Binding("Icon") { Mode = BindingMode.OneWay };
                BindingOperations.SetBinding(image, Image.SourceProperty, binding);
            }
            else
            {
                bool isAnimated = ParseXmlAttribute<bool>(xmlElement, "IsAnimated", false);
                if (!isAnimated)
                {
                    BitmapImage bitmapImage;
                    try
                    {
                        bitmapImage = new BitmapImage(sourceData.Uri!);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Image Failed to create BitmapImage: {ex.Message}", ex);
                    }

                    image.Source = bitmapImage;
                }
                else
                {
                    XamlAnimatedGif.AnimationBehavior.SetSourceUri(image, sourceData.Uri!);

                    RepeatBehavior repeatBehaviour = GetImageRepeatBehaviourData(xmlElement);
                    XamlAnimatedGif.AnimationBehavior.SetRepeatBehavior(image, repeatBehaviour);
                }
            }

            return image;
        }

        private static UIElement HandleXmlElement_MediaElement(CustomDialog dialog, XElement xmlElement)
        {
            var media = new MediaElement();
            HandleXmlElement_FrameworkElement(dialog, media, xmlElement);

            RenderOptions.SetBitmapScalingMode(media, BitmapScalingMode.HighQuality);

            media.LoadedBehavior = ParseXmlAttribute<MediaState>(xmlElement, "LoadedBehaviour", MediaState.Play);
            media.UnloadedBehavior = ParseXmlAttribute<MediaState>(xmlElement, "UnloadedBehaviour", MediaState.Close);

            media.Volume = ParseXmlAttribute<double>(xmlElement, "Volume", 0.5);

            media.Stretch = ParseXmlAttribute<Stretch>(xmlElement, "Stretch", Stretch.Uniform);
            media.StretchDirection = ParseXmlAttribute<StretchDirection>(xmlElement, "StretchDirection", StretchDirection.Both);

            media.Source = GetMediaSourceData(dialog, "Source", xmlElement); //ty return for doing the work
            return media;
        }



        private static T HandleXml<T>(CustomDialog dialog, XElement xmlElement) where T : class
        {
            if (!_elementHandlerMap.ContainsKey(xmlElement.Name.ToString()))
                throw new Exception($"Unknown element {xmlElement.Name}");

            var element = _elementHandlerMap[xmlElement.Name.ToString()](dialog, xmlElement);
            if (element is not T)
                throw new Exception($"{xmlElement.Parent!.Name} cannot have a child of {xmlElement.Name}");

            return (T)element;
        }

        private static void AddXml(CustomDialog dialog, XElement xmlElement)
        {
            if (xmlElement.Name.ToString().StartsWith($"{xmlElement.Parent!.Name}."))
                return; // not an xml element

            var uiElement = HandleXml<UIElement>(dialog, xmlElement);
            if (uiElement is not DummyFrameworkElement)
                dialog.ElementGrid.Children.Add(uiElement);
        }

        private void HandleXmlBase(XElement xml)
        {
            if (_initialised)
                throw new Exception("Custom dialog has already been initialised");

            if (xml.Name != "BloxstrapCustomBootstrapper")
                throw new Exception("XML root is not a BloxstrapCustomBootstrapper");

            if (xml.Attribute("Version")?.Value != "0")
                throw new Exception("Unknown BloxstrapCustomBootstrapper version");

            if (xml.Descendants().Count() > MaxElements)
                throw new Exception($"Custom bootstrappers can have a maximum of {MaxElements} elements");

            _initialised = true;

            // handle root
            HandleXmlElement_BloxstrapCustomBootstrapper(this, xml);

            // handle everything else
            foreach (var child in xml.Elements())
                AddXml(this, child);
        }
        #endregion

        #region Public APIs
        public void ApplyCustomTheme(string name, string contents)
        {
            ThemeDir = System.IO.Path.Combine(Paths.CustomThemes, name);

            XElement xml;

            try
            {
                using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(contents)))
                    xml = XElement.Load(ms);
            }
            catch (Exception ex)
            {
                throw new Exception($"XML parse failed: {ex.Message}", ex);
            }

            HandleXmlBase(xml);
        }

        public void ApplyCustomTheme(string name)
        {
            string path = System.IO.Path.Combine(Paths.CustomThemes, name, "Theme.xml");

            ApplyCustomTheme(name, File.ReadAllText(path));
        }
        #endregion
    }
}
