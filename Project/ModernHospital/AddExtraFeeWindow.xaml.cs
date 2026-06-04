using System;
using System.Windows;

namespace LaboratoireProgrammation.Project.ModernHospital;

public partial class AddExtraFeeWindow : Window {
    private readonly int _idPatient;
    private readonly DatabaseManager _db = new();

    public AddExtraFeeWindow(int idPatient) {
        InitializeComponent();
        _idPatient = idPatient;
    }

    public string FeeName { get; private set; } = string.Empty;
    public decimal FeeAmount { get; private set; }

    private void BtnSave_Click(object sender, RoutedEventArgs e) {
        var name = TbNomFrais.Text.Trim();
        var amtText = TbMontant.Text.Trim();

        if (string.IsNullOrEmpty(name)) {
            MessageBox.Show("Veuillez saisir un nom pour le frais.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!decimal.TryParse(amtText, out decimal amount) || amount < 0) {
            MessageBox.Show("Veuillez saisir un montant valide.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        _db.AddExtraFee(_idPatient, name, amount);

        FeeName = name;
        FeeAmount = amount;
        DialogResult = true;
        Close();
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e) {
        DialogResult = false;
        Close();
    }
}
