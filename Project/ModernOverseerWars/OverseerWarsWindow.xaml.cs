using System.Windows;

namespace LaboratoireProgrammation.Project.ModernOverseerWars;

public partial class OverseerWarsWindow : Window {

    public static ControlCard? heldCard;
    public OverseerWarsWindow() {
        InitializeComponent();
        Loaded += (s, e) => PopulateDeck();
    }

    private void PopulateDeck() {
        for (int i = 1; i <= 10; i++) {
            var card = ControlCardFactory.Create($"Unit {i}", i * 10);
            CardPanel.Children.Add(card);
        }
    }
    
    
}