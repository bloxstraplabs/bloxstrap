using Bloxstrap.UI.ViewModels;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Markdig;

namespace Bloxstrap.UI.Elements.Controls
{
    /// <summary>
    /// TextBlock with markdown support. <br/>
    /// Only supports text and urls.
    /// </summary>
    [ContentProperty("MarkdownText")]
    [Localizability(LocalizationCategory.Text)]
    class MarkdownTextBlock : TextBlock
    {
        public static readonly DependencyProperty MarkdownTextProperty = DependencyProperty.Register(
            "MarkdownText",
            typeof(string),
            typeof(MarkdownTextBlock),
            new FrameworkPropertyMetadata(
                string.Empty,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
                OnTextMarkdownChanged));

        public MarkdownTextBlock(string markdownText)
        {
            MarkdownText = markdownText;
        }

        public MarkdownTextBlock()
        {
        }

        [Localizability(LocalizationCategory.Text)]
        public string MarkdownText
        {
            get => Inlines.ToString() ?? "";
            set => SetValue(MarkdownTextProperty, value);
        }

        private static void OnTextMarkdownChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var markdownTextBlock = dependencyObject as MarkdownTextBlock;

            if (markdownTextBlock == null)
                return;

            MarkdownDocument document = Markdown.Parse((string)dependencyPropertyChangedEventArgs.NewValue);
            ParagraphBlock? paragraphBlock = document.FirstOrDefault() as ParagraphBlock;

            markdownTextBlock.Inlines.Clear();

            if (paragraphBlock == null || paragraphBlock.Inline == null)
                return;

            foreach (var inline in paragraphBlock.Inline)
            {
                if (inline is LiteralInline literalInline)
                {
                    markdownTextBlock.Inlines.Add(new Run(literalInline.ToString()));
                }
                else if (inline is LinkInline linkInline)
                {
                    string? url = linkInline.Url;
                    string? text = linkInline.FirstChild?.ToString();

                    if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(text))
                        continue;

                    var link = new Hyperlink(new Run(text))
                    {
                        Command = GlobalViewModel.OpenWebpageCommand,
                        CommandParameter = url
                    };

                    link.SetResourceReference(Control.ForegroundProperty, "TextFillColorPrimaryBrush");

                    markdownTextBlock.Inlines.Add(link);
                }
            }
        }
    }
}
