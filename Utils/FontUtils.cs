using System.Windows.Media;

namespace LaboratoireProgrammation.Utils;

public class FontUtils {

    public static FontFamily getRandomFont() {
        var installedFonts = Fonts.SystemFontFamilies.ToArray();
        var randomIndex = new Random().Next(installedFonts.Length);
        return installedFonts[randomIndex];
    }
    
}