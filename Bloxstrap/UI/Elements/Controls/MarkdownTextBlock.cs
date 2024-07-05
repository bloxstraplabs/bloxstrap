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
    /// TextBlock with markdown support.
    /// </summary>
    [ContentProperty("MarkdownText")]
    [Localizability(LocalizationCategory.Text)]
    class MarkdownTextBlock : TextBlock
    {
        private static readonly MarkdownPipeline _markdownPipeline = new MarkdownPipelineBuilder()
                .UseEmphasisExtras(Markdig.Extensions.EmphasisExtras.EmphasisExtraOptions.Marked) // enable '==' support
                .UseSoftlineBreakAsHardlineBreak()
                .Build();

        public static readonly DependencyProperty MarkdownTextProperty = 
            DependencyProperty.Register(nameof(MarkdownText), typeof(string), typeof(MarkdownTextBlock),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OnTextMarkdownChanged));

        [Localizability(LocalizationCategory.Text)]
        public string MarkdownText
        {
            get => (string)GetValue(MarkdownTextProperty);
            set => SetValue(MarkdownTextProperty, value);
        }

        private static System.Windows.Documents.Inline? GetWpfInlineFromMarkdownInline(Markdig.Syntax.Inlines.Inline? inline)
        {
            if (inline is LiteralInline literalInline)
            {
                return new Run(literalInline.ToString());
            }
            else if (inline is EmphasisInline emphasisInline)
            {
                switch (emphasisInline.DelimiterChar)
                {
                    case '*':
                    case '_':
                        {
                            if (emphasisInline.DelimiterCount == 1) // 1 = italic
                            {
                                var childInline = new Italic(GetWpfInlineFromMarkdownInline(emphasisInline.FirstChild));
                                return childInline;
                            }
                            else // 2 = bold
                            {
                                var childInline = new Bold(GetWpfInlineFromMarkdownInline(emphasisInline.FirstChild));
                                return childInline;
                            }
                        }

                    case '=': // marked
                        {
                            var childInline = new Span(GetWpfInlineFromMarkdownInline(emphasisInline.FirstChild));
                            childInline.Background = new SolidColorBrush(Color.FromArgb(50, 255, 255, 255)); // TODO: better colour?
                            return childInline;
                        }
                }

            }
            else if (inline is LinkInline linkInline)
            {
                string? url = linkInline.Url;
                var textInline = linkInline.FirstChild;

                if (string.IsNullOrEmpty(url))
                    return GetWpfInlineFromMarkdownInline(textInline);

                var childInline = GetWpfInlineFromMarkdownInline(textInline);

                return new Hyperlink(childInline)
                {
                    Command = GlobalViewModel.OpenWebpageCommand,
                    CommandParameter = url
                };
            }
            else if (inline is LineBreakInline)
            {
                return new LineBreak();
            }

            return null;
        }

        private void AddMarkdownInline(Markdig.Syntax.Inlines.Inline? inline)
        {
            var wpfInline = GetWpfInlineFromMarkdownInline(inline);

            if (wpfInline is not null)
                Inlines.Add(wpfInline);
        }

        private static void OnTextMarkdownChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (dependencyObject is not MarkdownTextBlock markdownTextBlock)
                return;

            if (dependencyPropertyChangedEventArgs.NewValue is not string rawDocument)
                return;

            var document = Markdown.Parse(rawDocument, _markdownPipeline);

            markdownTextBlock.Inlines.Clear();

            var lastBlock = document.Last();

            // matt was evidently very tired on the night he was first writing this
            // https://github.com/pizzaboxer/bloxstrap/blob/289b9dec77cf35b2cc6504019bc9c7701626be1f/Bloxstrap/UI/Elements/Controls/MarkdownTextBlock.cs#L111
            foreach (var block in document)
            {
                if (block is not ParagraphBlock paragraphBlock || paragraphBlock.Inline is null)
                    continue;

                foreach (var inline in paragraphBlock.Inline)
                    markdownTextBlock.AddMarkdownInline(inline);

                if (block != lastBlock)
                {
                    markdownTextBlock.AddMarkdownInline(new LineBreakInline());
                    markdownTextBlock.AddMarkdownInline(new LineBreakInline());
                }
            }
        }
    }
}
