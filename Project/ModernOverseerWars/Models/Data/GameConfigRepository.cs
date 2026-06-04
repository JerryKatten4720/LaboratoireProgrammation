namespace LaboratoireProgrammation.Project.ModernOverseerWars.Models.Data;

using System.Text.Json;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Data.Json;

public static class GameConfigRepository {
    public static GameConfig Config { get; private set; } = new();
    
    public static void Load(string path) {
        try {
            if (System.IO.File.Exists(path)) {
                string json = System.IO.File.ReadAllText(path);
                Config = JsonSerializer.Deserialize<GameConfig>(json) ?? new();
            }
        }
        catch {
        }
    }
}
