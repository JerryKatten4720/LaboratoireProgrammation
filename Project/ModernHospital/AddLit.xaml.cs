using System.Windows;
using System.Windows.Controls;

namespace LaboratoireProgrammation.Project.ModernHospital;

public partial class AddLitForm : Window {
    private readonly DatabaseManager _db;

    public AddLitForm(DatabaseManager db) {
        InitializeComponent();
        _db = db;
        Loaded += OnLoaded;
    }

    public Action? OnLitCreated { get; set; }
    public Action? OnCancelled { get; set; }

    private void OnLoaded(object sender, RoutedEventArgs e) {
        CbChambre.ItemsSource = _db.GetChambresList();
        if (CbChambre.Items.Count > 0) CbChambre.SelectedIndex = 0;
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e) {
        if (CbChambre.SelectedItem is not DatabaseManager.ChambreBase chambre) return;

        var numLit = TbNumeroLit.Text.Trim();
        if (string.IsNullOrEmpty(numLit)) return;

        if (!_db.AddLit(chambre.IdChambre, numLit)) {
            MessageBox.Show("Impossible d'ajouter le lit : la chambre a atteint sa capacité maximale de lits.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        OnLitCreated?.Invoke();
        Close();
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e) {
        OnCancelled?.Invoke();
        Close();
    }
}