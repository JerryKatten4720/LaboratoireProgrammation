using System.Windows.Media;

namespace LaboratoireProgrammation.Project.Helpers;

public class FontHelper {
    public static FontFamily getRandomFont() {
        var installedFonts = Fonts.SystemFontFamilies.ToArray();
        var randomIndex = new Random().Next(installedFonts.Length);
        return installedFonts[randomIndex];
    }
}