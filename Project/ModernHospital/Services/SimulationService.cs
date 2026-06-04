using System;
using System.Linq;
using System.Windows.Threading;

namespace LaboratoireProgrammation.Project.ModernHospital;

public class SimulationService {
    private readonly DatabaseManager _db;
    private DispatcherTimer? _clockTimer;
    private DateTime _gameTime;
    private DateTime _lastDoctorUpdate = DateTime.MinValue;

    public event Action<DateTime>? OnTick;
    public event Action<string>? OnEventLog;
    public event Action? OnPersonnelChanged;

    public SimulationService(DatabaseManager db) {
        _db = db;
        _gameTime = DateTime.Now;
    }

    public void Start() {
        _clockTimer = new DispatcherTimer {
            Interval = TimeSpan.FromSeconds(HospitalSettings.Current.GameTickIntervalSeconds)
        };
        _clockTimer.Tick += (s, e) => {
            _gameTime = _gameTime.AddSeconds(HospitalSettings.Current.GameTickIntervalSeconds * HospitalSettings.Current.TimeScaleMultiplier);
            OnTick?.Invoke(_gameTime);
            UpdateDoctorsShifts();
        };
        _clockTimer.Start();
        
        OnTick?.Invoke(_gameTime);
        UpdateDoctorsShifts();
    }

    public void Stop() {
        _clockTimer?.Stop();
    }

    private void UpdateDoctorsShifts() {
        if ((_gameTime - _lastDoctorUpdate).TotalMinutes < 1) return;
        
        int minutesPassed = (int)(_gameTime - _lastDoctorUpdate).TotalMinutes;
        if (_lastDoctorUpdate == DateTime.MinValue) minutesPassed = 1;
        _lastDoctorUpdate = _gameTime;

        var docs = _db.GetPersonnel().Where(p => p.Categorie == "Corps Médical").ToList();
        bool changed = false;

        foreach (var doc in docs) {
            if (doc.Statut == "En poste") {
                doc.MinutesTravaillees += minutesPassed;
                if (doc.MinutesTravaillees >= 120 && doc.MinutesTravaillees < 120 + 10) {
                    _db.ExecuteNonQuery($"UPDATE Personnel_Actif SET Statut = 'En pause', MinutesEnPause = 0 WHERE IdEmploye = {doc.IdEmploye}");
                    changed = true;
                    OnEventLog?.Invoke($"☕ {doc.NomComplet} part en pause.");
                } else if (doc.MinutesTravaillees >= 600) {
                    _db.ExecuteNonQuery($"UPDATE Personnel_Actif SET Statut = 'Hors poste', MinutesHorsPoste = 0, MinutesTravaillees = 0 WHERE IdEmploye = {doc.IdEmploye}");
                    changed = true;
                    OnEventLog?.Invoke($"🌙 {doc.NomComplet} a terminé son service (10h).");
                } else {
                    _db.ExecuteNonQuery($"UPDATE Personnel_Actif SET MinutesTravaillees = {doc.MinutesTravaillees} WHERE IdEmploye = {doc.IdEmploye}");
                }
            } else if (doc.Statut == "En pause") {
                doc.MinutesEnPause += minutesPassed;
                if (doc.MinutesEnPause >= 10) {
                    _db.ExecuteNonQuery($"UPDATE Personnel_Actif SET Statut = 'En poste', MinutesTravaillees = 0 WHERE IdEmploye = {doc.IdEmploye}");
                    changed = true;
                    OnEventLog?.Invoke($"🩺 {doc.NomComplet} revient de pause.");
                } else {
                    _db.ExecuteNonQuery($"UPDATE Personnel_Actif SET MinutesEnPause = {doc.MinutesEnPause} WHERE IdEmploye = {doc.IdEmploye}");
                }
            } else if (doc.Statut == "Hors poste") {
                doc.MinutesHorsPoste += minutesPassed;
                if (doc.MinutesHorsPoste >= 720) {
                    _db.ExecuteNonQuery($"UPDATE Personnel_Actif SET Statut = 'En poste', MinutesTravaillees = 0 WHERE IdEmploye = {doc.IdEmploye}");
                    changed = true;
                    OnEventLog?.Invoke($"☀️ {doc.NomComplet} commence son service.");
                } else {
                    _db.ExecuteNonQuery($"UPDATE Personnel_Actif SET MinutesHorsPoste = {doc.MinutesHorsPoste} WHERE IdEmploye = {doc.IdEmploye}");
                }
            }
        }

        if (changed) {
            OnPersonnelChanged?.Invoke();
        }
    }
}
