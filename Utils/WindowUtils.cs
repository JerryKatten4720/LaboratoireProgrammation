using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LaboratoireProgrammation.Utils;

public class WindowUtils {

    public static Window? getParentWindow(UserControl control) {
        var parent = VisualTreeHelper.GetParent(control);
        while (parent is not Window) { if (parent != null) parent = VisualTreeHelper.GetParent(parent); }
        return parent as Window;
    }
    
}