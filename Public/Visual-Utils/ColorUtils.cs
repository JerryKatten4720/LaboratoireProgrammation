using System.Windows.Media;

namespace LaboratoireProgrammation.Public.Visual_Utils;

public class ColorUtils {

    public static Color FancyText = HexToColor("#fcfe4d");
    public static Color HomeText = HexToColor("#ff9838");
    public static Color VisualText = HexToColor("#6fff59");
    public static Color Text = HexToColor("#ededed");
    public static Color UserInput = HexToColor("#6ee9ff");
    public static Color Information = HexToColor("#fdff6e");
    public static Color Warning = HexToColor("#ff8f4a");
    public static Color Critical = HexToColor("#c7312c");
    public static Color Success = HexToColor("#29e62c");
    
    public static SolidColorBrush FancyTextBrush {
        get => new SolidColorBrush(FancyText);
        set => FancyText = value.Color;
    }
    
    public static SolidColorBrush HomeBrush {
        get => new SolidColorBrush(FancyText);
        set => FancyText = value.Color;
    }

    public SolidColorBrush VisualBrush {
        get => new SolidColorBrush(VisualText);
        set => VisualText = value.Color;
    }

    public static SolidColorBrush TextBrush {
        get => new SolidColorBrush(Text);
        set => Text = value.Color;
    }

    public static SolidColorBrush UserInputBrush {
        get => new SolidColorBrush(UserInput);
        set => UserInput = value.Color;
    }

    public static SolidColorBrush InformationBrush {
        get => new SolidColorBrush(Information);
        set => Information = value.Color;
    }

    public static SolidColorBrush WarningBrush {
        get => new SolidColorBrush(Warning);
        set => Warning = value.Color;
    }

    public static SolidColorBrush CriticalBrush {
        get => new SolidColorBrush(Critical);
        set => Critical = value.Color;
    }

    public static SolidColorBrush SuccessBrush {
        get => new SolidColorBrush(Success);
        set => Success = value.Color;
    }


    public static Color HexToColor(string hex) {
        hex = hex.Replace("#", "");

        if (hex.Length == 6)
            hex = "FF" + hex;

        return Color.FromArgb(
            Convert.ToByte(hex.Substring(0, 2), 16), // A
            Convert.ToByte(hex.Substring(2, 2), 16), // R
            Convert.ToByte(hex.Substring(4, 2), 16), // G
            Convert.ToByte(hex.Substring(6, 2), 16)  // B
        );
    }

    public static SolidColorBrush generateRandomColor() {
        Random random = new Random();
        return new SolidColorBrush(Color.FromArgb((byte)random.Next(0, 256), (byte)random.Next(0, 256), (byte)random.Next(0, 256), 0xFF));
    }
    
}