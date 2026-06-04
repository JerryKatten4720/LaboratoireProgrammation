using System;
using System.Linq;
using System.Windows;

namespace LaboratoireProgrammation.Project.ModernHospital {
    public partial class ViewReservationsWindow : Window {
        private readonly DatabaseManager _db;

        public ViewReservationsWindow(DatabaseManager db) {
            InitializeComponent();
            _db = db;
            LoadReservations();
        }

        private void LoadReservations() {
            var patients = _db.GetPatients();
            LvReservations.ItemsSource = patients.Where(p => p.Statut == "Réservé").ToList();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }

        private void BtnAdmettre_Click(object sender, RoutedEventArgs e) {
            if (LvReservations.SelectedItem is PatientActif patient) {
                try {
                    _db.AdmitReservedPatient(patient.IdPatient);
                    PopupFactory.ShowAlert(this, $"Le patient {patient.Nom} a été admis avec succès.", "#00d4aa");
                    DialogResult = true;
                }
                catch (Exception ex) {
                    PopupFactory.ShowAlert(this, $"Erreur lors de l'admission : {ex.Message}", "#FF4D6A");
                }
            } else {
                PopupFactory.ShowAlert(this, "Veuillez sélectionner une réservation à admettre.");
            }
        }
    }
}
