using LaboratoireProgrammation.Project.ModernOverseerWars.Domain;
using LaboratoireProgrammation.Project.ModernOverseerWars.Domain.Entities;
using LaboratoireProgrammation.Project.ModernOverseerWars.Domain.Enums;
using LaboratoireProgrammation.Project.ModernOverseerWars.Domain.Interfaces;
using LaboratoireProgrammation.Project.ModernOverseerWars.Domain.Map;
using LaboratoireProgrammation.Project.ModernOverseerWars.Data.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace LaboratoireProgrammation.Project.ModernOverseerWars;

public partial class VaultCardHolder : UserControl {

    public Room?  BoundRoom  { get; private set; }
    public Vault? BoundVault { get; private set; }
    private readonly CardStack _stack = new();

    public event Action<VaultCardHolder, ControlCard>? CardAssigned;
    public event Action<VaultCardHolder>? AssignClicked;
    public event Action<string>? TechBonusRequested;

    public VaultCardHolder() { InitializeComponent(); }

    public void ApplyTheme(Color bg, Color accent) {
        HolderBorder.Background = new SolidColorBrush(bg);
        HolderBorderColor.Color = accent;
    }

    public void BindRoom(Room room, Vault vault) {
        BoundRoom  = room;
        BoundVault = vault;
        RoomNameText.Text = room.Name;
        TechPanel.Visibility   = room.Type == RoomType.TechCenter ? Visibility.Visible : Visibility.Collapsed;
        ActionButton.Visibility = room.Type == RoomType.TechCenter ? Visibility.Collapsed : Visibility.Visible;
        Refresh();
    }

    public void Refresh() {
        if (BoundRoom == null) return;
        WorkersText.Text = $"Workers: {BoundRoom.AssignedDwellers.Count(d => d.IsAlive)}";
        OutputText.Text  = $"Output: {BoundRoom.ProduceValue}";
        RebuildCardStack();
    }

    private void RebuildCardStack() {
        CardCanvas.Children.Clear();
        _stack.Cards.Clear();
        if (BoundRoom == null) return;
        var dwellers = BoundRoom.AssignedDwellers.Where(d => d.IsAlive).ToList();
        EmptyHint.Visibility = dwellers.Any() ? Visibility.Collapsed : Visibility.Visible;
        CardCanvas.Children.Add(EmptyHint);

        double stackWidth = 140 + Math.Max(0, dwellers.Count - 1) * 14;
        double stackHeight = 200 + Math.Max(0, dwellers.Count - 1) * 6;
        CardCanvas.Width = stackWidth;
        CardCanvas.Height = stackHeight;

        for (int i = 0; i < dwellers.Count; i++) {
            var card = new ControlCard();
            card.BindDweller(dwellers[i]);
            card.RenderTransform = new RotateTransform(Random.Shared.Next(-3, 3));
            Canvas.SetLeft(card, i * 14);
            Canvas.SetTop(card,  i * 6);
            CardCanvas.Children.Add(card);
            _stack.AddCard(card);
        }
    }

    private void OnDrop(object s, DragEventArgs e) {
        HolderBorderColor.Color = Color.FromRgb(0x33, 0x33, 0x44);
        var held = OverseerWarsWindow.heldCard;
        if (held?.BoundDweller == null || BoundRoom == null) return;
        CardAssigned?.Invoke(this, held);
        OverseerWarsWindow.heldCard = null;
    }

    private void OnDragEnter(object s, DragEventArgs e) {
        var anim = new ColorAnimation(Color.FromRgb(0x33, 0x55, 0x88), TimeSpan.FromMilliseconds(150));
        HolderBorderColor.BeginAnimation(SolidColorBrush.ColorProperty, anim);
    }
    private void OnDragLeave(object s, DragEventArgs e) {
        var anim = new ColorAnimation(Color.FromRgb(0x33, 0x33, 0x44), TimeSpan.FromMilliseconds(150));
        HolderBorderColor.BeginAnimation(SolidColorBrush.ColorProperty, anim);
    }
    private void OnDragOver(object s, DragEventArgs e) {
        e.Effects = DragDropEffects.Move;
        e.Handled = true;
    }

    private void OnActionClick(object s, RoutedEventArgs e) {
        AssignClicked?.Invoke(this);
    }

    private void OnTechAim(object s, RoutedEventArgs e)  => TechBonusRequested?.Invoke("ReliableAim");
    private void OnTechTank(object s, RoutedEventArgs e) => TechBonusRequested?.Invoke("FrankTheTank");

    private void OnMouseWheel(object s, MouseWheelEventArgs e) {
        if (BoundRoom == null || BoundRoom.AssignedDwellers.Count <= 1) return;
        if (e.Delta > 0) {
            var first = BoundRoom.AssignedDwellers[0];
            BoundRoom.AssignedDwellers.Remove(first);
            BoundRoom.AssignedDwellers.Add(first);
        } else {
            var last = BoundRoom.AssignedDwellers.Last();
            BoundRoom.AssignedDwellers.Remove(last);
            BoundRoom.AssignedDwellers.Insert(0, last);
        }
        Refresh();
        e.Handled = true;
    }
}
