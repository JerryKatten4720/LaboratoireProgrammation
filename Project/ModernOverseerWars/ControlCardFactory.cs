using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LaboratoireProgrammation.Project.ModernOverseerWars;

public static class ControlCardFactory {

    private enum CardRarity { 
        common, uncommon, rare, epic, legendary
    }
    public static ControlCard Create(string image, int power) {
        var card = new ControlCard {
            CardImage = image,
            Power = power
        };
        
        card.RenderTransform = new RotateTransform(new Random().Next(-2, 1));

        return card;
    }
}   