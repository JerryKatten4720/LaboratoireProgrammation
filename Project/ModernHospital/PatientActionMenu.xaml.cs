using System.Windows;
using System.Windows.Controls;

namespace LaboratoireProgrammation.Project.ModernHospital;

public partial class PatientActionMenu : UserControl {
    public PatientActionMenu() {
        InitializeComponent();
    }

    public PatientActif? Patient { get; set; }
    public event Action<string>? ActionTriggered;

    private void Gueri_Click(object sender, RoutedEventArgs e) {
        ActionTriggered?.Invoke("Gueri");
    }

    private void Decede_Click(object sender, RoutedEventArgs e) {
        ActionTriggered?.Invoke("Decede");
    }

    private void Supprimer_Click(object sender, RoutedEventArgs e) {
        ActionTriggered?.Invoke("Supprimer");
    }

    private void Status_Click(object sender, RoutedEventArgs e) {
        if (sender is Button btn) ActionTriggered?.Invoke(btn.Tag.ToString()!);
    }

    private void Prescrire_Click(object sender, RoutedEventArgs e) {
        ActionTriggered?.Invoke("Prescrire");
    }

    private void Facture_Click(object sender, RoutedEventArgs e) {
        ActionTriggered?.Invoke("Facture");
    }
}