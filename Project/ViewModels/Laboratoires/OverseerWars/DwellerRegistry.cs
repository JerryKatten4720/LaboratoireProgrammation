using System.IO;
using System.Text.Json;
using System.Windows;

namespace LaboratoireProgrammation.Project.ViewModels.Laboratoires.OverseerWars;

public class DwellerRegistry {
    private static List<Dweller> _cachedDwellers = new();
    public void Initialize() {
        var uri = new Uri("pack://application:,,,/LaboratoireProgrammation;component/Assets/data/dwellers.json");
        var resourceStream = System.Windows.Application.GetResourceStream(uri);
        
        if (resourceStream == null) throw new FileNotFoundException("Check dwellers.json Build Action!");

        using var reader = new StreamReader(resourceStream.Stream);
        string jsonContent = reader.ReadToEnd();

        var options = new JsonSerializerOptions { IncludeFields = true, PropertyNameCaseInsensitive = true };
        _cachedDwellers = JsonSerializer.Deserialize<List<Dweller>>(jsonContent, options) ?? new();
    }

    public Dweller? GetByName(string firstName) {
        var template = _cachedDwellers.FirstOrDefault(d => d.FirstName.Equals(firstName, StringComparison.OrdinalIgnoreCase));

        if (template == null) return null;
        
        return new Dweller(template.FirstName, template.LastName) {
            S = template.S, P = template.P, E = template.E,
            C = template.C, I = template.I, A = template.A, L = template.L,
            Rarity = template.Rarity,
            Texture = template.Texture,
        };
    }
    
    public List<Dweller> GetAllDwellers() {
        return _cachedDwellers;
    }
}