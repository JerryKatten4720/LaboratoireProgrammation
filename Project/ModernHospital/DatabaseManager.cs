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
            MinutesHorsPoste = reader.GetInt32("MinutesHorsPoste")
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
            TempsTraitementRestant = reader.IsDBNull(reader.GetOrdinal("TempsTraitementRestant")) ? 0f : reader.GetFloat("TempsTraitementRestant"),
            Statut = reader.GetString("Statut")
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
        return GetList("SELECT IdCas,Maladie,Symptomes,TempsTraitementHeures,RevenuPatient,RisqueErreurMedicale,TauxRemission,SpecialisteTraitement FROM Cas_Cliniques", reader => new CasClinique {
            IdCas = reader.GetInt32("IdCas"),
            Maladie = reader.GetString("Maladie"),
            Symptomes = reader.GetString("Symptomes"),
            TempsTraitementHeures = reader.IsDBNull(3) ? 0f : reader.GetFloat(3),
            RevenuPatient = reader.GetDecimal("RevenuPatient"),
            RisqueErreurMedicale = reader.GetInt32("RisqueErreurMedicale"),
            TauxRemission = reader.IsDBNull(6) ? 50f : reader.GetFloat(6),
            SpecialisteTraitement = reader.IsDBNull(7) ? "" : reader.GetString(7)
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
            IdUnite = reader.GetInt32("IdUnite")
        });
    }

    public void AddMedicament(string nom, string dci, string forme, int stockActuel, int stockMin, decimal prix, int idUnite) {
        ExecuteCommand("INSERT INTO Medicament (Nom, DCI, Forme, StockActuel, StockMinimum, PrixUnitaire, IdUnite) VALUES (@n, @d, @f, @sa, @sm, @p, @u)", cmd => {
            cmd.Parameters.AddWithValue("@n", nom);
            cmd.Parameters.AddWithValue("@d", dci);
            cmd.Parameters.AddWithValue("@f", forme);
            cmd.Parameters.AddWithValue("@sa", stockActuel);
            cmd.Parameters.AddWithValue("@sm", stockMin);
            cmd.Parameters.AddWithValue("@p", prix);
            cmd.Parameters.AddWithValue("@u", idUnite);
        });
    }

    public void UpdateMedicament(int id, string nom, string dci, string forme, int stockActuel, int stockMin, decimal prix, int idUnite) {
        ExecuteCommand("UPDATE Medicament SET Nom=@n, DCI=@d, Forme=@f, StockActuel=@sa, StockMinimum=@sm, PrixUnitaire=@p, IdUnite=@u WHERE IdMedicament=@id", cmd => {
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@n", nom);
            cmd.Parameters.AddWithValue("@d", dci);
            cmd.Parameters.AddWithValue("@f", forme);
            cmd.Parameters.AddWithValue("@sa", stockActuel);
            cmd.Parameters.AddWithValue("@sm", stockMin);
            cmd.Parameters.AddWithValue("@p", prix);
            cmd.Parameters.AddWithValue("@u", idUnite);
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

        decimal mChambre = duree * prixChambreJour;
        decimal mSoins = prixSoinBase;
        
        var prescriptions = GetPrescriptionsByPatient(idPatient);
        decimal mMed = 0;
        foreach (var p in prescriptions) {
            var medPrix = ExecuteScalar<decimal>("SELECT PrixUnitaire FROM Medicament WHERE IdMedicament=@id", cmd => cmd.Parameters.AddWithValue("@id", p.IdMedicament));
            mMed += p.Quantite * medPrix;
        }

        var patientStatut = ExecuteScalar<string>("SELECT Statut FROM Patients_Actifs WHERE IdPatient=@id", cmd => cmd.Parameters.AddWithValue("@id", idPatient));
        bool isDecede = (patientStatut == "Décédé");
        if (isDecede) {
            mChambre /= 2;
            mSoins /= 2;
            mMed /= 2;
        }

        decimal mTotal = mChambre + mSoins + mMed;
        string codeFacture = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();

        int idFacture = 0;
        using (var connection = GetConnection()) {
            string query = "INSERT INTO Facture (IdPatient, JourEmission, MontantChambre, MontantSoins, MontantMedicaments, MontantTotal, Statut, CodeFacture) VALUES (@p, @j, @mc, @ms, @mm, @mt, 'Générée', @code); SELECT LAST_INSERT_ID();";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@p", idPatient);
            command.Parameters.AddWithValue("@j", hopitalJour);
            command.Parameters.AddWithValue("@mc", mChambre);
            command.Parameters.AddWithValue("@ms", mSoins);
            command.Parameters.AddWithValue("@mm", mMed);
            command.Parameters.AddWithValue("@mt", mTotal);
            command.Parameters.AddWithValue("@code", codeFacture);
            idFacture = Convert.ToInt32(command.ExecuteScalar());
        }

        if (mChambre > 0) ExecuteCommand("INSERT INTO LigneFacture (IdFacture, TypePrestation, Description, Quantite, PrixUnitaire, SousTotal) VALUES (@id, 'Chambre', 'Frais de séjour', @q, @p, @s)", cmd => {
            cmd.Parameters.AddWithValue("@id", idFacture);
            cmd.Parameters.AddWithValue("@q", duree);
            cmd.Parameters.AddWithValue("@p", isDecede ? prixChambreJour / 2 : prixChambreJour);
            cmd.Parameters.AddWithValue("@s", mChambre);
        });
        if (mSoins > 0) ExecuteCommand("INSERT INTO LigneFacture (IdFacture, TypePrestation, Description, Quantite, PrixUnitaire, SousTotal) VALUES (@id, 'Soin', 'Prestations médicales de base', 1, @p, @s)", cmd => {
            cmd.Parameters.AddWithValue("@id", idFacture);
            cmd.Parameters.AddWithValue("@p", isDecede ? prixSoinBase / 2 : prixSoinBase);
            cmd.Parameters.AddWithValue("@s", mSoins);
        });
        
        foreach (var p in prescriptions) {
            var nomMed = ExecuteScalar<string>("SELECT Nom FROM Medicament WHERE IdMedicament=@id", cmd => cmd.Parameters.AddWithValue("@id", p.IdMedicament));
            var medPrix = ExecuteScalar<decimal>("SELECT PrixUnitaire FROM Medicament WHERE IdMedicament=@id", cmd => cmd.Parameters.AddWithValue("@id", p.IdMedicament));
            decimal sTotal = p.Quantite * (isDecede ? medPrix / 2 : medPrix);
            ExecuteCommand("INSERT INTO LigneFacture (IdFacture, TypePrestation, Description, Quantite, PrixUnitaire, SousTotal) VALUES (@id, 'Médicament', @d, @q, @p, @s)", cmd => {
                cmd.Parameters.AddWithValue("@id", idFacture);
                cmd.Parameters.AddWithValue("@d", nomMed);
                cmd.Parameters.AddWithValue("@q", p.Quantite);
                cmd.Parameters.AddWithValue("@p", isDecede ? medPrix / 2 : medPrix);
                cmd.Parameters.AddWithValue("@s", sTotal);
            });
            ExecuteCommand("UPDATE Prescription SET Statut='Terminée' WHERE IdPrescription=@id", cmd => cmd.Parameters.AddWithValue("@id", p.IdPrescription));
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
            @"SELECT c.TauxRemission, c.SpecialisteTraitement, p.IdMedecinAssigné 
              FROM Patients_Actifs p 
              JOIN Cas_Cliniques c ON p.IdMaladie = c.IdCas 
              WHERE p.IdPatient = @id",
            reader => new {
                TauxRemission = reader.IsDBNull(0) ? 50f : reader.GetFloat(0),
                SpecialisteTraitement = reader.IsDBNull(1) ? "" : reader.GetString(1),
                IdMedecin = reader.IsDBNull(2) ? (int?)null : reader.GetInt32(2)
            },
            cmd => cmd.Parameters.AddWithValue("@id", idPatient)
        );

        if (diseaseInfo == null) return "Guéri";

        float baseRemission = diseaseInfo.TauxRemission;
        string requiredSpecialty = diseaseInfo.SpecialisteTraitement;
        int? docId = diseaseInfo.IdMedecin;

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

        float finalRemission = baseRemission + bonus;
        if (finalRemission < 0f) finalRemission = 0f;
        if (finalRemission > 100f) finalRemission = 100f;

        var random = new Random();
        double roll = random.NextDouble() * 100.0;
        return (roll <= finalRemission) ? "Guéri" : "Décédé";
    }

    private static string? GetColString(MySqlDataReader reader, string column) {
        try {
            int ordinal = reader.GetOrdinal(column);
            return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
        } catch {
            return null;
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