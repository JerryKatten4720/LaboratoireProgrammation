using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
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
        PanelRecruter.Visibility = Visibility.Collapsed;
        PanelFinances.Visibility = Visibility.Collapsed;

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
            case "recruter": PanelRecruter.Visibility = Visibility.Visible; break;
            case "finances":
                PanelFinances.Visibility = Visibility.Visible;
                RefreshFinances();
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

    private void RefreshFinances() {
        LvTransactions.ItemsSource = _db.GetRecentTransactions(50);
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
            message,
            "#FFB347",
            () => { },
            () => { }
        );

        MainPanel.Children.Add(alertPopup);
    }
}