using LaboratoireProgrammation.Project.ModernOverseerWars.Domain;
using LaboratoireProgrammation.Project.ModernOverseerWars.Domain.Entities;
using LaboratoireProgrammation.Project.ModernOverseerWars.Domain.Enums;
using LaboratoireProgrammation.Project.ModernOverseerWars.Domain.Interfaces;
using LaboratoireProgrammation.Project.ModernOverseerWars.Domain.Map;
using LaboratoireProgrammation.Project.ModernOverseerWars.Data.Json;
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
