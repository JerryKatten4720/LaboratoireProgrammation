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
            RunSubprograms.RunSub(RunSubprograms.Subprograms.Exercice1, window);
        }
    }

    private void Exo2(object sender, RoutedEventArgs e) {
        if (ParentWindow is { } window) {
            Visibility = Visibility.Collapsed;
            RunSubprograms.RunSub(RunSubprograms.Subprograms.Exercice2, window);
        }
    }

    private void Exo3(object sender, RoutedEventArgs e) {
        if (ParentWindow is { } window) {
            Visibility = Visibility.Collapsed;
            RunSubprograms.RunSub(RunSubprograms.Subprograms.Exercice3, window);
        }
    }

    private void Exo4(object sender, RoutedEventArgs e) {
        if (ParentWindow is { } window) {
            Visibility = Visibility.Collapsed;
            RunSubprograms.RunSub(RunSubprograms.Subprograms.Exercice4, window);
        }
    }

    private void Exo5(object sender, RoutedEventArgs e) {
        if (ParentWindow is { } window) {
            Visibility = Visibility.Collapsed;
            RunSubprograms.RunSub(RunSubprograms.Subprograms.Exercice5, window);
        }
    }

    private void Exo6(object sender, RoutedEventArgs e) {
        if (ParentWindow is { } window) {
            Visibility = Visibility.Collapsed;
            RunSubprograms.RunSub(RunSubprograms.Subprograms.Exercice6, window);
        }
    }

    private void Exo7(object sender, RoutedEventArgs e) {
        if (ParentWindow is { } window) {
            Visibility = Visibility.Collapsed;
            RunSubprograms.RunSub(RunSubprograms.Subprograms.Exercice7, window);
        }
    }

    private void Lab1(object sender, RoutedEventArgs e) {
        if (ParentWindow is { } window) {
            Visibility = Visibility.Collapsed;
            RunSubprograms.RunSub(RunSubprograms.Subprograms.Labo1, window);
        }
    }

    private void Exo8(object sender, RoutedEventArgs e) {
        if (ParentWindow is { } window) {
            Visibility = Visibility.Collapsed;
            RunSubprograms.RunSub(RunSubprograms.Subprograms.Exercice8, window);
        }
    }

    private void MemfyAI(object sender, RoutedEventArgs e) {
        if (ParentWindow is { } window) {
            Visibility = Visibility.Collapsed;
            RunMemfy.Run(window);
        }
    }

    private void ESP8266(object sender, RoutedEventArgs e) {
        if (ParentWindow is { } window) {
            Visibility = Visibility.Collapsed;
            RunSubprograms.RunSub(RunSubprograms.Subprograms.ESP8266, window);
        }
    }


    private void OverseerWars(object sender, RoutedEventArgs e) {
        if (ParentWindow is { } window) {
            Visibility = Visibility.Collapsed;
            RunSubprograms.RunSub(RunSubprograms.Subprograms.OverseerWars, window);
        }
    }

    private void Exo9(object sender, RoutedEventArgs e) {
        if (ParentWindow is { } window) {
            Visibility = Visibility.Collapsed;
            RunSubprograms.RunSub(RunSubprograms.Subprograms.Exercice9, window);
        }
    }
}