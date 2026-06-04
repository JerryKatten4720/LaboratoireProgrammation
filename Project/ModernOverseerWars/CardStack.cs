using System.Collections.Generic;
using LaboratoireProgrammation.Project.ModernOverseerWars.Views;

namespace LaboratoireProgrammation.Project.ModernOverseerWars;

public class CardStack {
    public int currentIndex { get; set; } = 0;
    public List<ControlCard> Cards { get; set; } = new();

    public void AddCard(ControlCard card) => Cards.Add(card);
    public void RemoveCard(ControlCard card) => Cards.Remove(card);
    public ControlCard GetCard(int index) => Cards[index];
    public ControlCard? Current => Cards.Count > 0 ? Cards[currentIndex] : null;
    public void NextCard()     { if (currentIndex < Cards.Count - 1) currentIndex++; }
    public void PreviousCard() { if (currentIndex > 0) currentIndex--; }
}
