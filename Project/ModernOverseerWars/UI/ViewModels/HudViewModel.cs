namespace LaboratoireProgrammation.Project.ModernOverseerWars.UI.ViewModels;

using LaboratoireProgrammation.Project.ModernOverseerWars.Domain;
using LaboratoireProgrammation.Project.ModernOverseerWars.Domain.Entities;
using LaboratoireProgrammation.Project.ModernOverseerWars.Services;

public class HudViewModel : ViewModelBase {
    private readonly GameState _state;
    private readonly TurnService _turnService;

    public string ElectricityText => $" {_state.ActiveVault.Electricity}";
    public string WaterText => $"{_state.ActiveVault.Water}";
    public string FoodText => $"🌿 {_state.ActiveVault.Food}";
    public string ApText => $"PA: {_state.ActiveVault.ActionPoints} / {_state.ActiveVault.MaxActionPoints}";
    public string TurnText => $"[T{_state.TurnNumber}] {(_state.IsPlayer1Turn ? _state.Player1.Pseudo : _state.Player2.Pseudo)} - {_turnService.SecondsRemaining}s";

    public HudViewModel(GameState state, TurnService turnService) {
        _state = state;
        _turnService = turnService;
        _turnService.TurnTick += Refresh;
    }

    public void Refresh() {
        OnPropertyChanged(nameof(ElectricityText));
        OnPropertyChanged(nameof(WaterText));
        OnPropertyChanged(nameof(FoodText));
        OnPropertyChanged(nameof(ApText));
        OnPropertyChanged(nameof(TurnText));
    }
}
