using System.Windows;
using System.Windows.Controls;
using LaboratoireProgrammation.Public.Visual_Utils;
using LaboratoireProgrammation.Utils;

namespace LaboratoireProgrammation.Public.Model.Exercices;

public partial class Exo___1 : UserControl
{
    public Exo___1() {
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
        ButtonON.FontFamily = FontUtils.getRandomFont();
        ButtonOFF.FontFamily = FontUtils.getRandomFont();
        FontButton.FontFamily = FontUtils.getRandomFont();
        FontSizeButton.FontFamily = FontUtils.getRandomFont();
        BackgroundColorButton.FontFamily = FontUtils.getRandomFont();
        Label.FontFamily = FontUtils.getRandomFont();
    }

    private void ColorChange(object sender, RoutedEventArgs e) {
        var window = WindowUtils.getParentWindow(this);
        window.Background = ColorUtils.generateRandomColor();
    }

    private void FontSizeChange(object sender, RoutedEventArgs e) {
        int changeCounts = 6;
        List<int> randoms = new List<int>();

        for (int i = 0; i != changeCounts; i++) {
            int rand = new Random().Next(12, 33);
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