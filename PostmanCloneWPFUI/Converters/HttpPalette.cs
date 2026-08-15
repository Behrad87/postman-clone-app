using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PostmanCloneWPFUI.Converters;

internal static class HttpPalette
{
    public static Color Method(string? method) => (method ?? "").ToUpperInvariant() switch
    {
        "GET" => Color.FromRgb(0x22, 0xD3, 0xEE),
        "POST" => Color.FromRgb(0x34, 0xD3, 0x99),
        "PUT" => Color.FromRgb(0xFB, 0xBF, 0x24),
        "PATCH" => Color.FromRgb(0xC0, 0x84, 0xFC),
        "DELETE" => Color.FromRgb(0xFB, 0x71, 0x85),
        "HEAD" => Color.FromRgb(0x2D, 0xD4, 0xBF),
        "OPTIONS" => Color.FromRgb(0xFB, 0x92, 0x3C),
        _ => Color.FromRgb(0x8B, 0x93, 0xB8)
    };

    public static Color Status(int code) => code switch
    {
        >= 200 and < 300 => Color.FromRgb(0x34, 0xD3, 0x99),
        >= 300 and < 400 => Color.FromRgb(0xFB, 0xBF, 0x24),
        >= 400 and < 500 => Color.FromRgb(0xFB, 0x71, 0x85),
        >= 500 => Color.FromRgb(0xC0, 0x84, 0xFC),
        _ => Color.FromRgb(0x8B, 0x93, 0xB8)
    };

    public static SolidColorBrush Solid(Color color)
    {
        var b = new SolidColorBrush(color);
        b.Freeze();
        return b;
    }

    public static SolidColorBrush Translucent(Color color, byte alpha)
    {
        color.A = alpha;
        var b = new SolidColorBrush(color);
        b.Freeze();
        return b;
    }
}

public class MethodColorConverter : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c)
        => HttpPalette.Solid(HttpPalette.Method(value as string));

    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotImplementedException();
}

public class MethodBadgeBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c)
        => HttpPalette.Translucent(HttpPalette.Method(value as string), 0x36);

    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotImplementedException();
}

public class StatusCodeColorConverter : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c)
    {
        var code = value is int i ? i : 0;
        return HttpPalette.Solid(HttpPalette.Status(code));
    }

    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotImplementedException();
}

public class StatusBadgeBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c)
    {
        var code = value is int i ? i : 0;
        return HttpPalette.Translucent(HttpPalette.Status(code), 0x36);
    }

    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotImplementedException();
}
