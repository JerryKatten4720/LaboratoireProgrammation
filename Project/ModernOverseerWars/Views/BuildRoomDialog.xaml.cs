using LaboratoireProgrammation.Project.ModernOverseerWars.Models;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Entities;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Enums;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Interfaces;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Map;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Data.Json;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;

namespace LaboratoireProgrammation.Project.ModernOverseerWars.Views;

public class RoomOption {
    public RoomType Type        { get; init; }
    public string   Icon        { get; init; } = "";
    public string   Title       { get; init; } = "";
    public string   Description { get; init; } = "";
    public int      Cost        { get; init; }
    public string   CostText    => $"{Cost} ⚡";
}

public partial class BuildRoomDialog : Window {

    public RoomType? ChosenRoom { get; private set; }

    private static readonly RoomOption[] AllOptions = {
        new() { 
            Type = RoomType.WeaponFactory, 
            Icon = "🔫", Title = "WEAPON FACTORY", 
            Description = "Produces advanced weaponry to arm your dwellers.",
            Cost = 15 
        },
        new() { 
            Type = RoomType.OutfitFactory, 
            Icon = "🧥", Title = "OUTFIT FACTORY", 
            Description = "Manufactures tactical gear and armored suits.",
            Cost = 15 
        },
        new() { 
            Type = RoomType.TrainingCenter, 
            Icon = "🏋", Title = "TRAINING CENTER", 
            Description = "Improves dweller S.P.E.C.I.A.L. stats over time.",
            Cost = 20 
        },
        new() { 
            Type = RoomType.TechCenter, 
            Icon = "💻", Title = "TECH CENTER", 
            Description = "Unlocks global vault bonuses and terminal upgrades.",
            Cost = 25 
        },
    };

    public BuildRoomDialog(IEnumerable<RoomType> alreadyBuilt) {
        InitializeComponent();
        
        var available = AllOptions.Where(o => !alreadyBuilt.Contains(o.Type)).ToList();
        RoomList.ItemsSource = available;

        Loaded += (s, e) => {
            if (Owner != null) {
                this.Resources["PlayerAccentBrush"] = Owner.Resources["PlayerAccentBrush"];
                this.Resources["PlayerAccentDimBrush"] = Owner.Resources["PlayerAccentDimBrush"];
                this.Resources["PlayerBgBrush"] = Owner.Resources["PlayerBgBrush"];
                this.Resources["PlayerAccentGlow"] = Owner.Resources["PlayerAccentGlow"];
            }
        };
    }

    private void OnSelectionChanged(object s, SelectionChangedEventArgs e) {
        BuildBtn.IsEnabled = RoomList.SelectedItem != null;
    }

    private void OnBuild(object s, RoutedEventArgs e) {
        if (RoomList.SelectedItem is RoomOption opt) {
            ChosenRoom = opt.Type;
            DialogResult = true;
        }
    }

    private void OnCancel(object s, RoutedEventArgs e) => DialogResult = false;
}