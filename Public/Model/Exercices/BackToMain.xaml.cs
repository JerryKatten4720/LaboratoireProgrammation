using System.Windows;
using System.Windows.Controls;

namespace LaboratoireProgrammation.Public.Model.Exercices;

public partial class BackToMain : UserControl {
    public BackToMain() { InitializeComponent(); }
    private MainWindow? ParentWindow => Window.GetWindow(this) as MainWindow;
    private void BackToMenu(object sender, RoutedEventArgs e) {
        if (ParentWindow is { } window) {
            Visibility = Visibility.Collapsed;
        }
    }
    
}