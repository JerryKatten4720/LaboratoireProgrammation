using System;
using System.Windows;

namespace LaboratoireProgrammation.Project.ModernHospital;

public partial class AddMedicamentForm : Window {
    private readonly DatabaseManager _db;
    private readonly Medicament _medicament;

    public AddMedicamentForm(DatabaseManager db, Medicament medicament = null) {
        InitializeComponent();
        _db = db;
        _medicament = medicament;
        CbUnite.ItemsSource = _db.GetUnitesList();
        
        if (_medicament != null) {
            Title = "Modifier le Médicament";
            TbNom.Text = _medicament.Nom;
            TbDci.Text = _medicament.DCI;
            TbForme.Text = _medicament.Forme;
            TbStockActuel.Text = _medicament.StockActuel.ToString();
            TbStockMinimum.Text = _medicament.StockMinimum.ToString();
            TbPrix.Text = _medicament.PrixUnitaire.ToString();
            
            foreach (var item in CbUnite.Items) {
                var props = item.GetType().GetProperties();
                var idProp = props.FirstOrDefault(p => p.Name == "IdUniteMesure");
                if (idProp != null && (int)idProp.GetValue(item) == _medicament.IdUnite) {
                    CbUnite.SelectedItem = item;
                    break;
                }
            }
        } else if (CbUnite.Items.Count > 0) {
            CbUnite.SelectedIndex = 0;
        }
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e) {
        if (string.IsNullOrWhiteSpace(TbNom.Text)) {
            PopupFactory.ShowAlert(this, "Le nom est obligatoire.");
            return;
        }

        if (!int.TryParse(TbStockActuel.Text, out var sa) || sa < 0) {
            PopupFactory.ShowAlert(this, "Stock actuel invalide.");
            return;
        }

        if (!int.TryParse(TbStockMinimum.Text, out var sm) || sm < 0) {
            PopupFactory.ShowAlert(this, "Stock minimum invalide.");
            return;
        }

        if (!decimal.TryParse(TbPrix.Text, out var prix) || prix < 0) {
            PopupFactory.ShowAlert(this, "Prix unitaire invalide.");
            return;
        }

        if (CbUnite.SelectedValue is not int idUnite) {
            PopupFactory.ShowAlert(this, "Veuillez sélectionner une unité.");
            return;
        }

        try {
            if (_medicament == null) {
                _db.AddMedicament(TbNom.Text.Trim(), TbDci.Text.Trim(), TbForme.Text.Trim(), sa, sm, prix, idUnite);
            } else {
                _db.UpdateMedicament(_medicament.IdMedicament, TbNom.Text.Trim(), TbDci.Text.Trim(), TbForme.Text.Trim(), sa, sm, prix, idUnite);
            }
            DialogResult = true;
            Close();
        } catch (Exception ex) {
            PopupFactory.ShowAlert(this, "Erreur: " + ex.Message);
        }
    }
}
