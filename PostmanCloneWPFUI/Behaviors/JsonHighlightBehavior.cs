using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace PostmanCloneWPFUI.Behaviors;

public static class JsonHighlightBehavior
{
    private static readonly Regex Tokenizer = new(
        """(?<key>"(?:\\.|[^"\\])*")\s*(?=:)|(?<string>"(?:\\.|[^"\\])*")|(?<number>-?\d+(?:\.\d+)?(?:[eE][+-]?\d+)?)|(?<keyword>\b(?:true|false|null)\b)|(?<punct>[{}\[\]:,])""",
        RegexOptions.Compiled);

    private static readonly SolidColorBrush KeyBrush = Brush(0x2E, 0x6B, 0x6B);
    private static readonly SolidColorBrush StringBrush = Brush(0x3F, 0x7A, 0x4A);
    private static readonly SolidColorBrush NumberBrush = Brush(0xB8, 0x7A, 0x22);
    private static readonly SolidColorBrush KeywordBrush = Brush(0x6B, 0x4E, 0x8A);
    private static readonly SolidColorBrush PunctBrush = Brush(0xA8, 0x96, 0x7C);
    private static readonly SolidColorBrush DefaultBrush = Brush(0x2B, 0x24, 0x16);

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.RegisterAttached(
            "Text",
            typeof(string),
            typeof(JsonHighlightBehavior),
            new PropertyMetadata(null, OnChanged));

    public static readonly DependencyProperty HighlightProperty =
        DependencyProperty.RegisterAttached(
            "Highlight",
            typeof(bool),
            typeof(JsonHighlightBehavior),
            new PropertyMetadata(true, OnChanged));

    public static string? GetText(DependencyObject obj) => (string?)obj.GetValue(TextProperty);
    public static void SetText(DependencyObject obj, string? value) => obj.SetValue(TextProperty, value);
    public static bool GetHighlight(DependencyObject obj) => (bool)obj.GetValue(HighlightProperty);
    public static void SetHighlight(DependencyObject obj, bool value) => obj.SetValue(HighlightProperty, value);

    private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not RichTextBox rtb) return;
        var text = GetText(rtb) ?? string.Empty;
        var highlight = GetHighlight(rtb);
        Apply(rtb, text, highlight);
    }

    private static void Apply(RichTextBox rtb, string text, bool highlight)
    {
        var doc = new FlowDocument
        {
            PagePadding = new Thickness(0),
            Background = Brushes.Transparent
        };
        var paragraph = new Paragraph { Margin = new Thickness(0) };

        if (!highlight || text.Length > 256_000)
        {
            paragraph.Inlines.Add(new Run(text) { Foreground = DefaultBrush });
        }
        else
        {
            var last = 0;
            foreach (Match match in Tokenizer.Matches(text))
            {
                if (match.Index > last)
                    paragraph.Inlines.Add(new Run(text[last..match.Index]) { Foreground = DefaultBrush });

                var brush =
                    match.Groups["key"].Success ? KeyBrush :
                    match.Groups["string"].Success ? StringBrush :
                    match.Groups["number"].Success ? NumberBrush :
                    match.Groups["keyword"].Success ? KeywordBrush :
                    match.Groups["punct"].Success ? PunctBrush :
                    DefaultBrush;

                paragraph.Inlines.Add(new Run(match.Value) { Foreground = brush });
                last = match.Index + match.Length;
            }

            if (last < text.Length)
                paragraph.Inlines.Add(new Run(text[last..]) { Foreground = DefaultBrush });
        }

        doc.Blocks.Add(paragraph);
        rtb.Document = doc;
    }

    private static SolidColorBrush Brush(byte r, byte g, byte b)
    {
        var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
        brush.Freeze();
        return brush;
    }
}
