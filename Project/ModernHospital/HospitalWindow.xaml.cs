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
        } catch { /* Ignore if columns already exist */ }

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

        Button? firstBtn = null;
        foreach (var child in LogicalTreeHelper.GetChildren(this)) {
            if (child is StackPanel sp) {
                foreach (var inner in sp.Children) {
                    if (inner is Button b && b.Tag?.ToString() == "dashboard") {
                        firstBtn = b; break;
                    }
                }
            }
        }
        
        if (firstBtn != null) {
            Nav_Click(firstBtn, null);
        } else {
            PanelDashboard.Visibility = Visibility.Visible;
            RefreshDashboard();
        }

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
        LvPatients.ItemsSource = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Where(patients, p => p.Statut != "Guéri" && p.Statut != "Décédé"));
        LvPatientsHistorique.ItemsSource = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Where(patients, p => p.Statut == "Guéri" || p.Statut == "Décédé"));
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
            MessageBox.Show("Veuillez sélectionner la maladie, le lit et le médecin pour la réservation.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        string nom = TbResPatientNom.Text;
        if (string.IsNullOrWhiteSpace(nom)) {
            MessageBox.Show("Veuillez entrer un nom de patient valide.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var maladie = (CasClinique)CbResMaladie.SelectedItem;
        var lit = (LitDisponible)CbResLit.SelectedItem;
        var medecin = (Employe)CbResMedecin.SelectedItem;
        string classe = ((ComboBoxItem)CbResClasse.SelectedItem).Content.ToString();

        try {
            _db.ExecuteNonQuery($"INSERT INTO Patients_Actifs (Nom, IdMaladie, IdLit, IdMedecinAssigné, Classe, TempsTraitementRestant, Statut) VALUES ('{nom.Replace("'", "''")}', {maladie.IdCas}, {lit.IdLit}, {medecin.IdEmploye}, '{classe}', {maladie.TempsTraitementHeures}, 'Réservé')");
            _db.ExecuteNonQuery($"UPDATE Lit SET Statut = 'Réservé' WHERE IdLit = {lit.IdLit}");
            int dayTime = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalDays;
            int patientId = 0;
            _db.ExecuteCommand("SELECT MAX(IdPatient) AS IdPatient FROM Patients_Actifs", cmd => {
                using var reader = cmd.ExecuteReader();
                if (reader.Read() && !reader.IsDBNull(0)) {
                    patientId = reader.GetInt32(0);
                }
            });
            
            _db.ExecuteNonQuery($"INSERT INTO Reservation (IdPatient, IdLit, IdMedecin, DateEntree) VALUES ({patientId}, {lit.IdLit}, {medecin.IdEmploye}, {dayTime})");

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
        var doctorForm = new DoctorCreationForm {
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

    private void BtnRemoveLit_Click(object sender, RoutedEventArgs e) { }

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
            _dragWindow.Left = mousePos.X + 15;
            _dragWindow.Top = mousePos.Y + 15;
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
            _dragWindow.Left = pt.X + 15;
            _dragWindow.Top = pt.Y + 15;
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
        foreach (var m in list) {
            if (m.StockActuel < m.StockMinimum) {
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