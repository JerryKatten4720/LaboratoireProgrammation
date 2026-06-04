using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace LaboratoireProgrammation.Project.ModernHospital;

public partial class DoctorCreationForm : Window {
    private readonly Dictionary<string, List<string>> _roleByCategory = new() {
        {
            "Corps Médical",
            new List<string> {
                "Médecin Généraliste", "Chirurgien", "Cardiologue", "Neurologue", "Pédiatre", "Radiologue",
                "Anesthésiste", "Psychiatre", "Urgentiste"
            }
        }, {
            "Personnel Soignant",
            new List<string> { "Infirmier", "Aide-Soignant", "Sage-Femme", "Kinésithérapeute", "Ergothérapeute" }
        }, {
            "Personnel Administratif",
            new List<string>
                { "Secrétaire Médical", "Gestionnaire", "Comptable", "Responsable RH", "Directeur Adjoint" }
        }, {
            "Personnel Technique",
            new List<string>
                { "Technicien Laboratoire", "Technicien Radiologie", "Maintenancier", "Cuisinier", "Agent d'Entretien" }
        }
    };

    public DoctorCreationForm() {
        InitializeComponent();
        CbCategorie.SelectedIndex = 0;
    }

    public Action? OnDoctorCreated { get; set; }
    public Action? OnCancelled { get; set; }

    private void CbCategorie_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        if (CbCategorie.SelectedItem is not ComboBoxItem selectedItem) return;

        var category = selectedItem.Content?.ToString() ?? "";
        CbRole.Items.Clear();

        if (_roleByCategory.ContainsKey(category)) {
            foreach (var role in _roleByCategory[category]) CbRole.Items.Add(new ComboBoxItem { Content = role });
            CbRole.SelectedIndex = 0;
        }
    }

    private void BtnCreate_Click(object sender, RoutedEventArgs e) {
        if (string.IsNullOrWhiteSpace(TbNom.Text)) {
            ShowValidationError("Veuillez entrer un nom.");
            return;
        }

        if (string.IsNullOrWhiteSpace(TbPrenom.Text)) {
            ShowValidationError("Veuillez entrer un prénom.");
            return;
        }

        if (CbRole.SelectedItem == null) {
            ShowValidationError("Veuillez sélectionner un rôle.");
            return;
        }

        if (!decimal.TryParse(TbSalaire.Text, out var salaire) || salaire <= 0) {
            ShowValidationError("Le salaire doit être un montant strictement positif.");
            return;
        }

        var nom = TbNom.Text.Trim();
        var prenom = TbPrenom.Text.Trim();
        var categorie = (CbCategorie.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
        var role = (CbRole.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
        var specialite = string.IsNullOrWhiteSpace(TbSpecialite.Text) ? null : TbSpecialite.Text.Trim();
        var shift = (CbShift.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Jour";

        try {
            var db = new DatabaseManager();
            db.CreateEmploye(nom, prenom, categorie, role, specialite, salaire, shift);

            var successPopup = PopupFactory.CreateConfirmationPopup(
                $"{prenom} {nom} a été créé avec succès!\n\nPoste: {role}\nSalaire: ${salaire:N0}/jour",
                "#00D4AA",
                () => { },
                () => { }
            );

            successPopup.Owner = this;
            successPopup.ShowDialog();

            OnDoctorCreated?.Invoke();
            Close();
        }
        catch (Exception ex) {
            ShowValidationError($"Erreur lors de la création: {ex.Message}");
        }
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e) {
        OnCancelled?.Invoke();
        Close();
    }

    private void ShowValidationError(string message) {
        var errorPopup = PopupFactory.CreateConfirmationPopup(
            message,
            "#FF4D6A",
            () => { },
            () => { }
        );

        errorPopup.Owner = this;
        errorPopup.ShowDialog();
    }
}