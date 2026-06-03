namespace LaboratoireProgrammation.Project.ModernOverseerWars;

public class PlayerTheme {
    public string Name    { get; init; } = "";
    public string Color   { get; init; } = "";
    public string BgColor { get; init; } = "";
}

public static class PlayerThemes {
    public static readonly PlayerTheme[] All = {
        new() { Name = "Blue",    Color = "#2A66FF", BgColor = "#0A1224" },
        new() { Name = "Red",     Color = "#FF3A30", BgColor = "#240A08" },
        new() { Name = "Green",   Color = "#10C850", BgColor = "#061A0A" },
        new() { Name = "Gold",    Color = "#FFC000", BgColor = "#241B00" },
        new() { Name = "Purple",  Color = "#9D4EDD", BgColor = "#1B0630" },
        new() { Name = "Teal",    Color = "#00E5FF", BgColor = "#00242B" },
        new() { Name = "Orange",  Color = "#FF6F00", BgColor = "#241000" },
        new() { Name = "Mint",    Color = "#00FF88", BgColor = "#002414" },
        new() { Name = "Pink",    Color = "#FF007F", BgColor = "#240012" },
        new() { Name = "Silver",  Color = "#CCCCCC", BgColor = "#1E1E21" },
    };
}

public class PlayerProfile {
    public string       Pseudo { get; set; } = "Player";
    public PlayerTheme  Theme  { get; set; } = PlayerThemes.All[0];
    public int          Index  { get; set; } = 0;
}
