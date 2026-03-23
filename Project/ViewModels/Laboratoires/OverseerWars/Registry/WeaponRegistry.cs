using System.IO;
using System.Text.Json;
using System.Windows;
using LaboratoireProgrammation.Project.ViewModels.Laboratoires.OverseerWars.DwellerComponent;

namespace LaboratoireProgrammation.Project.ViewModels.Laboratoires.OverseerWars.Registry;

public class WeaponRegistry {
    private static readonly JsonSerializerOptions JsonOptions = new() { IncludeFields = true, PropertyNameCaseInsensitive = true };
    private static List<Dweller> _cachedDwellers = new();

    public void Initialize() {
        var uri = new Uri("pack://application:,,,/LaboratoireProgrammation;component/Assets/data/weapons.json");
        var stream = Application.GetResourceStream(uri) ?? throw new FileNotFoundException("Check dwellers.json Build Action!");

        using var reader = new StreamReader(stream.Stream);
        _cachedDwellers = JsonSerializer.Deserialize<List<Dweller>>(reader.ReadToEnd(), JsonOptions) ?? new();
    }

    public Weapon? GetByName(string firstName) {
        var t = _cachedDwellers.FirstOrDefault(d => d.FirstName.Equals(firstName, StringComparison.OrdinalIgnoreCase));
        if (t == null) return null;

        return null;
    }

    public List<Dweller> GetAllDwellers() => _cachedDwellers;
}