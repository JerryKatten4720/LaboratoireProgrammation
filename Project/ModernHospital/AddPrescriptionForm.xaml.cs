using System;
using System.Linq;
using System.Windows;

namespace LaboratoireProgrammation.Project.ModernHospital;

public partial class AddPrescriptionForm : Window {
    private readonly DatabaseManager _db;
    private readonly int _idPatient;
    private readonly int _idMedecin;

    public AddPrescriptionForm(DatabaseManager db, int idPatient, int idMedecin) {
        InitializeComponent();
        _db = db;
        _idPatient = idPatient;
        _idMedecin = idMedecin;

        var meds = _db.GetMedicaments().Where(m => m.StockActuel > 0).ToList();
        CbMedicament.ItemsSource = meds;
        if (meds.Count > 0) CbMedicament.SelectedIndex = 0;
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e) {
        if (CbMedicament.SelectedItem is not Medicament med) {
            PopupFactory.ShowAlert(this, "Veuillez sélectionner un médicament en stock.");
            return;
        }

        if (!int.TryParse(TbQuantite.Text, out var qte) || qte <= 0) {
            PopupFactory.ShowAlert(this, "Quantité invalide.");
            return;
        }

        if (qte > med.StockActuel) {
            PopupFactory.ShowAlert(this, $"Stock insuffisant (disponible : {med.StockActuel}).");
            return;
        }

        try {
            _db.AddPrescription(_idPatient, _idMedecin, med.IdMedicament, qte, TbPosologie.Text.Trim());
            DialogResult = true;
            Close();
        } catch (Exception ex) {
            PopupFactory.ShowAlert(this, "Erreur: " + ex.Message);
        }
    }
}
