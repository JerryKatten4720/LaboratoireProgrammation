using System;
using System.Windows;
using System.Windows.Controls;
using LaboratoireProgrammation.Project.ModernHospital.Helpers;

namespace LaboratoireProgrammation.Project.ModernHospital;

public partial class AddChambreForm : Window {
    private readonly DatabaseManager _db;

    public AddChambreForm(DatabaseManager db) {
        InitializeComponent();
        _db = db;
        Loaded += OnLoaded;
    }

    public Action? OnChambreCreated { get; set; }
    public Action? OnCancelled { get; set; }

    private void OnLoaded(object sender, RoutedEventArgs e) {
        SetupEventHandlers();
        InitializeDefaultValues();
        UpdateCostEstimate();
    }

    private void SetupEventHandlers() {
        CbUnite.SelectionChanged += (s, ev) => UpdateCostEstimate();
        TbCapaciteLits.TextChanged += (s, ev) => UpdateCostEstimate();
    }

    private void InitializeDefaultValues() {
        CbUnite.ItemsSource = _db.GetUnitesList();
        
        if (CbUnite.Items.Count > 0) {
            CbUnite.SelectedIndex = 0;
        }
        
        CbType.SelectedIndex = 0;
    }

    private decimal GetCurrentCostEstimate(out int capaciteLits) {
        capaciteLits = 2;
        
        if (!TryGetSelectedUnite(out var unite) || !TryGetCapacite(out capaciteLits)) {
            return 0m;
        }

        var coutEntretien = GetCoutEntretienForUnite(unite.IdUnite);
        
        return ChambrePricingHelper.CalculerCoutTotal(coutEntretien, capaciteLits);
    }

    private bool TryGetSelectedUnite(out DatabaseManager.UniteBase unite) {
        unite = CbUnite.SelectedItem as DatabaseManager.UniteBase;
        
        return unite != null;
    }

    private bool TryGetCapacite(out int capaciteLits) {
        return int.TryParse(TbCapaciteLits.Text.Trim(), out capaciteLits) && capaciteLits > 0;
    }

    private decimal GetCoutEntretienForUnite(int idUnite) {
        var unites = _db.GetUnites();
        var selectedUnit = unites.Find(u => u.IdUnite == idUnite);
        
        return selectedUnit?.CoutEntretienJour ?? 0m;
    }

    private void UpdateCostEstimate() {
        var cost = GetCurrentCostEstimate(out _);
        
        if (cost == 0m) {
            TbCostDisplay.Text = "Coût de création : Invalide";
            return;
        }
        
        TbCostDisplay.Text = $"Coût de création : {cost:N2} $";
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e) {
        if (!TryGetSelectedUnite(out var unite) || !ValidateInputs(out var numChambre, out var typeChambre)) {
            return;
        }

        var cost = GetCurrentCostEstimate(out var cap);
        
        if (cost == 0m) {
            ShowWarning("Veuillez saisir une capacité de lits valide.", "Erreur");
            return;
        }

        if (!IsBudgetSufficient(cost)) {
            return;
        }

        SaveChambre(unite.IdUnite, numChambre, typeChambre, cap);
    }

    private bool ValidateInputs(out string numChambre, out string typeChambre) {
        numChambre = TbNumeroChambre.Text.Trim();
        typeChambre = (CbType.SelectedItem as ComboBoxItem)?.Content?.ToString();

        return !string.IsNullOrEmpty(numChambre) && !string.IsNullOrEmpty(typeChambre);
    }

    private bool IsBudgetSufficient(decimal cost) {
        var hosp = _db.GetHospital();
        var budget = hosp?.Budget ?? 0m;
        
        if (cost > budget) {
            ShowWarning($"Impossible de créer la chambre : le coût de création ({cost:N2} $) excède le budget de l'hôpital ({budget:N2} $).", "Budget insuffisant");
            return false;
        }
        
        return true;
    }

    private void SaveChambre(int idUnite, string numChambre, string typeChambre, int cap) {
        _db.AddChambre(idUnite, numChambre, typeChambre, cap);
        
        OnChambreCreated?.Invoke();
        Close();
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e) {
        OnCancelled?.Invoke();
        Close();
    }

    private void ShowWarning(string message, string title) {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}