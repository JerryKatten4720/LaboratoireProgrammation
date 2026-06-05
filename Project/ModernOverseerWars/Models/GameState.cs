namespace LaboratoireProgrammation.Project.ModernOverseerWars.Models;

using System.Collections.Generic;
using LaboratoireProgrammation.Project.Models;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Enums;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Entities;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Map;

public class GameState {
    public PlayerProfile Player1 { get; set; } = new() { Index = 0 };
    public PlayerProfile Player2 { get; set; } = new() { Index = 1, Theme = PlayerThemes.All[1] };
    public Vault Vault1 { get; set; } = null!;
    public Vault Vault2 { get; set; } = null!;
    public HexMap Map { get; set; } = new HexMap();
    public GamePhase Phase { get; set; } = GamePhase.Setup;
    public int TurnNumber { get; set; } = 1;
    public string? WinnerName { get; set; }
    public List<string> Log { get; set; } = new();

    public void InitVaults() {
        Vault1 = new Vault(Player1.Pseudo);
        Vault2 = new Vault(Player2.Pseudo);
    }

    public bool IsPlayer1Turn => Phase == GamePhase.PlayerTurn;
    public Vault ActiveVault => IsPlayer1Turn ? Vault1 : Vault2;
    public PlayerProfile ActivePlayer => IsPlayer1Turn ? Player1 : Player2;

    public void AddLog(string msg) {
        Log.Add($"[T{TurnNumber}] {msg}");
        if (Log.Count > 80) Log.RemoveAt(0);
    }
}
