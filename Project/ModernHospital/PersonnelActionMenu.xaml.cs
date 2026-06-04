using System;
using System.Windows;
using System.Windows.Controls;

namespace LaboratoireProgrammation.Project.ModernHospital;

public partial class PersonnelActionMenu : Window {
    public PersonnelActionMenu(Employe employe) {
        InitializeComponent();
        Employe = employe;
        Deactivated += (s, e) => {
            try { Close(); } catch {}
        };
    }

    public Employe Employe { get; }
    public event Action<string>? ActionTriggered;

    private void Status_Click(object sender, RoutedEventArgs e) {
        if (sender is Button btn) {
            ActionTriggered?.Invoke(btn.Tag.ToString()!);
            Close();
        }
    }

    private void Licencier_Click(object sender, RoutedEventArgs e) {
        ActionTriggered?.Invoke("Licencier");
        Close();
    }
}