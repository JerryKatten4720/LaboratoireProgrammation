using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Color = System.Windows.Media.Color;

namespace LaboratoireProgrammation.Project.ModernHospital;

public partial class HospitalWindow : Window {
    private static readonly Style _navActive = Application.Current.Resources.Contains("NavBtnActive")
        ? (Style)Application.Current.Resources["NavBtnActive"]
        : new Style(typeof(Button));

    private readonly DatabaseManager _db = new();
    private readonly ObservableCollection<string> _eventLog = new();

    private Button? _activeNavBtn;
    private ObservableCollection<ChambreGroup> _chambresGroupes = new();
    private DispatcherTimer? _clockTimer;
    private HospitalInfo? _hospital;

    public HospitalWindow() {
        InitializeComponent();
        Loaded += OnLoaded;
        SizeChanged += OnSizeChanged;
    }

    private void OnLoaded(object sender, RoutedEventArgs e) {
        RootGrid.Width = ActualWidth;
        RootGrid.Height = ActualHeight;

        IcEventLog.ItemsSource = _eventLog;

        if (!_db.TestConnection()) {
            MainPanel.Visibility = Visibility.Hidden;
            DbErrorPanel.Visibility = Visibility.Visible;
            
            SetStatus("❌ Connexion DB échouée : vérifiez vos identifiants", Colors.OrangeRed);
            
            return;
        }

        SetStatus("✅ Connexion établie", Color.FromRgb(0, 212, 170));

        StartRealTimeClock();

        RefreshAll();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e) {
        RootGrid.Width = e.NewSize.Width;
        RootGrid.Height = e.NewSize.Height;
    }

    private void StartRealTimeClock() {
        _clockTimer = new DispatcherTimer {
            Interval = TimeSpan.FromSeconds(1)
        };
        _clockTimer.Tick += (s, e) => UpdateRealTimeClock();
        _clockTimer.Start();
        UpdateRealTimeClock();
    }

    private void UpdateRealTimeClock() {
        var now = DateTime.Now;
        TbDate.Text = now.ToString("dd/MM/yyyy");
        TbHeure.Text = now.ToString("HH:mm:ss");
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
        KpiLitsOccupes.Text = "occupés";
        KpiRevenu.Text = $"${stats.RevenuJour:N0}";
        KpiDepenses.Text = $"${stats.DepensesJour:N0} dépenses";
        KpiGueris.Text = stats.PatientsGueris.ToString();
        KpiDeces.Text = stats.PatientsDeces.ToString();
        KpiBudget.Text = $"${_hospital.Budget:N0}";
        KpiTraitement.Text = stats.PatientsEnTraitement.ToString();
    }

    private void RefreshPatients() {
        var patients = _db.GetPatients();
        LvPatients.ItemsSource = patients;
    }

    private void RefreshPersonnel() {
        LvPersonnel.ItemsSource = _db.GetPersonnel();
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
        CbMaladie.ItemsSource = _db.GetCasCliniques();
        CbMaladie.SelectedIndex = 0;
        CbLit.ItemsSource = _db.GetLitsLibres();
        CbLit.SelectedIndex = 0;
        CbMedecin.ItemsSource = _db.GetPersonnel()
            .Where(p => p.Categorie == "Corps Médical").ToList();
        CbMedecin.SelectedIndex = 0;
        CbClasse.SelectedIndex = 0;
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

        MainPanel.Children.Add(firePopup);
    }

    private void BtnHire_Click(object sender, RoutedEventArgs e) {
        var doctorForm = new DoctorCreationForm {
            OnDoctorCreated = () => {
                RefreshPersonnel();
                RefreshDashboard();
                SetStatus("✅ Médecin créé avec succès", Color.FromRgb(0, 212, 170));
                LogEvent("👨‍⚕️ Nouveau médecin créé");
            },
            OnCancelled = () => { }
        };

        MainPanel.Children.Add(doctorForm);
    }

    private void RefreshPatients_Click(object sender, RoutedEventArgs e) {
        RefreshPatients();
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

        MainPanel.Children.Add(alertPopup);
    }

    private void LvPatients_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
        if (LvPatients.SelectedItem is PatientActif patient) {
            PatientMenu.Patient = patient;
            PatientPopup.IsOpen = true;
        }
    }

    private void PatientMenu_ActionTriggered(string action) {
        PatientPopup.IsOpen = false;
        if (PatientMenu.Patient is not PatientActif patient) return;

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
                _db.ExecuteNonQuery($"DELETE FROM Patients_Actifs WHERE IdPatient = {patient.IdPatient}");
                LogEvent($"🗑 Patient supprimé: {patient.Nom}");
                break;
            case "Prescrire":
                if (patient.IdMedecinAssigne == null) {
                    ShowAlert("Médecin assigné requis pour prescrire.");
                    return;
                }
                var presForm = new AddPrescriptionForm(_db, patient.IdPatient, patient.IdMedecinAssigne.Value) { Owner = this };
                if (presForm.ShowDialog() == true) {
                    LogEvent($"📋 Prescription ajoutée pour {patient.Nom}");
                    RefreshMedicaments();
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
            PersonnelMenu.Employe = emp;
            PersonnelPopup.IsOpen = true;
        }
    }

    private void PersonnelMenu_ActionTriggered(string action) {
        PersonnelPopup.IsOpen = false;
        if (PersonnelMenu.Employe is not Employe emp) return;

        if (action == "Licencier") {
            BtnFire_Click(null!, null!);
        }
        else {
            _db.ExecuteNonQuery($"UPDATE Personnel_Actif SET Statut = '{action}' WHERE IdEmploye = {emp.IdEmploye}");
            RefreshPersonnel();
        }
    }

    private void BtnAddLit_Click(object sender, RoutedEventArgs e) {
        var addLitForm = new AddLitForm(_db) {
            Width = MainPanel.ActualWidth,
            Height = MainPanel.ActualHeight,
            OnLitCreated = () => {
                RefreshChambres();
                RefreshDashboard();
                SetStatus("✅ Nouveau lit installé avec succès", Color.FromRgb(0, 212, 170));
                LogEvent("🛏 Ajout d'un nouveau lit dans l'inventaire");
            },
            OnCancelled = () => { }
        };

        MainPanel.Children.Add(addLitForm);
    }

    private void BtnAddChambre_Click(object sender, RoutedEventArgs e) {
        var addChambreForm = new AddChambreForm(_db) {
            Width = MainPanel.ActualWidth,
            Height = MainPanel.ActualHeight,
            OnChambreCreated = () => {
                SetStatus("✅ Nouvelle chambre construite avec succès", Color.FromRgb(0, 212, 170));
                LogEvent("🏗 Construction d'une nouvelle chambre terminée");
            },
            OnCancelled = () => { }
        };

        MainPanel.Children.Add(addChambreForm);
    }

    private void BtnRemoveLit_Click(object sender, RoutedEventArgs e) { }

    private void BtnRemoveChambre_Click(object sender, RoutedEventArgs e) {
        if (sender is not FrameworkElement fe || fe.DataContext is not ChambreGroup chambre) return;

        var deletePopup = PopupFactory.CreateConfirmationPopup(
            $"Êtes-vous sûr de vouloir supprimer la chambre {chambre.NumeroChambre} ?",
            "#FF4D6A",
            () => {
                _db.ExecuteNonQuery($"DELETE FROM Chambre WHERE IdChambre = {chambre.IdChambre}");
                LogEvent($"🗑 Chambre {chambre.NumeroChambre} supprimée");
                RefreshChambres();
                RefreshDashboard();
                SetStatus($"✅ Chambre {chambre.NumeroChambre} supprimée", Color.FromRgb(255, 77, 106));
            },
            () => { }
        );

        MainPanel.Children.Add(deletePopup);
    }
    private void Lit_MouseMove(object sender, MouseEventArgs e) {
        if (e.LeftButton == MouseButtonState.Pressed && sender is FrameworkElement fe &&
            fe.DataContext is LitInventaire lit) DragDrop.DoDragDrop(fe, lit, DragDropEffects.Move);
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
                _db.ExecuteNonQuery($"DELETE FROM Lit WHERE IdLit = {lit.IdLit}");
                LogEvent($"🗑 Lit {lit.NumeroLit} détruit");
                RefreshChambres();
            };

            fe.ContextMenu.Items.Add(deleteItem);
        }
    }

    private void RefreshMedicaments() {
        var list = _db.GetMedicaments();
        LvMedicaments.ItemsSource = list;
        foreach (var m in list) {
            if (m.StockActuel < m.StockMinimum) {
                // optional: add logic for alert if needed
            }
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
                MessageBox.Show("Le fichier HTML de la facture est introuvable.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            LogEvent("🧪 Nouveau médicament ajouté.");
            RefreshMedicaments();
        }
    }
}