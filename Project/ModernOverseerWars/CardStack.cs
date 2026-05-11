namespace LaboratoireProgrammation.Project.ModernOverseerWars;

public class CardStack {
    
    public int currentIndex { get; set; } = 0;
    public List<ControlCard> Cards { get; set; } = new List<ControlCard>();

    public void AddCard(ControlCard card) {
        Cards.Add(card);
    }

    public void RemoveCard(ControlCard card) {
        Cards.Remove(card);
    }
    
    public ControlCard GetCard(int index) {
        return Cards[index];
    }
    
    public void NextCard() {
        currentIndex++;
    }
    
    public void PreviousCard() {
        currentIndex--;
    }
}