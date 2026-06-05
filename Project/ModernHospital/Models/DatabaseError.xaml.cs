using System.Windows;
using System.Windows.Controls;

namespace LaboratoireProgrammation.Project.ModernHospital;

public partial class DatabaseError : UserControl {
    public DatabaseError() {
        InitializeComponent();
    }
    
    private void OnCloseApp(object sender, RoutedEventArgs e) {
        Application.Current.Shutdown();
    }
}