using LaboratoireProgrammation.Project.ModernOverseerWars.Domain;
using LaboratoireProgrammation.Project.ModernOverseerWars.Domain.Entities;
using LaboratoireProgrammation.Project.ModernOverseerWars.Domain.Enums;
using LaboratoireProgrammation.Project.ModernOverseerWars.Domain.Interfaces;
using LaboratoireProgrammation.Project.ModernOverseerWars.Domain.Map;
using LaboratoireProgrammation.Project.ModernOverseerWars.Data.Json;
using System.Windows.Media;

namespace LaboratoireProgrammation.Project.ModernOverseerWars;

public static class ControlCardFactory {

    public static ControlCard Create(string image, int power) {
        var card = new ControlCard {
            CardImage = image,
            Power = power
        };
        card.RenderTransform = new RotateTransform(Random.Shared.Next(-2, 1));
        return card;
    }

    public static ControlCard FromDweller(Dweller d) {
        var card = new ControlCard();
        card.BindDweller(d);
        return card;
    }

    public static ControlCard FromWeapon(IWeapon w) {
        var card = new ControlCard();
        card.BindWeapon(w);
        return card;
    }

    public static ControlCard FromOutfit(IOutfit o) {
        var card = new ControlCard();
        card.BindOutfit(o);
        return card;
    }
}
