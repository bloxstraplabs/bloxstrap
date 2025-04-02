﻿using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Xml.Linq;

namespace Bloxstrap.UI.Elements.Bootstrapper
{
    public partial class CustomDialog
    {
        struct GetImageSourceDataResult
        {
            public bool IsIcon = false;
            public Uri? Uri = null;

            public GetImageSourceDataResult()
            {
            }
        }

        private static string GetXmlAttribute(XElement element, string attributeName, string? defaultValue = null)
        {
            var attribute = element.Attribute(attributeName);

            if (attribute == null)
            {
                if (defaultValue != null)
                    return defaultValue;

                throw new CustomThemeException("CustomTheme.Errors.ElementAttributeMissing", element.Name, attributeName);
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

                throw new CustomThemeException("CustomTheme.Errors.ElementAttributeMissing", element.Name, attributeName);
            }

            T? parsed = ConvertValue<T>(attribute.Value);
            if (parsed == null)
                throw new CustomThemeException("CustomTheme.Errors.ElementAttributeInvalidType", element.Name, attributeName, typeof(T).Name);

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
                throw new CustomThemeException("CustomTheme.Errors.ElementAttributeInvalidType", element.Name, attributeName, typeof(T).Name);

            return (T)parsed;
        }

        private static void ValidateXmlElement(string elementName, string attributeName, int value, int? min = null, int? max = null)
        {
            if (min != null && value < min)
                throw new CustomThemeException("CustomTheme.Errors.ElementAttributeMustBeLargerThanMin", elementName, attributeName, min);
            if (max != null && value > max)
                throw new CustomThemeException("CustomTheme.Errors.ElementAttributeMustBeSmallerThanMax", elementName, attributeName, max);
        }

        private static void ValidateXmlElement(string elementName, string attributeName, double value, double? min = null, double? max = null)
        {
            if (min != null && value < min)
                throw new CustomThemeException("CustomTheme.Errors.ElementAttributeMustBeLargerThanMin", elementName, attributeName, min);
            if (max != null && value > max)
                throw new CustomThemeException("CustomTheme.Errors.ElementAttributeMustBeSmallerThanMax", elementName, attributeName, max);
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
                    throw new CustomThemeException("CustomTheme.Errors.UnknownEnumValue", element.Name, "FontWeight", value);
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
                    throw new CustomThemeException("CustomTheme.Errors.UnknownEnumValue", element.Name, "FontStyle", value);
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
                    throw new CustomThemeException("CustomTheme.Errors.UnknownEnumValue", element.Name, "TextDecorations", value);
            }
        }

        private static string? GetTranslatedText(string? text)
        {
            if (text == null || !text.StartsWith('{') || !text.EndsWith('}'))
                return text; // can't be translated (not in the correct format)

            string resourceName = text[1..^1];

            if (resourceName == "Version")
                return App.ShortCommitHash;

            return Strings.ResourceManager.GetStringSafe(resourceName);
        }

        private static string? GetFullPath(CustomDialog dialog, string? sourcePath)
        {
            if (sourcePath == null)
                return null;

            // TODO: this is bad :(
            return sourcePath.Replace("theme://", $"{dialog.ThemeDir}\\");
        }

        private static GetImageSourceDataResult GetImageSourceData(CustomDialog dialog, string name, XElement xmlElement)
        {
            string path = GetXmlAttribute(xmlElement, name);

            if (path == "{Icon}")
                return new GetImageSourceDataResult { IsIcon = true };

            path = GetFullPath(dialog, path)!;

            if (!Uri.TryCreate(path, UriKind.RelativeOrAbsolute, out Uri? result))
                throw new CustomThemeException("CustomTheme.Errors.ElementAttributeParseError", xmlElement.Name, name, "Uri");

            if (result == null)
                throw new CustomThemeException("CustomTheme.Errors.ElementAttributeParseErrorNull", xmlElement.Name, name, "Uri");

            if (result.Scheme != "file")
                throw new CustomThemeException("CustomTheme.Errors.ElementAttributeBlacklistedUriScheme", xmlElement.Name, name, result.Scheme);

            return new GetImageSourceDataResult { Uri = result };
        }

        private static object? GetContentFromXElement(CustomDialog dialog, XElement xmlElement)
        {
            var contentAttr = xmlElement.Attribute("Content");
            var contentElement = xmlElement.Element($"{xmlElement.Name}.Content");
            if (contentAttr != null && contentElement != null)
                throw new CustomThemeException("CustomTheme.Errors.ElementAttributeMultipleDefinitions", xmlElement.Name, "Content");

            if (contentAttr != null)
                return GetTranslatedText(contentAttr.Value);

            if (contentElement == null)
                return null;

            var children = contentElement.Elements();
            if (children.Count() > 1)
                throw new CustomThemeException("CustomTheme.Errors.ElementAttributeMultipleChildren", xmlElement.Name, "Content");

            var first = contentElement.FirstNode as XElement;
            if (first == null)
                throw new CustomThemeException("CustomTheme.Errors.ElementAttributeMissingChild", xmlElement.Name, "Content");

            var uiElement = HandleXml<UIElement>(dialog, first);
            return uiElement;
        }

        private static void ApplyEffects_UIElement(CustomDialog dialog, UIElement uiElement, XElement xmlElement)
        {
            var effectElement = xmlElement.Element($"{xmlElement.Name}.Effect");
            if (effectElement == null)
                return;

            var children = effectElement.Elements();
            if (children.Count() > 1)
                throw new CustomThemeException("CustomTheme.Errors.ElementAttributeMultipleChildren", xmlElement.Name, "Effect");

            var child = children.FirstOrDefault();
            if (child == null)
                return;

            Effect effect = HandleXml<Effect>(dialog, child);
            uiElement.Effect = effect;
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
    }
}
