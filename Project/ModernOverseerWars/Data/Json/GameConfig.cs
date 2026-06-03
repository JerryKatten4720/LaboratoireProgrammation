namespace LaboratoireProgrammation.Project.ModernOverseerWars.Data.Json;

public class GameConfig {
    public int MaxActionPoints { get; set; } = 5;
    public int TurnDurationSeconds { get; set; } = 60;
    public int StarterDwellersCount { get; set; } = 3;
    public int StarterWeaponsCount { get; set; } = 1;
    public int StarterOutfitsCount { get; set; } = 1;
    public int MapCols { get; set; } = 12;
    public int MapRows { get; set; } = 5;
    public double MapTransparency { get; set; } = 0.8;
    public double VaultUITransparency { get; set; } = 1.0;
    public string UIColor { get; set; } = "#0D0D20";
    public bool EnableFogOfWar { get; set; } = false;
}
