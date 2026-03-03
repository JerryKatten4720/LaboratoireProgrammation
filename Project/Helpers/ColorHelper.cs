using System.Windows.Media;

namespace LaboratoireProgrammation.Project.Helpers;

public class ColorHelper {
    public static Color FancyText = HexToColor("#fcfe4d");
    public static Color HoverFancyText = HexToColor("e3b946");
    public static Color HomeText = HexToColor("#ff9838");
    public static Color VisualText = HexToColor("#6fff59");
    public static Color Text = HexToColor("#ededed");
    public static Color UserInput = HexToColor("#6ee9ff");
    public static Color Information = HexToColor("#fdff6e");
    public static Color Warning = HexToColor("#ff8f4a");
    public static Color Critical = HexToColor("#c7312c");
    public static Color Success = HexToColor("#29e62c");

    public static Color FadeGreen = HexToColor("#4FF743");
    public static Color FadeRed = HexToColor("#F74343");
    public static Color Snow = HexToColor("#fcfeff");

    public static SolidColorBrush FancyTextBrush {
        get => new(FancyText);
        set => FancyText = value.Color;
    }
    
    public static SolidColorBrush HoverFancyTextBrush {
        get => new(HoverFancyText);
        set => HoverFancyText = value.Color;
    }

    public static SolidColorBrush HomeBrush {
        get => new(FancyText);
        set => FancyText = value.Color;
    }

    public SolidColorBrush VisualBrush {
        get => new(VisualText);
        set => VisualText = value.Color;
    }

    public static SolidColorBrush TextBrush {
        get => new(Text);
        set => Text = value.Color;
    }

    public static SolidColorBrush UserInputBrush {
        get => new(UserInput);
        set => UserInput = value.Color;
    }

    public static SolidColorBrush InformationBrush {
        get => new(Information);
        set => Information = value.Color;
    }

    public static SolidColorBrush WarningBrush {
        get => new(Warning);
        set => Warning = value.Color;
    }

    public static SolidColorBrush CriticalBrush {
        get => new(Critical);
        set => Critical = value.Color;
    }

    public static SolidColorBrush SuccessBrush {
        get => new(Success);
        set => Success = value.Color;
    }
    
    public static SolidColorBrush FadeGreenBrush {
        get => new(FadeGreen);
        set => FadeGreen = value.Color;
    }

    public static SolidColorBrush FadeRedBrush {
        get => new(FadeRed);
        set => FadeRed = value.Color;
    }
    
    public static SolidColorBrush SnowBrush {
        get => new(Snow);
        set => Snow = value.Color;
    }


    public static Color HexToColor(string hex) {
        hex = hex.Replace("#", "");

        if (hex.Length == 6)
            hex = "FF" + hex;

        return Color.FromArgb(
            Convert.ToByte(hex.Substring(0, 2), 16), // A
            Convert.ToByte(hex.Substring(2, 2), 16), // R
            Convert.ToByte(hex.Substring(4, 2), 16), // G
            Convert.ToByte(hex.Substring(6, 2), 16) // B
        );
    }

    public static System.Drawing.Color HexToColorSystem(string hex) {
        hex = hex.Replace("#", "");

        if (hex.Length == 6)
            hex = "FF" + hex;

        return System.Drawing.Color.FromArgb(
            Convert.ToInt32(hex.Substring(0, 2), 16), // A
            Convert.ToInt32(hex.Substring(2, 2), 16), // R
            Convert.ToInt32(hex.Substring(4, 2), 16), // G
            Convert.ToInt32(hex.Substring(6, 2), 16) // B
        );
    }
    
    public static SolidColorBrush HexToColorBrush(string hex) {
        return new SolidColorBrush(HexToColor(hex));
    }

    public static SolidColorBrush generateRandomColor() {
        var random = new Random();
        return new SolidColorBrush(Color.FromArgb((byte)random.Next(0, 256), (byte)random.Next(0, 256),
            (byte)random.Next(0, 256), 0xFF));
    }

    public static SolidColorBrush GetLighter(Brush? brush, int amount) {
        if (brush is SolidColorBrush scb) {
            var color = scb.Color;
            var a = (byte)(color.A - (byte)(255 * (amount / 100.0)));
            var r = (byte)(color.R + (byte)(255 * (amount / 100.0)));
            var g = (byte)(color.G + (byte)(255 * (amount / 100.0)));
            var b = (byte)(color.B + (byte)(255 * (amount / 100.0)));
            return new SolidColorBrush(Color.FromArgb(a, Math.Min(a, r), Math.Min(a, g), Math.Min(a, b)));
        }
        return new SolidColorBrush(Colors.Transparent);
    }
}