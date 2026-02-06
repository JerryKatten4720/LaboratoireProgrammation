using System.Windows;
using System.Windows.Controls;

namespace LaboratoireProgrammation.Public.Model.Misc;

public partial class MemfyAgreement : UserControl {
    public MemfyAgreement() {
        InitializeComponent();
    }
    private MainWindow? ParentWindow => Window.GetWindow(this) as MainWindow;

    private void YesButton_OnClick(object sender, RoutedEventArgs e) {
        if (ParentWindow is { } window) {
            Visibility = Visibility.Collapsed; 
            window.MemfyAI();
        }
    }

    private void NoButton_OnClick(object sender, RoutedEventArgs e) {
        if (ParentWindow is { } window) {
            Visibility = Visibility.Collapsed;
            window.MainInit();
        }
    }
}