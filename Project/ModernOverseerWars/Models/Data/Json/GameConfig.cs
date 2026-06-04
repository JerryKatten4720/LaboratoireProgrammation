namespace LaboratoireProgrammation.Project.ModernOverseerWars.Models.Data.Json;

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
    public string UIColor { get; set; } = "#1a1a1a";
    public bool EnableFogOfWar { get; set; } = false;
    public string PathColor { get; set; } = "#A5FFD700";

    public int StarvationWaterHpPenalty { get; set; } = 2;
    public int StarvationFoodHpPenalty { get; set; } = 1;
    public int InitialElectricity { get; set; } = 20;
    public int InitialFood { get; set; } = 20;
    public int InitialWater { get; set; } = 20;
    public int RoomBuildCost { get; set; } = 10;

    public int DwellerBaseHp { get; set; } = 10;
    public int DwellerHpMultE { get; set; } = 2;
    public int DwellerStartingHp { get; set; } = 16;

    public double CardGlowSubtleness { get; set; } = 0.8;
    public double CardShakeIntensity { get; set; } = 1.0;
    public double CardTiltAngleX { get; set; } = 3.5;
    public double CardTiltAngleY { get; set; } = 3.5;
    public double CardTiltAngleRot { get; set; } = -1.5;
    public double CardHoloSweepSeconds { get; set; } = 3.5;

    public int CombatCritChanceFallback { get; set; } = 5;
    public int CombatFailChanceMultiplier { get; set; } = 3;
    public int CombatFleeChanceMultiplier { get; set; } = 8;
}
