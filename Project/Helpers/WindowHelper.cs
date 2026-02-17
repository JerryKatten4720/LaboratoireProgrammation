using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LaboratoireProgrammation.Project.ViewModels.Menu;

namespace LaboratoireProgrammation.Project.Helpers;

public class WindowHelper {
    public static Window getParentWindow(UserControl control) {
        var parent = VisualTreeHelper.GetParent(control);
        while (parent is not Window) if (parent != null) parent = VisualTreeHelper.GetParent(parent);
        return parent as Window;
    }
    
    public static MainWindow getParentMainWindow(UserControl control) {
        var parent = VisualTreeHelper.GetParent(control);
        while (parent is not Window) if (parent != null) parent = VisualTreeHelper.GetParent(parent);
        return parent as MainWindow;
    }
    
    public static T FindParent<T>(DependencyObject child) where T : DependencyObject {
        DependencyObject parentObject = VisualTreeHelper.GetParent(child);
        if (parentObject == null) return null;

        if (parentObject is T parent) return parent;
        else return FindParent<T>(parentObject);
    }

}