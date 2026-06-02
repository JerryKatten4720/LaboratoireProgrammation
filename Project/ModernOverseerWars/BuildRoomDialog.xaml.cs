using System.Windows;
using System.Windows.Controls;

namespace LaboratoireProgrammation.Project.ModernOverseerWars;

public class RoomOption {
    public RoomType Type        { get; init; }
    public string   DisplayName { get; init; } = "";
    public int      Cost        { get; init; }
    public string   CostText    => $"{Cost} ⚡";
}

public partial class BuildRoomDialog : Window {

    public RoomType? ChosenRoom { get; private set; }

    private static readonly RoomOption[] AllOptions = {
        new() { Type = RoomType.WeaponFactory,  DisplayName = "🔫 Weapon Factory",     Cost = 15 },
        new() { Type = RoomType.OutfitFactory,  DisplayName = "🧥 Outfit Factory",      Cost = 15 },
        new() { Type = RoomType.TrainingCenter, DisplayName = "🏋 Training Center",     Cost = 20 },
        new() { Type = RoomType.TechCenter,     DisplayName = "💻 Tech Center",          Cost = 25 },
    };

    public BuildRoomDialog(IEnumerable<RoomType> alreadyBuilt) {
        InitializeComponent();
        var available = AllOptions.Where(o => !alreadyBuilt.Contains(o.Type)).ToList();
        RoomList.ItemsSource = available;
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
