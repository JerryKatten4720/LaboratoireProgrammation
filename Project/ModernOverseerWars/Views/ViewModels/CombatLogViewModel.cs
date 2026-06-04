namespace LaboratoireProgrammation.Project.ModernOverseerWars.Views.ViewModels;

using System.Collections.ObjectModel;
using LaboratoireProgrammation.Project.ModernOverseerWars.Controllers;

public class CombatLogViewModel : ViewModelBase {
    private readonly CombatService _combatService;

    public ObservableCollection<string> Logs { get; } = new();

    public CombatLogViewModel(CombatService combatService) {
        _combatService = combatService;
        _combatService.CombatEvent += OnCombatEvent;
    }

    private void OnCombatEvent(string message) {
        App.Current.Dispatcher.InvokeAsync(() => {
            Logs.Add(message);
            if (Logs.Count > 100) {
                Logs.RemoveAt(0);
            }
        });
    }
}
