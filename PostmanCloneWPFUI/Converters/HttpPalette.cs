using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PostmanCloneWPFUI.Converters;

internal static class HttpPalette
{
    public static Color Method(string? method) => (method ?? "").ToUpperInvariant() switch
    {
        "GET" => Color.FromRgb(0x2E, 0x6B, 0x6B),
        "POST" => Color.FromRgb(0x3F, 0x7A, 0x4A),
        "PUT" => Color.FromRgb(0xB8, 0x7A, 0x22),
        "PATCH" => Color.FromRgb(0x6B, 0x4E, 0x8A),
        "DELETE" => Color.FromRgb(0xB0, 0x45, 0x3E),
        "HEAD" => Color.FromRgb(0x3D, 0x6E, 0x78),
        "OPTIONS" => Color.FromRgb(0xC4, 0x6B, 0x2A),
        _ => Color.FromRgb(0x6E, 0x5E, 0x4A)
    };

    public static Color Status(int code) => code switch
    {
        >= 200 and < 300 => Color.FromRgb(0x3F, 0x7A, 0x4A),
        >= 300 and < 400 => Color.FromRgb(0xB8, 0x7A, 0x22),
        >= 400 and < 500 => Color.FromRgb(0xB0, 0x45, 0x3E),
        >= 500 => Color.FromRgb(0x6B, 0x4E, 0x8A),
        _ => Color.FromRgb(0x6E, 0x5E, 0x4A)
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
        => HttpPalette.Translucent(HttpPalette.Method(value as string), 0x28);

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
        return HttpPalette.Translucent(HttpPalette.Status(code), 0x28);
    }

    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotImplementedException();
}
