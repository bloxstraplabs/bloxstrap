using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

using Wpf.Ui.Markup;

using Bloxstrap.UI.Elements.Controls;

namespace Bloxstrap.UI.Elements.Bootstrapper
{
    public partial class CustomDialog
    {
        private const int MaxElements = 50;

        private static ThicknessConverter? _thicknessConverter = null;
        private static ThicknessConverter ThicknessConverter { get => _thicknessConverter ??= new ThicknessConverter(); }

        private static BrushConverter? _brushConverter = null;
        private static BrushConverter BrushConverter { get => _brushConverter ??= new BrushConverter(); }

        private bool _initialised = false;

        // prevent users from creating elements with the same name multiple times
        private List<string> UsedNames { get; } = new List<string>();

        private string ThemeDir { get; set; } = "";

        delegate UIElement HandleXmlElementDelegate(CustomDialog dialog, XElement xmlElement);
        delegate void HandleXmlTransformationElementDelegate(TransformGroup group, XElement xmlElement);

        private static Dictionary<string, HandleXmlElementDelegate> _elementHandlerMap = new Dictionary<string, HandleXmlElementDelegate>()
        {
            //["BloxstrapCustomBootstrapper"] = HandleXmlElement_BloxstrapCustomBootstrapper,
            ["TitleBar"] = HandleXmlElement_TitleBar,
            ["Button"] = HandleXmlElement_Button,
            ["ProgressBar"] = HandleXmlElement_ProgressBar,
            ["TextBlock"] = HandleXmlElement_TextBlock,
            ["MarkdownTextBlock"] = HandleXmlElement_MarkdownTextBlock,
            ["Image"] = HandleXmlElement_Image
        };

        private static Dictionary<string, HandleXmlTransformationElementDelegate> _transformationHandlerMap = new Dictionary<string, HandleXmlTransformationElementDelegate>()
        {
            ["ScaleTransform"] = HandleXmlTransformationElement_ScaleTransform,
            ["SkewTransform"] = HandleXmlTransformationElement_SkewTransform,
            ["RotateTransform"] = HandleXmlTransformationElement_RotateTransform,
            ["TranslateTransform"] = HandleXmlTransformationElement_TranslateTransform
        };

        #region Utilities
        // https://stackoverflow.com/a/2961702
        private static T? ConvertValue<T>(string input) where T : struct
        {
            try
            {
                var converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter != null)
                {
                    // Cast ConvertFromString(string text) : object to (T)
                    //return (T?)converter.ConvertFromString(input);
                    return (T?)converter.ConvertFromInvariantString(input);
                }
                return default;
            }
            catch (NotSupportedException)
            {
                return default;
            }
        }

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
                throw new Exception($"{element.Name} height is not a valid {typeof(T).Name}");

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
                throw new Exception($"{element.Name} height is not a valid {typeof(T).Name}");

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
        private static double ParseXmlAttributeClamped(XElement element, string attributeName, double? defaultValue = null, double? min = null, double? max = null)
        {
            double value = ParseXmlAttribute<double>(element, attributeName, defaultValue);
            ValidateXmlElement(element.Name.ToString(), attributeName, value, min, max);
            return value;
        }

        private static int ParseXmlAttributeClamped(XElement element, string attributeName, int? defaultValue = null, int? min = null, int? max = null)
        {
            int value = ParseXmlAttribute<int>(element, attributeName, defaultValue);
            ValidateXmlElement(element.Name.ToString(), attributeName, value, min, max);
            return value;
        }

        private static object? GetThicknessFromXElement(XElement xmlElement, string attributeName)
        {
            string? attributeValue = xmlElement.Attribute(attributeName)?.Value?.ToString();
            if (attributeValue == null)
                return null;

            object? thickness;
            try
            {
                thickness = ThicknessConverter.ConvertFromInvariantString(attributeValue);
            }
            catch (Exception ex)
            {
                throw new Exception($"{xmlElement.Name} has invalid {attributeName}: {ex.Message}", ex);
            }

            if (thickness == null)
                throw new Exception($"{xmlElement.Name} has invalid {attributeName}");

            return thickness;
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
        #endregion

        #region Transformation Elements
        private static void HandleXmlTransformationElement_ScaleTransform(TransformGroup group, XElement xmlElement)
        {
            var st = new ScaleTransform();

            st.ScaleX = ParseXmlAttribute<double>(xmlElement, "ScaleX", 1);
            st.ScaleY = ParseXmlAttribute<double>(xmlElement, "ScaleY", 1);
            st.CenterX = ParseXmlAttribute<double>(xmlElement, "CenterX", 0);
            st.CenterY = ParseXmlAttribute<double>(xmlElement, "CenterY", 0);

            group.Children.Add(st);
        }

        private static void HandleXmlTransformationElement_SkewTransform(TransformGroup group, XElement xmlElement)
        {
            var st = new SkewTransform();

            st.AngleX = ParseXmlAttribute<double>(xmlElement, "AngleX", 0);
            st.AngleY = ParseXmlAttribute<double>(xmlElement, "AngleY", 0);
            st.CenterX = ParseXmlAttribute<double>(xmlElement, "CenterX", 0);
            st.CenterY = ParseXmlAttribute<double>(xmlElement, "CenterY", 0);

            group.Children.Add(st);
        }

        private static void HandleXmlTransformationElement_RotateTransform(TransformGroup group, XElement xmlElement)
        {
            var rt = new RotateTransform();

            rt.Angle = ParseXmlAttribute<double>(xmlElement, "Angle", 0);
            rt.CenterX = ParseXmlAttribute<double>(xmlElement, "CenterX", 0);
            rt.CenterY = ParseXmlAttribute<double>(xmlElement, "CenterY", 0);

            group.Children.Add(rt);
        }

        private static void HandleXmlTransformationElement_TranslateTransform(TransformGroup group, XElement xmlElement)
        {
            var tt = new TranslateTransform();

            tt.X = ParseXmlAttribute<double>(xmlElement, "X", 0);
            tt.Y = ParseXmlAttribute<double>(xmlElement, "Y", 0);

            group.Children.Add(tt);
        }

        private static void HandleXmlTransformation(TransformGroup group, XElement xmlElement)
        {
            if (!_transformationHandlerMap.ContainsKey(xmlElement.Name.ToString()))
                throw new Exception($"Unknown transformation {xmlElement.Name}");

            _transformationHandlerMap[xmlElement.Name.ToString()](group, xmlElement);
        }

        private static void ApplyTransformations_UIElement(UIElement uiElement, XElement xmlElement)
        {
            var renderTransform = xmlElement.Element("RenderTransform");

            if (renderTransform != null)
            {
                var tg = new TransformGroup();

                foreach (var child in renderTransform.Elements())
                    HandleXmlTransformation(tg, child);

                if (tg.Children.Any())
                    uiElement.RenderTransform = tg;
            }
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

            uiElement.Height = ParseXmlAttributeClamped(xmlElement, "Height", defaultValue: 100.0, min: 0, max: 1000);
            uiElement.Width = ParseXmlAttributeClamped(xmlElement, "Width", defaultValue: 100.0, min: 0, max: 1000);

            // default values of these were originally Stretch but that was no good
            uiElement.HorizontalAlignment = ParseXmlAttribute<HorizontalAlignment>(xmlElement, "HorizontalAlignment", HorizontalAlignment.Left);
            uiElement.VerticalAlignment = ParseXmlAttribute<VerticalAlignment>(xmlElement, "VerticalAlignment", VerticalAlignment.Top);

            int zIndex = ParseXmlAttributeClamped(xmlElement, "ZIndex", defaultValue: 0, min: 0, max: 1000);
            Panel.SetZIndex(uiElement, zIndex);
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

            object? foregroundBrush = GetBrushFromXElement(xmlElement, "Background");
            if (foregroundBrush is Brush)
                uiElement.Background = (Brush)foregroundBrush;
            else if (foregroundBrush is string)
                uiElement.SetResourceReference(Control.BackgroundProperty, foregroundBrush);

            object? borderBrush = GetBrushFromXElement(xmlElement, "BorderBrush");
            if (borderBrush is Brush)
                uiElement.BorderBrush = (Brush)borderBrush;
            else if (borderBrush is string)
                uiElement.SetResourceReference(Control.BorderBrushProperty, borderBrush);
        }

        private static UIElement? HandleXmlElement_BloxstrapCustomBootstrapper(CustomDialog dialog, XElement xmlElement)
        {
            xmlElement.SetAttributeValue("Visibility", "Collapsed"); // don't show the bootstrapper yet!!!
            xmlElement.SetAttributeValue("IsEnabled", "True");
            HandleXmlElement_Control(dialog, dialog, xmlElement);

            var theme = ParseXmlAttribute<Theme>(xmlElement, "Theme", Theme.Default);
            dialog.Resources.MergedDictionaries.Clear();
            dialog.Resources.MergedDictionaries.Add(new ThemesDictionary() { Theme = theme.GetFinal() == Theme.Dark ? Wpf.Ui.Appearance.ThemeType.Dark : Wpf.Ui.Appearance.ThemeType.Light });

            // set the margin & padding on the element grid
            dialog.ElementGrid.Margin = dialog.Margin;
            // TODO: put elementgrid inside a border?

            dialog.Margin = new Thickness(0, 0, 0, 0);
            dialog.Padding = new Thickness(0, 0, 0, 0);

            return null; // dont add anything
        }

        private static UIElement? HandleXmlElement_TitleBar(CustomDialog dialog, XElement xmlElement)
        {
            xmlElement.SetAttributeValue("Name", "TitleBar"); // prevent two titlebars from existing
            xmlElement.SetAttributeValue("IsEnabled", "True");
            HandleXmlElement_Control(dialog, dialog.RootTitleBar, xmlElement);

            Panel.SetZIndex(dialog.RootTitleBar, 1001); // always show above others

            // properties we dont want modifiable
            dialog.RootTitleBar.Height = double.NaN;
            dialog.RootTitleBar.Width = double.NaN;
            dialog.RootTitleBar.HorizontalAlignment = HorizontalAlignment.Stretch;
            dialog.RootTitleBar.Margin = new Thickness(0, 0, 0, 0);

            dialog.RootTitleBar.ShowMinimize = ParseXmlAttribute<bool>(xmlElement, "ShowMinimize", true);
            dialog.RootTitleBar.ShowClose = ParseXmlAttribute<bool>(xmlElement, "ShowClose", true);

            string? title = xmlElement.Attribute("Title")?.Value?.ToString() ?? "Bloxstrap";
            dialog.Title = title;
            dialog.RootTitleBar.Title = title;

            return null; // dont add anything
        }

        private static object? GetContentFromXElement(CustomDialog dialog, XElement xmlElement)
        {
            var contentAttr = xmlElement.Attribute("Content");
            if (contentAttr != null)
                return contentAttr.Value.ToString();

            var contentElement = xmlElement.Element("Content");
            if (contentElement != null)
            {
                var first = contentElement.FirstNode as XElement;
                if (first == null)
                    throw new Exception($"{xmlElement.Name} Content is missing the content");

                var uiElement = HandleXml(dialog, first);
                return uiElement;
            }

            return null;
        }

        private static UIElement? HandleXmlElement_Button(CustomDialog dialog, XElement xmlElement)
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

            ApplyTransformations_UIElement(button, xmlElement);

            return button;
        }

        private static UIElement? HandleXmlElement_ProgressBar(CustomDialog dialog, XElement xmlElement)
        {
            var progressBar = new ProgressBar();
            HandleXmlElement_Control(dialog, progressBar, xmlElement);

            progressBar.IsIndeterminate = ParseXmlAttribute<bool>(xmlElement, "IsIndeterminate", false);

            progressBar.Value = ParseXmlAttribute<double>(xmlElement, "Value", 0);
            progressBar.Maximum = ParseXmlAttribute<double>(xmlElement, "Maximum", 100);

            if (xmlElement.Attribute("Name")?.Value == "PrimaryProgressBar")
            {
                Binding isIndeterminateBinding = new Binding("ProgressIndeterminate") { Mode = BindingMode.OneWay };
                BindingOperations.SetBinding(progressBar, ProgressBar.IsIndeterminateProperty, isIndeterminateBinding);

                Binding maximumBinding = new Binding("ProgressMaximum") { Mode = BindingMode.OneWay };
                BindingOperations.SetBinding(progressBar, ProgressBar.MaximumProperty, maximumBinding);

                Binding valueBinding = new Binding("ProgressValue") { Mode = BindingMode.OneWay };
                BindingOperations.SetBinding(progressBar, ProgressBar.ValueProperty, valueBinding);
            }

            ApplyTransformations_UIElement(progressBar, xmlElement);

            return progressBar;
        }

        private static void HandleXmlElement_TextBlock_Base(CustomDialog dialog, TextBlock textBlock, XElement xmlElement)
        {
            HandleXmlElement_FrameworkElement(dialog, textBlock, xmlElement);

            textBlock.Text = xmlElement.Attribute("Text")?.Value;

            object? foregroundBrush = GetBrushFromXElement(xmlElement, "Foreground");
            if (foregroundBrush is Brush)
                textBlock.Foreground = (Brush)foregroundBrush;
            else if (foregroundBrush is string)
                textBlock.SetResourceReference(TextBlock.ForegroundProperty, foregroundBrush);

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

            object? padding = GetThicknessFromXElement(xmlElement, "Padding");
            if (padding != null)
                textBlock.Padding = (Thickness)padding;

            if (xmlElement.Attribute("Name")?.Value == "StatusText")
            {
                Binding textBinding = new Binding("Message") { Mode = BindingMode.OneWay };
                BindingOperations.SetBinding(textBlock, TextBlock.TextProperty, textBinding);
            }

            ApplyTransformations_UIElement(textBlock, xmlElement);
        }

        private static UIElement? HandleXmlElement_TextBlock(CustomDialog dialog, XElement xmlElement)
        {
            var textBlock = new TextBlock();
            HandleXmlElement_TextBlock_Base(dialog, textBlock, xmlElement);

            return textBlock;
        }

        private static UIElement? HandleXmlElement_MarkdownTextBlock(CustomDialog dialog, XElement xmlElement)
        {
            var textBlock = new MarkdownTextBlock();
            HandleXmlElement_TextBlock_Base(dialog, textBlock, xmlElement);

            string? text = xmlElement.Attribute("Text")?.Value;
            if (text != null)
                textBlock.MarkdownText = text;

            return textBlock;
        }

        private static UIElement? HandleXmlElement_Image(CustomDialog dialog, XElement xmlElement)
        {
            var image = new Image();
            HandleXmlElement_FrameworkElement(dialog, image, xmlElement);

            image.Stretch = ParseXmlAttribute<Stretch>(xmlElement, "Stretch", Stretch.Uniform);
            image.StretchDirection = ParseXmlAttribute<StretchDirection>(xmlElement, "StretchDirection", StretchDirection.Both);

            string sourcePath = GetXmlAttribute(xmlElement, "Source");
            sourcePath = sourcePath.Replace("theme://", $"{dialog.ThemeDir}\\");

            RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.HighQuality); // should this be modifiable by the user?

            if (sourcePath == "{Icon}")
            {
                // bind the icon property
                Binding binding = new Binding("Icon") { Mode = BindingMode.OneWay };
                BindingOperations.SetBinding(image, Image.SourceProperty, binding);
            }
            else
            {
                if (!Uri.TryCreate(sourcePath, UriKind.RelativeOrAbsolute, out Uri? result))
                    throw new Exception("Image failed to parse Source as Uri");

                if (result == null)
                    throw new Exception("Image Source uri is null");

                bool isAnimated = ParseXmlAttribute<bool>(xmlElement, "IsAnimated", false);
                if (!isAnimated)
                {
                    BitmapImage bitmapImage;
                    try
                    {
                        bitmapImage = new BitmapImage(result);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Image Failed to create BitmapImage: {ex.Message}", ex);
                    }

                    image.Source = bitmapImage;
                }
                else
                {
                    XamlAnimatedGif.AnimationBehavior.SetSourceUri(image, result);
                }
            }

            ApplyTransformations_UIElement(image, xmlElement);

            return image;
        }

        private static UIElement? HandleXml(CustomDialog dialog, XElement xmlElement)
        {
            if (!_elementHandlerMap.ContainsKey(xmlElement.Name.ToString()))
                throw new Exception($"Unknown element {xmlElement.Name}");

            var uiElement = _elementHandlerMap[xmlElement.Name.ToString()](dialog, xmlElement);
            return uiElement;
        }

        private static void HandleAndAddXml(CustomDialog dialog, XElement xmlElement)
        {
            var uiElement = HandleXml(dialog, xmlElement);
            if (uiElement != null)
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

            if (xml.Elements().Count() > MaxElements)
                throw new Exception($"Custom bootstrappers can have a maximum of {MaxElements} elements");

            _initialised = true;

            // handle root
            HandleXmlElement_BloxstrapCustomBootstrapper(this, xml);

            // handle everything else
            foreach (var child in xml.Elements())
                HandleAndAddXml(this, child);
        }
        #endregion

        #region Public APIs
        public void ApplyCustomTheme(string name, string contents)
        {
            ThemeDir = Path.Combine(Paths.CustomThemes, name);

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
            string path = Path.Combine(Paths.CustomThemes, name, "Theme.xml");

            ApplyCustomTheme(name, File.ReadAllText(path));
        }
        #endregion
    }
}
