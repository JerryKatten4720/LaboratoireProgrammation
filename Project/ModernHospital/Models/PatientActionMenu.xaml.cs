using System;
using System.Windows;
using System.Windows.Controls;

namespace LaboratoireProgrammation.Project.ModernHospital;

public partial class PatientActionMenu : Window {
    public PatientActionMenu(PatientActif patient) {
        InitializeComponent();
        Patient = patient;
        Deactivated += (s, e) => {
            try { Close(); } catch {}
        };
    }

    public PatientActif Patient { get; }
    public event Action<string>? ActionTriggered;

    private void Gueri_Click(object sender, RoutedEventArgs e) {
        ActionTriggered?.Invoke("Gueri");
        Close();
    }

    private void Decede_Click(object sender, RoutedEventArgs e) {
        ActionTriggered?.Invoke("Decede");
        Close();
    }

    private void Supprimer_Click(object sender, RoutedEventArgs e) {
        ActionTriggered?.Invoke("Supprimer");
        Close();
    }

    private void Status_Click(object sender, RoutedEventArgs e) {
        if (sender is Button btn) {
            ActionTriggered?.Invoke(btn.Tag.ToString()!);
            Close();
        }
    }

    private void Prescrire_Click(object sender, RoutedEventArgs e) {
        ActionTriggered?.Invoke("Prescrire");
        Close();
    }

    private void AssignerMedecin_Click(object sender, RoutedEventArgs e) {
        ActionTriggered?.Invoke("AssignerMedecin");
        Close();
    }

    private void Facture_Click(object sender, RoutedEventArgs e) {
        ActionTriggered?.Invoke("Facture");
        Close();
    }

    private void FraisSupplementaires_Click(object sender, RoutedEventArgs e) {
        ActionTriggered?.Invoke("FraisSupplementaires");
        Close();
    }
}