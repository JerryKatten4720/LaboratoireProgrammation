using System.Windows;
using System.Windows.Controls;

namespace LaboratoireProgrammation.Public.Model.Exercices;

public partial class Exo___1b : UserControl {
    private bool _interverted = false;
    
    public Exo___1b() {
        InitializeComponent();

    }
    private void ButtonClickIntervert(object sender, RoutedEventArgs e) {
        if (Grid.GetColumn(IMG1) == 0) {
            Grid.SetColumn(IMG1, 2);
            Grid.SetColumn(IMG2, 0);
            _interverted = true;
        } else {
            Grid.SetColumn(IMG1, 0);
            Grid.SetColumn(IMG2, 2);
            _interverted = false;
        }
    }
    private void ButtonClickSwitch1(object sender, RoutedEventArgs e) {
        if (_interverted) {
            if (IMG2.Visibility == Visibility.Hidden) IMG2.Visibility = Visibility.Visible;
            else IMG2.Visibility = Visibility.Hidden;
        } else {
            if (IMG1.Visibility == Visibility.Hidden) IMG1.Visibility = Visibility.Visible;
            else IMG1.Visibility = Visibility.Hidden;
        }
    }
    private void ButtonClickSwitch2(object sender, RoutedEventArgs e) {
        if (!_interverted) {
            if (IMG2.Visibility == Visibility.Hidden) IMG2.Visibility = Visibility.Visible;
            else IMG2.Visibility = Visibility.Hidden;
        } else {
            if (IMG1.Visibility == Visibility.Hidden) IMG1.Visibility = Visibility.Visible;
            else IMG1.Visibility = Visibility.Hidden;
        }
    }
}