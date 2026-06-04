using System;
using System.Linq;
using System.Windows;

namespace LaboratoireProgrammation.Project.ModernHospital;

public partial class AdmitWaitingRoomPatientWindow : Window {
    private readonly DatabaseManager _db;
    private readonly WaitingRoomPatient _patient;
    private readonly CasClinique? _disease;

    public AdmitWaitingRoomPatientWindow(DatabaseManager db, WaitingRoomPatient patient) {
        InitializeComponent();
        _db = db;
        _patient = patient;

        _disease = _db.GetCasCliniques().FirstOrDefault(c => c.IdCas == _patient.IdMaladie);
        string diseaseName = _disease?.Maladie ?? "Maladie Inconnue";
        string reqUnit = _disease?.UniteRequise ?? "Aucune";
        TbPatientInfo.Text = $"Patient: {_patient.NomComplet} | Maladie: {diseaseName} (Unité requise: {reqUnit})";

        
        var docs = _db.GetPersonnel().Where(p => p.Categorie == "Corps Médical").ToList();
        CbMedecin.ItemsSource = docs;
        if (docs.Count > 0) CbMedecin.SelectedIndex = 0;

        
        var beds = _db.GetLitsLibresPourUnite(_disease?.UniteRequise);
        CbLit.ItemsSource = beds;
        if (beds.Count > 0) {
            CbLit.SelectedIndex = 0;
        } else {
            MessageBox.Show($"Aucun lit libre dans l'unité requise ({reqUnit}) !", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void BtnConfirm_Click(object sender, RoutedEventArgs e) {
        if (CbMedecin.SelectedItem is not Employe doc) {
            PopupFactory.ShowAlert(this, "Veuillez sélectionner un médecin.");
            return;
        }

        if (CbLit.SelectedItem is not LitDisponible lit) {
            PopupFactory.ShowAlert(this, "Veuillez sélectionner un lit.");
            return;
        }

        if (_disease == null) {
            PopupFactory.ShowAlert(this, "Données de la maladie introuvables.");
            return;
        }

        try {
            string nomComplet = $"{_patient.Prenom} {_patient.Nom}";
            
            _db.AdmitPatient(nomComplet, _patient.IdMaladie, lit.IdLit, doc.IdEmploye, "Standard", _disease.TempsTraitementHeures);
            
            
            int newPatientId = _db.ExecuteScalar<int>("SELECT MAX(IdPatient) FROM Patients_Actifs");
            
            
            float tempsSansMedecin = _patient.TempsAttenteMinutes / 60.0f;
            _db.UpdatePatientTempsSansMedecin(newPatientId, tempsSansMedecin);
            
            
            _db.RemoveWaitingRoomPatient(_patient.IdPatientAttente);
            
            
            _db.IncrementPatientsAdmis();

            DialogResult = true;
            Close();
        } catch (Exception ex) {
            PopupFactory.ShowAlert(this, "Erreur lors de l'admission : " + ex.Message);
        }
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e) {
        DialogResult = false;
        Close();
    }
}
