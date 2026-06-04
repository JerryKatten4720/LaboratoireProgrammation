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
    private int _lastSimulatedDay = 1;

    public DateTime GameTime => _gameTime;

    public event Action<DateTime>? OnTick;
    public event Action<string>? OnEventLog;
    public event Action? OnPersonnelChanged;
    public event Action? OnPatientsChanged;

    public SimulationService(DatabaseManager db) {
        _db = db;
        var hosp = _db.GetHospital();
        if (hosp != null) {
            if (TimeSpan.TryParse(hosp.HeureSimulation, out var ts)) {
                _gameTime = new DateTime(2026, 1, 1).AddDays(hosp.JourSimulation - 1).Add(ts);
            } else {
                _gameTime = new DateTime(2026, 1, 1).AddDays(hosp.JourSimulation - 1).AddHours(8);
            }
            _lastSimulatedDay = hosp.JourSimulation;
        } else {
            _gameTime = new DateTime(2026, 1, 1).AddHours(8);
            _lastSimulatedDay = 1;
        }
        _lastDoctorUpdate = _gameTime;
        _lastPatientUpdate = _gameTime;
    }

    public void Start() {
        CatchUpSimulation();

        _clockTimer = new DispatcherTimer {
            Interval = TimeSpan.FromSeconds(HospitalSettings.Current.GameTickIntervalSeconds)
        };
        _clockTimer.Tick += (s, e) => {
            _gameTime = _gameTime.AddSeconds(HospitalSettings.Current.GameTickIntervalSeconds * HospitalSettings.Current.TimeScaleMultiplier);
            OnTick?.Invoke(_gameTime);

            int currentDay = (_gameTime - new DateTime(2026, 1, 1)).Days + 1;
            _db.SaveRealLifeTime(DateTime.Now, currentDay, _gameTime.TimeOfDay);

            UpdateDoctorsShifts();
            UpdatePatientsTreatment();
            CheckDayTransition();
        };
        _clockTimer.Start();
        
        OnTick?.Invoke(_gameTime);
        UpdateDoctorsShifts();
        UpdatePatientsTreatment();
        CheckDayTransition();
    }

    public void Stop() {
        _clockTimer?.Stop();
    }

    private void UpdateDoctorsShifts() {
        if ((_gameTime - _lastDoctorUpdate).TotalMinutes < 1) return;
        int minutesPassed = (int)(_gameTime - _lastDoctorUpdate).TotalMinutes;
        _lastDoctorUpdate = _gameTime;
        UpdateDoctorsShiftsStep(minutesPassed);
    }

    private void UpdateDoctorsShiftsStep(int minutesPassed) {
        var docs = _db.GetPersonnel().Where(p => p.Categorie == "Corps Médical").ToList();
        bool changed = false;

        foreach (var doc in docs) {
            if (doc.Statut == "En poste") {
                doc.MinutesTravaillees += minutesPassed;
                if (doc.MinutesTravaillees >= doc.ProchainePauseMinutes) {
                    _db.UpdatePersonnelShiftData(doc.IdEmploye, "En pause", doc.MinutesTravaillees, 0, doc.MinutesHorsPoste);
                    changed = true;
                    OnEventLog?.Invoke($"☕ {doc.NomComplet} part en pause.");
                } else if (doc.MinutesTravaillees >= 600) {
                    _db.UpdatePersonnelShiftData(doc.IdEmploye, "Hors poste", 0, 0, 0);
                    changed = true;
                    OnEventLog?.Invoke($"🌙 {doc.NomComplet} a terminé son service (10h).");
                } else {
                    _db.UpdatePersonnelMinutes(doc.IdEmploye, "MinutesTravaillees", doc.MinutesTravaillees);
                }
            } else if (doc.Statut == "En pause") {
                doc.MinutesEnPause += minutesPassed;
                if (doc.MinutesEnPause >= 15) {
                    int nextPause = doc.MinutesTravaillees + 120;
                    _db.UpdatePersonnelShiftDataAndNextPause(doc.IdEmploye, "En poste", doc.MinutesTravaillees, 0, doc.MinutesHorsPoste, nextPause);
                    changed = true;
                    OnEventLog?.Invoke($"🩺 {doc.NomComplet} revient de pause.");
                } else {
                    _db.UpdatePersonnelMinutes(doc.IdEmploye, "MinutesEnPause", doc.MinutesEnPause);
                }
            } else if (doc.Statut == "Hors poste") {
                doc.MinutesHorsPoste += minutesPassed;
                if (doc.MinutesHorsPoste >= 720) {
                    _db.UpdatePersonnelShiftDataAndNextPause(doc.IdEmploye, "En poste", 0, 0, 0, 120);
                    changed = true;
                    OnEventLog?.Invoke($"☀️ {doc.NomComplet} commence son service.");
                } else {
                    _db.UpdatePersonnelMinutes(doc.IdEmploye, "MinutesHorsPoste", doc.MinutesHorsPoste);
                }
            }
        }

        OnPersonnelChanged?.Invoke();
    }

    private void UpdatePatientsTreatment() {
        if ((_gameTime - _lastPatientUpdate).TotalMinutes < 1) return;
        int minutesPassed = (int)(_gameTime - _lastPatientUpdate).TotalMinutes;
        _lastPatientUpdate = _gameTime;
        UpdatePatientsTreatmentStep(minutesPassed);
    }

    private void UpdatePatientsTreatmentStep(int minutesPassed) {
        float hoursPassed = minutesPassed / 60.0f;
        var activePatients = _db.GetPatientsRaw().Where(p => p.Statut == "En Diagnostic" || p.Statut == "En Traitement").ToList();
        var docs = _db.GetPersonnel().ToDictionary(d => d.IdEmploye, d => d);
        bool changed = false;

        foreach (var patient in activePatients) {
            bool docOnDuty = false;
            if (patient.IdMedecinAssigne.HasValue && docs.TryGetValue(patient.IdMedecinAssigne.Value, out var doc)) {
                if (doc.Statut == "En poste") {
                    docOnDuty = true;
                }
            }

            if (!docOnDuty) {
                continue;
            }

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
                if (patient.Statut == "En Diagnostic" && newTime <= patient.TempsTraitementTotal * 0.8f) {
                    _db.UpdatePatientStatut(patient.IdPatient, "En Traitement");
                    patient.Statut = "En Traitement";
                    changed = true;
                }
            }
        }

        if (changed) {
            OnPatientsChanged?.Invoke();
        }
    }

    private void CheckDayTransition() {
        int currentDay = (_gameTime - new DateTime(2026, 1, 1)).Days + 1;
        if (currentDay > _lastSimulatedDay) {
            for (int d = _lastSimulatedDay + 1; d <= currentDay; d++) {
                ExecuteDayTransition(d);
            }
            _lastSimulatedDay = currentDay;
        }
    }

    private void ExecuteDayTransition(int newDay) {
        var personnel = _db.GetPersonnel();
        decimal totalSalaries = 0;
        foreach (var emp in personnel) {
            totalSalaries += emp.SalaireJour;
        }
        if (totalSalaries > 0) {
            _db.AddBudget(-totalSalaries, $"Salaires du personnel (Jour {newDay - 1})", newDay - 1, "Paiement Salaire");
            _db.CreateFactureHopital("Paiement Salaire", totalSalaries, $"Salaires versés au personnel hospitalier pour le Jour {newDay - 1}.");
        }

        var unites = _db.GetUnites();
        decimal totalMaintenance = 0;
        foreach (var u in unites) {
            totalMaintenance += u.CoutEntretienJour;
        }
        if (totalMaintenance > 0) {
            _db.AddBudget(-totalMaintenance, $"Frais d'entretien des services (Jour {newDay - 1})", newDay - 1, "Frais Entretien");
            _db.CreateFactureHopital("Frais Entretien", totalMaintenance, $"Frais de maintenance et d'entretien des services hospitaliers pour le Jour {newDay - 1}.");
        }

        decimal funeralFees = _db.GetAccumulatedFuneralFees();
        if (funeralFees > 0) {
            _db.AddBudget(-funeralFees, $"Services Funéraires (Jour {newDay - 1})", newDay - 1, "Frais Funéraires");
            _db.CreateFactureHopital("Services Funéraires", funeralFees, $"Frais de services funéraires pour les cadavres qui n'ont pas pu être placés en Chambre froide le Jour {newDay - 1}.");
            _db.ResetAccumulatedFuneralFees();
        }

        _db.UpdateHospitalTime(newDay, _gameTime.TimeOfDay);
        OnEventLog?.Invoke($"📅 Jour {newDay} : Paie du personnel (-{totalSalaries:N0} $) & Frais d'entretien (-{totalMaintenance:N0} $)");
    }

    public void CatchUpSimulation() {
        var hosp = _db.GetHospital();
        if (hosp == null) return;

        var lastRealTime = _db.GetLastRealLifeTime();
        if (!lastRealTime.HasValue) return;

        double realSecondsPassed = (DateTime.Now - lastRealTime.Value).TotalSeconds;
        if (realSecondsPassed <= 0) return;

        double simSecondsPassed = realSecondsPassed * HospitalSettings.Current.TimeScaleMultiplier;
        int simMinutesPassed = (int)(simSecondsPassed / 60.0);

        if (simMinutesPassed <= 0) return;

        OnEventLog?.Invoke($"⏳ Application fermée pendant {simMinutesPassed} minutes simulées. Rattrapage en cours...");

        for (int i = 0; i < simMinutesPassed; i++) {
            _gameTime = _gameTime.AddMinutes(1);
            UpdateDoctorsShiftsStep(1);
            UpdatePatientsTreatmentStep(1);
            CheckDayTransition();
        }

        int currentDay = (_gameTime - new DateTime(2026, 1, 1)).Days + 1;
        _db.SaveRealLifeTime(DateTime.Now, currentDay, _gameTime.TimeOfDay);
        OnEventLog?.Invoke($"✅ Rattrapage de la simulation terminé.");
    }

    public void Dispose() {
        Stop();
        _clockTimer = null;
    }
}
