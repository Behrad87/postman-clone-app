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

    private static readonly SolidColorBrush KeyBrush = Brush(0x22, 0xD3, 0xEE);
    private static readonly SolidColorBrush StringBrush = Brush(0x34, 0xD3, 0x99);
    private static readonly SolidColorBrush NumberBrush = Brush(0xFB, 0xBF, 0x24);
    private static readonly SolidColorBrush KeywordBrush = Brush(0xC0, 0x84, 0xFC);
    private static readonly SolidColorBrush PunctBrush = Brush(0x8B, 0x93, 0xB8);
    private static readonly SolidColorBrush DefaultBrush = Brush(0xF4, 0xF6, 0xFF);

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
