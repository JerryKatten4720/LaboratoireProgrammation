namespace LaboratoireProgrammation.Project.ModernOverseerWars.Controllers;

using System.Linq;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Entities;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Map;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Enums;

public class MovementService {
    private readonly GameState _state;
    private readonly CombatService _combatService;

    public MovementService(GameState state, CombatService combatService) {
        _state = state;
        _combatService = combatService;
    }

    public void RemoveDwellerFromMap(Dweller dweller) {
        for (int c = 0; c < _state.Map.Cols; c++) {
            for (int r = 0; r < _state.Map.Rows; r++) {
                _state.Map.Get(c, r).Player1Dwellers.Remove(dweller);
                _state.Map.Get(c, r).Player2Dwellers.Remove(dweller);
            }
        }
    }

    public (bool success, string error) TryDeployDweller(Dweller dweller, HexTile targetTile, int cost) {
        if (!targetTile.IsNavigable) return (false, "Tile is unnavigable!");
        
        var vault = _state.ActiveVault;
        if (cost > vault.ActionPoints) return (false, "Not enough PA to move there!");
        
        vault.ActionPoints -= cost;
        RemoveDwellerFromMap(dweller);
        
        bool isP1 = _state.IsPlayer1Turn;
        bool isHomeVault = (isP1 && targetTile.Type == TileType.Player1Vault) || (!isP1 && targetTile.Type == TileType.Player2Vault);
        
        if (!isHomeVault) {
            var dwellerList = isP1 ? targetTile.Player1Dwellers : targetTile.Player2Dwellers;
            dwellerList.Add(dweller);
        }
        
        if (isP1) targetTile.IsRevealedByP1 = true;
        else targetTile.IsRevealedByP2 = true;

        if (targetTile.HasConflict) {
            _combatService.ResolveCombatsOnMap();
        }
        
        _state.AddLog($"{dweller.Name} deployed to ({targetTile.Col},{targetTile.Row}) [{cost} PA]");
        return (true, string.Empty);
    }
}
