namespace LaboratoireProgrammation.Project.ModernOverseerWars.Models.Data;

using System.Collections.Generic;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Data.Json;

public static class GameDataRepository {
    public static List<DwellerJson> Dwellers { get; set; } = new();
    public static List<WeaponJson> Weapons { get; set; } = new();
    public static List<OutfitJson> Outfits { get; set; } = new();
    public static List<ScrapJson> Scraps { get; set; } = new();
}
