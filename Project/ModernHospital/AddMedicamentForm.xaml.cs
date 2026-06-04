using System;
using System.Windows;

namespace LaboratoireProgrammation.Project.ModernHospital;

public partial class AddMedicamentForm : Window {
    private readonly DatabaseManager _db;

    public AddMedicamentForm(DatabaseManager db) {
        InitializeComponent();
        _db = db;
        CbUnite.ItemsSource = _db.GetUnitesList();
        if (CbUnite.Items.Count > 0) CbUnite.SelectedIndex = 0;
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e) {
        if (string.IsNullOrWhiteSpace(TbNom.Text)) {
            MessageBox.Show("Le nom est obligatoire.");
            return;
        }

        if (!int.TryParse(TbStockActuel.Text, out var sa) || sa < 0) {
            MessageBox.Show("Stock actuel invalide.");
            return;
        }

        if (!int.TryParse(TbStockMinimum.Text, out var sm) || sm < 0) {
            MessageBox.Show("Stock minimum invalide.");
            return;
        }

        if (!decimal.TryParse(TbPrix.Text, out var prix) || prix < 0) {
            MessageBox.Show("Prix unitaire invalide.");
            return;
        }

        if (CbUnite.SelectedValue is not int idUnite) {
            MessageBox.Show("Veuillez sélectionner une unité.");
            return;
        }

        try {
            _db.AddMedicament(TbNom.Text.Trim(), TbDci.Text.Trim(), TbForme.Text.Trim(), sa, sm, prix, idUnite);
            DialogResult = true;
            Close();
        } catch (Exception ex) {
            MessageBox.Show("Erreur: " + ex.Message);
        }
    }
}
