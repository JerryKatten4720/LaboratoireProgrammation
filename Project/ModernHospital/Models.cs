using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace LaboratoireProgrammation.Project.ModernHospital;

public class ChambreGroup {
    public int IdChambre { get; set; }
    public string NumeroChambre { get; set; } = "";
    public string TypeChambre { get; set; } = "";
    public string Unite { get; set; } = "";
    public ObservableCollection<LitInventaire> Lits { get; set; } = new();
}

public static class DragDropHelper {
    public static void HandleAutoScroll(ScrollViewer sv, DragEventArgs e, double t = 40, double o = 10) {
        var y = e.GetPosition(sv).Y;
        if (y < t) sv.ScrollToVerticalOffset(sv.VerticalOffset - o);
        else if (y > sv.ActualHeight - t) sv.ScrollToVerticalOffset(sv.VerticalOffset + o);
    }
}

public class HospitalInfo {
    public int IdHopital { get; set; }
    public string Nom { get; set; } = "";
    public string DirecteurGeneral { get; set; } = "";
    public string DirecteurMedical { get; set; } = "";
    public decimal Budget { get; set; }
    public int Reputation { get; set; }
    public int JourSimulation { get; set; }
    public string HeureSimulation { get; set; } = "08:00:00";
    public decimal MontantEmprunt { get; set; }
    public decimal TauxInteret { get; set; }
}

public class Employe {
    public int IdEmploye { get; set; }
    public int? IdUnite { get; set; }
    public string Nom { get; set; } = "";
    public string Prenom { get; set; } = "";
    public string Categorie { get; set; } = "";
    public string RoleExact { get; set; } = "";
    public string? Specialite { get; set; }
    public int Experience { get; set; }

    public decimal SalaireJour { get; set; }
    public string Shift { get; set; } = "Jour";
    public string Statut { get; set; } = "En poste";
    public int MinutesTravaillees { get; set; }
    public int MinutesEnPause { get; set; }
    public int MinutesHorsPoste { get; set; }
    public int ProchainePauseMinutes { get; set; } = 120;
    public string NomComplet => $"{Prenom} {Nom}";
    public string StatutAffichage {
        get {
            if (Statut == "En poste") {
                int remaining = 600 - MinutesTravaillees;
                if (remaining < 0) remaining = 0;
                return $"En poste ({remaining / 60}h {remaining % 60:D2}m)";
            }
            if (Statut == "Hors poste") {
                int remaining = 720 - MinutesHorsPoste;
                if (remaining < 0) remaining = 0;
                return $"Hors poste ({remaining / 60}h {remaining % 60:D2}m)";
            }
            return Statut;
        }
    }
}

public class Candidat {
    public int IdCandidat { get; set; }
    public string Nom { get; set; } = "";
    public string Prenom { get; set; } = "";
    public string Categorie { get; set; } = "";
    public string RoleExact { get; set; } = "";
    public string? Specialite { get; set; }
    public decimal SalaireJour { get; set; }
    public decimal PrimeEmbauche { get; set; }
    public string NomComplet => $"{Prenom} {Nom}";
    public decimal CoutTotal => PrimeEmbauche + SalaireJour;
}

public class PatientActif {
    public int IdPatient { get; set; }
    public string Nom { get; set; } = "";
    public string Maladie { get; set; } = "";
    public string Statut { get; set; } = "";
    public string Classe { get; set; } = "";
    public string? NumeroChambre { get; set; }
    public string? MedecinEnCharge { get; set; }
    public int SanteActuelle { get; set; }
    public int Satisfaction { get; set; }
    public string? NumeroLit { get; set; }
    public float TempsTraitementRestant { get; set; }
    public int IdMaladie { get; set; }
    public int? IdLit { get; set; }
    public int? IdMedecinAssigne { get; set; }
    public string WarningIcon => string.IsNullOrEmpty(NumeroChambre) || string.IsNullOrEmpty(MedecinEnCharge) ? "⚠️" : "";
    public int? DateEntree { get; set; }
    public int? IdFacture { get; set; }
    public float TempsTraitementTotal { get; set; }
    public decimal EstimatedProfit { get; set; }
    public float TempsSansMedecin { get; set; }
    public string TempsRestantAffichage {
        get {
            if (Statut == "Réservé") return "Non démarré";
            int totalMinutes = (int)(TempsTraitementRestant * 60);
            if (totalMinutes < 0) totalMinutes = 0;
            return $"{totalMinutes / 60}h {totalMinutes % 60:D2}m";
        }
    }
}

public class LitInventaire {
    public int IdLit { get; set; }
    public int IdChambre { get; set; }
    public string NumeroLit { get; set; } = "";
    public string NumeroChambre { get; set; } = "";
    public string TypeChambre { get; set; } = "";
    public string Unite { get; set; } = "";
    public string Statut { get; set; } = "";
}

public class Medicament {
    public int IdMedicament { get; set; }
    public string Nom { get; set; } = "";
    public string DCI { get; set; } = "";
    public string Forme { get; set; } = "";
    public int StockActuel { get; set; }
    public int StockMinimum { get; set; }
    public decimal PrixUnitaire { get; set; }
    public int IdUnite { get; set; }
    public string? MaladiesCibles { get; set; }
    public string? MaladiesIncompatibles { get; set; }
    public int TempsLivraisonBase { get; set; } = 120;
    public int QuantitePrescriptionDefaut { get; set; } = 1;
}

public class Prescription {
    public int IdPrescription { get; set; }
    public int IdPatient { get; set; }
    public int IdMedecin { get; set; }
    public int IdMedicament { get; set; }
    public int Quantite { get; set; }
    public string Posologie { get; set; } = "";
    public int DatePrescription { get; set; }
    public string Statut { get; set; } = "En cours";
}

public class Facture {
    public int IdFacture { get; set; }
    public string CodeFacture { get; set; } = "";
    public int IdPatient { get; set; }
    public int JourEmission { get; set; }
    public decimal MontantChambre { get; set; }
    public decimal MontantSoins { get; set; }
    public decimal MontantMedicaments { get; set; }
    public decimal MontantTotal { get; set; }
    public string Statut { get; set; } = "Générée";
    public string PatientNom { get; set; } = "";
    
    public string? CheminFichier {
        get {
            var baseDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Factures");
            if (System.IO.Directory.Exists(baseDir)) {
                var nomComplet = PatientNom ?? "";
                var parts = nomComplet.Split(' ', 2, System.StringSplitOptions.RemoveEmptyEntries);
                var nom = parts.Length > 0 ? parts[0] : "inconnu";
                var prenom = parts.Length > 1 ? parts[1] : "inconnu";

                var safeNom = nom.ToLowerInvariant();
                safeNom = System.Text.RegularExpressions.Regex.Replace(safeNom, @"[^\w]", "");
                var safePrenom = prenom.ToLowerInvariant();
                safePrenom = System.Text.RegularExpressions.Regex.Replace(safePrenom, @"[^\w]", "");

                var patientDir = System.IO.Path.Combine(baseDir, $"{safeNom}_{safePrenom}");
                if (System.IO.Directory.Exists(patientDir)) {
                    if (!string.IsNullOrEmpty(CodeFacture)) {
                        var files = System.IO.Directory.GetFiles(patientDir, $"facture-{CodeFacture}-*.html");
                        if (files.Length > 0) return files[0];
                    }
                    var idFiles = System.IO.Directory.GetFiles(patientDir, $"facture-{IdFacture}-*.html");
                    if (idFiles.Length > 0) return idFiles[0];
                }

                if (!string.IsNullOrEmpty(CodeFacture)) {
                    var fbFiles = System.IO.Directory.GetFiles(baseDir, $"facture-{CodeFacture}-*.html", System.IO.SearchOption.AllDirectories);
                    if (fbFiles.Length > 0) return fbFiles[0];
                }
                var fbIdFiles = System.IO.Directory.GetFiles(baseDir, $"facture-{IdFacture}-*.html", System.IO.SearchOption.AllDirectories);
                if (fbIdFiles.Length > 0) return fbIdFiles[0];
            }
            return null;
        }
    }
}

public class FactureDisplayItem {
    public string CodeFacture { get; set; } = "";
    public string PatientNom { get; set; } = "";
    public int JourEmission { get; set; }
    public decimal MontantTotal { get; set; }
    public string Statut { get; set; } = "";
    public string Type { get; set; } = "";
    public string Description { get; set; } = "";
    public int IdRealFacture { get; set; }
    public string CheminFichier { get; set; } = "";
}

public class LigneFacture {
    public int IdLigne { get; set; }
    public int IdFacture { get; set; }
    public string TypePrestation { get; set; } = "";
    public string Description { get; set; } = "";
    public int Quantite { get; set; }
    public decimal PrixUnitaire { get; set; }
    public decimal SousTotal { get; set; }
}

public class Reservation {
    public int IdReservation { get; set; }
    public string Chambre { get; set; } = "";
    public string Lit { get; set; } = "";
    public string Patient { get; set; } = "";
    public string Medecin { get; set; } = "";
    public int DateEntree { get; set; }
    public int? DateSortie { get; set; }
    public string Statut { get; set; } = "";
    public int DureeSejour => DateSortie.HasValue ? DateSortie.Value - DateEntree : 0;
}

public class HitParadeEntry {
    public int Rang { get; set; }
    public int IdEmploye { get; set; }
    public string Nom { get; set; } = "";
    public string Specialite { get; set; } = "";
    public int PatientsTraites { get; set; }
    public decimal TauxGuerison { get; set; }
    public decimal RevenuGenere { get; set; }
}

public static class PatientHelper {
    public static bool HandlePatientStatusChange(DatabaseManager db, PatientActif p, string s) {
        try {
            if (p == null) return false;
            db.UpdatePatientStatut(p.IdPatient, s);
            
            if (s == "Guéri" || s == "Décédé") {
                int idFacture = db.GenerateFacture(p.IdPatient);
                var facture = db.GetFactures().FirstOrDefault(f => f.IdFacture == idFacture);
                var lignes = db.GetLignesFacture(idFacture);
                var hosp = db.GetHospital();
                if (facture != null && hosp != null) {
                    Services.FactureGenerator.ExporterFactureHtml(facture, p, hosp, lignes, p.MedecinEnCharge ?? "Docteur Inconnu");
                }
                db.CloseReservation(p.IdPatient);
                if (s == "Décédé") {
                    int? coldBedId = db.GetFreeColdChamberBed();
                    if (coldBedId.HasValue) {
                        db.TransferToColdChamber(p.IdPatient, coldBedId.Value);
                    } else {
                        db.ReleaseBed(p.IdPatient);
                        db.IncrementAccumulatedFuneralFees();
                    }
                } else {
                    db.ReleaseBed(p.IdPatient);
                }
            }
            return true;
        } catch (System.Exception ex) {
            try {
                System.Windows.MessageBox.Show($"Erreur dans HandlePatientStatusChange: {ex.Message}\n{ex.StackTrace}", "Erreur critique", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            } catch {}
            return false;
        }
    }
}

public class Transaction {
    public int IdTransaction { get; set; }
    public int JourSimulation { get; set; }
    public string TypeTransaction { get; set; } = "";
    public decimal Montant { get; set; }
    public string Description { get; set; } = "";
    public bool IsGain => Montant > 0;
}

public class CasClinique {
    public int IdCas { get; set; }
    public string Maladie { get; set; } = "";
    public string Symptomes { get; set; } = "";
    public float TempsTraitementHeures { get; set; }
    public decimal RevenuPatient { get; set; }
    public int RisqueErreurMedicale { get; set; }
    public float TauxRemission { get; set; }
    public string SpecialisteTraitement { get; set; } = "";
    public string? UniteRequise { get; set; }
    public decimal CoutLogistique { get; set; }
}

public class LitDisponible {
    public int IdLit { get; set; }
    public string Unite { get; set; } = "";
    public string NumeroChambre { get; set; } = "";
    public string TypeChambre { get; set; } = "";
    public string NumeroLit { get; set; } = "";
}

public class WaitingRoomPatient {
    public int IdPatientAttente { get; set; }
    public string Nom { get; set; } = "";
    public string Prenom { get; set; } = "";
    public int IdMaladie { get; set; }
    public int TempsAttenteMinutes { get; set; }
    public string Symptomes { get; set; } = "";
    public string NomComplet => $"{Prenom} {Nom}";
    public string TempsAttenteAffichage => $"{TempsAttenteMinutes / 60}h {TempsAttenteMinutes % 60:D2}m";
}

public class VirtualQueuePatient {
    public int IdVirtual { get; set; }
    public string Nom { get; set; } = "";
    public int IdMaladie { get; set; }
    public int? IdMedecin { get; set; }
    public string Classe { get; set; } = "";
    public int DateEntree { get; set; }
}

public class Unite {
    public int IdUnite { get; set; }
    public string Nom { get; set; } = "";
    public decimal CoutEntretienJour { get; set; }
}

public class DashboardStats {
    public int TotalPatientsActifs { get; set; }
    public int TotalPersonnel { get; set; }
    public int LitsLibres { get; set; }
    public int LitsOccupes { get; set; }
    public decimal RevenuJour { get; set; }
    public decimal DepensesJour { get; set; }
    public int PatientsEnAttente { get; set; }
    public int PatientsEnTraitement { get; set; }
    public int PatientsGueris { get; set; }
    public int PatientsDeces { get; set; }
}

public class EventEntry {
    public string Message { get; set; } = "";
    public string Category { get; set; } = "Logistique";
}

public class CartItem {
    public Medicament Medicament { get; set; } = null!;
    public int Quantite { get; set; }
    public string Nom => Medicament.Nom;
    public string DCI => Medicament.DCI;
    public decimal PrixAchatUnitaire => Medicament.PrixUnitaire * 0.80m;
    public decimal PrixAchatTotal => PrixAchatUnitaire * Quantite;
    
    public string PrixAchatUnitaireAffichage => $"$ {PrixAchatUnitaire:N2}";
    public string PrixAchatTotalAffichage => $"$ {PrixAchatTotal:N2}";
}

public class MedicamentCommande {
    public int IdCommande { get; set; }
    public int IdMedicament { get; set; }
    public int Quantite { get; set; }
    public int TempsLivraisonRestant { get; set; }
    public decimal PrixAchat { get; set; }
}