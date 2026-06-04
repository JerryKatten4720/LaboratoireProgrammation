using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Color = System.Windows.Media.Color;

namespace LaboratoireProgrammation.Project.ModernHospital;

public partial class HospitalWindow : Window {

    private readonly DatabaseManager _db = new();
    private readonly ObservableCollection<string> _eventLog = new();
    
    private Button? _activeNavBtn;
    private ObservableCollection<ChambreGroup> _chambresGroupes = new();
    private HospitalInfo? _hospital;
    private SimulationService _simulationService;

    public HospitalWindow() {
        InitializeComponent();
        Loaded += OnLoaded;
        SizeChanged += OnSizeChanged;

        HospitalSettings.Load();
        
        _simulationService = new SimulationService(_db);
        _simulationService.OnTick += t => {
            TbDate.Text = t.ToString("dd/MM/yyyy");
            TbHeure.Text = t.ToString("HH:mm:ss");
        };
        _simulationService.OnEventLog += LogEvent;
        _simulationService.OnPersonnelChanged += RefreshPersonnel;
        _simulationService.OnPatientsChanged += () => {
            Dispatcher.Invoke(() => {
                RefreshPatients();
                RefreshDashboard();
                RefreshFinances();
            });
        };
        Closing += (_, _) => _simulationService.Dispose();
    }

    private void OnLoaded(object sender, RoutedEventArgs e) {
        try {
            _db.ExecuteCommand("ALTER TABLE Chambre MODIFY COLUMN TypeChambre ENUM('Standard', 'Standard - Individuel', 'Standard - Commune', 'Soins Intensifs', 'Isolement', 'Bloc Opératoire') NOT NULL");
            _db.ExecuteCommand("UPDATE Chambre SET TypeChambre = 'Standard - Commune' WHERE TypeChambre = 'Standard'");
            _db.ExecuteCommand("ALTER TABLE Chambre MODIFY COLUMN TypeChambre ENUM('Standard - Individuel', 'Standard - Commune', 'Soins Intensifs', 'Isolement', 'Bloc Opératoire') NOT NULL");

            _db.ExecuteCommand("ALTER TABLE Personnel_Actif MODIFY COLUMN Statut ENUM('En poste', 'En pause', 'Absent', 'Épuisé', 'Hors poste') DEFAULT 'En poste'");
            _db.ExecuteCommand("ALTER TABLE Personnel_Actif ADD COLUMN MinutesTravaillees INT DEFAULT 0");
            _db.ExecuteCommand("ALTER TABLE Personnel_Actif ADD COLUMN MinutesEnPause INT DEFAULT 0");
            _db.ExecuteCommand("ALTER TABLE Personnel_Actif ADD COLUMN MinutesHorsPoste INT DEFAULT 0");
        } catch { }

        try {
            _db.ExecuteCommand("ALTER TABLE Cas_Cliniques MODIFY COLUMN TempsTraitementHeures FLOAT NOT NULL");
            _db.ExecuteCommand("ALTER TABLE Patients_Actifs MODIFY COLUMN TempsTraitementRestant FLOAT DEFAULT 0");
            _db.ExecuteCommand("ALTER TABLE Cas_Cliniques ADD COLUMN TauxRemission FLOAT DEFAULT 50.0");
            _db.ExecuteCommand("ALTER TABLE Cas_Cliniques ADD COLUMN SpecialisteTraitement VARCHAR(100)");
        } catch { }

        try {
            _db.ExecuteCommand("ALTER TABLE Facture ADD COLUMN CodeFacture VARCHAR(10)");
        } catch { }

        try {
            var invoices = _db.GetFactures();
            foreach (var inv in invoices) {
                if (string.IsNullOrEmpty(inv.CodeFacture) || inv.CodeFacture.Length < 6) {
                    string newCode = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
                    _db.ExecuteCommand(
                        "UPDATE Facture SET CodeFacture = @code WHERE IdFacture = @id",
                        cmd => {
                            cmd.Parameters.AddWithValue("@code", newCode);
                            cmd.Parameters.AddWithValue("@id", inv.IdFacture);
                        }
                    );
                }
            }
        } catch { }

        try {
            _db.ExecuteCommand("ALTER TABLE Facture ADD UNIQUE INDEX idx_code_facture (CodeFacture)");
        } catch { }

        try {
            _db.ExecuteCommand("CREATE TABLE IF NOT EXISTS Specialite (IdSpecialite INT AUTO_INCREMENT PRIMARY KEY, Nom VARCHAR(100) NOT NULL UNIQUE)");
            int specCount = _db.ExecuteScalar<int>("SELECT COUNT(*) FROM Specialite");
            if (specCount == 0) {
                string[] specs = { "Urgences", "Chirurgie", "Pathologie", "Cardiologie", "Oncologie", "Gériatrie", "Médecine Interne", "Neuroimagerie", "Traumatologie", "Médecine Nucléaire", "Prélèvements", "Chimiothérapie", "Logistique", "Général" };
                foreach (var s in specs) {
                    try {
                        _db.ExecuteCommand("INSERT INTO Specialite (Nom) VALUES (@nom)", cmd => cmd.Parameters.AddWithValue("@nom", s));
                    } catch { }
                }
            }
        } catch { }

        try {
            _db.ExecuteCommand("ALTER TABLE LigneFacture MODIFY COLUMN TypePrestation ENUM('Chambre', 'Soin', 'Médicament') NOT NULL");
        } catch { }

        try {
            _db.ExecuteCommand("UPDATE To_Hire SET Specialite = 'Urgences' WHERE Specialite = 'Diagnostic'");
            _db.ExecuteCommand("UPDATE To_Hire SET Specialite = 'Neuroimagerie' WHERE Specialite = 'Neurochirurgie'");
            _db.ExecuteCommand("UPDATE To_Hire SET Specialite = 'Chirurgie' WHERE Specialite = 'Chirurgie Plastique'");
            _db.ExecuteCommand("UPDATE To_Hire SET Specialite = 'Pathologie' WHERE Specialite = 'Pédiatrie'");
            _db.ExecuteCommand("UPDATE To_Hire SET Specialite = 'Général' WHERE Specialite = 'Médecine Générale'");
            _db.ExecuteCommand("UPDATE To_Hire SET Specialite = 'Général' WHERE Specialite = 'Obstétrique'");
            _db.ExecuteCommand("UPDATE To_Hire SET Specialite = 'Général' WHERE Specialite = 'Gynécologie'");
            _db.ExecuteCommand("UPDATE To_Hire SET Specialite = 'Urgences' WHERE Specialite = 'Soins Intensifs'");
        } catch { }

        RootGrid.Width = ActualWidth;
        RootGrid.Height = ActualHeight;

        IcEventLog.ItemsSource = _eventLog;

        if (!_db.TestConnection()) {
            MainPanel.Visibility = Visibility.Hidden;
            DbErrorPanel.Visibility = Visibility.Visible;
            
            SetStatus("❌ Connexion DB échouée : vérifiez vos identifiants", Colors.OrangeRed);
            
            return;
        }

        SetStatus("✔️ Connexion établie", Color.FromRgb(0, 212, 170));

        Nav_Click(BtnDashboard, null!);

        LoadAdmitForm();

        RefreshAll();
        _simulationService.Start();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e) {
        RootGrid.Width = e.NewSize.Width;
        RootGrid.Height = e.NewSize.Height;
    }

    private void Nav_Click(object sender, RoutedEventArgs e) {
        if (sender is not Button btn) return;
        var tag = btn.Tag?.ToString() ?? "";

        if (_activeNavBtn != null) _activeNavBtn.Style = (Style)FindResource("NavBtn");
        btn.Style = (Style)FindResource("NavBtnActive");
        _activeNavBtn = btn;

        PanelDashboard.Visibility = Visibility.Collapsed;
        PanelPatients.Visibility = Visibility.Collapsed;
        PanelPersonnel.Visibility = Visibility.Collapsed;
        PanelFinances.Visibility = Visibility.Collapsed;
        PanelChambres.Visibility = Visibility.Collapsed;
        PanelMedicaments.Visibility = Visibility.Collapsed;
        PanelFacturation.Visibility = Visibility.Collapsed;
        PanelHitParade.Visibility = Visibility.Collapsed;
        PanelReservations.Visibility = Visibility.Collapsed;

        switch (tag) {
            case "dashboard":
                PanelDashboard.Visibility = Visibility.Visible;
                RefreshDashboard();
                break;
            case "patients":
                PanelPatients.Visibility = Visibility.Visible;
                RefreshPatients();
                LoadAdmitForm();
                break;
            case "personnel":
                PanelPersonnel.Visibility = Visibility.Visible;
                RefreshPersonnel();
                break;
            case "finances":
                PanelFinances.Visibility = Visibility.Visible;
                RefreshFinances();
                break;
            case "chambres":
                PanelChambres.Visibility = Visibility.Visible;
                RefreshChambres();
                break;
            case "medicaments":
                PanelMedicaments.Visibility = Visibility.Visible;
                RefreshMedicaments();
                break;
            case "facturation":
                PanelFacturation.Visibility = Visibility.Visible;
                RefreshFactures();
                break;
            case "hitparade":
                PanelHitParade.Visibility = Visibility.Visible;
                RefreshHitParade();
                break;
            case "reservations":
                PanelReservations.Visibility = Visibility.Visible;
                RefreshReservations();
                break;
        }
    }

    private void RefreshAll() {
        _hospital = _db.GetHospital();
        if (_hospital == null) return;
        RefreshSidebar();
        RefreshDashboard();
    }

    private void RefreshSidebar() {
        if (_hospital == null) return;
        TbBudget.Text = $"$ {_hospital.Budget:N0}";
    }

    private void RefreshDashboard() {
        _hospital = _db.GetHospital();
        if (_hospital == null) return;
        RefreshSidebar();

        var stats = _db.GetStats();
        KpiPatientsActifs.Text = stats.TotalPatientsActifs.ToString();
        KpiAttente.Text = $"{stats.PatientsEnAttente} en attente";
        KpiPersonnel.Text = stats.TotalPersonnel.ToString();
        KpiLitsLibres.Text = stats.LitsLibres.ToString();
        KpiLitsOccupes.Text = $"{stats.LitsOccupes} occupés";
        KpiRevenu.Text = $"+ {stats.RevenuJour:N0} $";
        KpiDepenses.Text = $"- {stats.DepensesJour:N0} $";
        
        if (stats.RevenuJour > stats.DepensesJour) KpiTotal.Text = $"+ {stats.RevenuJour - stats.DepensesJour:N0} $";
        else if (stats.RevenuJour < stats.DepensesJour) KpiTotal.Text = $"- {Math.Abs(stats.RevenuJour - stats.DepensesJour):N0} $";
        else KpiTotal.Text = "0 $";
        
        KpiGueris.Text = stats.PatientsGueris.ToString();
        KpiDeces.Text = stats.PatientsDeces.ToString();
        KpiBudget.Text = $"${_hospital.Budget:N0}";
        KpiTraitement.Text = stats.PatientsEnTraitement.ToString();
    }

    private void RefreshPatients() {
        var patients = _db.GetPatients();
        LvPatients.ItemsSource = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Where(patients, p => p.Statut != "Guéri" && p.Statut != "Décédé"));
        LvPatientsHistorique.ItemsSource = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Where(patients, p => p.Statut == "Guéri" || p.Statut == "Décédé"));
    }

    private void RefreshPersonnel() {
        LvPersonnel.ItemsSource = _db.GetPersonnel();
        LvCandidats.ItemsSource = _db.GetCandidats();
    }

    private void RefreshChambres() {
        _chambresGroupes = _db.GetChambresHierarchiques();
        IcChambres.ItemsSource = _chambresGroupes;
    }

    private void RefreshFinances() {
        LvTransactions.ItemsSource = _db.GetRecentTransactions(50);
    }

    private void RefreshAllData() {
        RefreshPatients();
        RefreshPersonnel();
        RefreshChambres();
        RefreshDashboard();
        RefreshFinances();
    }

    private void LoadAdmitForm() {
        var casCliniques = _db.GetCasCliniques();
        var litsLibres = _db.GetLitsLibres();
        var medecins = _db.GetPersonnel().Where(p => p.Categorie == "Corps Médical").ToList();

        CbMaladie.ItemsSource = casCliniques;
        CbMaladie.SelectedIndex = 0;
        CbLit.ItemsSource = litsLibres;
        CbLit.SelectedIndex = 0;
        CbMedecin.ItemsSource = medecins;
        CbMedecin.SelectedIndex = 0;
        CbClasse.SelectedIndex = 0;

        CbResMaladie.ItemsSource = casCliniques;
        CbResMaladie.SelectedIndex = 0;
        CbResLit.ItemsSource = litsLibres;
        CbResLit.SelectedIndex = 0;
        CbResMedecin.ItemsSource = medecins;
        CbResMedecin.SelectedIndex = 0;
        CbResClasse.SelectedIndex = 0;
    }

    private void BtnReserver_Click(object sender, RoutedEventArgs e) {
        if (CbResMaladie.SelectedItem == null || CbResLit.SelectedItem == null || CbResMedecin.SelectedItem == null) {
            PopupFactory.ShowAlert(this, "Veuillez sélectionner la maladie, le lit et le médecin pour la réservation.");
            return;
        }

        string nom = TbResPatientNom.Text;
        if (string.IsNullOrWhiteSpace(nom)) {
            PopupFactory.ShowAlert(this, "Veuillez entrer un nom de patient valide.");
            return;
        }

        var maladie = (CasClinique)CbResMaladie.SelectedItem;
        var lit = (LitDisponible)CbResLit.SelectedItem;
        var medecin = (Employe)CbResMedecin.SelectedItem;
        string classe = ((ComboBoxItem)CbResClasse.SelectedItem).Content.ToString();

        try {
            _db.ReservePatient(nom, maladie.IdCas, lit.IdLit, medecin.IdEmploye, classe, maladie.TempsTraitementHeures);

            LogEvent($"📅 Réservation enregistrée pour {nom} au lit {lit.NumeroLit}");
            TbResPatientNom.Text = "";
            LoadAdmitForm();
            RefreshPatients();
            RefreshReservations();
            RefreshDashboard();
            SetStatus($"✔️ Réservation effectuée avec succès", Color.FromRgb(0, 212, 170));
        }
        catch (Exception ex) {
            SetStatus($"❌ Erreur réservation: {ex.Message}", Colors.OrangeRed);
        }
    }

    private void BtnAdmettre_Click(object sender, RoutedEventArgs e) {
        if (_hospital == null) return;
        if (CbMaladie.SelectedItem is not CasClinique cas) {
            ShowAlert("Sélectionnez une maladie.");
            return;
        }

        if (CbLit.SelectedItem is not LitDisponible lit) {
            ShowAlert("Aucun lit disponible.");
            return;
        }

        var nom = TbPatientNom.Text.Trim();
        if (string.IsNullOrEmpty(nom)) {
            ShowAlert("Entrez un nom de patient.");
            return;
        }

        var idMedecin = (CbMedecin.SelectedItem as Employe)?.IdEmploye;
        var classe = (CbClasse.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Standard";

        try {
            _db.AdmitPatient(nom, cas.IdCas, lit.IdLit, idMedecin, classe, cas.TempsTraitementHeures);
            LogEvent($"🏥 Patient admis: {nom} ({cas.Maladie}) : Lit {lit.NumeroLit}");
            RefreshPatients();
            LoadAdmitForm();
            RefreshDashboard();
            SetStatus($"✅ Patient {nom} admis avec succès", Color.FromRgb(0, 212, 170));
        }
        catch (Exception ex) {
            SetStatus($"❌ Erreur admission: {ex.Message}", Colors.OrangeRed);
        }
    }

    private void BtnFire_Click(object sender, RoutedEventArgs e) {
        if (LvPersonnel.SelectedItem is not Employe emp) {
            ShowAlert("Sélectionnez un employé.");
            return;
        }

        var firePopup = PopupFactory.CreateConfirmationPopup(
            $"Êtes-vous sûr de vouloir licencier {emp.NomComplet} ?",
            "#FF4D6A",
            () => {
                _db.FireEmploye(emp.IdEmploye);
                LogEvent($"🗑 {emp.NomComplet} licencié(e)");
                RefreshPersonnel();
                RefreshDashboard();
                SetStatus($"✅ {emp.NomComplet} a été licencié", Color.FromRgb(255, 77, 106));
            },
            () => { }
        );

        firePopup.Owner = this;
        DimOverlay.Visibility = Visibility.Visible;
        firePopup.ShowDialog();
        DimOverlay.Visibility = Visibility.Collapsed;
    }

    private void BtnHire_Click(object sender, RoutedEventArgs e) {
        var doctorForm = new DoctorCreationForm(_db) {
            OnDoctorCreated = () => {
                RefreshPersonnel();
                RefreshDashboard();
                SetStatus("✅ Médecin créé avec succès", Color.FromRgb(0, 212, 170));
                LogEvent("👨‍⚕️ Nouveau médecin créé");
            },
            OnCancelled = () => { }
        };
        doctorForm.Owner = this;
        DimOverlay.Visibility = Visibility.Visible;
        doctorForm.ShowDialog();
        DimOverlay.Visibility = Visibility.Collapsed;
    }

    private void BtnRecruterCandidat_Click(object sender, RoutedEventArgs e) {
        if (_hospital == null) return;
        if (sender is Button btn && btn.Tag is Candidat candidat) {
            if (_hospital.Budget < candidat.PrimeEmbauche) {
                PopupFactory.ShowAlert(this, $"Budget insuffisant pour embaucher {candidat.NomComplet}. Prime requise: ${candidat.PrimeEmbauche:N0}");
                return;
            }

            var confirm = PopupFactory.CreateConfirmationPopup(
                $"Recruter {candidat.NomComplet} (Prime: ${candidat.PrimeEmbauche:N0}, Salaire: ${candidat.SalaireJour}/j) ?",
                "#00d4aa",
                () => {
                    bool success = _db.HireCandidat(candidat.IdCandidat, _hospital);
                    if (success) {
                        LogEvent($"🤝 {candidat.NomComplet} embauché(e).");
                        RefreshPersonnel();
                        RefreshDashboard();
                        SetStatus($"✔️ {candidat.NomComplet} embauché avec succès", Color.FromRgb(0, 212, 170));
                    } else {
                        PopupFactory.ShowAlert(this, "Erreur lors de l'embauche.");
                    }
                }
            );
            confirm.Owner = this;
            DimOverlay.Visibility = Visibility.Visible;
            confirm.ShowDialog();
            DimOverlay.Visibility = Visibility.Collapsed;
        }
    }

    private void RefreshPatients_Click(object sender, RoutedEventArgs e) {
        RefreshPatients();
    }

    private void BtnVoirReservations_Click(object sender, RoutedEventArgs e) {
        var win = new ViewReservationsWindow(_db);
        win.Owner = this;
        if (win.ShowDialog() == true) {
            RefreshPatients();
            RefreshReservations();
            RefreshDashboard();
        }
    }

    private void RefreshFinances_Click(object sender, RoutedEventArgs e) {
        RefreshFinances();
    }

    private void LogEvent(string msg) {
        _eventLog.Insert(0, msg);
        if (_eventLog.Count > 100) _eventLog.RemoveAt(_eventLog.Count - 1);
        TbLastEvent.Text = msg;
    }

    private void SetStatus(string msg, Color color) {
        TbStatus.Text = msg;
        TbStatus.Foreground = new SolidColorBrush(color);
    }

    private void ShowAlert(string message) {
        var alertPopup = PopupFactory.CreateConfirmationPopup(
            message, "#FFB347", () => { }, () => { }
        );

        alertPopup.Owner = this;
        DimOverlay.Visibility = Visibility.Visible;
        alertPopup.ShowDialog();
        DimOverlay.Visibility = Visibility.Collapsed;
    }

    private void LvPatients_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
        if (LvPatients.SelectedItem is PatientActif patient) {
            var menu = new PatientActionMenu(patient) { Owner = this };
            menu.ActionTriggered += (act) => HandlePatientAction(patient, act);

            var mousePos = PointToScreen(Mouse.GetPosition(this));
            var source = PresentationSource.FromVisual(this);
            if (source?.CompositionTarget != null) {
                var matrix = source.CompositionTarget.TransformToDevice;
                menu.Left = mousePos.X / matrix.M11;
                menu.Top = mousePos.Y / matrix.M22;
            } else {
                menu.Left = mousePos.X;
                menu.Top = mousePos.Y;
            }
            menu.Show();
        }
    }

    private void HandlePatientAction(PatientActif patient, string action) {
        switch (action) {
            case "Gueri":
                PatientHelper.HandlePatientStatusChange(_db, patient, "Guéri");
                LogEvent($"✨ Patient guéri: {patient.Nom}");
                break;
            case "Decede":
                PatientHelper.HandlePatientStatusChange(_db, patient, "Décédé");
                LogEvent($"☠ Patient décédé: {patient.Nom}");
                break;
            case "Supprimer":
                _db.ReleaseBed(patient.IdPatient);
                _db.DeletePatient(patient.IdPatient);
                LogEvent($"🗑 Patient supprimé: {patient.Nom}");
                break;
            case "Prescrire":
                int docId = patient.IdMedecinAssigne ?? _db.GetPersonnel().FirstOrDefault(p => p.Categorie == "Corps Médical")?.IdEmploye ?? 1;
                var presForm = new AddPrescriptionForm(_db, patient.IdPatient, docId) { Owner = this };
                if (presForm.ShowDialog() == true) {
                    LogEvent($"📋 Prescription ajoutée pour {patient.Nom}");
                    RefreshMedicaments();
                }
                break;
            case "AssignerMedecin":
                var assignForm = new AssignDoctorWindow(_db, patient.IdPatient) { Owner = this };
                if (assignForm.ShowDialog() == true) {
                    LogEvent($"🩺 Médecin assigné pour {patient.Nom}");
                }
                break;
            case "Facture":
                Nav_Click(BtnFacturation, null!);
                break;
            default:
                PatientHelper.HandlePatientStatusChange(_db, patient, action);
                break;
        }

        RefreshAllData();
    }

    private void LvPersonnel_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
        if (LvPersonnel.SelectedItem is Employe emp) {
            var menu = new PersonnelActionMenu(emp) { Owner = this };
            menu.ActionTriggered += (act) => HandlePersonnelAction(emp, act);

            var mousePos = PointToScreen(Mouse.GetPosition(this));
            var source = PresentationSource.FromVisual(this);
            if (source?.CompositionTarget != null) {
                var matrix = source.CompositionTarget.TransformToDevice;
                menu.Left = mousePos.X / matrix.M11;
                menu.Top = mousePos.Y / matrix.M22;
            } else {
                menu.Left = mousePos.X;
                menu.Top = mousePos.Y;
            }
            menu.Show();
        }
    }

    private void HandlePersonnelAction(Employe emp, string action) {
        if (action == "Licencier") {
            var firePopup = PopupFactory.CreateConfirmationPopup(
                $"Êtes-vous sûr de vouloir licencier {emp.NomComplet} ?",
                "#FF4D6A",
                () => {
                    _db.FireEmploye(emp.IdEmploye);
                    LogEvent($"🗑 {emp.NomComplet} licencié(e)");
                    RefreshPersonnel();
                    RefreshDashboard();
                    SetStatus($"✅ {emp.NomComplet} a été licencié", Color.FromRgb(255, 77, 106));
                },
                () => { }
            );
            firePopup.Owner = this;
            DimOverlay.Visibility = Visibility.Visible;
            firePopup.ShowDialog();
            DimOverlay.Visibility = Visibility.Collapsed;
        }
        else {
            _db.UpdatePersonnelStatus(emp.IdEmploye, action);
            RefreshPersonnel();
        }
    }

    private void BtnAddLit_Click(object sender, RoutedEventArgs e) {
        var addLitForm = new AddLitForm(_db) {
            OnLitCreated = () => {
                RefreshChambres();
                RefreshDashboard();
                SetStatus("✅ Nouveau lit installé avec succès", Color.FromRgb(0, 212, 170));
                LogEvent("🛏 Ajout d'un nouveau lit dans l'inventaire");
            },
            OnCancelled = () => { }
        };
        addLitForm.Owner = this;
        DimOverlay.Visibility = Visibility.Visible;
        addLitForm.ShowDialog();
        DimOverlay.Visibility = Visibility.Collapsed;
    }

    private void BtnAddChambre_Click(object sender, RoutedEventArgs e) {
        var addChambreForm = new AddChambreForm(_db) {
            OnChambreCreated = () => {
                SetStatus("✅ Nouvelle chambre construite avec succès", Color.FromRgb(0, 212, 170));
                LogEvent("🏗 Construction d'une nouvelle chambre terminée");
            },
            OnCancelled = () => { }
        };
        addChambreForm.Owner = this;
        DimOverlay.Visibility = Visibility.Visible;
        addChambreForm.ShowDialog();
        DimOverlay.Visibility = Visibility.Collapsed;
    }

    private void BtnRemoveLit_Click(object sender, RoutedEventArgs e) {
        if (sender is not FrameworkElement fe || fe.DataContext is not LitInventaire lit) return;

        var deletePopup = PopupFactory.CreateConfirmationPopup(
            $"Êtes-vous sûr de vouloir supprimer le lit {lit.NumeroLit} ?",
            "#FF4D6A",
            () => {
                _db.RemoveLit(lit.IdLit);
                LogEvent($"🗑 Lit {lit.NumeroLit} détruit");
                RefreshChambres();
                RefreshDashboard();
            },
            () => { }
        );
        deletePopup.Owner = this;
        DimOverlay.Visibility = Visibility.Visible;
        deletePopup.ShowDialog();
        DimOverlay.Visibility = Visibility.Collapsed;
    }

    private void BtnRemoveChambre_Click(object sender, RoutedEventArgs e) {
        if (sender is not FrameworkElement fe || fe.DataContext is not ChambreGroup chambre) return;

        var deletePopup = PopupFactory.CreateConfirmationPopup(
            $"Êtes-vous sûr de vouloir supprimer la chambre {chambre.NumeroChambre} ?",
            "#FF4D6A",
            () => {
                _db.RemoveChambre(chambre.IdChambre);
                LogEvent($"🗑 Chambre {chambre.NumeroChambre} supprimée");
                RefreshChambres();
                RefreshDashboard();
                SetStatus($"✅ Chambre {chambre.NumeroChambre} supprimée", Color.FromRgb(255, 77, 106));
            },
            () => { }
        );

        deletePopup.Owner = this;
        DimOverlay.Visibility = Visibility.Visible;
        deletePopup.ShowDialog();
        DimOverlay.Visibility = Visibility.Collapsed;
    }
    private Window? _dragWindow;

    private void Lit_MouseMove(object sender, MouseEventArgs e) {
        if (e.LeftButton == MouseButtonState.Pressed && sender is FrameworkElement fe &&
            fe.DataContext is LitInventaire lit) {
            _dragWindow = new Window {
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Background = Brushes.Transparent,
                IsHitTestVisible = false,
                Topmost = true,
                ShowInTaskbar = false,
                SizeToContent = SizeToContent.WidthAndHeight,
                Opacity = 0.8,
                Content = new Border {
                    Background = new SolidColorBrush(Color.FromRgb(40, 40, 50)),
                    CornerRadius = new CornerRadius(5),
                    Padding = new Thickness(10),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(0, 212, 170)),
                    BorderThickness = new Thickness(1),
                    Child = new TextBlock {
                        Text = "🛏️ " + lit.NumeroLit,
                        Foreground = Brushes.White,
                        FontWeight = FontWeights.Bold
                    }
                }
            };

            var mousePos = PointToScreen(e.GetPosition(this));
            double dpiX = 1.0;
            double dpiY = 1.0;
            var source = PresentationSource.FromVisual(this);
            if (source?.CompositionTarget != null) {
                dpiX = source.CompositionTarget.TransformToDevice.M11;
                dpiY = source.CompositionTarget.TransformToDevice.M22;
            }
            _dragWindow.Left = (mousePos.X / dpiX) + 15;
            _dragWindow.Top = (mousePos.Y / dpiY) + 15;
            _dragWindow.Show();

            fe.GiveFeedback += Fe_GiveFeedback;
            DragDrop.DoDragDrop(fe, lit, DragDropEffects.Move);
            fe.GiveFeedback -= Fe_GiveFeedback;

            _dragWindow.Close();
            _dragWindow = null;
        }
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
    private static extern bool GetCursorPos(ref Win32Point pt);

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    private struct Win32Point {
        public int X;
        public int Y;
    }

    private void Fe_GiveFeedback(object sender, GiveFeedbackEventArgs e) {
        if (_dragWindow != null) {
            Win32Point pt = new Win32Point();
            GetCursorPos(ref pt);
            double dpiX = 1.0;
            double dpiY = 1.0;
            var source = PresentationSource.FromVisual(this);
            if (source?.CompositionTarget != null) {
                dpiX = source.CompositionTarget.TransformToDevice.M11;
                dpiY = source.CompositionTarget.TransformToDevice.M22;
            }
            _dragWindow.Left = (pt.X / dpiX) + 15;
            _dragWindow.Top = (pt.Y / dpiY) + 15;
        }
    }

    private void SvChambres_DragOver(object sender, DragEventArgs e) {
        DragDropHelper.HandleAutoScroll(SvChambres, e);
    }

    private void Chambre_Drop(object sender, DragEventArgs e) {
        if (sender is FrameworkElement fe && fe.DataContext is ChambreGroup chambreCible)
            if (e.Data.GetData(typeof(LitInventaire)) is LitInventaire litDragged) {
                if (litDragged.IdChambre == chambreCible.IdChambre) return;

                _db.UpdateLitChambre(litDragged.IdLit, chambreCible.IdChambre);
                LogEvent($"🛏 Lit {litDragged.NumeroLit} transféré vers {chambreCible.NumeroChambre}");
                RefreshChambres();
            }
    }

    private void Lit_ContextMenuOpening(object sender, ContextMenuEventArgs e) {
        if (sender is FrameworkElement fe && fe.DataContext is LitInventaire lit && fe.ContextMenu != null) {
            fe.ContextMenu.Items.Clear();

            var menuTitle = new MenuItem { Header = "Assigner à la chambre...", IsEnabled = false };
            fe.ContextMenu.Items.Add(menuTitle);
            fe.ContextMenu.Items.Add(new Separator());

            foreach (var chambre in _chambresGroupes) {
                if (chambre.IdChambre == lit.IdChambre) continue;

                var menuItem = new MenuItem {
                    Header = chambre.NumeroChambre,
                    Foreground = new SolidColorBrush(Color.FromRgb(240, 240, 245))
                };

                menuItem.Click += (s, args) => {
                    _db.UpdateLitChambre(lit.IdLit, chambre.IdChambre);
                    LogEvent($"🛏 Lit {lit.NumeroLit} réassigné à {chambre.NumeroChambre}");
                    RefreshChambres();
                };

                fe.ContextMenu.Items.Add(menuItem);
            }

            fe.ContextMenu.Items.Add(new Separator());

            var deleteItem = new MenuItem {
                Header = "Supprimer le lit",
                Foreground = new SolidColorBrush(Color.FromRgb(255, 77, 106))
            };

            deleteItem.Click += (s, args) => {
                _db.RemoveLit(lit.IdLit);
                LogEvent($"🗑 Lit {lit.NumeroLit} détruit");
                RefreshChambres();
            };

            fe.ContextMenu.Items.Add(deleteItem);
        }
    }

    private void RefreshMedicaments() {
        var list = _db.GetMedicaments();
        LvMedicaments.ItemsSource = list;
        var lowStock = list.Where(m => m.StockActuel < m.StockMinimum).ToList();
        foreach (var m in lowStock) {
            LogEvent($"⚠️ Stock bas: {m.Nom} ({m.StockActuel}/{m.StockMinimum})");
        }
    }

    private void RefreshFactures() {
        LvFactures.ItemsSource = _db.GetFactures();
    }

    private void BtnOuvrirFactureHtml_Click(object sender, RoutedEventArgs e) {
        if (sender is Button btn && btn.Tag is Facture facture) {
            string path = facture.CheminFichier;
            if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path)) {
                var p = _db.GetPatients().FirstOrDefault(pat => pat.IdPatient == facture.IdPatient);
                if (p == null) {
                    p = new PatientActif { Nom = facture.PatientNom, Classe = "Inconnue" };
                }
                var hosp = _db.GetHospital();
                var lignes = _db.GetLignesFacture(facture.IdFacture);
                if (hosp != null) {
                    path = Services.FactureGenerator.ExporterFactureHtml(facture, p, hosp, lignes);
                }
            }

            if (!string.IsNullOrEmpty(path) && System.IO.File.Exists(path)) {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true });
            } else {
                PopupFactory.ShowAlert(this, "Le fichier HTML de la facture est introuvable.");
            }
        }
    }

    private void RefreshHitParade() {
        LvHitParade.ItemsSource = _db.GetHitParade();
    }

    private void RefreshReservations() {
        LvReservations.ItemsSource = _db.GetReservations();
    }

    private void BtnAddMedicament_Click(object sender, RoutedEventArgs e) {
        var form = new AddMedicamentForm(_db) { Owner = this };
        if (form.ShowDialog() == true) {
            RefreshMedicaments();
            LogEvent("💊 Nouveau médicament ajouté au catalogue");
        }
    }

    private void LvMedicaments_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
        if (LvMedicaments.SelectedItem is not Medicament medicament) return;

        var fe = e.OriginalSource as FrameworkElement;
        if (fe == null) return;

        fe.ContextMenu = new ContextMenu();
        fe.ContextMenu.Background = new SolidColorBrush(Color.FromRgb(28, 28, 34));
        fe.ContextMenu.Foreground = new SolidColorBrush(Color.FromRgb(240, 240, 245));
        fe.ContextMenu.BorderBrush = new SolidColorBrush(Color.FromRgb(42, 42, 56));

        var editItem = new MenuItem { Header = "Modifier" };
        editItem.Click += (s, args) => {
            var form = new AddMedicamentForm(_db, medicament) { Owner = this };
            if (form.ShowDialog() == true) {
                RefreshMedicaments();
                LogEvent($"💊 Médicament {medicament.Nom} modifié");
            }
        };

        var delItem = new MenuItem { Header = "Supprimer", Foreground = new SolidColorBrush(Color.FromRgb(255, 77, 106)) };
        delItem.Click += (s, args) => {
            _db.RemoveMedicament(medicament.IdMedicament);
            RefreshMedicaments();
            LogEvent($"🗑 Médicament {medicament.Nom} supprimé");
        };

        fe.ContextMenu.Items.Add(editItem);
        fe.ContextMenu.Items.Add(delItem);
        fe.ContextMenu.IsOpen = true;
    }
}