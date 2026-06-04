using System.Windows;
using System.Windows.Controls;

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
        CbUnite.ItemsSource = _db.GetUnitesList();
        if (CbUnite.Items.Count > 0) CbUnite.SelectedIndex = 0;
        CbType.SelectedIndex = 0;
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e) {
        if (CbUnite.SelectedItem is not DatabaseManager.UniteBase unite) return;

        var numChambre = TbNumeroChambre.Text.Trim();
        var typeChambre = (CbType.SelectedItem as ComboBoxItem)?.Content?.ToString();

        if (string.IsNullOrEmpty(numChambre) || string.IsNullOrEmpty(typeChambre)) return;

        _db.AddChambre(unite.IdUnite, numChambre, typeChambre);

        OnChambreCreated?.Invoke();
        Close();
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e) {
        OnCancelled?.Invoke();
        Close();
    }
}