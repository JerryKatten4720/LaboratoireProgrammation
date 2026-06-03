namespace LaboratoireProgrammation.Project.ModernOverseerWars.Domain.Map;

using System.Collections.Generic;
using System.Linq;
using LaboratoireProgrammation.Project.ModernOverseerWars.Domain.Entities;
using LaboratoireProgrammation.Project.ModernOverseerWars.Domain.Enums;
using LaboratoireProgrammation.Project.ModernOverseerWars.Data;

public class HexTile {
    public int Col { get; set; }
    public int Row { get; set; }
    public bool IsRevealedByP1 { get; set; } = false;
    public bool IsRevealedByP2 { get; set; } = false;
    public TileType Type { get; set; } = TileType.Empty;
    public string LocationName { get; set; } = "";
    public bool IsNavigable { get; set; } = true;
    
    public List<Dweller> Player1Dwellers { get; } = new();
    public List<Dweller> Player2Dwellers { get; } = new();

    public bool HasConflict => Player1Dwellers.Any(d => d.IsAlive) && Player2Dwellers.Any(d => d.IsAlive);

    public bool IsRevealedFor(bool isP1) {
        if (!GameConfigRepository.Config.EnableFogOfWar) return true;
        return isP1 ? IsRevealedByP1 : IsRevealedByP2;
    }

    public void RevealFor(bool isP1) {
        if (isP1) IsRevealedByP1 = true;
        else IsRevealedByP2 = true;
    }
}
