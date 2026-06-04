namespace LaboratoireProgrammation.Project.ModernOverseerWars.Models.Interfaces;

using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Enums;

public interface ICard {
    string Name { get; set; }
    CardRarity Rarity { get; set; }
}
