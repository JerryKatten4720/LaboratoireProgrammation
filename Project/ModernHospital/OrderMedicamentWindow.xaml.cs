using System;
using System.Windows;
using System.Windows.Controls;

namespace LaboratoireProgrammation.Project.ModernHospital;

public partial class OrderMedicamentWindow : Window {
    private readonly Medicament _medicament;

    public OrderMedicamentWindow(Medicament medicament) {
        InitializeComponent();
        _medicament = medicament;

        TbDrugInfo.Text = $"Médicament: {_medicament.Nom} | DCI: {_medicament.DCI} ({_medicament.Forme})";
        TbEstimatedDelay.Text = $"{_medicament.TempsLivraisonBase} min (in-game)";
        UpdateEstimates();
    }

    public int Quantity { get; private set; }

    private void TbQuantity_TextChanged(object sender, TextChangedEventArgs e) {
        UpdateEstimates();
    }

    private void UpdateEstimates() {
        if (TbQuantity == null || TbEstimatedPrice == null) return;

        string qtyText = TbQuantity.Text.Trim();
        if (int.TryParse(qtyText, out int qty) && qty > 0) {
            decimal purchasePriceUnit = _medicament.PrixUnitaire * 0.80m;
            decimal totalEstimate = qty * purchasePriceUnit;
            TbEstimatedPrice.Text = $"$ {totalEstimate:N2}";
        } else {
            TbEstimatedPrice.Text = "Quantité invalide";
        }
    }

    private void BtnConfirm_Click(object sender, RoutedEventArgs e) {
        string qtyText = TbQuantity.Text.Trim();
        if (!int.TryParse(qtyText, out int qty) || qty <= 0) {
            MessageBox.Show("Veuillez saisir une quantité positive valide.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        Quantity = qty;
        DialogResult = true;
        Close();
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e) {
        DialogResult = false;
        Close();
    }
}
