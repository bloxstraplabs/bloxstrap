using Bloxstrap.UI.ViewModels;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Markdig;
using System.Windows.Media;

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
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OnTextMarkdownChanged));

        [Localizability(LocalizationCategory.Text)]
        public string MarkdownText
        {
            get => (string)GetValue(MarkdownTextProperty);
            set => SetValue(MarkdownTextProperty, value);
        }

        /// <returns>Span, skip</returns>
        private static (Span, int) GetInlineUntilEndTagDetected(Markdig.Syntax.Inlines.Inline? inline, string tagName)
        {
            string endTag = $"</{tagName}>"; // TODO: better way of doing this

            var span = new Span();

            int skip = 0;
            var current = inline;
            while (current is Markdig.Syntax.Inlines.Inline currentInline)
            {
                skip++;

                if (currentInline is HtmlInline html)
                {
                    if (html.Tag == endTag)
                        return (span, skip);
                }

                (var childInline, int childSkip) = GetWpfInlineFromMarkdownInline(currentInline);
                if (childInline != null)
                    span.Inlines.Add(childInline);

                skip += childSkip;

                current = currentInline.NextSibling;
            }

            throw new Exception("End tag not detected");
        }

        /// <returns>Inline, skip</returns>
        private static (System.Windows.Documents.Inline?, int) GetWpfInlineFromMarkdownInline(Markdig.Syntax.Inlines.Inline? inline)
        {
            if (inline is LiteralInline literalInline)
            {
                return (new Run(literalInline.ToString()), 0);
            }
            else if (inline is LinkInline linkInline)
            {
                string? url = linkInline.Url;
                var textInline = linkInline.FirstChild;

                if (string.IsNullOrEmpty(url))
                {
                    return GetWpfInlineFromMarkdownInline(textInline);
                }

                (var childInline, int skip) = GetWpfInlineFromMarkdownInline(textInline);

                return (new Hyperlink(childInline)
                {
                    Command = GlobalViewModel.OpenWebpageCommand,
                    CommandParameter = url
                }, skip);
            }
            else if (inline is HtmlInline htmlInline)
            {
                string? tag = htmlInline.Tag; // TODO: parse tag
                var nextInline = htmlInline.NextSibling;

                if (tag == "<highlight>")
                {
                    (var span, int skip) = GetInlineUntilEndTagDetected(nextInline, "highlight");
                    span.Background = new SolidColorBrush(Color.FromArgb(50,255,255,255));
                    return (span, skip);
                }
            }

            return (null, 0);
        }

        /// <returns>Skip</returns>
        private int AddMarkdownInline(Markdig.Syntax.Inlines.Inline? inline)
        {
            (var wpfInline, int skip) = GetWpfInlineFromMarkdownInline(inline);
            if (wpfInline != null)
                Inlines.Add(wpfInline);

            return skip;
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

            for (int i = 0; i < paragraphBlock.Inline.Count(); i++)
            {
                var inline = paragraphBlock.Inline.ElementAt(i);

                int skip = markdownTextBlock.AddMarkdownInline(inline);
                i += skip;
            }
        }
    }
}
