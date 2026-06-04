using System;
using System.Windows;
using System.Windows.Input;

namespace LaboratoireProgrammation.Project.ModernHospital;

public partial class AdminPasswordWindow : Window {
    public AdminPasswordWindow() {
        InitializeComponent();
        PbPassword.Focus();
    }

    private void BtnConfirm_Click(object sender, RoutedEventArgs e) {
        ValidatePassword();
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e) {
        DialogResult = false;
        Close();
    }

    private void PbPassword_KeyDown(object sender, KeyEventArgs e) {
        if (e.Key == Key.Enter) {
            ValidatePassword();
        }
    }

    private void ValidatePassword() {
        if (PbPassword.Password == "@anto.cldl") {
            DialogResult = true;
            Close();
        } else {
            MessageBox.Show("Mot de passe incorrect.", "Accès Refusé", MessageBoxButton.OK, MessageBoxImage.Error);
            PbPassword.Clear();
            PbPassword.Focus();
        }
    }
}
