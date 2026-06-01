using System.Windows;
using System.Windows.Controls;

namespace LaboratoireProgrammation.Project.ModernHospital;

public partial class PersonnelActionMenu : UserControl {
    public PersonnelActionMenu() {
        InitializeComponent();
    }

    public Employe? Employe { get; set; }
    public event Action<string>? ActionTriggered;

    private void Status_Click(object sender, RoutedEventArgs e) {
        if (sender is Button btn) ActionTriggered?.Invoke(btn.Tag.ToString()!);
    }

    private void Licencier_Click(object sender, RoutedEventArgs e) {
        ActionTriggered?.Invoke("Licencier");
    }
}