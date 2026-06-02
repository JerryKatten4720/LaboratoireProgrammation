using System;
using System.Collections.Generic;
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
        var connection = new MySqlConnection(_connectionString);
        connection.Open();
        return connection;
    }

    private void ExecuteCommand(string query, Action<MySqlCommand>? setParameters = null) {
        using var connection = GetConnection();
        using var command = new MySqlCommand(query, connection);
        setParameters?.Invoke(command);
        command.ExecuteNonQuery();
    }

    private T? ExecuteScalar<T>(string query, Action<MySqlCommand>? setParameters = null) {
        using var connection = GetConnection();
        using var command = new MySqlCommand(query, connection);
        setParameters?.Invoke(command);
        var result = command.ExecuteScalar();
        
        if (result == null || result == DBNull.Value) return default;
        return (T)Convert.ChangeType(result, Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T));
    }
    private List<T> GetList<T>(string query, Func<MySqlDataReader, T> mapReader, Action<MySqlCommand>? setParameters = null) {
        var list = new List<T>();
        using var connection = GetConnection();
        using var command = new MySqlCommand(query, connection);
        setParameters?.Invoke(command);
        using var reader = command.ExecuteReader();
        
        while (reader.Read()) { list.Add(mapReader(reader)); }
        
        return list;
    }

    private T? GetSingle<T>(string query, Func<MySqlDataReader, T> mapReader, Action<MySqlCommand>? setParameters = null) where T : class {
        using var connection = GetConnection();
        using var command = new MySqlCommand(query, connection);
        setParameters?.Invoke(command);
        using var reader = command.ExecuteReader();
        
        if (reader.Read()) { return mapReader(reader); }
        
        return null;
    }

    private static string? GetStringNullable(MySqlDataReader reader, string column) {
        return reader.IsDBNull(reader.GetOrdinal(column)) ? null : reader.GetString(column);
    }

    private static int? GetIntNullable(MySqlDataReader reader, string column) {
        return reader.IsDBNull(reader.GetOrdinal(column)) ? null : reader.GetInt32(column);
    }

    public HospitalInfo? GetHospital() {
        return GetSingle("SELECT * FROM Hopital LIMIT 1", reader => new HospitalInfo {
            IdHopital = reader.GetInt32("IdHopital"),
            Nom = reader.GetString("Nom"),
            DirecteurGeneral = GetStringNullable(reader, "DirecteurGeneral") ?? "",
            DirecteurMedical = GetStringNullable(reader, "DirecteurMedical") ?? "",
            Budget = reader.GetDecimal("Budget"),
            Reputation = reader.GetInt32("Reputation"),
            JourSimulation = reader.GetInt32("JourSimulation"),
            HeureSimulation = reader.GetTimeSpan("HeureSimulation").ToString(@"hh\:mm"),
            MontantEmprunt = reader.GetDecimal("MontantEmprunt"),
            TauxInteret = reader.GetDecimal("TauxInteret")
        });
    }

    public void UpdateHospital(decimal budget, int reputation, int jour, string heure) {
        ExecuteCommand(
            "UPDATE Hopital SET Budget=@b, Reputation=@r, JourSimulation=@j, HeureSimulation=@h WHERE IdHopital=1",
            cmd => {
                cmd.Parameters.AddWithValue("@b", budget);
                cmd.Parameters.AddWithValue("@r", reputation);
                cmd.Parameters.AddWithValue("@j", jour);
                cmd.Parameters.AddWithValue("@h", heure);
            });
    }

    public void AddBudget(decimal amount, string description, int jour, string type) {
        ExecuteCommand(
            "UPDATE Hopital SET Budget = Budget + @a WHERE IdHopital=1", 
            cmd => cmd.Parameters.AddWithValue("@a", amount));
            
        LogTransactionInternal(jour, type, amount, description);
    }

    private void LogTransactionInternal(int jour, string type, decimal montant, string desc) {
        ExecuteCommand(
            "INSERT INTO Transactions_Financieres (JourSimulation,TypeTransaction,Montant,Description) VALUES (@j,@t,@m,@d)",
            cmd => {
                cmd.Parameters.AddWithValue("@j", jour);
                cmd.Parameters.AddWithValue("@t", type);
                cmd.Parameters.AddWithValue("@m", montant);
                cmd.Parameters.AddWithValue("@d", desc);
            });
    }

    public List<Employe> GetPersonnel() {
        return GetList("SELECT * FROM Personnel_Actif ORDER BY Categorie, Nom", reader => new Employe {
            IdEmploye = reader.GetInt32("IdEmploye"),
            IdUnite = GetIntNullable(reader, "IdUnite"),
            Nom = reader.GetString("Nom"),
            Prenom = reader.GetString("Prenom"),
            Categorie = reader.GetString("Categorie"),
            RoleExact = reader.GetString("RoleExact"),
            Specialite = GetStringNullable(reader, "Specialite"),
            NiveauCompetence = reader.GetInt32("NiveauCompetence"),
            Experience = reader.GetInt32("Experience"),
            Energie = reader.GetInt32("Energie"),
            Faim = reader.GetInt32("Faim"),
            Stress = reader.GetInt32("Stress"),
            SalaireJour = reader.GetDecimal("SalaireJour"),
            Shift = reader.GetString("Shift"),
            Statut = reader.GetString("Statut")
        });
    }

    public List<Candidat> GetCandidats() {
        return GetList("SELECT * FROM To_Hire ORDER BY NiveauCompetence DESC", reader => new Candidat {
            IdCandidat = reader.GetInt32("IdCandidat"),
            Nom = reader.GetString("Nom"),
            Prenom = reader.GetString("Prenom"),
            Categorie = reader.GetString("Categorie"),
            RoleExact = reader.GetString("RoleExact"),
            Specialite = GetStringNullable(reader, "Specialite"),
            NiveauCompetence = reader.GetInt32("NiveauCompetence"),
            SalaireJour = reader.GetDecimal("SalaireJour"),
            PrimeEmbauche = reader.GetDecimal("PrimeEmbauche")
        });
    }

    public bool HireCandidat(int idCandidat, HospitalInfo hospital) {
        var candidat = GetSingle("SELECT * FROM To_Hire WHERE IdCandidat=@id", reader => new Candidat {
            IdCandidat = reader.GetInt32("IdCandidat"),
            Nom = reader.GetString("Nom"),
            Prenom = reader.GetString("Prenom"),
            Categorie = reader.GetString("Categorie"),
            RoleExact = reader.GetString("RoleExact"),
            Specialite = GetStringNullable(reader, "Specialite"),
            NiveauCompetence = reader.GetInt32("NiveauCompetence"),
            SalaireJour = reader.GetDecimal("SalaireJour"),
            PrimeEmbauche = reader.GetDecimal("PrimeEmbauche")
        }, cmd => cmd.Parameters.AddWithValue("@id", idCandidat));

        if (candidat == null || hospital.Budget < candidat.PrimeEmbauche) return false;

        ExecuteCommand(
            "INSERT INTO Personnel_Actif (Nom,Prenom,Categorie,RoleExact,Specialite,NiveauCompetence,SalaireJour) VALUES (@n,@p,@c,@r,@s,@nc,@sal)",
            cmd => {
                cmd.Parameters.AddWithValue("@n", candidat.Nom);
                cmd.Parameters.AddWithValue("@p", candidat.Prenom);
                cmd.Parameters.AddWithValue("@c", candidat.Categorie);
                cmd.Parameters.AddWithValue("@r", candidat.RoleExact);
                cmd.Parameters.AddWithValue("@s", (object?)candidat.Specialite ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@nc", candidat.NiveauCompetence);
                cmd.Parameters.AddWithValue("@sal", candidat.SalaireJour);
            });

        ExecuteCommand(
            "UPDATE Hopital SET Budget = Budget - @p WHERE IdHopital=1", 
            cmd => cmd.Parameters.AddWithValue("@p", candidat.PrimeEmbauche));

        LogTransactionInternal(hospital.JourSimulation, "Embauche", -candidat.PrimeEmbauche, $"Prime d'embauche: {candidat.Prenom} {candidat.Nom}");

        ExecuteCommand(
            "DELETE FROM To_Hire WHERE IdCandidat=@id", 
            cmd => cmd.Parameters.AddWithValue("@id", idCandidat));

        return true;
    }

    public void FireEmploye(int idEmploye, HospitalInfo hospital) {
        FireEmploye(idEmploye);
    }

    public void FireEmploye(int idEmploye) {
        ExecuteCommand("DELETE FROM Personnel_Actif WHERE IdEmploye=@id", cmd => cmd.Parameters.AddWithValue("@id", idEmploye));
    }

    public List<PatientActif> GetPatients() {
        return GetList("SELECT * FROM Helper_Patients_Dashboard", reader => new PatientActif {
            IdPatient = reader.GetInt32("IdPatient"),
            Nom = reader.GetString("Patient"),
            Maladie = reader.GetString("Maladie"),
            SanteActuelle = reader.GetInt32("SanteActuelle"),
            Satisfaction = reader.GetInt32("Satisfaction"),
            Classe = reader.GetString("Classe"),
            Statut = reader.GetString("Statut"),
            NumeroChambre = GetStringNullable(reader, "NumeroChambre") ?? "N/A",
            NumeroLit = GetStringNullable(reader, "NumeroLit") ?? "N/A",
            MedecinEnCharge = GetStringNullable(reader, "MedecinEnCharge") ?? "Non assigné"
        });
    }

    public List<PatientActif> GetPatientsRaw() {
        return GetList("SELECT * FROM Patients_Actifs", reader => new PatientActif {
            IdPatient = reader.GetInt32("IdPatient"),
            Nom = reader.GetString("Nom"),
            IdMaladie = reader.GetInt32("IdMaladie"),
            IdLit = GetIntNullable(reader, "IdLit"),
            IdMedecinAssigne = GetIntNullable(reader, "IdMedecinAssigné"),
            SanteActuelle = reader.GetInt32("SanteActuelle"),
            Satisfaction = reader.GetInt32("Satisfaction"),
            Classe = reader.GetString("Classe"),
            TempsTraitementRestant = GetIntNullable(reader, "TempsTraitementRestant") ?? 0,
            Statut = reader.GetString("Statut")
        });
    }

    public void AdmitPatient(string nom, int idMaladie, int idLit, int? idMedecin, string classe, int tempsTraitement) {
        ExecuteCommand(
            "INSERT INTO Patients_Actifs (Nom,IdMaladie,IdLit,IdMedecinAssigné,Classe,TempsTraitementRestant,Statut) VALUES (@n,@m,@l,@md,@cl,@t,'En Diagnostic')",
            cmd => {
                cmd.Parameters.AddWithValue("@n", nom);
                cmd.Parameters.AddWithValue("@m", idMaladie);
                cmd.Parameters.AddWithValue("@l", idLit);
                cmd.Parameters.AddWithValue("@md", (object?)idMedecin ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@cl", classe);
                cmd.Parameters.AddWithValue("@t", tempsTraitement);
            });

        ExecuteCommand("UPDATE Lit SET Statut='Occupé' WHERE IdLit=@id", cmd => cmd.Parameters.AddWithValue("@id", idLit));
    }

    public void UpdatePatientStatut(int idPatient, string statut) {
        ExecuteCommand(
            "UPDATE Patients_Actifs SET Statut=@s WHERE IdPatient=@id", 
            cmd => {
                cmd.Parameters.AddWithValue("@s", statut);
                cmd.Parameters.AddWithValue("@id", idPatient);
            });
    }

    public void DischargePatient(int idPatient, int? idLit, decimal revenu, int jour, string typeRevenu) {
        ExecuteCommand("UPDATE Patients_Actifs SET Statut='Guéri' WHERE IdPatient=@id", cmd => cmd.Parameters.AddWithValue("@id", idPatient));
        
        if (idLit.HasValue) {
            ExecuteCommand("UPDATE Lit SET Statut='En Nettoyage' WHERE IdLit=@id", cmd => cmd.Parameters.AddWithValue("@id", idLit.Value));
        }

        ExecuteCommand("UPDATE Hopital SET Budget = Budget + @r WHERE IdHopital=1", cmd => cmd.Parameters.AddWithValue("@r", revenu));
        
        LogTransactionInternal(jour, typeRevenu, revenu, $"Prise en charge patient #{idPatient}");
    }

    public List<CasClinique> GetCasCliniques() {
        return GetList("SELECT IdCas,Maladie,Symptomes,TempsTraitementHeures,RevenuPatient,RisqueErreurMedicale FROM Cas_Cliniques", reader => new CasClinique {
            IdCas = reader.GetInt32("IdCas"),
            Maladie = reader.GetString("Maladie"),
            Symptomes = reader.GetString("Symptomes"),
            TempsTraitementHeures = reader.GetInt32("TempsTraitementHeures"),
            RevenuPatient = reader.GetDecimal("RevenuPatient"),
            RisqueErreurMedicale = reader.GetInt32("RisqueErreurMedicale")
        });
    }

    public List<LitDisponible> GetLitsLibres() {
        return GetList("SELECT * FROM Helper_Disponibilite_Lits", reader => new LitDisponible {
            IdLit = reader.GetInt32("IdLit"),
            Unite = reader.GetString("Unite"),
            NumeroChambre = reader.GetString("NumeroChambre"),
            TypeChambre = reader.GetString("TypeChambre"),
            NumeroLit = reader.GetString("NumeroLit")
        });
    }

    public void FreeBed(int idLit) {
        ExecuteCommand("UPDATE Lit SET Statut='Libre' WHERE IdLit=@id", cmd => cmd.Parameters.AddWithValue("@id", idLit));
    }

    public void UpdateTransaction(int idTransaction, decimal montant, string description) {
        ExecuteCommand(
            "UPDATE Transactions_Financieres SET Montant=@m, Description=@d WHERE IdTransaction=@id", 
            cmd => {
                cmd.Parameters.AddWithValue("@m", montant);
                cmd.Parameters.AddWithValue("@d", description);
                cmd.Parameters.AddWithValue("@id", idTransaction);
            });
    }

    public void DeleteTransaction(int idTransaction) {
        ExecuteCommand("DELETE FROM Transactions_Financieres WHERE IdTransaction=@id", cmd => cmd.Parameters.AddWithValue("@id", idTransaction));
    }

    public void SendBillToPatient(int idPatient, decimal amount, string description) {
        ExecuteCommand("UPDATE Hopital SET Budget = Budget + @a WHERE IdHopital=1", cmd => cmd.Parameters.AddWithValue("@a", amount));
        LogTransactionInternal(0, "Facture Patient", amount, description);
    }

    public void SetPayrollDay(int dayOfMonth) {
        ExecuteCommand("UPDATE Hopital SET JourPaie=@day WHERE IdHopital=1", cmd => cmd.Parameters.AddWithValue("@day", dayOfMonth));
    }

    public int GetPayrollDay() {
        return ExecuteScalar<int>("SELECT JourPaie FROM Hopital LIMIT 1");
    }

    public void CreateEmploye(string nom, string prenom, string categorie, string roleExact, string? specialite, int niveauCompetence, decimal salaireJour, string shift = "Jour") {
        ExecuteCommand(
            "INSERT INTO Personnel_Actif (Nom, Prenom, Categorie, RoleExact, Specialite, NiveauCompetence, SalaireJour, Shift, Energie, Faim, Stress, Statut) VALUES (@n, @p, @c, @r, @s, @nc, @sal, @shift, 100, 0, 0, 'En poste')",
            cmd => {
                cmd.Parameters.AddWithValue("@n", nom);
                cmd.Parameters.AddWithValue("@p", prenom);
                cmd.Parameters.AddWithValue("@c", categorie);
                cmd.Parameters.AddWithValue("@r", roleExact);
                cmd.Parameters.AddWithValue("@s", (object?)specialite ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@nc", niveauCompetence);
                cmd.Parameters.AddWithValue("@sal", salaireJour);
                cmd.Parameters.AddWithValue("@shift", shift);
            });
    }

    public void UpdateEmploye(int idEmploye, string nom, string prenom, string categorie, string roleExact, string? specialite, int niveauCompetence, decimal salaireJour, string shift) {
        ExecuteCommand(
            "UPDATE Personnel_Actif SET Nom=@n, Prenom=@p, Categorie=@c, RoleExact=@r, Specialite=@s, NiveauCompetence=@nc, SalaireJour=@sal, Shift=@shift WHERE IdEmploye=@id",
            cmd => {
                cmd.Parameters.AddWithValue("@n", nom);
                cmd.Parameters.AddWithValue("@p", prenom);
                cmd.Parameters.AddWithValue("@c", categorie);
                cmd.Parameters.AddWithValue("@r", roleExact);
                cmd.Parameters.AddWithValue("@s", (object?)specialite ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@nc", niveauCompetence);
                cmd.Parameters.AddWithValue("@sal", salaireJour);
                cmd.Parameters.AddWithValue("@shift", shift);
                cmd.Parameters.AddWithValue("@id", idEmploye);
            });
    }

    public List<Transaction> GetRecentTransactions(int limit = 20) {
        return GetList($"SELECT * FROM Transactions_Financieres ORDER BY IdTransaction DESC LIMIT {limit}", reader => new Transaction {
            IdTransaction = reader.GetInt32("IdTransaction"),
            JourSimulation = reader.GetInt32("JourSimulation"),
            TypeTransaction = reader.GetString("TypeTransaction"),
            Montant = reader.GetDecimal("Montant"),
            Description = GetStringNullable(reader, "Description") ?? ""
        });
    }

    public DashboardStats GetStats() {
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

        var result = GetSingle(sql, reader => new DashboardStats {
            TotalPatientsActifs = reader.GetInt32("PatientsActifs"),
            TotalPersonnel = reader.GetInt32("TotalPersonnel"),
            LitsLibres = reader.GetInt32("LitsLibres"),
            LitsOccupes = reader.GetInt32("LitsOccupes"),
            RevenuJour = reader.GetDecimal("RevenuJour"),
            DepensesJour = reader.GetDecimal("DepensesJour"),
            PatientsEnAttente = reader.GetInt32("EnAttente"),
            PatientsEnTraitement = reader.GetInt32("EnTraitement"),
            PatientsGueris = reader.GetInt32("Gueris"),
            PatientsDeces = reader.GetInt32("Deces")
        });

        return result ?? new DashboardStats();
    }

    public List<string> AdvanceHour(HospitalInfo hospital) {
        var log = new List<string>();
        var rng = new Random();

        var timeSpan = TimeSpan.Parse(hospital.HeureSimulation).Add(TimeSpan.FromHours(1));
        var newJour = hospital.JourSimulation;
        
        if (timeSpan.TotalHours >= 24) {
            timeSpan = timeSpan.Subtract(TimeSpan.FromHours(24));
            newJour++;
        }

        var newHeure = timeSpan.ToString(@"hh\:mm");
        var salaireHoraire = ExecuteScalar<decimal>("SELECT COALESCE(SUM(SalaireJour),0) FROM Personnel_Actif WHERE Statut != 'Absent'") / 24m;

        if (salaireHoraire > 0) {
            ExecuteCommand("UPDATE Hopital SET Budget = Budget - @s WHERE IdHopital=1", cmd => cmd.Parameters.AddWithValue("@s", salaireHoraire));
            if (timeSpan.Hours == 0) {
                LogTransactionInternal(newJour, "Paiement Salaire", -salaireHoraire * 24, $"Salaires quotidiens - {hospital.JourSimulation + 1} employés actifs");
            }
        }

        if (timeSpan.Hours == 0) {
            var entretien = ExecuteScalar<decimal>("SELECT COALESCE(SUM(CoutEntretienJour),0) FROM Unite");
            ExecuteCommand("UPDATE Hopital SET Budget = Budget - @e WHERE IdHopital=1", cmd => cmd.Parameters.AddWithValue("@e", entretien));
            
            if (entretien > 0) {
                LogTransactionInternal(newJour, "Frais Entretien", -entretien, "Coût d'entretien quotidien des unités");
                log.Add($"💰 Entretien des unités: -{entretien:C0}");
            }
        }

        var patients = GetList(
            "SELECT IdPatient,TempsTraitementRestant,IdMaladie,IdLit,IdMedecinAssigné,Classe FROM Patients_Actifs WHERE Statut IN ('En Diagnostic','En Traitement')",
            reader => (
                reader.GetInt32(0), 
                GetIntNullable(reader, "TempsTraitementRestant") ?? 0, 
                reader.GetInt32(2), 
                GetIntNullable(reader, "IdLit"), 
                GetIntNullable(reader, "IdMedecinAssigné"), 
                reader.GetString(5)
            ));

        foreach (var (id, tempsRestant, idMaladie, idLit, idMedecin, classe) in patients) {
            var newTemps = tempsRestant - 1;

            ExecuteCommand(
                "UPDATE Patients_Actifs SET Statut='En Traitement', TempsTraitementRestant=@t WHERE IdPatient=@id AND Statut='En Diagnostic'",
                cmd => {
                    cmd.Parameters.AddWithValue("@t", newTemps);
                    cmd.Parameters.AddWithValue("@id", id);
                });

            if (newTemps <= 0) {
                var revenu = ExecuteScalar<decimal>("SELECT RevenuPatient FROM Cas_Cliniques WHERE IdCas=@m", cmd => cmd.Parameters.AddWithValue("@m", idMaladie));
                
                if (classe == "VIP") revenu *= 2m;
                else if (classe == "Non-assuré") revenu *= 0.3m;

                ExecuteCommand("UPDATE Patients_Actifs SET Statut='Guéri',TempsTraitementRestant=0 WHERE IdPatient=@id", cmd => cmd.Parameters.AddWithValue("@id", id));
                if (idLit.HasValue) FreeBed(idLit.Value);
                
                ExecuteCommand("UPDATE Hopital SET Budget=Budget+@r WHERE IdHopital=1", cmd => cmd.Parameters.AddWithValue("@r", revenu));
                
                LogTransactionInternal(newJour, "Revenu Patient", revenu, $"Patient #{id} traité ({classe})");
                log.Add($"✅ Patient #{id} guéri → +{revenu:C0}");
                
                ExecuteCommand("UPDATE Hopital SET Reputation=LEAST(100,Reputation+1) WHERE IdHopital=1");
            } else {
                ExecuteCommand("UPDATE Patients_Actifs SET TempsTraitementRestant=@t WHERE IdPatient=@id", cmd => {
                    cmd.Parameters.AddWithValue("@t", newTemps);
                    cmd.Parameters.AddWithValue("@id", id);
                });

                var risk = ExecuteScalar<int>("SELECT RisqueErreurMedicale FROM Cas_Cliniques WHERE IdCas=@m", cmd => cmd.Parameters.AddWithValue("@m", idMaladie));
                var competence = idMedecin.HasValue ? ExecuteScalar<int>("SELECT NiveauCompetence FROM Personnel_Actif WHERE IdEmploye=@e", cmd => cmd.Parameters.AddWithValue("@e", idMedecin.Value)) : 50;

                var effectiveRisk = Math.Max(1, risk - competence / 20);
                
                if (rng.Next(100) < effectiveRisk) {
                    var damage = rng.Next(5, 20);
                    ExecuteCommand(
                        "UPDATE Patients_Actifs SET SanteActuelle=GREATEST(0,SanteActuelle-@d), Satisfaction=GREATEST(0,Satisfaction-5) WHERE IdPatient=@id",
                        cmd => 
                        {
                            cmd.Parameters.AddWithValue("@d", damage);
                            cmd.Parameters.AddWithValue("@id", id);
                        });
                        
                    log.Add($"⚠️ Complication patient #{id} (-{damage} santé)");

                    var sante = ExecuteScalar<int>("SELECT SanteActuelle FROM Patients_Actifs WHERE IdPatient=@id", cmd => cmd.Parameters.AddWithValue("@id", id));
                    
                    if (sante <= 0) {
                        ExecuteCommand("UPDATE Patients_Actifs SET Statut='Décédé' WHERE IdPatient=@id", cmd => cmd.Parameters.AddWithValue("@id", id));
                        if (idLit.HasValue) FreeBed(idLit.Value);
                        
                        ExecuteCommand("UPDATE Hopital SET Reputation=GREATEST(0,Reputation-5) WHERE IdHopital=1");
                        log.Add($"💀 Patient #{id} est décédé");
                    }
                }
            }
        }

        ExecuteCommand("UPDATE Personnel_Actif SET Energie=GREATEST(0,Energie-2), Stress=LEAST(100,Stress+1), Faim=GREATEST(0,Faim-1) WHERE Statut='En poste'");
        ExecuteCommand("UPDATE Personnel_Actif SET Statut='Épuisé' WHERE Energie <= 10 AND Statut='En poste'");
        
        ExecuteCommand("UPDATE Hopital SET HeureSimulation=@h, JourSimulation=@j WHERE IdHopital=1", cmd => 
        {
            cmd.Parameters.AddWithValue("@h", newHeure + ":00");
            cmd.Parameters.AddWithValue("@j", newJour);
        });

        if (log.Count == 0) log.Add($"⏱ Heure avancée → Jour {newJour} {newHeure}");
        
        return log;
    }

    public bool TestConnection() {
        try {
            using var connection = GetConnection();
            return true;
        } catch { return false; }
    }

    public void ExecuteNonQuery(string query) {
        ExecuteCommand(query);
    }

    public T? ExecuteScalar<T>(string query) {
        return ExecuteScalar<T>(query, null);
    }

    public void RecordTransaction(string type, decimal montant, string description) {
        var hospital = GetHospital();
        if (hospital != null) {
            LogTransactionInternal(hospital.JourSimulation, type, montant, description);
        }
    }

    public void ReleaseBed(int idPatient) {
        var idLit = ExecuteScalar<int?>("SELECT IdLit FROM Patients_Actifs WHERE IdPatient=@id", cmd => cmd.Parameters.AddWithValue("@id", idPatient));
        if (idLit.HasValue) FreeBed(idLit.Value);
    }

    public List<LitInventaire> GetLitsInventaire() {
        var sql = @"
            SELECT l.IdLit, l.NumeroLit, c.NumeroChambre, c.TypeChambre, u.Nom AS Unite, l.Statut 
            FROM Lit l 
            JOIN Chambre c ON l.IdChambre = c.IdChambre 
            JOIN Unite u ON c.IdUnite = u.IdUnite";

        return GetList(sql, reader => new LitInventaire {
            IdLit = reader.GetInt32("IdLit"),
            NumeroLit = reader.GetString("NumeroLit"),
            NumeroChambre = reader.GetString("NumeroChambre"),
            TypeChambre = reader.GetString("TypeChambre"),
            Unite = reader.GetString("Unite"),
            Statut = reader.GetString("Statut")
        });
    }

    public ObservableCollection<ChambreGroup> GetChambresHierarchiques() {
        var chambres = new Dictionary<int, ChambreGroup>();
        var sqlChambres = "SELECT c.IdChambre, c.NumeroChambre, c.TypeChambre, u.Nom AS Unite FROM Chambre c JOIN Unite u ON c.IdUnite = u.IdUnite ORDER BY c.NumeroChambre";
        
        GetList(sqlChambres, reader => {
            var id = reader.GetInt32("IdChambre");
            chambres[id] = new ChambreGroup {
                IdChambre = id,
                NumeroChambre = reader.GetString("NumeroChambre"),
                TypeChambre = reader.GetString("TypeChambre"),
                Unite = reader.GetString("Unite")
            };
            return 0;
        });

        GetList("SELECT IdLit, IdChambre, NumeroLit, Statut FROM Lit", reader => {
            var idChambre = reader.GetInt32("IdChambre");
            if (chambres.TryGetValue(idChambre, out var chambreGroup)) {
                chambreGroup.Lits.Add(new LitInventaire {
                    IdLit = reader.GetInt32("IdLit"),
                    IdChambre = idChambre,
                    NumeroLit = reader.GetString("NumeroLit"),
                    Statut = reader.GetString("Statut")
                });
            }
            return 0;
        });

        return new ObservableCollection<ChambreGroup>(chambres.Values);
    }

    public void UpdateLitChambre(int idLit, int nouvelleIdChambre) {
        ExecuteCommand("UPDATE Lit SET IdChambre = @idC WHERE IdLit = @idL", cmd => {
            cmd.Parameters.AddWithValue("@idC", nouvelleIdChambre);
            cmd.Parameters.AddWithValue("@idL", idLit);
        });
    }

    public List<ChambreBase> GetChambresList() {
        return GetList("SELECT IdChambre, NumeroChambre, TypeChambre FROM Chambre ORDER BY NumeroChambre", reader => new ChambreBase {
            IdChambre = reader.GetInt32("IdChambre"),
            NumeroChambre = reader.GetString("NumeroChambre"),
            TypeChambre = reader.GetString("TypeChambre")
        });
    }

    public void AddLit(int idChambre, string numeroLit) {
        ExecuteCommand("INSERT INTO Lit (IdChambre, NumeroLit, Statut) VALUES (@id, @num, 'Libre')", cmd => {
            cmd.Parameters.AddWithValue("@id", idChambre);
            cmd.Parameters.AddWithValue("@num", numeroLit);
        });
    }

    public List<UniteBase> GetUnitesList() {
        return GetList("SELECT IdUnite, Nom FROM Unite ORDER BY Nom", reader => new UniteBase {
            IdUnite = reader.GetInt32("IdUnite"),
            Nom = reader.GetString("Nom")
        });
    }

    public void AddChambre(int idUnite, string numeroChambre, string typeChambre) {
        ExecuteCommand("INSERT INTO Chambre (IdUnite, NumeroChambre, TypeChambre) VALUES (@id, @num, @type)", cmd => {
            cmd.Parameters.AddWithValue("@id", idUnite);
            cmd.Parameters.AddWithValue("@num", numeroChambre);
            cmd.Parameters.AddWithValue("@type", typeChambre);
        });
    }

    public void RemoveChambre(int idChambre) {
        ExecuteCommand("DELETE FROM Chambre WHERE IdChambre = @id", cmd => cmd.Parameters.AddWithValue("@id", idChambre));
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