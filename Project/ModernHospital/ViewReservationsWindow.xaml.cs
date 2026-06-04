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
                    int dayTime = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalDays;
                    _db.ExecuteNonQuery($"UPDATE Patients_Actifs SET Statut = 'En Attente', DateEntree = {dayTime} WHERE IdPatient = {patient.IdPatient}");
                    
                    int litId = 0;
                    _db.ExecuteCommand($"SELECT IdLit FROM Patients_Actifs WHERE IdPatient = {patient.IdPatient}", cmd => {
                        using var reader = cmd.ExecuteReader();
                        if (reader.Read() && !reader.IsDBNull(0)) {
                            litId = reader.GetInt32(0);
                        }
                    });

                    if (litId > 0) {
                        _db.ExecuteNonQuery($"UPDATE Lit SET Statut = 'Occupé' WHERE IdLit = {litId}");
                    }

                    _db.ExecuteNonQuery($"UPDATE Reservation SET Statut = 'Terminée' WHERE IdPatient = {patient.IdPatient} AND Statut = 'Active'");

                    MessageBox.Show($"Le patient {patient.Nom} a été admis avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                }
                catch (Exception ex) {
                    MessageBox.Show($"Erreur lors de l'admission : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            } else {
                MessageBox.Show("Veuillez sélectionner une réservation à admettre.", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
