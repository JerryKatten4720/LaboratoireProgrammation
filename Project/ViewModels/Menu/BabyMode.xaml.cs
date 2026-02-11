using System.Windows;
using System.Windows.Controls;
using LaboratoireProgrammation.Project.Models.Menu;
using LaboratoireProgrammation.Project.Models.Miscellaneous;

namespace LaboratoireProgrammation.Project.ViewModels.Menu;

public partial class BabyMode : UserControl {
    public BabyMode() {
        InitializeComponent();
    }

    private MainWindow? ParentWindow => Window.GetWindow(this) as MainWindow;

    private void Exo1(object sender, RoutedEventArgs e) {
        if (ParentWindow is { } window) {
            Visibility = Visibility.Collapsed;
            RunExo1.Run(window);
        }
    }

    private void Exo1B(object sender, RoutedEventArgs e) {
        if (ParentWindow is { } window) {
            Visibility = Visibility.Collapsed;
            RunExo1.Run(window);
        }
    }

    private void Exo2(object sender, RoutedEventArgs e) {
        if (ParentWindow is { } window) {
            Visibility = Visibility.Collapsed;
            RunExo2.Run(window);
        }
    }

    private void Lab1(object sender, RoutedEventArgs e) {
        if (ParentWindow is { } window) {
            Visibility = Visibility.Collapsed;
            RunLab1.Run(window);
        }
    }

    private void MemfyAI(object sender, RoutedEventArgs e) {
        if (ParentWindow is { } window) {
            Visibility = Visibility.Collapsed;
            RunMemfy.Run(window);
        }
    }
}