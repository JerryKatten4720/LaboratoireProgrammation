namespace LaboratoireProgrammation.Project.ModernHospital;

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
    public string Nom { get; set; } = "";
    public string Maladie { get; set; } = "";
    public int SanteActuelle { get; set; }
    public int Satisfaction { get; set; }
    public string Classe { get; set; } = "Standard";
    public string Statut { get; set; } = "En Attente";
    public string NumeroChambre { get; set; } = "N/A";
    public string NumeroLit { get; set; } = "N/A";
    public string MedecinEnCharge { get; set; } = "Non assigné";
    public int TempsTraitementRestant { get; set; }
    public int IdMaladie { get; set; }
    public int? IdLit { get; set; }
    public int? IdMedecinAssigne { get; set; }
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