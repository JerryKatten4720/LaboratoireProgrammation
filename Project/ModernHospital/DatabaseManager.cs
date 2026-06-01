using System.Collections.ObjectModel;
using MySqlConnector;

namespace LaboratoireProgrammation.Project.ModernHospital;

public class DatabaseManager {
    private const string Host = "localhost";
    private const string Port = "3306";
    private const string Database = "lab_hopital";
    private const string User = "root";
    private const string Password = "";


    private readonly string _connectionString =
        $"Server={Host};Port={Port};Database={Database};Uid={User};Pwd={Password};";

    private MySqlConnection GetConnection() {
        return new MySqlConnection(_connectionString);
    }

    public HospitalInfo? GetHospital() {
        using var conn = GetConnection();
        conn.Open();
        using var cmd = new MySqlCommand("SELECT * FROM Hopital LIMIT 1", conn);
        using var r = cmd.ExecuteReader();
        if (!r.Read()) return null;
        return new HospitalInfo {
            IdHopital = r.GetInt32("IdHopital"),
            Nom = r.GetString("Nom"),
            DirecteurGeneral = r.IsDBNull(r.GetOrdinal("DirecteurGeneral")) ? "" : r.GetString("DirecteurGeneral"),
            DirecteurMedical = r.IsDBNull(r.GetOrdinal("DirecteurMedical")) ? "" : r.GetString("DirecteurMedical"),
            Budget = r.GetDecimal("Budget"),
            Reputation = r.GetInt32("Reputation"),
            JourSimulation = r.GetInt32("JourSimulation"),
            HeureSimulation = r.GetTimeSpan("HeureSimulation").ToString(@"hh\:mm"),
            MontantEmprunt = r.GetDecimal("MontantEmprunt"),
            TauxInteret = r.GetDecimal("TauxInteret")
        };
    }

    public void UpdateHospital(decimal budget, int reputation, int jour, string heure) {
        using var conn = GetConnection();
        conn.Open();
        using var cmd = new MySqlCommand(
            "UPDATE Hopital SET Budget=@b, Reputation=@r, JourSimulation=@j, HeureSimulation=@h WHERE IdHopital=1",
            conn);
        cmd.Parameters.AddWithValue("@b", budget);
        cmd.Parameters.AddWithValue("@r", reputation);
        cmd.Parameters.AddWithValue("@j", jour);
        cmd.Parameters.AddWithValue("@h", heure);
        cmd.ExecuteNonQuery();
    }

    public void AddBudget(decimal amount, string description, int jour, string type) {
        using var conn = GetConnection();
        conn.Open();
        using var cmd = new MySqlCommand("UPDATE Hopital SET Budget = Budget + @a WHERE IdHopital=1", conn);
        cmd.Parameters.AddWithValue("@a", amount);
        cmd.ExecuteNonQuery();

        LogTransaction(conn, jour, type, amount, description);
    }

    private void LogTransaction(MySqlConnection conn, int jour, string type, decimal montant, string desc) {
        using var cmd = new MySqlCommand(
            "INSERT INTO Transactions_Financieres (JourSimulation,TypeTransaction,Montant,Description) VALUES (@j,@t,@m,@d)",
            conn);
        cmd.Parameters.AddWithValue("@j", jour);
        cmd.Parameters.AddWithValue("@t", type);
        cmd.Parameters.AddWithValue("@m", montant);
        cmd.Parameters.AddWithValue("@d", desc);
        cmd.ExecuteNonQuery();
    }


    public List<Employe> GetPersonnel() {
        var list = new List<Employe>();
        using var conn = GetConnection();
        conn.Open();
        using var cmd = new MySqlCommand("SELECT * FROM Personnel_Actif ORDER BY Categorie, Nom", conn);
        using var r = cmd.ExecuteReader();
        while (r.Read())
            list.Add(ReadEmploye(r));
        return list;
    }

    private static Employe ReadEmploye(MySqlDataReader r) {
        return new Employe {
            IdEmploye = r.GetInt32("IdEmploye"),
            IdUnite = r.IsDBNull(r.GetOrdinal("IdUnite")) ? null : r.GetInt32("IdUnite"),
            Nom = r.GetString("Nom"),
            Prenom = r.GetString("Prenom"),
            Categorie = r.GetString("Categorie"),
            RoleExact = r.GetString("RoleExact"),
            Specialite = r.IsDBNull(r.GetOrdinal("Specialite")) ? null : r.GetString("Specialite"),
            NiveauCompetence = r.GetInt32("NiveauCompetence"),
            Experience = r.GetInt32("Experience"),
            Energie = r.GetInt32("Energie"),
            Faim = r.GetInt32("Faim"),
            Stress = r.GetInt32("Stress"),
            SalaireJour = r.GetDecimal("SalaireJour"),
            Shift = r.GetString("Shift"),
            Statut = r.GetString("Statut")
        };
    }


    public List<Candidat> GetCandidats() {
        var list = new List<Candidat>();
        using var conn = GetConnection();
        conn.Open();
        using var cmd = new MySqlCommand("SELECT * FROM To_Hire ORDER BY NiveauCompetence DESC", conn);
        using var r = cmd.ExecuteReader();
        while (r.Read())
            list.Add(new Candidat {
                IdCandidat = r.GetInt32("IdCandidat"),
                Nom = r.GetString("Nom"),
                Prenom = r.GetString("Prenom"),
                Categorie = r.GetString("Categorie"),
                RoleExact = r.GetString("RoleExact"),
                Specialite = r.IsDBNull(r.GetOrdinal("Specialite")) ? null : r.GetString("Specialite"),
                NiveauCompetence = r.GetInt32("NiveauCompetence"),
                SalaireJour = r.GetDecimal("SalaireJour"),
                PrimeEmbauche = r.GetDecimal("PrimeEmbauche")
            });
        return list;
    }

    public bool HireCandidat(int idCandidat, HospitalInfo hospital) {
        using var conn = GetConnection();
        conn.Open();


        Candidat? c = null;
        using (var cmd = new MySqlCommand("SELECT * FROM To_Hire WHERE IdCandidat=@id", conn)) {
            cmd.Parameters.AddWithValue("@id", idCandidat);
            using var r = cmd.ExecuteReader();
            if (!r.Read()) return false;
            c = new Candidat {
                IdCandidat = r.GetInt32("IdCandidat"),
                Nom = r.GetString("Nom"), Prenom = r.GetString("Prenom"),
                Categorie = r.GetString("Categorie"), RoleExact = r.GetString("RoleExact"),
                Specialite = r.IsDBNull(r.GetOrdinal("Specialite")) ? null : r.GetString("Specialite"),
                NiveauCompetence = r.GetInt32("NiveauCompetence"),
                SalaireJour = r.GetDecimal("SalaireJour"),
                PrimeEmbauche = r.GetDecimal("PrimeEmbauche")
            };
        }

        if (hospital.Budget < c.PrimeEmbauche) return false;


        using (var cmd = new MySqlCommand(
                   "INSERT INTO Personnel_Actif (Nom,Prenom,Categorie,RoleExact,Specialite,NiveauCompetence,SalaireJour) VALUES (@n,@p,@c,@r,@s,@nc,@sal)",
                   conn)) {
            cmd.Parameters.AddWithValue("@n", c.Nom);
            cmd.Parameters.AddWithValue("@p", c.Prenom);
            cmd.Parameters.AddWithValue("@c", c.Categorie);
            cmd.Parameters.AddWithValue("@r", c.RoleExact);
            cmd.Parameters.AddWithValue("@s", (object?)c.Specialite ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@nc", c.NiveauCompetence);
            cmd.Parameters.AddWithValue("@sal", c.SalaireJour);
            cmd.ExecuteNonQuery();
        }


        using (var cmd = new MySqlCommand("UPDATE Hopital SET Budget = Budget - @p WHERE IdHopital=1", conn)) {
            cmd.Parameters.AddWithValue("@p", c.PrimeEmbauche);
            cmd.ExecuteNonQuery();
        }

        LogTransaction(conn, hospital.JourSimulation, "Embauche", -c.PrimeEmbauche,
            $"Prime d'embauche: {c.Prenom} {c.Nom}");


        using (var cmd = new MySqlCommand("DELETE FROM To_Hire WHERE IdCandidat=@id", conn)) {
            cmd.Parameters.AddWithValue("@id", idCandidat);
            cmd.ExecuteNonQuery();
        }

        return true;
    }

    public void FireEmploye(int idEmploye, HospitalInfo hospital) {
        using var conn = GetConnection();
        conn.Open();
        using var cmd = new MySqlCommand("DELETE FROM Personnel_Actif WHERE IdEmploye=@id", conn);
        cmd.Parameters.AddWithValue("@id", idEmploye);
        cmd.ExecuteNonQuery();
    }


    public List<PatientActif> GetPatients() {
        var list = new List<PatientActif>();
        using var conn = GetConnection();
        conn.Open();
        using var cmd = new MySqlCommand("SELECT * FROM Helper_Patients_Dashboard", conn);
        using var r = cmd.ExecuteReader();
        while (r.Read())
            list.Add(new PatientActif {
                IdPatient = r.GetInt32("IdPatient"),
                Nom = r.GetString("Patient"),
                Maladie = r.GetString("Maladie"),
                SanteActuelle = r.GetInt32("SanteActuelle"),
                Satisfaction = r.GetInt32("Satisfaction"),
                Classe = r.GetString("Classe"),
                Statut = r.GetString("Statut"),
                NumeroChambre = r.IsDBNull(r.GetOrdinal("NumeroChambre")) ? "N/A" : r.GetString("NumeroChambre"),
                NumeroLit = r.IsDBNull(r.GetOrdinal("NumeroLit")) ? "N/A" : r.GetString("NumeroLit"),
                MedecinEnCharge = r.IsDBNull(r.GetOrdinal("MedecinEnCharge"))
                    ? "Non assigné"
                    : r.GetString("MedecinEnCharge")
            });
        return list;
    }

    public List<PatientActif> GetPatientsRaw() {
        var list = new List<PatientActif>();
        using var conn = GetConnection();
        conn.Open();
        using var cmd = new MySqlCommand("SELECT * FROM Patients_Actifs", conn);
        using var r = cmd.ExecuteReader();
        while (r.Read())
            list.Add(new PatientActif {
                IdPatient = r.GetInt32("IdPatient"),
                Nom = r.GetString("Nom"),
                IdMaladie = r.GetInt32("IdMaladie"),
                IdLit = r.IsDBNull(r.GetOrdinal("IdLit")) ? null : r.GetInt32("IdLit"),
                IdMedecinAssigne = r.IsDBNull(r.GetOrdinal("IdMedecinAssigné")) ? null : r.GetInt32("IdMedecinAssigné"),
                SanteActuelle = r.GetInt32("SanteActuelle"),
                Satisfaction = r.GetInt32("Satisfaction"),
                Classe = r.GetString("Classe"),
                TempsTraitementRestant = r.IsDBNull(r.GetOrdinal("TempsTraitementRestant"))
                    ? 0
                    : r.GetInt32("TempsTraitementRestant"),
                Statut = r.GetString("Statut")
            });
        return list;
    }

    public void AdmitPatient(string nom, int idMaladie, int idLit, int? idMedecin, string classe, int tempsTraitement) {
        using var conn = GetConnection();
        conn.Open();
        using var cmd = new MySqlCommand(
            "INSERT INTO Patients_Actifs (Nom,IdMaladie,IdLit,IdMedecinAssigné,Classe,TempsTraitementRestant,Statut) VALUES (@n,@m,@l,@md,@cl,@t,'En Diagnostic')",
            conn);
        cmd.Parameters.AddWithValue("@n", nom);
        cmd.Parameters.AddWithValue("@m", idMaladie);
        cmd.Parameters.AddWithValue("@l", idLit);
        cmd.Parameters.AddWithValue("@md", (object?)idMedecin ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@cl", classe);
        cmd.Parameters.AddWithValue("@t", tempsTraitement);
        cmd.ExecuteNonQuery();


        using var cmd2 = new MySqlCommand("UPDATE Lit SET Statut='Occupé' WHERE IdLit=@id", conn);
        cmd2.Parameters.AddWithValue("@id", idLit);
        cmd2.ExecuteNonQuery();
    }

    public void UpdatePatientStatut(int idPatient, string statut) {
        using var conn = GetConnection();
        conn.Open();
        using var cmd = new MySqlCommand("UPDATE Patients_Actifs SET Statut=@s WHERE IdPatient=@id", conn);
        cmd.Parameters.AddWithValue("@s", statut);
        cmd.Parameters.AddWithValue("@id", idPatient);
        cmd.ExecuteNonQuery();
    }

    public void DischargePatient(int idPatient, int? idLit, decimal revenu, int jour, string typeRevenu) {
        using var conn = GetConnection();
        conn.Open();

        using var cmdUpd = new MySqlCommand("UPDATE Patients_Actifs SET Statut='Guéri' WHERE IdPatient=@id", conn);
        cmdUpd.Parameters.AddWithValue("@id", idPatient);
        cmdUpd.ExecuteNonQuery();

        if (idLit.HasValue) {
            using var cmdLit = new MySqlCommand("UPDATE Lit SET Statut='En Nettoyage' WHERE IdLit=@id", conn);
            cmdLit.Parameters.AddWithValue("@id", idLit.Value);
            cmdLit.ExecuteNonQuery();
        }

        using var cmdBudget = new MySqlCommand("UPDATE Hopital SET Budget = Budget + @r WHERE IdHopital=1", conn);
        cmdBudget.Parameters.AddWithValue("@r", revenu);
        cmdBudget.ExecuteNonQuery();

        LogTransaction(conn, jour, typeRevenu, revenu, $"Prise en charge patient #{idPatient}");
    }


    public List<CasClinique> GetCasCliniques() {
        var list = new List<CasClinique>();
        using var conn = GetConnection();
        conn.Open();
        using var cmd =
            new MySqlCommand(
                "SELECT IdCas,Maladie,Symptomes,TempsTraitementHeures,RevenuPatient,RisqueErreurMedicale FROM Cas_Cliniques",
                conn);
        using var r = cmd.ExecuteReader();
        while (r.Read())
            list.Add(new CasClinique {
                IdCas = r.GetInt32("IdCas"),
                Maladie = r.GetString("Maladie"),
                Symptomes = r.GetString("Symptomes"),
                TempsTraitementHeures = r.GetInt32("TempsTraitementHeures"),
                RevenuPatient = r.GetDecimal("RevenuPatient"),
                RisqueErreurMedicale = r.GetInt32("RisqueErreurMedicale")
            });
        return list;
    }


    public List<LitDisponible> GetLitsLibres() {
        var list = new List<LitDisponible>();
        using var conn = GetConnection();
        conn.Open();
        using var cmd = new MySqlCommand("SELECT * FROM Helper_Disponibilite_Lits", conn);
        using var r = cmd.ExecuteReader();
        while (r.Read())
            list.Add(new LitDisponible {
                IdLit = r.GetInt32("IdLit"),
                Unite = r.GetString("Unite"),
                NumeroChambre = r.GetString("NumeroChambre"),
                TypeChambre = r.GetString("TypeChambre"),
                NumeroLit = r.GetString("NumeroLit")
            });
        return list;
    }

    public void FreeBed(int idLit) {
        using var conn = GetConnection();
        conn.Open();
        using var cmd = new MySqlCommand("UPDATE Lit SET Statut='Libre' WHERE IdLit=@id", conn);
        cmd.Parameters.AddWithValue("@id", idLit);
        cmd.ExecuteNonQuery();
    }


    public void UpdateTransaction(int idTransaction, decimal montant, string description) {
        using var conn = GetConnection();
        conn.Open();
        using var cmd = new MySqlCommand(
            "UPDATE Transactions_Financieres SET Montant=@m, Description=@d WHERE IdTransaction=@id",
            conn);
        cmd.Parameters.AddWithValue("@m", montant);
        cmd.Parameters.AddWithValue("@d", description);
        cmd.Parameters.AddWithValue("@id", idTransaction);
        cmd.ExecuteNonQuery();
    }

    public void DeleteTransaction(int idTransaction) {
        using var conn = GetConnection();
        conn.Open();
        using var cmd = new MySqlCommand("DELETE FROM Transactions_Financieres WHERE IdTransaction=@id", conn);
        cmd.Parameters.AddWithValue("@id", idTransaction);
        cmd.ExecuteNonQuery();
    }

    public void SendBillToPatient(int idPatient, decimal amount, string description) {
        using var conn = GetConnection();
        conn.Open();


        using var cmd = new MySqlCommand("UPDATE Hopital SET Budget = Budget + @a WHERE IdHopital=1", conn);
        cmd.Parameters.AddWithValue("@a", amount);
        cmd.ExecuteNonQuery();

        LogTransaction(conn, 0, "Facture Patient", amount, description);
    }

    public void SetPayrollDay(int dayOfMonth) {
        using var conn = GetConnection();
        conn.Open();
        using var cmd = new MySqlCommand(
            "UPDATE Hopital SET JourPaie=@day WHERE IdHopital=1",
            conn);
        cmd.Parameters.AddWithValue("@day", dayOfMonth);
        cmd.ExecuteNonQuery();
    }

    public int GetPayrollDay() {
        using var conn = GetConnection();
        conn.Open();
        using var cmd = new MySqlCommand("SELECT JourPaie FROM Hopital LIMIT 1", conn);
        var result = cmd.ExecuteScalar();
        return result != null ? Convert.ToInt32(result) : 1;
    }

    public void CreateEmploye(string nom, string prenom, string categorie, string roleExact, string? specialite,
        int niveauCompetence, decimal salaireJour, string shift = "Jour") {
        using var conn = GetConnection();
        conn.Open();
        using var cmd = new MySqlCommand(
            "INSERT INTO Personnel_Actif (Nom, Prenom, Categorie, RoleExact, Specialite, NiveauCompetence, SalaireJour, Shift, Energie, Faim, Stress, Statut) " +
            "VALUES (@n, @p, @c, @r, @s, @nc, @sal, @shift, 100, 0, 0, 'En poste')",
            conn);
        cmd.Parameters.AddWithValue("@n", nom);
        cmd.Parameters.AddWithValue("@p", prenom);
        cmd.Parameters.AddWithValue("@c", categorie);
        cmd.Parameters.AddWithValue("@r", roleExact);
        cmd.Parameters.AddWithValue("@s", (object?)specialite ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@nc", niveauCompetence);
        cmd.Parameters.AddWithValue("@sal", salaireJour);
        cmd.Parameters.AddWithValue("@shift", shift);
        cmd.ExecuteNonQuery();
    }

    public void UpdateEmploye(int idEmploye, string nom, string prenom, string categorie, string roleExact,
        string? specialite, int niveauCompetence, decimal salaireJour, string shift) {
        using var conn = GetConnection();
        conn.Open();
        using var cmd = new MySqlCommand(
            "UPDATE Personnel_Actif SET Nom=@n, Prenom=@p, Categorie=@c, RoleExact=@r, Specialite=@s, " +
            "NiveauCompetence=@nc, SalaireJour=@sal, Shift=@shift WHERE IdEmploye=@id",
            conn);
        cmd.Parameters.AddWithValue("@n", nom);
        cmd.Parameters.AddWithValue("@p", prenom);
        cmd.Parameters.AddWithValue("@c", categorie);
        cmd.Parameters.AddWithValue("@r", roleExact);
        cmd.Parameters.AddWithValue("@s", (object?)specialite ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@nc", niveauCompetence);
        cmd.Parameters.AddWithValue("@sal", salaireJour);
        cmd.Parameters.AddWithValue("@shift", shift);
        cmd.Parameters.AddWithValue("@id", idEmploye);
        cmd.ExecuteNonQuery();
    }

    public List<Transaction> GetRecentTransactions(int limit = 20) {
        var list = new List<Transaction>();
        using var conn = GetConnection();
        conn.Open();
        using var cmd = new MySqlCommand(
            $"SELECT * FROM Transactions_Financieres ORDER BY IdTransaction DESC LIMIT {limit}", conn);
        using var r = cmd.ExecuteReader();
        while (r.Read())
            list.Add(new Transaction {
                IdTransaction = r.GetInt32("IdTransaction"),
                JourSimulation = r.GetInt32("JourSimulation"),
                TypeTransaction = r.GetString("TypeTransaction"),
                Montant = r.GetDecimal("Montant"),
                Description = r.IsDBNull(r.GetOrdinal("Description")) ? "" : r.GetString("Description")
            });
        return list;
    }


    public DashboardStats GetStats() {
        using var conn = GetConnection();
        conn.Open();
        var stats = new DashboardStats();

        var sql = @"
                SELECT
                  (SELECT COUNT(*) FROM Patients_Actifs WHERE Statut NOT IN ('Guéri','Décédé')) AS PatientsActifs,
                  (SELECT COUNT(*) FROM Personnel_Actif) AS TotalPersonnel,
                  (SELECT COUNT(*) FROM Lit WHERE Statut='Libre') AS LitsLibres,
                  (SELECT COUNT(*) FROM Lit WHERE Statut='Occupé') AS LitsOccupes,
                  (SELECT COALESCE(SUM(Montant),0) FROM Transactions_Financieres WHERE TypeTransaction='Revenu Patient' AND JourSimulation=(SELECT JourSimulation FROM Hopital LIMIT 1)) AS RevenuJour,
                  (SELECT COALESCE(SUM(ABS(Montant)),0) FROM Transactions_Financieres WHERE Montant < 0 AND JourSimulation=(SELECT JourSimulation FROM Hopital LIMIT 1)) AS DepensesJour,
                  (SELECT COUNT(*) FROM Patients_Actifs WHERE Statut='En Attente') AS EnAttente,
                  (SELECT COUNT(*) FROM Patients_Actifs WHERE Statut='En Traitement') AS EnTraitement,
                  (SELECT COUNT(*) FROM Patients_Actifs WHERE Statut='Guéri') AS Gueris,
                  (SELECT COUNT(*) FROM Patients_Actifs WHERE Statut='Décédé') AS Deces";

        using var cmd = new MySqlCommand(sql, conn);
        using var r = cmd.ExecuteReader();
        if (r.Read()) {
            stats.TotalPatientsActifs = r.GetInt32("PatientsActifs");
            stats.TotalPersonnel = r.GetInt32("TotalPersonnel");
            stats.LitsLibres = r.GetInt32("LitsLibres");
            stats.LitsOccupes = r.GetInt32("LitsOccupes");
            stats.RevenuJour = r.GetDecimal("RevenuJour");
            stats.DepensesJour = r.GetDecimal("DepensesJour");
            stats.PatientsEnAttente = r.GetInt32("EnAttente");
            stats.PatientsEnTraitement = r.GetInt32("EnTraitement");
            stats.PatientsGueris = r.GetInt32("Gueris");
            stats.PatientsDeces = r.GetInt32("Deces");
        }

        return stats;
    }


    public List<string> AdvanceHour(HospitalInfo hospital) {
        var log = new List<string>();
        var rng = new Random();

        using var conn = GetConnection();
        conn.Open();


        var ts = TimeSpan.Parse(hospital.HeureSimulation);
        ts = ts.Add(TimeSpan.FromHours(1));
        var newJour = hospital.JourSimulation;
        if (ts.TotalHours >= 24) {
            ts = ts.Subtract(TimeSpan.FromHours(24));
            newJour++;
        }

        var newHeure = ts.ToString(@"hh\:mm");


        decimal salaireHoraire = 0;
        using (var cmd = new MySqlCommand(
                   "SELECT COALESCE(SUM(SalaireJour),0) FROM Personnel_Actif WHERE Statut != 'Absent'", conn)) {
            salaireHoraire = Convert.ToDecimal(cmd.ExecuteScalar()) / 24m;
        }

        if (salaireHoraire > 0) {
            using var cmd = new MySqlCommand("UPDATE Hopital SET Budget = Budget - @s WHERE IdHopital=1", conn);
            cmd.Parameters.AddWithValue("@s", salaireHoraire);
            cmd.ExecuteNonQuery();
            if (ts.Hours == 0)
                LogTransaction(conn, newJour, "Paiement Salaire", -salaireHoraire * 24,
                    $"Salaires quotidiens - {hospital.JourSimulation + 1} employés actifs");
        }


        if (ts.Hours == 0) {
            decimal entretien = 0;
            using (var cmd = new MySqlCommand("SELECT COALESCE(SUM(CoutEntretienJour),0) FROM Unite", conn)) {
                entretien = Convert.ToDecimal(cmd.ExecuteScalar());
            }

            using (var cmd = new MySqlCommand("UPDATE Hopital SET Budget = Budget - @e WHERE IdHopital=1", conn)) {
                cmd.Parameters.AddWithValue("@e", entretien);
                cmd.ExecuteNonQuery();
            }

            if (entretien > 0) {
                LogTransaction(conn, newJour, "Frais Entretien", -entretien, "Coût d'entretien quotidien des unités");
                log.Add($"💰 Entretien des unités: -{entretien:C0}");
            }
        }


        var patients = new List<(int id, int tempsRestant, int idMaladie, int? idLit, int? idMedecin, string classe)>();
        using (var cmd = new MySqlCommand(
                   "SELECT IdPatient,TempsTraitementRestant,IdMaladie,IdLit,IdMedecinAssigné,Classe FROM Patients_Actifs WHERE Statut IN ('En Diagnostic','En Traitement')",
                   conn))
        using (var r = cmd.ExecuteReader()) {
            while (r.Read())
                patients.Add((r.GetInt32(0), r.IsDBNull(1) ? 0 : r.GetInt32(1),
                    r.GetInt32(2), r.IsDBNull(3) ? null : r.GetInt32(3),
                    r.IsDBNull(4) ? null : r.GetInt32(4), r.GetString(5)));
        }

        foreach (var (id, temps, idMaladie, idLit, idMedecin, classe) in patients) {
            var newTemps = temps - 1;


            using (var cmd = new MySqlCommand(
                       "UPDATE Patients_Actifs SET Statut='En Traitement', TempsTraitementRestant=@t WHERE IdPatient=@id AND Statut='En Diagnostic'",
                       conn)) {
                cmd.Parameters.AddWithValue("@t", newTemps);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }

            if (newTemps <= 0) {
                decimal revenu = 0;
                using (var cmd = new MySqlCommand("SELECT RevenuPatient FROM Cas_Cliniques WHERE IdCas=@m", conn)) {
                    cmd.Parameters.AddWithValue("@m", idMaladie);
                    revenu = Convert.ToDecimal(cmd.ExecuteScalar());
                }

                if (classe == "VIP") revenu *= 2m;
                else if (classe == "Non-assuré") revenu *= 0.3m;

                using (var cmd = new MySqlCommand(
                           "UPDATE Patients_Actifs SET Statut='Guéri',TempsTraitementRestant=0 WHERE IdPatient=@id",
                           conn)) {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }

                if (idLit.HasValue) {
                    using var cmd = new MySqlCommand("UPDATE Lit SET Statut='Libre' WHERE IdLit=@l", conn);
                    cmd.Parameters.AddWithValue("@l", idLit.Value);
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = new MySqlCommand("UPDATE Hopital SET Budget=Budget+@r WHERE IdHopital=1", conn)) {
                    cmd.Parameters.AddWithValue("@r", revenu);
                    cmd.ExecuteNonQuery();
                }

                LogTransaction(conn, newJour, "Revenu Patient", revenu, $"Patient #{id} traité ({classe})");
                log.Add($"✅ Patient #{id} guéri → +{revenu:C0}");


                using (var cmd = new MySqlCommand(
                           "UPDATE Hopital SET Reputation=LEAST(100,Reputation+1) WHERE IdHopital=1", conn)) {
                    cmd.ExecuteNonQuery();
                }
            }
            else {
                using (var cmd = new MySqlCommand(
                           "UPDATE Patients_Actifs SET TempsTraitementRestant=@t WHERE IdPatient=@id", conn)) {
                    cmd.Parameters.AddWithValue("@t", newTemps);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }


                var risk = 3;
                using (var cmd = new MySqlCommand("SELECT RisqueErreurMedicale FROM Cas_Cliniques WHERE IdCas=@m",
                           conn)) {
                    cmd.Parameters.AddWithValue("@m", idMaladie);
                    risk = Convert.ToInt32(cmd.ExecuteScalar());
                }

                var competence = 50;
                if (idMedecin.HasValue)
                    using (var cmd = new MySqlCommand("SELECT NiveauCompetence FROM Personnel_Actif WHERE IdEmploye=@e",
                               conn)) {
                        cmd.Parameters.AddWithValue("@e", idMedecin.Value);
                        competence = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                var effectiveRisk = Math.Max(1, risk - competence / 20);
                if (rng.Next(100) < effectiveRisk) {
                    var dmg = rng.Next(5, 20);
                    using var cmd = new MySqlCommand(
                        "UPDATE Patients_Actifs SET SanteActuelle=GREATEST(0,SanteActuelle-@d), Satisfaction=GREATEST(0,Satisfaction-5) WHERE IdPatient=@id",
                        conn);
                    cmd.Parameters.AddWithValue("@d", dmg);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                    log.Add($"⚠️ Complication patient #{id} (-{dmg} santé)");


                    var sante = 0;
                    using var cmd2 = new MySqlCommand("SELECT SanteActuelle FROM Patients_Actifs WHERE IdPatient=@id",
                        conn);
                    cmd2.Parameters.AddWithValue("@id", id);
                    sante = Convert.ToInt32(cmd2.ExecuteScalar());
                    if (sante <= 0) {
                        using var cmdDead =
                            new MySqlCommand("UPDATE Patients_Actifs SET Statut='Décédé' WHERE IdPatient=@id", conn);
                        cmdDead.Parameters.AddWithValue("@id", id);
                        cmdDead.ExecuteNonQuery();
                        if (idLit.HasValue) {
                            using var cmdFree = new MySqlCommand("UPDATE Lit SET Statut='Libre' WHERE IdLit=@l", conn);
                            cmdFree.Parameters.AddWithValue("@l", idLit.Value);
                            cmdFree.ExecuteNonQuery();
                        }

                        using var cmdRep =
                            new MySqlCommand("UPDATE Hopital SET Reputation=GREATEST(0,Reputation-5) WHERE IdHopital=1",
                                conn);
                        cmdRep.ExecuteNonQuery();
                        log.Add($"💀 Patient #{id} est décédé");
                    }
                }
            }
        }


        using (var cmd = new MySqlCommand(
                   "UPDATE Personnel_Actif SET Energie=GREATEST(0,Energie-2), Stress=LEAST(100,Stress+1), Faim=GREATEST(0,Faim-1) WHERE Statut='En poste'",
                   conn)) {
            cmd.ExecuteNonQuery();
        }


        using (var cmd = new MySqlCommand(
                   "UPDATE Personnel_Actif SET Statut='Épuisé' WHERE Energie <= 10 AND Statut='En poste'", conn)) {
            cmd.ExecuteNonQuery();
        }


        using (var cmd = new MySqlCommand("UPDATE Hopital SET HeureSimulation=@h, JourSimulation=@j WHERE IdHopital=1",
                   conn)) {
            cmd.Parameters.AddWithValue("@h", newHeure + ":00");
            cmd.Parameters.AddWithValue("@j", newJour);
            cmd.ExecuteNonQuery();
        }

        if (log.Count == 0) log.Add($"⏱ Heure avancée → Jour {newJour} {newHeure}");
        return log;
    }


    public bool TestConnection() {
        try {
            using var conn = GetConnection();
            conn.Open();
            return true;
        }
        catch {
            return false;
        }
    }

    public void FireEmploye(int empIdEmploye) {
        using var conn = GetConnection();
        conn.Open();
        using var cmd = new MySqlCommand("DELETE FROM Personnel_Actif WHERE IdEmploye=@id", conn);
        cmd.Parameters.AddWithValue("@id", empIdEmploye);
        cmd.ExecuteNonQuery();
    }

    public void ExecuteNonQuery(string query) {
        using var conn = GetConnection();
        conn.Open();
        using var cmd = new MySqlCommand(query, conn);
        cmd.ExecuteNonQuery();
    }

    public T? ExecuteScalar<T>(string query) {
        using var conn = GetConnection();
        conn.Open();
        using var cmd = new MySqlCommand(query, conn);
        var result = cmd.ExecuteScalar();
        if (result == null || result == DBNull.Value) return default;
        return (T)Convert.ChangeType(result, typeof(T));
    }

    public void RecordTransaction(string type, decimal montant, string description) {
        var hospital = GetHospital();
        if (hospital == null) return;
        using var conn = GetConnection();
        conn.Open();
        LogTransaction(conn, hospital.JourSimulation, type, montant, description);
    }

    public void ReleaseBed(int idPatient) {
        using var conn = GetConnection();
        conn.Open();
        // On récupère le lit occupé par le patient
        int? idLit = null;
        using (var cmd = new MySqlCommand("SELECT IdLit FROM Patients_Actifs WHERE IdPatient=@id", conn)) {
            cmd.Parameters.AddWithValue("@id", idPatient);
            var result = cmd.ExecuteScalar();
            if (result != null && result != DBNull.Value) idLit = Convert.ToInt32(result);
        }

        if (idLit.HasValue) {
            using var cmd = new MySqlCommand("UPDATE Lit SET Statut='Libre' WHERE IdLit=@id", conn);
            cmd.Parameters.AddWithValue("@id", idLit.Value);
            cmd.ExecuteNonQuery();
        }
    }

    public List<LitInventaire> GetLitsInventaire() {
        var list = new List<LitInventaire>();
        using var conn = GetConnection();
        conn.Open();

        var sql = @"
        SELECT 
            l.IdLit, 
            l.NumeroLit, 
            c.NumeroChambre, 
            c.TypeChambre, 
            u.Nom AS Unite, 
            l.Statut 
        FROM Lit l 
        JOIN Chambre c ON l.IdChambre = c.IdChambre 
        JOIN Unite u ON c.IdUnite = u.IdUnite";

        using var cmd = new MySqlCommand(sql, conn);
        using var r = cmd.ExecuteReader();

        while (r.Read())
            list.Add(new LitInventaire {
                IdLit = r.GetInt32("IdLit"),
                NumeroLit = r.GetString("NumeroLit"),
                NumeroChambre = r.GetString("NumeroChambre"),
                TypeChambre = r.GetString("TypeChambre"),
                Unite = r.GetString("Unite"),
                Statut = r.GetString("Statut")
            });

        return list;
    }

    public ObservableCollection<ChambreGroup> GetChambresHierarchiques() {
        var chambres = new Dictionary<int, ChambreGroup>();
        using var conn = GetConnection();
        conn.Open();

        var sqlChambres = @"
            SELECT c.IdChambre, c.NumeroChambre, c.TypeChambre, u.Nom AS Unite 
            FROM Chambre c JOIN Unite u ON c.IdUnite = u.IdUnite ORDER BY c.NumeroChambre";
        using var cmdC = new MySqlCommand(sqlChambres, conn);
        using var rC = cmdC.ExecuteReader();
        while (rC.Read()) {
            var id = rC.GetInt32("IdChambre");
            chambres[id] = new ChambreGroup {
                IdChambre = id,
                NumeroChambre = rC.GetString("NumeroChambre"),
                TypeChambre = rC.GetString("TypeChambre"),
                Unite = rC.GetString("Unite")
            };
        }

        rC.Close();

        var sqlLits = "SELECT IdLit, IdChambre, NumeroLit, Statut FROM Lit";
        using var cmdL = new MySqlCommand(sqlLits, conn);
        using var rL = cmdL.ExecuteReader();
        while (rL.Read()) {
            var idChambre = rL.GetInt32("IdChambre");
            if (chambres.TryGetValue(idChambre, out var chambreGroup))
                chambreGroup.Lits.Add(new LitInventaire {
                    IdLit = rL.GetInt32("IdLit"),
                    IdChambre = idChambre,
                    NumeroLit = rL.GetString("NumeroLit"),
                    Statut = rL.GetString("Statut")
                });
        }

        return new ObservableCollection<ChambreGroup>(chambres.Values);
    }

    public void UpdateLitChambre(int idLit, int nouvelleIdChambre) {
        using var conn = GetConnection();
        conn.Open();
        using var cmd = new MySqlCommand("UPDATE Lit SET IdChambre = @idC WHERE IdLit = @idL", conn);
        cmd.Parameters.AddWithValue("@idC", nouvelleIdChambre);
        cmd.Parameters.AddWithValue("@idL", idLit);
        cmd.ExecuteNonQuery();
    }

    public List<ChambreBase> GetChambresList() {
        var list = new List<ChambreBase>();
        using var conn = GetConnection();
        conn.Open();

        using var cmd =
            new MySqlCommand("SELECT IdChambre, NumeroChambre, TypeChambre FROM Chambre ORDER BY NumeroChambre", conn);
        using var r = cmd.ExecuteReader();

        while (r.Read())
            list.Add(new ChambreBase {
                IdChambre = r.GetInt32("IdChambre"),
                NumeroChambre = r.GetString("NumeroChambre"),
                TypeChambre = r.GetString("TypeChambre")
            });

        return list;
    }

    public void AddLit(int idChambre, string numeroLit) {
        using var conn = GetConnection();
        conn.Open();

        using var cmd = new MySqlCommand("INSERT INTO Lit (IdChambre, NumeroLit, Statut) VALUES (@id, @num, 'Libre')",
            conn);
        cmd.Parameters.AddWithValue("@id", idChambre);
        cmd.Parameters.AddWithValue("@num", numeroLit);

        cmd.ExecuteNonQuery();
    }

    public List<UniteBase> GetUnitesList() {
        var list = new List<UniteBase>();
        using var conn = GetConnection();
        conn.Open();

        using var cmd = new MySqlCommand("SELECT IdUnite, Nom FROM Unite ORDER BY Nom", conn);
        using var r = cmd.ExecuteReader();

        while (r.Read())
            list.Add(new UniteBase {
                IdUnite = r.GetInt32("IdUnite"),
                Nom = r.GetString("Nom")
            });

        return list;
    }

    public void AddChambre(int idUnite, string numeroChambre, string typeChambre) {
        using var conn = GetConnection();
        conn.Open();

        using var cmd = new MySqlCommand(
            "INSERT INTO Chambre (IdUnite, NumeroChambre, TypeChambre) VALUES (@id, @num, @type)", conn);
        cmd.Parameters.AddWithValue("@id", idUnite);
        cmd.Parameters.AddWithValue("@num", numeroChambre);
        cmd.Parameters.AddWithValue("@type", typeChambre);

        cmd.ExecuteNonQuery();
    }

    public void RemoveChambre(int idChambre) {
        using var conn = GetConnection();
        conn.Open();

        using var cmd = new MySqlCommand("DELETE FROM Chambre WHERE IdChambre = @id", conn);
        cmd.Parameters.AddWithValue("@id", idChambre);

        cmd.ExecuteNonQuery();
    }

    public class ChambreBase {
        public int IdChambre { get; set; }
        public string NumeroChambre { get; set; } = string.Empty;
        public string TypeChambre { get; set; } = string.Empty;
        public string Display => $"{NumeroChambre} - {TypeChambre}";
    }

    public class UniteBase {
        public int IdUnite { get; set; }
        public string Nom { get; set; } = string.Empty;
    }
}