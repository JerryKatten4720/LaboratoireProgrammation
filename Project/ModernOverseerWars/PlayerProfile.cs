namespace LaboratoireProgrammation.Project.ModernOverseerWars;

public class PlayerTheme {
    public string Name    { get; init; } = "";
    public string Color   { get; init; } = "";
    public string BgColor { get; init; } = "";
}

public static class PlayerThemes {
    public static readonly PlayerTheme[] All = {
        new() { Name = "Olive",   Color = "#41521F", BgColor = "#171E0B" },
        new() { Name = "Purple",  Color = "#783F8E", BgColor = "#2A1535" },
        new() { Name = "Red",     Color = "#C84630", BgColor = "#2A0C08" },
        new() { Name = "Lime",    Color = "#CCFF66", BgColor = "#2A3510" },
        new() { Name = "Teal",    Color = "#2EC4B6", BgColor = "#0A2926" },
        new() { Name = "Gold",    Color = "#FDCA40", BgColor = "#2A2208" },
    };
}

public class PlayerProfile {
    public string       Pseudo { get; set; } = "Player";
    public PlayerTheme  Theme  { get; set; } = PlayerThemes.All[0];
    public int          Index  { get; set; } = 0;
}
