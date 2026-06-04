using System.IO;
using System.Text.Json;
using System.Windows;

namespace LaboratoireProgrammation.Project.ModernHospital;

public class HospitalSettingsData {
    public int TimeScaleMultiplier { get; set; } = 60;
    public int GameTickIntervalSeconds { get; set; } = 1;
    public System.Collections.Generic.Dictionary<string, decimal> CoutCreationBaseUnites { get; set; } = new System.Collections.Generic.Dictionary<string, decimal>();
}

public static class HospitalSettings {
    public static HospitalSettingsData Current { get; private set; } = new HospitalSettingsData();

    public static void Load() {
        string path = "appsettings.json";
        if (File.Exists(path)) {
            try {
                var json = File.ReadAllText(path);
                var data = JsonSerializer.Deserialize<HospitalSettingsData>(json);
                if (data != null) Current = data;
            } catch { }
        } else {
            try {
                File.WriteAllText(path, JsonSerializer.Serialize(Current, new JsonSerializerOptions { WriteIndented = true }));
            } catch { }
        }
    }
}
