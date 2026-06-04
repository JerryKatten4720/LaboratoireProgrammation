using System;
using System.Linq;
using System.Windows;

namespace LaboratoireProgrammation.Project.ModernHospital;

public partial class AssignDoctorWindow : Window {
    private readonly DatabaseManager _db;
    private readonly int _idPatient;

    public AssignDoctorWindow(DatabaseManager db, int idPatient) {
        InitializeComponent();
        _db = db;
        _idPatient = idPatient;

        var docs = _db.GetPersonnel().Where(p => p.Categorie == "Corps Médical").ToList();
        CbMedecin.ItemsSource = docs;
        if (docs.Count > 0) CbMedecin.SelectedIndex = 0;
    }

    private void BtnConfirm_Click(object sender, RoutedEventArgs e) {
        if (CbMedecin.SelectedItem is not Employe doc) {
            PopupFactory.ShowAlert(this, "Veuillez sélectionner un médecin.");
            return;
        }

        try {
            _db.UpdatePatientDoctor(_idPatient, doc.IdEmploye);
            DialogResult = true;
            Close();
        } catch (Exception ex) {
            PopupFactory.ShowAlert(this, "Erreur: " + ex.Message);
        }
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e) {
        DialogResult = false;
        Close();
    }
}
