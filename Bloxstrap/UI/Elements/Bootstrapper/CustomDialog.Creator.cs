using System.Windows;
using System.Xml.Linq;

namespace Bloxstrap.UI.Elements.Bootstrapper
{
    public partial class CustomDialog
    {
        const int Version = 0;

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
            ["Grid"] = HandleXmlElement_Grid,
            ["StackPanel"] = HandleXmlElement_StackPanel,
            ["Border"] = HandleXmlElement_Border,
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

            ["SvgViewbox"] = HandleXmlElement_SvgViewbox,
            ["SvgIcon"] = HandleXmlElement_SvgIcon,
            ["SvgBitmap"] = HandleXmlElement_SvgBitmap,

            ["Path"] = HandleXmlElement_Path,
            ["Ellipse"] = HandleXmlElement_Ellipse,
            ["Line"] = HandleXmlElement_Line,
            ["Rectangle"] = HandleXmlElement_Rectangle,

            ["RowDefinition"] = HandleXmlElement_RowDefinition,
            ["ColumnDefinition"] = HandleXmlElement_ColumnDefinition
        };

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

            if (xml.Attribute("Version")?.Value != Version.ToString())
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
