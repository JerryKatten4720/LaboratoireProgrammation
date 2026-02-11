using System.Windows.Media;

namespace LaboratoireProgrammation.Project.Helpers;

public class ColorHelper {
    public static Color FancyText = HexToColor("#fcfe4d");
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

    public static SolidColorBrush FancyTextBrush {
        get => new(FancyText);
        set => FancyText = value.Color;
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

    public static SolidColorBrush generateRandomColor() {
        var random = new Random();
        return new SolidColorBrush(Color.FromArgb((byte)random.Next(0, 256), (byte)random.Next(0, 256),
            (byte)random.Next(0, 256), 0xFF));
    }
}