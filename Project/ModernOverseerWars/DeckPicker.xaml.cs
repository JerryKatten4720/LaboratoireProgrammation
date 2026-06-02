using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LaboratoireProgrammation.Project.ModernOverseerWars;

public partial class DeckPicker : UserControl {
    public enum DeckTab { Dwellers, Weapons, Outfits, Scraps }
    public DeckTab CurrentTab { get; private set; } = DeckTab.Dwellers;

    public event Action<ControlCard>? CardSelected;

    private List<Dweller> _dwellers = new();
    private List<IWeapon> _weapons = new();
    private List<IOutfit> _outfits = new();
    private List<Scrap> _scraps = new();

    public DeckPicker() { InitializeComponent(); }

    public void LoadDeck(IEnumerable<Dweller> dwellers, IEnumerable<IWeapon> weapons, IEnumerable<IOutfit> outfits, IEnumerable<Scrap> scraps) {
        _dwellers = dwellers.ToList();
        _weapons = weapons.ToList();
        _outfits = outfits.ToList();
        _scraps = scraps.ToList();
        RefreshDeck();
    }

    public void RefreshDeck() {
        CardRow.Children.Clear();
        UpdateButtonHighlights();

        if (CurrentTab == DeckTab.Dwellers) {
            foreach (var d in _dwellers) {
                var card = new ControlCard { Margin = new Thickness(0, 0, 10, 0) };
                card.BindDweller(d);
                card.SetDeckMode(true);
                card.MouseLeftButtonDown += (s, e) => {
                    OverseerWarsWindow.heldCard = card;
                    CardSelected?.Invoke(card);
                    e.Handled = true;
                };
                CardRow.Children.Add(card);
            }
        }
        else if (CurrentTab == DeckTab.Weapons) {
            foreach (var w in _weapons) {
                var card = new ControlCard { Margin = new Thickness(0, 0, 10, 0) };
                card.BindWeapon(w);
                card.SetDeckMode(true);
                card.MouseLeftButtonDown += (s, e) => {
                    OverseerWarsWindow.heldCard = card;
                    CardSelected?.Invoke(card);
                    e.Handled = true;
                };
                CardRow.Children.Add(card);
            }
        }
        else if (CurrentTab == DeckTab.Outfits) {
            foreach (var o in _outfits) {
                var card = new ControlCard { Margin = new Thickness(0, 0, 10, 0) };
                card.BindOutfit(o);
                card.SetDeckMode(true);
                card.MouseLeftButtonDown += (s, e) => {
                    OverseerWarsWindow.heldCard = card;
                    CardSelected?.Invoke(card);
                    e.Handled = true;
                };
                CardRow.Children.Add(card);
            }
        }
        else if (CurrentTab == DeckTab.Scraps) {
            foreach (var s in _scraps) {
                var card = new ControlCard { Margin = new Thickness(0, 0, 10, 0) };
                card.BindScrap(s);
                card.SetDeckMode(true);
                card.MouseLeftButtonDown += (ev, args) => {
                    OverseerWarsWindow.heldCard = card;
                    CardSelected?.Invoke(card);
                    args.Handled = true;
                };
                CardRow.Children.Add(card);
            }
        }
        DeckCountText.Text = $" — {CardRow.Children.Count} cards";
    }

    private void UpdateButtonHighlights() {
        TabDwellersBtn.Background = new SolidColorBrush(CurrentTab == DeckTab.Dwellers ? Color.FromRgb(0x3A, 0x3A, 0x60) : Color.FromRgb(0x1E, 0x1E, 0x38));
        TabWeaponsBtn.Background = new SolidColorBrush(CurrentTab == DeckTab.Weapons ? Color.FromRgb(0x3A, 0x3A, 0x60) : Color.FromRgb(0x1E, 0x1E, 0x38));
        TabOutfitsBtn.Background = new SolidColorBrush(CurrentTab == DeckTab.Outfits ? Color.FromRgb(0x3A, 0x3A, 0x60) : Color.FromRgb(0x1E, 0x1E, 0x38));
        TabScrapsBtn.Background = new SolidColorBrush(CurrentTab == DeckTab.Scraps ? Color.FromRgb(0x3A, 0x3A, 0x60) : Color.FromRgb(0x1E, 0x1E, 0x38));
    }

    public void RefreshAll() {
        foreach (ControlCard card in CardRow.Children)
            card.RefreshHp();
    }

    private void OnTabDwellers(object s, RoutedEventArgs e) { CurrentTab = DeckTab.Dwellers; RefreshDeck(); }
    private void OnTabWeapons(object s, RoutedEventArgs e) { CurrentTab = DeckTab.Weapons; RefreshDeck(); }
    private void OnTabOutfits(object s, RoutedEventArgs e) { CurrentTab = DeckTab.Outfits; RefreshDeck(); }
    private void OnTabScraps(object s, RoutedEventArgs e) { CurrentTab = DeckTab.Scraps; RefreshDeck(); }

    private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e) {
        if (e.Delta > 0) DeckScrollViewer.LineLeft();
        else DeckScrollViewer.LineRight();
        e.Handled = true;
    }
}
