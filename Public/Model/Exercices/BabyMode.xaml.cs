using System.Windows;
using System.Windows.Controls;
using LaboratoireProgrammation.Utils;

namespace LaboratoireProgrammation.Public.Model.Exercices;

public partial class BabyMode : UserControl {
    
    public BabyMode() {
        InitializeComponent();
    }
    private MainWindow? ParentWindow => Window.GetWindow(this) as MainWindow;
    private void Exo1(object sender, RoutedEventArgs e) {
        if (ParentWindow is { } window) {
            Visibility = Visibility.Collapsed; 
            window.FirstExo();
        }        
    }
    
    private void Exo1B(object sender, RoutedEventArgs e) {
        if (ParentWindow is { } window) {
            Visibility = Visibility.Collapsed; 
            window.BackToMain.Visibility = Visibility.Hidden;
            window.FirstExo(); 
        }
    }
    
    private void Exo2(object sender, RoutedEventArgs e) {
        if (ParentWindow is { } window) {
            Visibility = Visibility.Collapsed; 
            window.BackToMain.Visibility = Visibility.Hidden;
            window.SecondExo();
        }
    }
    
    private void Lab1(object sender, RoutedEventArgs e) {
        if (ParentWindow is { } window) {
            Visibility = Visibility.Collapsed; 
            window.BackToMain.Visibility = Visibility.Hidden;
            window.FirstLab();
        }
    }
    
    private void MemfyAI(object sender, RoutedEventArgs e) {
        if (ParentWindow is { } window) {
            Visibility = Visibility.Collapsed;
            window.BackToMain.Visibility = Visibility.Hidden;
            window.MemfyAI();
        }
    }
}