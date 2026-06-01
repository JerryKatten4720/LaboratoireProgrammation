using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace LaboratoireProgrammation.Project.ModernHospital;

public class ChambreGroup {
    public int IdChambre { get; set; }
    public string NumeroChambre { get; set; } = string.Empty;
    public string TypeChambre { get; set; } = string.Empty;
    public string Unite { get; set; } = string.Empty;
    public ObservableCollection<LitInventaire> Lits { get; set; } = new();
}

public static class DragDropHelper {
    public static void HandleAutoScroll(ScrollViewer sv, DragEventArgs e, double tolerance = 40, double offset = 10) {
        var verticalPos = e.GetPosition(sv).Y;
        if (verticalPos < tolerance)
            sv.ScrollToVerticalOffset(sv.VerticalOffset - offset);
        else if (verticalPos > sv.ActualHeight - tolerance)
            sv.ScrollToVerticalOffset(sv.VerticalOffset + offset);
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
    public int NiveauCompetence { get; set; }
    public int Experience { get; set; }
    public int Energie { get; set; }
    public int Faim { get; set; }
    public int Stress { get; set; }
    public decimal SalaireJour { get; set; }
    public string Shift { get; set; } = "Jour";
    public string Statut { get; set; } = "En poste";
    public string NomComplet => $"{Prenom} {Nom}";
}

public class Candidat {
    public int IdCandidat { get; set; }
    public string Nom { get; set; } = "";
    public string Prenom { get; set; } = "";
    public string Categorie { get; set; } = "";
    public string RoleExact { get; set; } = "";
    public string? Specialite { get; set; }
    public int NiveauCompetence { get; set; }
    public decimal SalaireJour { get; set; }
    public decimal PrimeEmbauche { get; set; }
    public string NomComplet => $"{Prenom} {Nom}";
    public decimal CoutTotal => PrimeEmbauche + SalaireJour;
}

public class PatientActif {
    public int IdPatient { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Maladie { get; set; } = string.Empty;
    public string Statut { get; set; } = string.Empty;
    public string Classe { get; set; } = string.Empty;
    public string? NumeroChambre { get; set; }
    public string? MedecinEnCharge { get; set; }
    public int SanteActuelle { get; set; }
    public int Satisfaction { get; set; }
    public string? NumeroLit { get; set; }
    public int TempsTraitementRestant { get; set; }
    public int IdMaladie { get; set; }
    public int? IdLit { get; set; }
    public int? IdMedecinAssigne { get; set; }

    public string WarningIcon =>
        string.IsNullOrEmpty(NumeroChambre) || string.IsNullOrEmpty(MedecinEnCharge) ? "⚠️" : "";
}

public class LitInventaire {
    public int IdLit { get; set; }
    public int IdChambre { get; set; }
    public string NumeroLit { get; set; } = string.Empty;
    public string NumeroChambre { get; set; } = string.Empty;
    public string TypeChambre { get; set; } = string.Empty;
    public string Unite { get; set; } = string.Empty;
    public string Statut { get; set; } = string.Empty;
}

public static class PatientHelper {
    public static bool HandlePatientStatusChange(DatabaseManager db, PatientActif patient, string newStatus) {
        if (patient == null) return false;

        db.ExecuteNonQuery($"UPDATE Patients_Actifs SET Statut = '{newStatus}' WHERE IdPatient = {patient.IdPatient}");

        if (newStatus == "Guéri") {
            var revenu = db.ExecuteScalar<decimal>(
                $"SELECT c.RevenuPatient FROM Patients_Actifs p JOIN Cas_Cliniques c ON p.IdMaladie = c.IdCas WHERE p.IdPatient = {patient.IdPatient}");
            db.ExecuteNonQuery($"UPDATE Hopital SET Budget = Budget + {revenu}");
            db.RecordTransaction("Revenu Patient", revenu, $"Patient {patient.Nom} guéri.");
            db.ReleaseBed(patient.IdPatient);
        }
        else if (newStatus == "Décédé") {
            db.ReleaseBed(patient.IdPatient);
        }

        return true;
    }
}

public class Transaction {
    public int IdTransaction { get; set; }
    public int JourSimulation { get; set; }
    public string TypeTransaction { get; set; } = "";
    public decimal Montant { get; set; }
    public string Description { get; set; } = "";
}

public class CasClinique {
    public int IdCas { get; set; }
    public string Maladie { get; set; } = "";
    public string Symptomes { get; set; } = "";
    public int TempsTraitementHeures { get; set; }
    public decimal RevenuPatient { get; set; }
    public int RisqueErreurMedicale { get; set; }
}

public class LitDisponible {
    public int IdLit { get; set; }
    public string Unite { get; set; } = "";
    public string NumeroChambre { get; set; } = "";
    public string TypeChambre { get; set; } = "";
    public string NumeroLit { get; set; } = "";
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