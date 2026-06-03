namespace LaboratoireProgrammation.Project.ModernOverseerWars.Domain.Interfaces;

using LaboratoireProgrammation.Project.ModernOverseerWars.Domain.Enums;

public interface ICard {
    string Name { get; set; }
    CardRarity Rarity { get; set; }
}
