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
        public static readonly DependencyProperty MarkdownTextProperty = 
            DependencyProperty.Register(nameof(MarkdownText), typeof(string), typeof(MarkdownTextBlock),
                new FrameworkPropertyMetadata(string.Empty,FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OnTextMarkdownChanged));

        [Localizability(LocalizationCategory.Text)]
        public string MarkdownText
        {
            get => Inlines.ToString() ?? "";
            set => SetValue(MarkdownTextProperty, value);
        }

        private static void OnTextMarkdownChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (dependencyObject is not MarkdownTextBlock markdownTextBlock)
                return;

            if (dependencyPropertyChangedEventArgs.NewValue is not string rawDocument)
                return;

            MarkdownDocument document = Markdown.Parse(rawDocument);

            markdownTextBlock.Inlines.Clear();

            if (document.FirstOrDefault() is not ParagraphBlock paragraphBlock || paragraphBlock.Inline == null)
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

                    markdownTextBlock.Inlines.Add(new Hyperlink(new Run(text))
                    {
                        Command = GlobalViewModel.OpenWebpageCommand,
                        CommandParameter = url
                    });
                }
            }
        }
    }
}
