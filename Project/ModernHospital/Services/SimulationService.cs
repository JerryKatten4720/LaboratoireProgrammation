using System;
using System.Linq;
using System.Windows.Threading;

namespace LaboratoireProgrammation.Project.ModernHospital;

public class SimulationService : IDisposable {
    private readonly DatabaseManager _db;
    private DispatcherTimer? _clockTimer;
    private DateTime _gameTime;
    private DateTime _lastDoctorUpdate = DateTime.MinValue;
    private DateTime _lastPatientUpdate = DateTime.MinValue;

    public event Action<DateTime>? OnTick;
    public event Action<string>? OnEventLog;
    public event Action? OnPersonnelChanged;
    public event Action? OnPatientsChanged;

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
            UpdatePatientsTreatment();
        };
        _clockTimer.Start();
        
        OnTick?.Invoke(_gameTime);
        UpdateDoctorsShifts();
        UpdatePatientsTreatment();
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
                    _db.UpdatePersonnelShiftData(doc.IdEmploye, "En pause", doc.MinutesTravaillees, 0, doc.MinutesHorsPoste);
                    changed = true;
                    OnEventLog?.Invoke($"☕ {doc.NomComplet} part en pause.");
                } else if (doc.MinutesTravaillees >= 600) {
                    _db.UpdatePersonnelShiftData(doc.IdEmploye, "Hors poste", 0, doc.MinutesEnPause, 0);
                    changed = true;
                    OnEventLog?.Invoke($"🌙 {doc.NomComplet} a terminé son service (10h).");
                } else {
                    _db.UpdatePersonnelMinutes(doc.IdEmploye, "MinutesTravaillees", doc.MinutesTravaillees);
                }
            } else if (doc.Statut == "En pause") {
                doc.MinutesEnPause += minutesPassed;
                if (doc.MinutesEnPause >= 10) {
                    _db.UpdatePersonnelShiftData(doc.IdEmploye, "En poste", 0, doc.MinutesEnPause, doc.MinutesHorsPoste);
                    changed = true;
                    OnEventLog?.Invoke($"🩺 {doc.NomComplet} revient de pause.");
                } else {
                    _db.UpdatePersonnelMinutes(doc.IdEmploye, "MinutesEnPause", doc.MinutesEnPause);
                }
            } else if (doc.Statut == "Hors poste") {
                doc.MinutesHorsPoste += minutesPassed;
                if (doc.MinutesHorsPoste >= 720) {
                    _db.UpdatePersonnelShiftData(doc.IdEmploye, "En poste", 0, doc.MinutesEnPause, doc.MinutesHorsPoste);
                    changed = true;
                    OnEventLog?.Invoke($"☀️ {doc.NomComplet} commence son service.");
                } else {
                    _db.UpdatePersonnelMinutes(doc.IdEmploye, "MinutesHorsPoste", doc.MinutesHorsPoste);
                }
            }
        }

        if (changed) {
            OnPersonnelChanged?.Invoke();
        }
    }

    private void UpdatePatientsTreatment() {
        if ((_gameTime - _lastPatientUpdate).TotalMinutes < 1) return;
        
        int minutesPassed = (int)(_gameTime - _lastPatientUpdate).TotalMinutes;
        if (_lastPatientUpdate == DateTime.MinValue) minutesPassed = 1;
        _lastPatientUpdate = _gameTime;

        float hoursPassed = minutesPassed / 60.0f;
        var activePatients = _db.GetPatientsRaw().Where(p => p.Statut == "En Diagnostic" || p.Statut == "En Traitement").ToList();
        bool changed = false;

        foreach (var patient in activePatients) {
            float newTime = patient.TempsTraitementRestant - hoursPassed;
            if (newTime <= 0f) {
                string outcome = _db.EvaluateTreatmentOutcome(patient.IdPatient);
                var fullPatient = _db.GetPatients().FirstOrDefault(p => p.IdPatient == patient.IdPatient);
                if (fullPatient != null) {
                    PatientHelper.HandlePatientStatusChange(_db, fullPatient, outcome);
                    changed = true;
                    if (outcome == "Guéri") {
                        OnEventLog?.Invoke($"✨ Patient {fullPatient.Nom} est guéri après son traitement.");
                    } else {
                        OnEventLog?.Invoke($"☠ Patient {fullPatient.Nom} est décédé.");
                    }
                }
            } else {
                _db.UpdatePatientTreatmentTime(patient.IdPatient, newTime);
            }
        }

        if (changed) {
            OnPatientsChanged?.Invoke();
        }
    }

    public void Dispose() {
        Stop();
        _clockTimer = null;
    }
}
