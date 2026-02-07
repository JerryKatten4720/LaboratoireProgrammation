using System.Windows;

namespace LaboratoireProgrammation.Project.Helpers;

public class SizeHelper {
    public static string Separator;

    public static double getHeight() {
        if (Application.Current.MainWindow == null) return 0;
        return Application.Current.MainWindow.Height;
    }

    public static double getWidth() {
        if (Application.Current.MainWindow == null) return 0;
        return Application.Current.MainWindow.Width;
    }

    public static async void setFullscreen(Window window) {
        if (window.WindowState != WindowState.Maximized)
            window.WindowState = WindowState.Maximized;
        else return;
        regenerateSeparator();
        await Task.Delay(200);
    }

    public static void regenerateSeparator() {
        Separator = "";
        for (var i = 1; i < getWidth() / 2; i++) Separator += "- ";
    }
}