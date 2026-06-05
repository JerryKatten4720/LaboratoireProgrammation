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
        $"Server={Host};Port={Port};Database={Database};Uid={User};Pwd={Password};CharSet=utf8;";

    public string ConnectionString => _connectionString;

    private MySqlConnection GetConnection() {
        var connection = new MySqlConnection(_connectionString);
        connection.Open();
        return connection;
    }

    public void ExecuteCommand(string query, Action<MySqlCommand>? setParameters = null) {
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
            Experience = reader.GetInt32("Experience"),
            SalaireJour = reader.GetDecimal("SalaireJour"),
            Shift = reader.GetString("Shift"),
            Statut = reader.GetString("Statut"),
            MinutesTravaillees = reader.GetInt32("MinutesTravaillees"),
            MinutesEnPause = reader.GetInt32("MinutesEnPause"),
            MinutesHorsPoste = reader.GetInt32("MinutesHorsPoste"),
            ProchainePauseMinutes = reader.IsDBNull(reader.GetOrdinal("ProchainePauseMinutes")) ? 120 : reader.GetInt32("ProchainePauseMinutes")
        });
    }

    public List<Candidat> GetCandidats() {
        return GetList("SELECT * FROM To_Hire", reader => new Candidat {
            IdCandidat = reader.GetInt32("IdCandidat"),
            Nom = reader.GetString("Nom"),
            Prenom = reader.GetString("Prenom"),
            Categorie = reader.GetString("Categorie"),
            RoleExact = reader.GetString("RoleExact"),
            Specialite = GetStringNullable(reader, "Specialite"),
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
            SalaireJour = reader.GetDecimal("SalaireJour"),
            PrimeEmbauche = reader.GetDecimal("PrimeEmbauche")
        }, cmd => cmd.Parameters.AddWithValue("@id", idCandidat));

        if (candidat == null || hospital.Budget < candidat.PrimeEmbauche) return false;

        ExecuteCommand(
            "INSERT INTO Personnel_Actif (Nom,Prenom,Categorie,RoleExact,Specialite,SalaireJour) VALUES (@n,@p,@c,@r,@s,@sal)",
            cmd => {
                cmd.Parameters.AddWithValue("@n", candidat.Nom);
                cmd.Parameters.AddWithValue("@p", candidat.Prenom);
                cmd.Parameters.AddWithValue("@c", candidat.Categorie);
                cmd.Parameters.AddWithValue("@r", candidat.RoleExact);
                cmd.Parameters.AddWithValue("@s", (object?)candidat.Specialite ?? DBNull.Value);
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
        var sql = @"
            SELECT 
                p.IdPatient,
                p.Nom AS Patient,
                m.Maladie,
                p.SanteActuelle,
                p.Satisfaction,
                p.Classe,
                p.Statut,
                c.NumeroChambre,
                l.NumeroLit,
                med.Nom AS MedecinEnCharge,
                p.TempsTraitementRestant,
                p.IdMaladie,
                p.IdLit,
                p.IdMedecinAssigné,
                p.DateEntree,
                p.IdFacture,
                m.TempsTraitementHeures,
                m.PrixChambreJour,
                m.PrixSoinBase,
                (SELECT JourSimulation FROM Hopital LIMIT 1) AS JourSimulation,
                COALESCE((
                    SELECT SUM(pr.Quantite * med_p.PrixUnitaire)
                    FROM Prescription pr
                    JOIN Medicament med_p ON pr.IdMedicament = med_p.IdMedicament
                    WHERE pr.IdPatient = p.IdPatient
                ), 0) AS MontantMedicaments
            FROM Patients_Actifs p
            JOIN Cas_Cliniques m ON p.IdMaladie = m.IdCas
            LEFT JOIN Lit l ON p.IdLit = l.IdLit
            LEFT JOIN Chambre c ON l.IdChambre = c.IdChambre
            LEFT JOIN Personnel_Actif med ON p.IdMedecinAssigné = med.IdEmploye";

        return GetList(sql, reader => {
            int currentJour = reader.GetInt32("JourSimulation");
            int dateEntree = reader.IsDBNull(reader.GetOrdinal("DateEntree")) ? currentJour : reader.GetInt32("DateEntree");
            int duree = currentJour - dateEntree;
            if (duree <= 0) duree = 1;
            decimal prixChambreJour = reader.GetDecimal("PrixChambreJour");
            decimal prixSoinBase = reader.GetDecimal("PrixSoinBase");
            decimal montMedicaments = reader.GetDecimal("MontantMedicaments");
            decimal estimatedProfit = (duree * prixChambreJour) + prixSoinBase + montMedicaments;

            return new PatientActif {
                IdPatient = reader.GetInt32("IdPatient"),
                Nom = reader.GetString("Patient"),
                Maladie = reader.GetString("Maladie"),
                SanteActuelle = reader.GetInt32("SanteActuelle"),
                Satisfaction = reader.GetInt32("Satisfaction"),
                Classe = reader.GetString("Classe"),
                Statut = reader.GetString("Statut"),
                NumeroChambre = GetStringNullable(reader, "NumeroChambre") ?? "N/A",
                NumeroLit = GetStringNullable(reader, "NumeroLit") ?? "N/A",
                MedecinEnCharge = GetStringNullable(reader, "MedecinEnCharge") ?? "Non assigné",
                TempsTraitementRestant = reader.IsDBNull(reader.GetOrdinal("TempsTraitementRestant")) ? 0f : reader.GetFloat("TempsTraitementRestant"),
                IdMaladie = reader.GetInt32("IdMaladie"),
                IdLit = GetIntNullable(reader, "IdLit"),
                IdMedecinAssigne = GetIntNullable(reader, "IdMedecinAssigné"),
                DateEntree = reader.IsDBNull(reader.GetOrdinal("DateEntree")) ? (int?)null : reader.GetInt32("DateEntree"),
                IdFacture = GetIntNullable(reader, "IdFacture"),
                TempsTraitementTotal = reader.IsDBNull(reader.GetOrdinal("TempsTraitementHeures")) ? 0f : reader.GetFloat("TempsTraitementHeures"),
                EstimatedProfit = estimatedProfit
            };
        });
    }

    public List<PatientActif> GetPatientsRaw() {
        return GetList(@"
            SELECT p.*, c.TempsTraitementHeures 
            FROM Patients_Actifs p 
            JOIN Cas_Cliniques c ON p.IdMaladie = c.IdCas", reader => new PatientActif {
            IdPatient = reader.GetInt32("IdPatient"),
            Nom = reader.GetString("Nom"),
            IdMaladie = reader.GetInt32("IdMaladie"),
            IdLit = GetIntNullable(reader, "IdLit"),
            IdMedecinAssigne = GetIntNullable(reader, "IdMedecinAssigné"),
            SanteActuelle = reader.GetInt32("SanteActuelle"),
            Satisfaction = reader.GetInt32("Satisfaction"),
            Classe = reader.GetString("Classe"),
            TempsTraitementRestant = reader.IsDBNull(reader.GetOrdinal("TempsTraitementRestant")) ? 0f : reader.GetFloat("TempsTraitementRestant"),
            Statut = reader.GetString("Statut"),
            TempsTraitementTotal = reader.IsDBNull(reader.GetOrdinal("TempsTraitementHeures")) ? 0f : reader.GetFloat("TempsTraitementHeures")
        });
    }

    public void AdmitPatient(string nom, int idMaladie, int idLit, int? idMedecin, string classe, float tempsTraitement) {
        if (idMedecin.HasValue && GetDoctorActivePatientCount(idMedecin.Value) >= 3) {
            throw new InvalidOperationException("Ce médecin a déjà 3 patients à sa charge.");
        }

        var jour = ExecuteScalar<int>("SELECT JourSimulation FROM Hopital LIMIT 1");
        ExecuteCommand(
            "INSERT INTO Patients_Actifs (Nom,IdMaladie,IdLit,IdMedecinAssigné,Classe,TempsTraitementRestant,Statut,DateEntree) VALUES (@n,@m,@l,@md,@cl,@t,'En Diagnostic',@j)",
            cmd => {
                cmd.Parameters.AddWithValue("@n", nom);
                cmd.Parameters.AddWithValue("@m", idMaladie);
                cmd.Parameters.AddWithValue("@l", idLit);
                cmd.Parameters.AddWithValue("@md", (object?)idMedecin ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@cl", classe);
                cmd.Parameters.AddWithValue("@t", tempsTraitement);
                cmd.Parameters.AddWithValue("@j", jour);
            });

        int idPatient = ExecuteScalar<int>("SELECT MAX(IdPatient) FROM Patients_Actifs");
        
        if (idMedecin.HasValue) {
            ExecuteCommand("INSERT INTO Reservation (IdPatient, IdLit, IdMedecin, DateEntree, Statut) VALUES (@p, @l, @m, @d, 'Active')", cmd => {
                cmd.Parameters.AddWithValue("@p", idPatient);
                cmd.Parameters.AddWithValue("@l", idLit);
                cmd.Parameters.AddWithValue("@m", idMedecin.Value);
                cmd.Parameters.AddWithValue("@d", jour);
            });
        }

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

    public void UpdatePatientDoctor(int idPatient, int? idMedecin) {
        if (idMedecin.HasValue && GetDoctorActivePatientCount(idMedecin.Value) >= 3) {
            throw new InvalidOperationException("Ce médecin a déjà 3 patients à sa charge.");
        }

        ExecuteCommand("UPDATE Patients_Actifs SET IdMedecinAssigné = @med WHERE IdPatient = @id", cmd => {
            cmd.Parameters.AddWithValue("@med", (object?)idMedecin ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@id", idPatient);
        });

        ExecuteCommand("UPDATE Reservation SET IdMedecin = @med WHERE IdPatient = @id AND Statut = 'Active'", cmd => {
            cmd.Parameters.AddWithValue("@med", (object?)idMedecin ?? DBNull.Value);
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
        return GetList("SELECT IdCas,Maladie,Symptomes,TempsTraitementHeures,RevenuPatient,RisqueErreurMedicale,TauxRemission,SpecialisteTraitement,UniteRequise,CoutLogistique FROM Cas_Cliniques", reader => new CasClinique {
            IdCas = reader.GetInt32("IdCas"),
            Maladie = reader.GetString("Maladie"),
            Symptomes = reader.GetString("Symptomes"),
            TempsTraitementHeures = reader.IsDBNull(3) ? 0f : reader.GetFloat(3),
            RevenuPatient = reader.GetDecimal("RevenuPatient"),
            RisqueErreurMedicale = reader.GetInt32("RisqueErreurMedicale"),
            TauxRemission = reader.IsDBNull(6) ? 50f : reader.GetFloat(6),
            SpecialisteTraitement = reader.IsDBNull(7) ? "" : reader.GetString(7),
            UniteRequise = reader.IsDBNull(8) ? null : reader.GetString(8),
            CoutLogistique = reader.IsDBNull(9) ? 0.00m : reader.GetDecimal(9)
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

    public void CreateEmploye(string nom, string prenom, string categorie, string roleExact, string? specialite, decimal salaireJour, string shift = "Jour") {
        ExecuteCommand(
            "INSERT INTO Personnel_Actif (Nom, Prenom, Categorie, RoleExact, Specialite, SalaireJour, Shift, Statut) VALUES (@n, @p, @c, @r, @s, @sal, @shift, 'En poste')",
            cmd => {
                cmd.Parameters.AddWithValue("@n", nom);
                cmd.Parameters.AddWithValue("@p", prenom);
                cmd.Parameters.AddWithValue("@c", categorie);
                cmd.Parameters.AddWithValue("@r", roleExact);
                cmd.Parameters.AddWithValue("@s", (object?)specialite ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@sal", salaireJour);
                cmd.Parameters.AddWithValue("@shift", shift);
            });
    }

    public void UpdateEmploye(int idEmploye, string nom, string prenom, string categorie, string roleExact, string? specialite, decimal salaireJour, string shift) {
        ExecuteCommand(
            "UPDATE Personnel_Actif SET Nom=@n, Prenom=@p, Categorie=@c, RoleExact=@r, Specialite=@s, SalaireJour=@sal, Shift=@shift WHERE IdEmploye=@id",
            cmd => {
                cmd.Parameters.AddWithValue("@n", nom);
                cmd.Parameters.AddWithValue("@p", prenom);
                cmd.Parameters.AddWithValue("@c", categorie);
                cmd.Parameters.AddWithValue("@r", roleExact);
                cmd.Parameters.AddWithValue("@s", (object?)specialite ?? DBNull.Value);
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

    public List<Medicament> GetMedicaments() {
        return GetList("SELECT * FROM Medicament", reader => new Medicament {
            IdMedicament = reader.GetInt32("IdMedicament"),
            Nom = reader.GetString("Nom"),
            DCI = GetStringNullable(reader, "DCI") ?? "",
            Forme = GetStringNullable(reader, "Forme") ?? "",
            StockActuel = reader.GetInt32("StockActuel"),
            StockMinimum = reader.GetInt32("StockMinimum"),
            PrixUnitaire = reader.GetDecimal("PrixUnitaire"),
            IdUnite = reader.GetInt32("IdUnite"),
            MaladiesCibles = GetStringNullable(reader, "Maladies_Cibles"),
            MaladiesIncompatibles = GetStringNullable(reader, "Maladies_Incompatibles"),
            TempsLivraisonBase = reader.GetInt32("Temps_Livraison_Base"),
            QuantitePrescriptionDefaut = reader.GetInt32("Quantite_Prescription_Defaut")
        });
    }

    public void AddMedicament(string nom, string dci, string forme, int stockActuel, int stockMin, decimal prix, int idUnite, string cibles, string incompatibles, int tempsLivraison) {
        ExecuteCommand("INSERT INTO Medicament (Nom, DCI, Forme, StockActuel, StockMinimum, PrixUnitaire, IdUnite, Maladies_Cibles, Maladies_Incompatibles, Temps_Livraison_Base) VALUES (@n, @d, @f, @sa, @sm, @p, @u, @c, @i, @tl)", cmd => {
            cmd.Parameters.AddWithValue("@n", nom);
            cmd.Parameters.AddWithValue("@d", dci);
            cmd.Parameters.AddWithValue("@f", forme);
            cmd.Parameters.AddWithValue("@sa", stockActuel);
            cmd.Parameters.AddWithValue("@sm", stockMin);
            cmd.Parameters.AddWithValue("@p", prix);
            cmd.Parameters.AddWithValue("@u", idUnite);
            cmd.Parameters.AddWithValue("@c", cibles);
            cmd.Parameters.AddWithValue("@i", incompatibles);
            cmd.Parameters.AddWithValue("@tl", tempsLivraison);
        });
    }

    public void UpdateMedicament(int id, string nom, string dci, string forme, int stockActuel, int stockMin, decimal prix, int idUnite, string cibles, string incompatibles, int tempsLivraison) {
        ExecuteCommand("UPDATE Medicament SET Nom=@n, DCI=@d, Forme=@f, StockActuel=@sa, StockMinimum=@sm, PrixUnitaire=@p, IdUnite=@u, Maladies_Cibles=@c, Maladies_Incompatibles=@i, Temps_Livraison_Base=@tl WHERE IdMedicament=@id", cmd => {
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@n", nom);
            cmd.Parameters.AddWithValue("@d", dci);
            cmd.Parameters.AddWithValue("@f", forme);
            cmd.Parameters.AddWithValue("@sa", stockActuel);
            cmd.Parameters.AddWithValue("@sm", stockMin);
            cmd.Parameters.AddWithValue("@p", prix);
            cmd.Parameters.AddWithValue("@u", idUnite);
            cmd.Parameters.AddWithValue("@c", cibles);
            cmd.Parameters.AddWithValue("@i", incompatibles);
            cmd.Parameters.AddWithValue("@tl", tempsLivraison);
        });
    }

    public void UpdateStock(int idMed, int delta) {
        ExecuteCommand("UPDATE Medicament SET StockActuel = StockActuel + @d WHERE IdMedicament = @id", cmd => {
            cmd.Parameters.AddWithValue("@d", delta);
            cmd.Parameters.AddWithValue("@id", idMed);
        });
    }

    public void AddPrescription(int idPatient, int idMedecin, int idMed, int qte, string posologie) {
        var jour = ExecuteScalar<int>("SELECT JourSimulation FROM Hopital LIMIT 1");
        ExecuteCommand("INSERT INTO Prescription (IdPatient, IdMedecin, IdMedicament, Quantite, Posologie, DatePrescription, Statut) VALUES (@p, @m, @med, @q, @pos, @d, 'En cours')", cmd => {
            cmd.Parameters.AddWithValue("@p", idPatient);
            cmd.Parameters.AddWithValue("@m", idMedecin);
            cmd.Parameters.AddWithValue("@med", idMed);
            cmd.Parameters.AddWithValue("@q", qte);
            cmd.Parameters.AddWithValue("@pos", posologie);
            cmd.Parameters.AddWithValue("@d", jour);
        });
        UpdateStock(idMed, -qte);
    }

    public List<Prescription> GetPrescriptionsByPatient(int idPatient) {
        return GetList("SELECT * FROM Prescription WHERE IdPatient=@id", reader => new Prescription {
            IdPrescription = reader.GetInt32("IdPrescription"),
            IdPatient = reader.GetInt32("IdPatient"),
            IdMedecin = reader.GetInt32("IdMedecin"),
            IdMedicament = reader.GetInt32("IdMedicament"),
            Quantite = reader.GetInt32("Quantite"),
            Posologie = GetStringNullable(reader, "Posologie") ?? "",
            DatePrescription = reader.GetInt32("DatePrescription"),
            Statut = GetStringNullable(reader, "Statut") ?? ""
        }, cmd => cmd.Parameters.AddWithValue("@id", idPatient));
    }

    public int GenerateFacture(int idPatient) {
        var dateEntree = ExecuteScalar<int>("SELECT DateEntree FROM Patients_Actifs WHERE IdPatient=@id", cmd => cmd.Parameters.AddWithValue("@id", idPatient));
        var idMaladie = ExecuteScalar<int>("SELECT IdMaladie FROM Patients_Actifs WHERE IdPatient=@id", cmd => cmd.Parameters.AddWithValue("@id", idPatient));
        if (dateEntree == 0) dateEntree = 1;
        var hopitalJour = ExecuteScalar<int>("SELECT JourSimulation FROM Hopital LIMIT 1");
        int duree = hopitalJour - dateEntree;
        if (duree <= 0) duree = 1;

        decimal prixChambreJour = ExecuteScalar<decimal>("SELECT PrixChambreJour FROM Cas_Cliniques WHERE IdCas=@id", cmd => cmd.Parameters.AddWithValue("@id", idMaladie));
        decimal prixSoinBase = ExecuteScalar<decimal>("SELECT PrixSoinBase FROM Cas_Cliniques WHERE IdCas=@id", cmd => cmd.Parameters.AddWithValue("@id", idMaladie));
        decimal prixMaladie = ExecuteScalar<decimal>("SELECT RevenuPatient FROM Cas_Cliniques WHERE IdCas=@id", cmd => cmd.Parameters.AddWithValue("@id", idMaladie));

        float tempsSansMed = ExecuteScalar<float>("SELECT TempsSansMedecin FROM Patients_Actifs WHERE IdPatient=@id", cmd => cmd.Parameters.AddWithValue("@id", idPatient));
        float tempsTotal = ExecuteScalar<float>("SELECT TempsTraitementHeures FROM Cas_Cliniques WHERE IdCas=@id", cmd => cmd.Parameters.AddWithValue("@id", idMaladie));
        if (tempsTotal <= 0f) tempsTotal = 1f;
        float negligenceRate = tempsSansMed / tempsTotal;
        if (negligenceRate > 1f) negligenceRate = 1f;

        decimal discount = 1.0m - (decimal)negligenceRate;

        var patientStatut = ExecuteScalar<string>("SELECT Statut FROM Patients_Actifs WHERE IdPatient=@id", cmd => cmd.Parameters.AddWithValue("@id", idPatient));
        bool isDecede = (patientStatut == "Décédé");

        decimal prixChambreJourEffectif = (isDecede ? prixChambreJour / 2 : prixChambreJour) * discount;
        decimal prixSoinBaseEffectif = (isDecede ? prixSoinBase / 2 : prixSoinBase) * discount;
        
        decimal unitPriceHopitalisation = prixMaladie * 0.0015m;
        decimal unitPriceHopitalisationEffectif = (isDecede ? unitPriceHopitalisation / 2 : unitPriceHopitalisation) * discount;
        decimal sTotalHopitalisation = (decimal)tempsTotal * unitPriceHopitalisationEffectif;

        decimal mChambre = duree * prixChambreJourEffectif;
        decimal mChambreCombined = mChambre + sTotalHopitalisation;
        decimal mSoins = prixSoinBaseEffectif;
        
        var prescriptions = GetPrescriptionsByPatient(idPatient);
        decimal mMed = 0;
        foreach (var p in prescriptions) {
            var medPrix = ExecuteScalar<decimal>("SELECT PrixUnitaire FROM Medicament WHERE IdMedicament=@id", cmd => cmd.Parameters.AddWithValue("@id", p.IdMedicament));
            decimal medPrixEffectif = (isDecede ? medPrix / 2 : medPrix) * discount;
            mMed += p.Quantite * medPrixEffectif;
        }

        var extraFees = GetList("SELECT NomFrais, Montant FROM Frais_Supplementaires WHERE IdPatient=@id", reader => new {
            Nom = reader.GetString("NomFrais"),
            Montant = reader.GetDecimal("Montant")
        }, cmd => cmd.Parameters.AddWithValue("@id", idPatient));

        decimal mExtra = 0;
        foreach (var fee in extraFees) {
            mExtra += fee.Montant;
        }

        decimal mTotal = mChambreCombined + mSoins + mMed + mExtra;
        string codeFacture = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();

        int idFacture = 0;
        using (var connection = GetConnection()) {
            string query = "INSERT INTO Facture (IdPatient, JourEmission, MontantChambre, MontantSoins, MontantMedicaments, MontantTotal, Statut, CodeFacture) VALUES (@p, @j, @mc, @ms, @mm, @mt, 'Générée', @code); SELECT LAST_INSERT_ID();";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@p", idPatient);
            command.Parameters.AddWithValue("@j", hopitalJour);
            command.Parameters.AddWithValue("@mc", mChambreCombined);
            command.Parameters.AddWithValue("@ms", mSoins);
            command.Parameters.AddWithValue("@mm", mMed);
            command.Parameters.AddWithValue("@mt", mTotal);
            command.Parameters.AddWithValue("@code", codeFacture);
            idFacture = Convert.ToInt32(command.ExecuteScalar());
        }

        if (mChambre > 0) ExecuteCommand("INSERT INTO LigneFacture (IdFacture, TypePrestation, Description, Quantite, PrixUnitaire, SousTotal) VALUES (@id, 'Chambre', 'Frais de séjour', @q, @p, @s)", cmd => {
            cmd.Parameters.AddWithValue("@id", idFacture);
            cmd.Parameters.AddWithValue("@q", duree);
            cmd.Parameters.AddWithValue("@p", prixChambreJourEffectif);
            cmd.Parameters.AddWithValue("@s", mChambre);
        });
        if (sTotalHopitalisation > 0) {
            ExecuteCommand("INSERT INTO LigneFacture (IdFacture, TypePrestation, Description, Quantite, PrixUnitaire, SousTotal) VALUES (@id, 'Chambre', 'Supplément de frais de séjour', @q, @p, @s)", cmd => {
                cmd.Parameters.AddWithValue("@id", idFacture);
                cmd.Parameters.AddWithValue("@q", (int)tempsTotal);
                cmd.Parameters.AddWithValue("@p", unitPriceHopitalisationEffectif);
                cmd.Parameters.AddWithValue("@s", sTotalHopitalisation);
            });
        }
        if (mSoins > 0) {
            ExecuteCommand("INSERT INTO LigneFacture (IdFacture, TypePrestation, Description, Quantite, PrixUnitaire, SousTotal) VALUES (@id, 'Soin', 'Prestations médicales de base', 1, @p, @s)", cmd => {
                cmd.Parameters.AddWithValue("@id", idFacture);
                cmd.Parameters.AddWithValue("@p", prixSoinBaseEffectif);
                cmd.Parameters.AddWithValue("@s", mSoins);
            });
        }
        
        foreach (var p in prescriptions) {
            var nomMed = ExecuteScalar<string>("SELECT Nom FROM Medicament WHERE IdMedicament=@id", cmd => cmd.Parameters.AddWithValue("@id", p.IdMedicament));
            var medPrix = ExecuteScalar<decimal>("SELECT PrixUnitaire FROM Medicament WHERE IdMedicament=@id", cmd => cmd.Parameters.AddWithValue("@id", p.IdMedicament));
            decimal medPrixEffectif = (isDecede ? medPrix / 2 : medPrix) * discount;
            decimal sTotal = p.Quantite * medPrixEffectif;
            ExecuteCommand("INSERT INTO LigneFacture (IdFacture, TypePrestation, Description, Quantite, PrixUnitaire, SousTotal) VALUES (@id, 'Médicament', @d, @q, @p, @s)", cmd => {
                cmd.Parameters.AddWithValue("@id", idFacture);
                cmd.Parameters.AddWithValue("@d", nomMed);
                cmd.Parameters.AddWithValue("@q", p.Quantite);
                cmd.Parameters.AddWithValue("@p", medPrixEffectif);
                cmd.Parameters.AddWithValue("@s", sTotal);
            });
            ExecuteCommand("UPDATE Prescription SET Statut='Terminée' WHERE IdPrescription=@id", cmd => cmd.Parameters.AddWithValue("@id", p.IdPrescription));
        }

        foreach (var fee in extraFees) {
            ExecuteCommand("INSERT INTO LigneFacture (IdFacture, TypePrestation, Description, Quantite, PrixUnitaire, SousTotal) VALUES (@id, 'Autre', @d, 1, @p, @s)", cmd => {
                cmd.Parameters.AddWithValue("@id", idFacture);
                cmd.Parameters.AddWithValue("@d", fee.Nom);
                cmd.Parameters.AddWithValue("@p", fee.Montant);
                cmd.Parameters.AddWithValue("@s", fee.Montant);
            });
        }

        ExecuteCommand("UPDATE Patients_Actifs SET IdFacture=@f WHERE IdPatient=@p", cmd => { cmd.Parameters.AddWithValue("@f", idFacture); cmd.Parameters.AddWithValue("@p", idPatient); });
        AddBudget(mTotal, $"Facture #{codeFacture} patient #{idPatient}", hopitalJour, "Revenu Patient");

        return idFacture;
    }

    public List<Facture> GetFactures() {
        return GetList("SELECT f.*, p.Nom as PatientNom FROM Facture f JOIN Patients_Actifs p ON f.IdPatient = p.IdPatient", reader => new Facture {
            IdFacture = reader.GetInt32("IdFacture"),
            CodeFacture = GetColString(reader, "CodeFacture") ?? "",
            IdPatient = reader.GetInt32("IdPatient"),
            JourEmission = reader.GetInt32("JourEmission"),
            MontantChambre = reader.GetDecimal("MontantChambre"),
            MontantSoins = reader.GetDecimal("MontantSoins"),
            MontantMedicaments = reader.GetDecimal("MontantMedicaments"),
            MontantTotal = reader.GetDecimal("MontantTotal"),
            Statut = reader.GetString("Statut"),
            PatientNom = reader.GetString("PatientNom")
        });
    }

    public List<LigneFacture> GetLignesFacture(int idFacture) {
        return GetList("SELECT * FROM LigneFacture WHERE IdFacture=@id", reader => new LigneFacture {
            IdLigne = reader.GetInt32("IdLigne"),
            IdFacture = reader.GetInt32("IdFacture"),
            TypePrestation = reader.GetString("TypePrestation"),
            Description = reader.GetString("Description"),
            Quantite = reader.GetInt32("Quantite"),
            PrixUnitaire = reader.GetDecimal("PrixUnitaire"),
            SousTotal = reader.GetDecimal("SousTotal")
        }, cmd => cmd.Parameters.AddWithValue("@id", idFacture));
    }

    public List<HitParadeEntry> GetHitParade() {
        var list = GetList("SELECT * FROM Helper_HitParade_Medecins", reader => new HitParadeEntry {
            IdEmploye = reader.GetInt32("IdEmploye"),
            Nom = reader.GetString("Nom"),
            Specialite = GetStringNullable(reader, "Specialite") ?? "",
            PatientsTraites = reader.GetInt32("PatientsTraites"),
            TauxGuerison = reader.GetDecimal("TauxGuerison"),
            RevenuGenere = reader.GetDecimal("RevenuGenere")
        });
        for (int i = 0; i < list.Count; i++) {
            list[i].Rang = i + 1;
        }
        return list;
    }

    public List<Reservation> GetReservations() {
        return GetList("SELECT * FROM Helper_Etat_Reservations", reader => new Reservation {
            IdReservation = reader.GetInt32("IdReservation"),
            Chambre = GetStringNullable(reader, "NumeroChambre") ?? "",
            Lit = GetStringNullable(reader, "NumeroLit") ?? "",
            Patient = GetStringNullable(reader, "Patient") ?? "",
            Medecin = GetStringNullable(reader, "Medecin") ?? "",
            DateEntree = reader.GetInt32("DateEntree"),
            DateSortie = GetIntNullable(reader, "DateSortie"),
            Statut = GetStringNullable(reader, "Statut") ?? ""
        });
    }

    public void CloseReservation(int idPatient) {
        var hopitalJour = ExecuteScalar<int>("SELECT JourSimulation FROM Hopital LIMIT 1");
        ExecuteCommand("UPDATE Reservation SET Statut='Terminée', DateSortie=@j WHERE IdPatient=@id AND Statut='Active'", cmd => {
            cmd.Parameters.AddWithValue("@j", hopitalJour);
            cmd.Parameters.AddWithValue("@id", idPatient);
        });
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
        if (idLit.HasValue) SetBedStatus(idLit.Value, "En Nettoyage");
    }

    public void SetBedStatus(int idLit, string statut) {
        ExecuteCommand("UPDATE Lit SET Statut=@s WHERE IdLit=@id", cmd => {
            cmd.Parameters.AddWithValue("@s", statut);
            cmd.Parameters.AddWithValue("@id", idLit);
        });
    }

    public List<int> GetLitsEnNettoyage() {
        return GetList("SELECT IdLit FROM Lit WHERE Statut = 'En Nettoyage'", reader => reader.GetInt32(0));
    }

    public bool TryAutoAdmitFromWaitingRoom(Random rng) {
        var waiting = GetWaitingRoomPatients();
        if (waiting.Count == 0) return false;

        var diseases = GetCasCliniques();
        foreach (var p in waiting) {
            var cas = diseases.FirstOrDefault(c => c.IdCas == p.IdMaladie);
            if (cas == null) continue;

            int? freeBed = GetFirstFreeBedForUnit(cas.UniteRequise);
            if (!freeBed.HasValue) continue;

            var docs = GetPersonnel()
                .Where(d => d.Categorie == "Corps Médical" && d.Statut == "En poste" && GetDoctorActivePatientCount(d.IdEmploye) < 3)
                .ToList();

            int? docId = docs.Count > 0 ? (int?)docs[rng.Next(docs.Count)].IdEmploye : null;

            try {
                AdmitPatient(p.NomComplet, p.IdMaladie, freeBed.Value, docId, "Standard", cas.TempsTraitementHeures);
                RemoveWaitingRoomPatient(p.IdPatientAttente);
                IncrementPatientsAdmis();
                return true;
            } catch { }
        }
        return false;
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

    public bool AddLit(int idChambre, string numeroLit) {
        int count = ExecuteScalar<int>("SELECT COUNT(*) FROM Lit WHERE IdChambre = @id", cmd => cmd.Parameters.AddWithValue("@id", idChambre));
        int capacity = ExecuteScalar<int>("SELECT COALESCE(CapaciteLits, 2) FROM Chambre WHERE IdChambre = @id", cmd => cmd.Parameters.AddWithValue("@id", idChambre));
        if (count >= capacity) {
            return false;
        }
        ExecuteCommand("INSERT INTO Lit (IdChambre, NumeroLit, Statut) VALUES (@id, @num, 'Libre')", cmd => {
            cmd.Parameters.AddWithValue("@id", idChambre);
            cmd.Parameters.AddWithValue("@num", numeroLit);
        });
        return true;
    }

    public List<UniteBase> GetUnitesList() {
        return GetList("SELECT IdUnite, Nom FROM Unite ORDER BY Nom", reader => new UniteBase {
            IdUnite = reader.GetInt32("IdUnite"),
            Nom = reader.GetString("Nom")
        });
    }

    public void AddChambre(int idUnite, string numeroChambre, string typeChambre, int capaciteLits) {
        ExecuteCommand("INSERT INTO Chambre (IdUnite, NumeroChambre, TypeChambre, CapaciteLits) VALUES (@id, @num, @type, @cap)", cmd => {
            cmd.Parameters.AddWithValue("@id", idUnite);
            cmd.Parameters.AddWithValue("@num", numeroChambre);
            cmd.Parameters.AddWithValue("@type", typeChambre);
            cmd.Parameters.AddWithValue("@cap", capaciteLits);
        });

        decimal coutEntretien = ExecuteScalar<decimal>("SELECT CoutEntretienJour FROM Unite WHERE IdUnite = @id", cmd => cmd.Parameters.AddWithValue("@id", idUnite));
        string uniteNom = ExecuteScalar<string>("SELECT Nom FROM Unite WHERE IdUnite = @id", cmd => cmd.Parameters.AddWithValue("@id", idUnite)) ?? "";
        decimal baseCost = LaboratoireProgrammation.Project.ModernHospital.Helpers.ChambrePricingHelper.GetFraisFixesCreation(uniteNom);
        decimal cost = LaboratoireProgrammation.Project.ModernHospital.Helpers.ChambrePricingHelper.CalculerCoutTotal(coutEntretien, capaciteLits, baseCost);

        int jour = ExecuteScalar<int>("SELECT JourSimulation FROM Hopital LIMIT 1");

        AddBudget(-cost, $"Création chambre {numeroChambre} ({uniteNom})", jour, "Dépense Construction");
        CreateFactureHopital("Construction Chambre", cost, $"Construction de la chambre {numeroChambre} dans l'unité {uniteNom} avec une capacité de {capaciteLits} lits.");
    }

    public void RemoveChambre(int idChambre) {
        ExecuteCommand("DELETE FROM Chambre WHERE IdChambre = @id", cmd => cmd.Parameters.AddWithValue("@id", idChambre));
    }

    public void RemoveLit(int idLit) {
        ExecuteCommand("DELETE FROM Lit WHERE IdLit = @id", cmd => cmd.Parameters.AddWithValue("@id", idLit));
    }

    public void RemoveMedicament(int idMedicament) {
        ExecuteCommand("DELETE FROM Medicament WHERE IdMedicament = @id", cmd => cmd.Parameters.AddWithValue("@id", idMedicament));
    }

    public void DeletePatient(int idPatient) {
        ExecuteCommand("DELETE FROM Patients_Actifs WHERE IdPatient = @id", cmd => cmd.Parameters.AddWithValue("@id", idPatient));
    }

    public void UpdatePersonnelStatus(int idEmploye, string statut) {
        ExecuteCommand("UPDATE Personnel_Actif SET Statut = @statut WHERE IdEmploye = @id", cmd => {
            cmd.Parameters.AddWithValue("@statut", statut);
            cmd.Parameters.AddWithValue("@id", idEmploye);
        });
    }

    public void UpdatePersonnelShiftData(int idEmploye, string statut, int minutesTravaillees, int minutesEnPause, int minutesHorsPoste) {
        ExecuteCommand("UPDATE Personnel_Actif SET Statut=@s, MinutesTravaillees=@mt, MinutesEnPause=@mp, MinutesHorsPoste=@mh WHERE IdEmploye=@id", cmd => {
            cmd.Parameters.AddWithValue("@s", statut);
            cmd.Parameters.AddWithValue("@mt", minutesTravaillees);
            cmd.Parameters.AddWithValue("@mp", minutesEnPause);
            cmd.Parameters.AddWithValue("@mh", minutesHorsPoste);
            cmd.Parameters.AddWithValue("@id", idEmploye);
        });
    }

    public void UpdatePersonnelMinutes(int idEmploye, string minuteColumn, int value) {
        ExecuteCommand($"UPDATE Personnel_Actif SET {minuteColumn}=@v WHERE IdEmploye=@id", cmd => {
            cmd.Parameters.AddWithValue("@v", value);
            cmd.Parameters.AddWithValue("@id", idEmploye);
        });
    }

    public void ReservePatient(string nom, int idMaladie, int idLit, int idMedecin, string classe, float tempsTraitement) {
        if (GetDoctorActivePatientCount(idMedecin) >= 3) {
            throw new InvalidOperationException("Ce médecin a déjà 3 patients à sa charge.");
        }

        var jour = ExecuteScalar<int>("SELECT JourSimulation FROM Hopital LIMIT 1");
        ExecuteCommand(
            "INSERT INTO Patients_Actifs (Nom,IdMaladie,IdLit,IdMedecinAssigné,Classe,TempsTraitementRestant,Statut,DateEntree) VALUES (@n,@m,@l,@md,@cl,@t,'Réservé',@j)",
            cmd => {
                cmd.Parameters.AddWithValue("@n", nom);
                cmd.Parameters.AddWithValue("@m", idMaladie);
                cmd.Parameters.AddWithValue("@l", idLit);
                cmd.Parameters.AddWithValue("@md", idMedecin);
                cmd.Parameters.AddWithValue("@cl", classe);
                cmd.Parameters.AddWithValue("@t", tempsTraitement);
                cmd.Parameters.AddWithValue("@j", jour);
            });

        int idPatient = ExecuteScalar<int>("SELECT MAX(IdPatient) FROM Patients_Actifs");
        
        ExecuteCommand("INSERT INTO Reservation (IdPatient, IdLit, IdMedecin, DateEntree, Statut) VALUES (@p, @l, @m, @d, 'Active')", cmd => {
            cmd.Parameters.AddWithValue("@p", idPatient);
            cmd.Parameters.AddWithValue("@l", idLit);
            cmd.Parameters.AddWithValue("@m", idMedecin);
            cmd.Parameters.AddWithValue("@d", jour);
        });

        ExecuteCommand("UPDATE Lit SET Statut='Réservé' WHERE IdLit=@id", cmd => cmd.Parameters.AddWithValue("@id", idLit));
    }

    public void AdmitReservedPatient(int idPatient) {
        var jour = ExecuteScalar<int>("SELECT JourSimulation FROM Hopital LIMIT 1");
        int litId = 0;
        ExecuteCommand("SELECT IdLit FROM Patients_Actifs WHERE IdPatient = @id", cmd => {
            cmd.Parameters.AddWithValue("@id", idPatient);
            using var reader = cmd.ExecuteReader();
            if (reader.Read() && !reader.IsDBNull(0)) {
                litId = reader.GetInt32(0);
            }
        });

        ExecuteCommand("UPDATE Patients_Actifs SET Statut = 'En Diagnostic', DateEntree = @j WHERE IdPatient = @id", cmd => {
            cmd.Parameters.AddWithValue("@j", jour);
            cmd.Parameters.AddWithValue("@id", idPatient);
        });

        if (litId > 0) {
            ExecuteCommand("UPDATE Lit SET Statut = 'Occupé' WHERE IdLit = @litId", cmd => {
                cmd.Parameters.AddWithValue("@litId", litId);
            });
        }

        ExecuteCommand("UPDATE Reservation SET Statut = 'Terminée' WHERE IdPatient = @id AND Statut = 'Active'", cmd => {
            cmd.Parameters.AddWithValue("@id", idPatient);
        });
    }

    public int GetDoctorActivePatientCount(int idMedecin) {
        return ExecuteScalar<int>(
            "SELECT COUNT(*) FROM Patients_Actifs WHERE IdMedecinAssigné = @id AND Statut NOT IN ('Guéri', 'Décédé')",
            cmd => cmd.Parameters.AddWithValue("@id", idMedecin)
        );
    }

    public void UpdatePatientTreatmentTime(int idPatient, float newTime) {
        ExecuteCommand(
            "UPDATE Patients_Actifs SET TempsTraitementRestant = @t WHERE IdPatient = @id",
            cmd => {
                cmd.Parameters.AddWithValue("@t", newTime);
                cmd.Parameters.AddWithValue("@id", idPatient);
            }
        );
    }

    public string EvaluateTreatmentOutcome(int idPatient) {
        var diseaseInfo = GetSingle(
            @"SELECT c.TauxRemission, c.SpecialisteTraitement, p.IdMedecinAssigné, c.UniteRequise, p.IdMaladie 
              FROM Patients_Actifs p 
              JOIN Cas_Cliniques c ON p.IdMaladie = c.IdCas 
              WHERE p.IdPatient = @id",
            reader => new {
                TauxRemission = reader.IsDBNull(0) ? 50f : reader.GetFloat(0),
                SpecialisteTraitement = reader.IsDBNull(1) ? "" : reader.GetString(1),
                IdMedecin = reader.IsDBNull(2) ? (int?)null : reader.GetInt32(2),
                UniteRequise = reader.IsDBNull(3) ? (string?)null : reader.GetString(3),
                IdMaladie = reader.GetInt32(4)
            },
            cmd => cmd.Parameters.AddWithValue("@id", idPatient)
        );

        if (diseaseInfo == null) return "Guéri";

        if (!string.IsNullOrEmpty(diseaseInfo.UniteRequise)) {
            var currentUnit = ExecuteScalar<string>(
                @"SELECT u.Nom 
                  FROM Patients_Actifs p
                  JOIN Lit l ON p.IdLit = l.IdLit
                  JOIN Chambre c ON l.IdChambre = c.IdChambre
                  JOIN Unite u ON c.IdUnite = u.IdUnite
                  WHERE p.IdPatient = @id",
                cmd => cmd.Parameters.AddWithValue("@id", idPatient)
            );
            if (string.IsNullOrEmpty(currentUnit) || !currentUnit.Equals(diseaseInfo.UniteRequise, StringComparison.OrdinalIgnoreCase)) {
                return "Décédé";
            }
        }

        float baseRemission = diseaseInfo.TauxRemission;
        string requiredSpecialty = diseaseInfo.SpecialisteTraitement;
        int? docId = diseaseInfo.IdMedecin;

        int numJanitors = GetActivePersonnelCountByRole("Agent d'entretien");
        int numBeds = Math.Max(1, GetBedsCount());
        if (numJanitors > 0) {
            baseRemission *= 1f + (0.10f * numJanitors / numBeds);
        } else {
            baseRemission *= 0.90f;
        }

        float bonus = -15f;
        if (docId.HasValue) {
            var docSpecialty = ExecuteScalar<string>(
                "SELECT Specialite FROM Personnel_Actif WHERE IdEmploye = @id",
                cmd => cmd.Parameters.AddWithValue("@id", docId.Value)
            );
            if (!string.IsNullOrEmpty(requiredSpecialty) && !string.IsNullOrEmpty(docSpecialty) && 
                docSpecialty.Equals(requiredSpecialty, StringComparison.OrdinalIgnoreCase)) {
                bonus = 15f;
            }
        }

        float drugMod = 0f;
        var prescriptions = GetPrescriptionsByPatient(idPatient);
        foreach (var p in prescriptions) {
            var med = GetMedicamentById(p.IdMedicament);
            if (med != null) {
                var target = med.GetCiblesList().FirstOrDefault(c => c.IdCas == diseaseInfo.IdMaladie);
                if (target != null) {
                    drugMod += target.Bonus;
                }
                var incompatible = med.GetIncompatiblesList().FirstOrDefault(i => i.IdCas == diseaseInfo.IdMaladie);
                if (incompatible != null) {
                    drugMod -= incompatible.Malus;
                }
            }
        }

        float finalRemission = baseRemission + bonus + drugMod;
        if (finalRemission < 0f) finalRemission = 0f;
        if (finalRemission > 100f) finalRemission = 100f;

        var random = new Random();
        double roll = random.NextDouble() * 100.0;
        return (roll <= finalRemission) ? "Guéri" : "Décédé";
    }

    public void SaveRealLifeTime(DateTime realLifeNow, int jour, TimeSpan heure) {
        ExecuteCommand(
            "UPDATE Hopital SET DerniereFermetureVraieVie=@r, JourSimulation=@j, HeureSimulation=@h WHERE IdHopital=1",
            cmd => {
                cmd.Parameters.AddWithValue("@r", realLifeNow);
                cmd.Parameters.AddWithValue("@j", jour);
                cmd.Parameters.AddWithValue("@h", heure);
            });
    }

    public DateTime? GetLastRealLifeTime() {
        return ExecuteScalar<DateTime?>("SELECT DerniereFermetureVraieVie FROM Hopital LIMIT 1");
    }

    public void UpdateHospitalTime(int jour, TimeSpan heure) {
        ExecuteCommand(
            "UPDATE Hopital SET JourSimulation=@j, HeureSimulation=@h WHERE IdHopital=1",
            cmd => {
                cmd.Parameters.AddWithValue("@j", jour);
                cmd.Parameters.AddWithValue("@h", heure);
            });
    }

    public List<Unite> GetUnites() {
        return GetList("SELECT IdUnite, Nom, CoutEntretienJour FROM Unite", reader => new Unite {
            IdUnite = reader.GetInt32("IdUnite"),
            Nom = reader.GetString("Nom"),
            CoutEntretienJour = reader.GetDecimal("CoutEntretienJour")
        });
    }

    public void UpdatePersonnelShiftDataAndNextPause(int idEmploye, string statut, int minutesTravaillees, int minutesEnPause, int minutesHorsPoste, int prochainePause) {
        ExecuteCommand("UPDATE Personnel_Actif SET Statut=@s, MinutesTravaillees=@mt, MinutesEnPause=@mp, MinutesHorsPoste=@mh, ProchainePauseMinutes=@pp WHERE IdEmploye=@id", cmd => {
            cmd.Parameters.AddWithValue("@s", statut);
            cmd.Parameters.AddWithValue("@mt", minutesTravaillees);
            cmd.Parameters.AddWithValue("@mp", minutesEnPause);
            cmd.Parameters.AddWithValue("@mh", minutesHorsPoste);
            cmd.Parameters.AddWithValue("@pp", prochainePause);
            cmd.Parameters.AddWithValue("@id", idEmploye);
        });
    }

    public void AddExtraFee(int idPatient, string nomFrais, decimal montant) {
        ExecuteCommand("INSERT INTO Frais_Supplementaires (IdPatient, NomFrais, Montant) VALUES (@p, @n, @m)", cmd => {
            cmd.Parameters.AddWithValue("@p", idPatient);
            cmd.Parameters.AddWithValue("@n", nomFrais);
            cmd.Parameters.AddWithValue("@m", montant);
        });
    }

    public void CreateFactureHopital(string type, decimal montant, string description) {
        CreateFactureHopital(type, montant, description, null);
    }

    public void CreateFactureHopital(string type, decimal montant, string description, List<LigneFacture>? lignes) {
        var day = ExecuteScalar<int>("SELECT JourSimulation FROM Hopital LIMIT 1");
        string code = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
        string? lignesJson = lignes != null && lignes.Count > 0
            ? Newtonsoft.Json.JsonConvert.SerializeObject(lignes)
            : null;
        ExecuteCommand("INSERT INTO Facture_Hopital (CodeFacture, JourEmission, TypeFacture, MontantTotal, Description, Statut, LignesJson) VALUES (@c, @j, @t, @m, @d, 'Payée', @lj)", cmd => {
            cmd.Parameters.AddWithValue("@c", code);
            cmd.Parameters.AddWithValue("@j", day);
            cmd.Parameters.AddWithValue("@t", type);
            cmd.Parameters.AddWithValue("@m", montant);
            cmd.Parameters.AddWithValue("@d", description);
            cmd.Parameters.AddWithValue("@lj", (object?)lignesJson ?? DBNull.Value);
        });
    }

    public List<LigneFacture> GetLignesFactureHopital(int idFactureHopital) {
        var json = ExecuteScalar<string?>("SELECT LignesJson FROM Facture_Hopital WHERE IdFactureHopital = @id",
            cmd => cmd.Parameters.AddWithValue("@id", idFactureHopital));
        if (string.IsNullOrEmpty(json)) return new List<LigneFacture>();
        return Newtonsoft.Json.JsonConvert.DeserializeObject<List<LigneFacture>>(json) ?? new List<LigneFacture>();
    }

    public List<FactureDisplayItem> GetFacturesHopital() {
        return GetList("SELECT fh.* FROM Facture_Hopital fh", reader => new FactureDisplayItem {
            CodeFacture = reader.GetString("CodeFacture"),
            PatientNom = "Hôpital (" + reader.GetString("TypeFacture") + ")",
            JourEmission = reader.GetInt32("JourEmission"),
            MontantTotal = reader.GetDecimal("MontantTotal"),
            Statut = reader.GetString("Statut"),
            Type = "Hopital",
            Description = reader.GetString("Description"),
            IdRealFacture = reader.GetInt32("IdFactureHopital")
        });
    }

    public List<FactureDisplayItem> GetFacturesPatient() {
        return GetList("SELECT f.*, p.Nom as PatientNom FROM Facture f JOIN Patients_Actifs p ON f.IdPatient = p.IdPatient", reader => new FactureDisplayItem {
            CodeFacture = GetColString(reader, "CodeFacture") ?? "",
            PatientNom = reader.GetString("PatientNom"),
            JourEmission = reader.GetInt32("JourEmission"),
            MontantTotal = reader.GetDecimal("MontantTotal"),
            Statut = reader.GetString("Statut"),
            Type = "Patient",
            IdRealFacture = reader.GetInt32("IdFacture"),
            Description = "Frais de séjour patient"
        });
    }

    public int? GetFreeColdChamberBed() {
        return ExecuteScalar<int?>(
            @"SELECT l.IdLit 
              FROM Lit l 
              JOIN Chambre c ON l.IdChambre = c.IdChambre 
              JOIN Unite u ON c.IdUnite = u.IdUnite 
              WHERE u.Nom = 'Chambre froide' AND l.Statut = 'Libre' 
              LIMIT 1"
        );
    }

    public void TransferToColdChamber(int idPatient, int coldBedId) {
        ReleaseBed(idPatient);
        ExecuteCommand("UPDATE Lit SET Statut = 'Occupé' WHERE IdLit = @lId", cmd => cmd.Parameters.AddWithValue("@lId", coldBedId));
        ExecuteCommand("UPDATE Patients_Actifs SET IdLit = @lId WHERE IdPatient = @pId", cmd => {
            cmd.Parameters.AddWithValue("@lId", coldBedId);
            cmd.Parameters.AddWithValue("@pId", idPatient);
        });
    }

    public decimal GetAccumulatedFuneralFees() {
        return ExecuteScalar<decimal>("SELECT COALESCE(FraisFuneraireCumule, 0.00) FROM Hopital LIMIT 1");
    }

    public void ResetAccumulatedFuneralFees() {
        ExecuteCommand("UPDATE Hopital SET FraisFuneraireCumule = 0.00");
    }

    public void IncrementAccumulatedFuneralFees() {
        ExecuteCommand("UPDATE Hopital SET FraisFuneraireCumule = COALESCE(FraisFuneraireCumule, 0.00) + 400.00");
    }

    public decimal GetAccumulatedLogisticalFees() {
        return ExecuteScalar<decimal>("SELECT COALESCE(CoutLogistiqueCumule, 0.00) FROM Hopital LIMIT 1");
    }

    public void ResetAccumulatedLogisticalFees() {
        ExecuteCommand("UPDATE Hopital SET CoutLogistiqueCumule = 0.00");
    }

    public void IncrementAccumulatedLogisticalFees(decimal amount) {
        ExecuteCommand("UPDATE Hopital SET CoutLogistiqueCumule = COALESCE(CoutLogistiqueCumule, 0.00) + @amount", cmd => cmd.Parameters.AddWithValue("@amount", amount));
    }


    // --- RESTORED PHARMACY & ADMISSION METHODS ---
    public List<WaitingRoomPatient> GetWaitingRoomPatients() {
        return GetList(
            @"SELECT p.IdPatientAttente, p.Nom, p.Prenom, p.IdMaladie, p.TempsAttenteMinutes, c.Symptomes 
              FROM Salle_Attente_Patients p 
              JOIN Cas_Cliniques c ON p.IdMaladie = c.IdCas 
              ORDER BY p.TempsAttenteMinutes DESC",
            reader => new WaitingRoomPatient {
                IdPatientAttente = reader.GetInt32("IdPatientAttente"),
                Nom = reader.GetString("Nom"),
                Prenom = reader.GetString("Prenom"),
                IdMaladie = reader.GetInt32("IdMaladie"),
                TempsAttenteMinutes = reader.GetInt32("TempsAttenteMinutes"),
                Symptomes = reader.GetString("Symptomes")
            });
    }

    public void AddWaitingRoomPatient(string nom, string prenom, int idMaladie) {
        ExecuteCommand(
            "INSERT INTO Salle_Attente_Patients (Nom, Prenom, IdMaladie, TempsAttenteMinutes) VALUES (@n, @p, @m, 0)",
            cmd => {
                cmd.Parameters.AddWithValue("@n", nom);
                cmd.Parameters.AddWithValue("@p", prenom);
                cmd.Parameters.AddWithValue("@m", idMaladie);
            });
        IncrementPatientsGeneres();
    }

    public void IncrementWaitingRoomPatientsTime(int minutes) {
        ExecuteCommand("UPDATE Salle_Attente_Patients SET TempsAttenteMinutes = TempsAttenteMinutes + @m", cmd => cmd.Parameters.AddWithValue("@m", minutes));
    }

    public void RemoveWaitingRoomPatient(int id) {
        ExecuteCommand("DELETE FROM Salle_Attente_Patients WHERE IdPatientAttente = @id", cmd => cmd.Parameters.AddWithValue("@id", id));
    }

    public void IncrementPatientsGeneres() {
        ExecuteCommand("UPDATE Hopital SET PatientsGeneres = PatientsGeneres + 1 WHERE IdHopital = 1");
    }

    public void IncrementPatientsAdmis() {
        ExecuteCommand("UPDATE Hopital SET PatientsAdmis = PatientsAdmis + 1 WHERE IdHopital = 1");
    }

    public void GetWaitingRoomStats(out int gen, out int adm, out double ratio) {
        var result = GetSingle(
            "SELECT PatientsGeneres, PatientsAdmis FROM Hopital LIMIT 1",
            reader => new {
                Gen = reader.GetInt32(0),
                Adm = reader.GetInt32(1)
            });
        if (result != null) {
            gen = result.Gen;
            adm = result.Adm;
            ratio = gen > 0 ? ((double)adm / gen) * 100.0 : 100.0;
        } else {
            gen = 0;
            adm = 0;
            ratio = 100.0;
        }
    }

    public List<VirtualQueuePatient> GetVirtualQueue() {
        return GetList(
            "SELECT IdVirtual, Nom, IdMaladie, IdMedecin, Classe, DateEntree FROM Virtual_Queue ORDER BY IdVirtual ASC",
            reader => new VirtualQueuePatient {
                IdVirtual = reader.GetInt32("IdVirtual"),
                Nom = reader.GetString("Nom"),
                IdMaladie = reader.GetInt32("IdMaladie"),
                IdMedecin = GetIntNullable(reader, "IdMedecin"),
                Classe = reader.GetString("Classe"),
                DateEntree = reader.GetInt32("DateEntree")
            });
    }

    public void AddToVirtualQueue(string nom, int idMaladie, int? idMedecin, string classe, int dateEntree) {
        ExecuteCommand(
            "INSERT INTO Virtual_Queue (Nom, IdMaladie, IdMedecin, Classe, DateEntree) VALUES (@n, @m, @md, @c, @d)",
            cmd => {
                cmd.Parameters.AddWithValue("@n", nom);
                cmd.Parameters.AddWithValue("@m", idMaladie);
                cmd.Parameters.AddWithValue("@md", (object?)idMedecin ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@c", classe);
                cmd.Parameters.AddWithValue("@d", dateEntree);
            });
    }

    public void RemoveFromVirtualQueue(int idVirtual) {
        ExecuteCommand("DELETE FROM Virtual_Queue WHERE IdVirtual = @id", cmd => cmd.Parameters.AddWithValue("@id", idVirtual));
    }

    public int? GetFirstFreeBedForUnit(string? unitName) {
        if (string.IsNullOrEmpty(unitName)) {
            return ExecuteScalar<int?>("SELECT IdLit FROM Lit WHERE Statut = 'Libre' LIMIT 1");
        } else {
            return ExecuteScalar<int?>(
                @"SELECT l.IdLit 
                  FROM Lit l 
                  JOIN Chambre c ON l.IdChambre = c.IdChambre 
                  JOIN Unite u ON c.IdUnite = u.IdUnite 
                  WHERE u.Nom = @uNom AND l.Statut = 'Libre' 
                  LIMIT 1",
                cmd => cmd.Parameters.AddWithValue("@uNom", unitName));
        }
    }

    public void UpdatePatientTreatmentTimeAndTempsSansMedecin(int idPatient, float remainingTime, float tempsSansMedecin) {
        ExecuteCommand(
            "UPDATE Patients_Actifs SET TempsTraitementRestant = @r, TempsSansMedecin = @t WHERE IdPatient = @id",
            cmd => {
                cmd.Parameters.AddWithValue("@r", remainingTime);
                cmd.Parameters.AddWithValue("@t", tempsSansMedecin);
                cmd.Parameters.AddWithValue("@id", idPatient);
            }
        );
    }

    public void UpdatePatientTempsSansMedecin(int idPatient, float tempsSansMedecin) {
        ExecuteCommand(
            "UPDATE Patients_Actifs SET TempsSansMedecin = @t WHERE IdPatient = @id",
            cmd => {
                cmd.Parameters.AddWithValue("@t", tempsSansMedecin);
                cmd.Parameters.AddWithValue("@id", idPatient);
            }
        );
    }


    public List<LitDisponible> GetLitsLibresPourUnite(string? unitName) {
        if (string.IsNullOrEmpty(unitName)) {
            return GetLitsLibres();
        }
        return GetList(
            "SELECT * FROM Helper_Disponibilite_Lits WHERE Unite = @uNom",
            reader => new LitDisponible {
                IdLit = reader.GetInt32("IdLit"),
                Unite = reader.GetString("Unite"),
                NumeroChambre = reader.GetString("NumeroChambre"),
                TypeChambre = reader.GetString("TypeChambre"),
                NumeroLit = reader.GetString("NumeroLit")
            },
            cmd => cmd.Parameters.AddWithValue("@uNom", unitName)
        );
    }

    public WaitingRoomPatient? GetWaitingRoomPatient(int id) {
        return GetSingle(
            @"SELECT p.IdPatientAttente, p.Nom, p.Prenom, p.IdMaladie, p.TempsAttenteMinutes, c.Symptomes 
              FROM Salle_Attente_Patients p 
              JOIN Cas_Cliniques c ON p.IdMaladie = c.IdCas 
              WHERE p.IdPatientAttente = @id",
            reader => new WaitingRoomPatient {
                IdPatientAttente = reader.GetInt32("IdPatientAttente"),
                Nom = reader.GetString("Nom"),
                Prenom = reader.GetString("Prenom"),
                IdMaladie = reader.GetInt32("IdMaladie"),
                TempsAttenteMinutes = reader.GetInt32("TempsAttenteMinutes"),
                Symptomes = reader.GetString("Symptomes")
            },
            cmd => cmd.Parameters.AddWithValue("@id", id)
        );
    }


    public Medicament? GetMedicamentById(int id) {
        return GetSingle(
            "SELECT * FROM Medicament WHERE IdMedicament = @id",
            reader => new Medicament {
                IdMedicament = reader.GetInt32("IdMedicament"),
                Nom = reader.GetString("Nom"),
                DCI = GetStringNullable(reader, "DCI") ?? "",
                Forme = GetStringNullable(reader, "Forme") ?? "",
                StockActuel = reader.GetInt32("StockActuel"),
                StockMinimum = reader.GetInt32("StockMinimum"),
                PrixUnitaire = reader.GetDecimal("PrixUnitaire"),
                IdUnite = reader.GetInt32("IdUnite"),
                MaladiesCibles = GetStringNullable(reader, "Maladies_Cibles"),
                MaladiesIncompatibles = GetStringNullable(reader, "Maladies_Incompatibles"),
                TempsLivraisonBase = reader.GetInt32("Temps_Livraison_Base"),
                QuantitePrescriptionDefaut = reader.GetInt32("Quantite_Prescription_Defaut")
            },
            cmd => cmd.Parameters.AddWithValue("@id", id)
        );
    }

    public List<MedicamentCommande> GetPendingDeliveries() {
        return GetList("SELECT IdCommande, IdMedicament, Quantite, TempsLivraisonRestant, PrixAchat FROM Commandes_Medicaments", reader => new MedicamentCommande {
            IdCommande = reader.GetInt32("IdCommande"),
            IdMedicament = reader.GetInt32("IdMedicament"),
            Quantite = reader.GetInt32("Quantite"),
            TempsLivraisonRestant = reader.GetInt32("TempsLivraisonRestant"),
            PrixAchat = reader.GetDecimal("PrixAchat")
        });
    }

    public void UpdateDeliveryTime(int id, int newTime) {
        ExecuteCommand("UPDATE Commandes_Medicaments SET TempsLivraisonRestant = @t WHERE IdCommande = @id", cmd => {
            cmd.Parameters.AddWithValue("@t", newTime);
            cmd.Parameters.AddWithValue("@id", id);
        });
    }

    public void DeleteDelivery(int id) {
        ExecuteCommand("DELETE FROM Commandes_Medicaments WHERE IdCommande = @id", cmd => {
            cmd.Parameters.AddWithValue("@id", id);
        });
    }

    public void AddDelivery(int idMedicament, int quantite, int tempsLivraison, decimal prixAchat) {
        ExecuteCommand("INSERT INTO Commandes_Medicaments (IdMedicament, Quantite, TempsLivraisonRestant, PrixAchat) VALUES (@m, @q, @t, @p)", cmd => {
            cmd.Parameters.AddWithValue("@m", idMedicament);
            cmd.Parameters.AddWithValue("@q", quantite);
            cmd.Parameters.AddWithValue("@t", tempsLivraison);
            cmd.Parameters.AddWithValue("@p", prixAchat);
        });
    }

    private static string? GetColString(MySqlDataReader reader, string column) {
        try {
            int ordinal = reader.GetOrdinal(column);
            return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
        } catch {
            return null;
        }
    }

    public int GetActivePersonnelCountByRole(string roleExact) {
        return ExecuteScalar<int>(
            "SELECT COUNT(*) FROM Personnel_Actif WHERE RoleExact = @r",
            cmd => cmd.Parameters.AddWithValue("@r", roleExact));
    }

    public int GetBedsCount() {
        return ExecuteScalar<int>("SELECT COUNT(*) FROM Lit");
    }

    public int GetTotalEquipmentLevel() {
        return ExecuteScalar<int>("SELECT COALESCE(SUM(NiveauEquipement), 1) FROM Chambre");
    }

    public int GetActivePatientCount() {
        return ExecuteScalar<int>("SELECT COUNT(*) FROM Patients_Actifs WHERE Statut NOT IN ('Guéri','Décédé')");
    }

    public void TryOpenProces(PatientActif patient, int currentDay) {
        var disease = GetSingle(
            "SELECT TauxRemission, Maladie FROM Cas_Cliniques WHERE IdCas = @id",
            reader => new { TauxRemission = reader.GetFloat(0), Maladie = reader.GetString(1) },
            cmd => cmd.Parameters.AddWithValue("@id", patient.IdMaladie));

        if (disease == null) return;

        double roll = new Random().NextDouble() * 100.0;
        if (roll > disease.TauxRemission) return;

        ExecuteCommand(
            "INSERT INTO Proces (NomPatient, NomMaladie, TauxRemission, JourOuverture, JourFermeture, Statut, MontantPenalite) VALUES (@np, @nm, @tr, @jo, @jf, 'En cours', 0)",
            cmd => {
                cmd.Parameters.AddWithValue("@np", patient.Nom);
                cmd.Parameters.AddWithValue("@nm", disease.Maladie);
                cmd.Parameters.AddWithValue("@tr", disease.TauxRemission);
                cmd.Parameters.AddWithValue("@jo", currentDay);
                cmd.Parameters.AddWithValue("@jf", currentDay + 7);
            });
    }

    public List<Proces> GetProces() {
        return GetList("SELECT * FROM Proces ORDER BY JourFermeture ASC", reader => new Proces {
            IdProces = reader.GetInt32("IdProces"),
            NomPatient = reader.GetString("NomPatient"),
            NomMaladie = reader.GetString("NomMaladie"),
            TauxRemission = reader.GetFloat("TauxRemission"),
            JourOuverture = reader.GetInt32("JourOuverture"),
            JourFermeture = reader.GetInt32("JourFermeture"),
            Statut = reader.GetString("Statut"),
            MontantPenalite = reader.GetDecimal("MontantPenalite")
        });
    }

    public void ResolveExpiredProces(int currentDay, decimal hospitalBudget, Action<string, decimal> onLoss) {
        var expired = GetList(
            "SELECT * FROM Proces WHERE Statut = 'En cours' AND JourFermeture <= @d",
            reader => new Proces {
                IdProces = reader.GetInt32("IdProces"),
                NomPatient = reader.GetString("NomPatient"),
                NomMaladie = reader.GetString("NomMaladie"),
                TauxRemission = reader.GetFloat("TauxRemission"),
                JourOuverture = reader.GetInt32("JourOuverture"),
                JourFermeture = reader.GetInt32("JourFermeture"),
                Statut = reader.GetString("Statut"),
                MontantPenalite = 0
            },
            cmd => cmd.Parameters.AddWithValue("@d", currentDay));

        if (expired.Count == 0) return;

        int numLawyers = GetActivePersonnelCountByRole("Avocat");
        int numActive = Math.Max(1, expired.Count);

        foreach (var p in expired) {
            double lossPct = p.TauxRemission / (0.5 * Math.Max(0.01, (double)numLawyers / numActive));
            lossPct = Math.Min(100.0, lossPct);

            double lossRoll = new Random().NextDouble() * 100.0;
            if (lossRoll <= lossPct) {
                decimal penalty = hospitalBudget * ((decimal)p.TauxRemission / 100m);
                ExecuteCommand(
                    "UPDATE Proces SET Statut = 'Perdu', MontantPenalite = @m WHERE IdProces = @id",
                    cmd => {
                        cmd.Parameters.AddWithValue("@m", penalty);
                        cmd.Parameters.AddWithValue("@id", p.IdProces);
                    });
                AddBudget(-penalty, $"Perte procès : {p.NomPatient} ({p.NomMaladie})", currentDay, "Procès Perdu");
                CreateFactureHopital("Procès Perdu", penalty, $"Condamnation judiciaire suite au décès de {p.NomPatient}. Maladie : {p.NomMaladie}. Taux de rémission était {p.TauxRemission:F1}%.");
                onLoss($"Procès perdu : {p.NomPatient}", penalty);
            } else {
                ExecuteCommand(
                    "UPDATE Proces SET Statut = 'Gagné' WHERE IdProces = @id",
                    cmd => cmd.Parameters.AddWithValue("@id", p.IdProces));
            }
        }
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