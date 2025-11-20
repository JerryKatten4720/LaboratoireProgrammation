using System.Windows;

namespace LaboratoireProgrammation.Utils;

public class SizeUtils {
    
    public static double getHeight() {
        if (Application.Current.MainWindow == null) return 0;
        return Application.Current.MainWindow.Height;
    }
    
    public static double getWidth() {
        if (Application.Current.MainWindow == null) return 0;
        return Application.Current.MainWindow.Width;
    }
}