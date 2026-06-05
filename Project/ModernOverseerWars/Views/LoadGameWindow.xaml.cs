using System;
using System.Windows;
using LaboratoireProgrammation.Project.ModernOverseerWars.Helpers;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models;

namespace LaboratoireProgrammation.Project.ModernOverseerWars.Views;

public partial class LoadGameWindow : Window {
    public GameState? LoadedState { get; private set; }

    public LoadGameWindow() {
        InitializeComponent();
        LoadSaves();
    }

    private void LoadSaves() {
        try {
            var saves = SaveLoadManager.GetSaves();
            SavesList.ItemsSource = saves;
        }
        catch (Exception ex) {
            MessageBox.Show($"Failed to load saves: {ex.Message}");
        }
    }

    private void OnLoadClick(object sender, RoutedEventArgs e) {
        if (sender is FrameworkElement { Tag: string path }) {
            try {
                LoadedState = SaveLoadManager.Load(path);
                DialogResult = true;
            }
            catch (Exception ex) {
                MessageBox.Show($"Failed to load save: {ex.Message}");
            }
        }
    }

    private void OnBack(object sender, RoutedEventArgs e) {
        DialogResult = false;
    }
}
