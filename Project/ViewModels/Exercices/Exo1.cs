using System.Windows;
using System.Windows.Controls;
using LaboratoireProgrammation.Project.Helpers;

namespace LaboratoireProgrammation.Project.ViewModels.Exercices;

public partial class Exo1 : UserControl {
    public Exo1() {
        InitializeComponent();
        ButtonOFF.Visibility = Visibility.Hidden;

        FontButton.Visibility = Visibility.Hidden;
        FontSizeButton.Visibility = Visibility.Hidden;
        BackgroundColorButton.Visibility = Visibility.Hidden;
    }

    private void ButtonClick_ON(object sender, RoutedEventArgs e) {
        ButtonOFF.Visibility = Visibility.Visible;
        ButtonON.Visibility = Visibility.Hidden;

        FontButton.Visibility = Visibility.Visible;
        FontSizeButton.Visibility = Visibility.Visible;
        BackgroundColorButton.Visibility = Visibility.Visible;

        Label.Content = "Boutons ! (ON)";
    }

    private void ButtonClick_OFF(object sender, RoutedEventArgs e) {
        ButtonOFF.Visibility = Visibility.Hidden;

        ButtonON.Visibility = Visibility.Visible;

        FontButton.Visibility = Visibility.Hidden;
        FontSizeButton.Visibility = Visibility.Hidden;
        BackgroundColorButton.Visibility = Visibility.Hidden;

        Label.Content = "Boutons ! (OFF)";
    }

    private void FontChange(object sender, RoutedEventArgs e) {
        ButtonON.FontFamily = FontHelper.getRandomFont();
        ButtonOFF.FontFamily = FontHelper.getRandomFont();
        FontButton.FontFamily = FontHelper.getRandomFont();
        FontSizeButton.FontFamily = FontHelper.getRandomFont();
        BackgroundColorButton.FontFamily = FontHelper.getRandomFont();
        Label.FontFamily = FontHelper.getRandomFont();
    }

    private void ColorChange(object sender, RoutedEventArgs e) {
        var window = WindowHelper.getParentWindow(this);
        window.Background = ColorHelper.generateRandomColor();
    }

    private void FontSizeChange(object sender, RoutedEventArgs e) {
        var changeCounts = 6;
        var randoms = new List<int>();

        for (var i = 0; i != changeCounts; i++) {
            var rand = new Random().Next(12, 33);
            randoms.Add(rand);
        }

        ButtonON.FontSize = randoms[0];
        ButtonOFF.FontSize = randoms[1];
        FontButton.FontSize = randoms[2];
        FontSizeButton.FontSize = randoms[3];
        BackgroundColorButton.FontSize = randoms[4];
        Label.FontSize = randoms[5];
    }
}