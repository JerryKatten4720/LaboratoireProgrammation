using LaboratoireProgrammation.Project.ModernOverseerWars.Models;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Entities;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Enums;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Interfaces;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Map;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Data.Json;
namespace LaboratoireProgrammation.Project.ModernOverseerWars.Helpers;

public static class RarityHelper {
    public static CardRarity ParseRarity(string rarityString) {
        if (int.TryParse(rarityString, out int rarityInt)) {
            return ParseRarity(rarityInt);
        }
        return CardRarity.Common;
    }

    public static CardRarity ParseRarity(int rarity) {
        return rarity switch {
            1 => CardRarity.Common,
            2 => CardRarity.Rare,
            3 => CardRarity.Legendary,
            _ => CardRarity.Common
        };
    }
}
