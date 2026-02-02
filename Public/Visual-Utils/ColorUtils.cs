using System.Windows.Media;

namespace LaboratoireProgrammation.Public.Visual_Utils;

public class ColorUtils {

    public static Color FancyText = HexToColor("#fcfe4d");
    public static Color Text = HexToColor("#ededed");
    public static Color UserInput = HexToColor("#6ee9ff");
    public static Color Information = HexToColor("#fdff6e");
    public static Color Warning = HexToColor("#ff8f4a");
    public static Color Critical = HexToColor("#c7312c");
    public static Color Success = HexToColor("#29e62c");
    
    
    public static SolidColorBrush FancyTextBrush => new SolidColorBrush(FancyText);
    public static SolidColorBrush TextBrush => new SolidColorBrush(Text);
    public static SolidColorBrush UserInputBrush => new SolidColorBrush(UserInput);
    public static SolidColorBrush InformationBrush => new SolidColorBrush(Information);
    public static SolidColorBrush WarningBrush => new SolidColorBrush(Warning);
    public static SolidColorBrush CriticalBrush => new SolidColorBrush(Critical);
    public static SolidColorBrush SuccessBrush => new SolidColorBrush(Success);
    
    private static Color HexToColor(string hex) {
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
    
}