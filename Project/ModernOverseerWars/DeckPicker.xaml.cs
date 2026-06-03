using LaboratoireProgrammation.Project.ModernOverseerWars.Domain;
using LaboratoireProgrammation.Project.ModernOverseerWars.Domain.Entities;
using LaboratoireProgrammation.Project.ModernOverseerWars.Domain.Enums;
using LaboratoireProgrammation.Project.ModernOverseerWars.Domain.Interfaces;
using LaboratoireProgrammation.Project.ModernOverseerWars.Domain.Map;
using LaboratoireProgrammation.Project.ModernOverseerWars.Data.Json;
using System;
using System.Linq;
using System.Collections.Generic;
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
    private Color _bgColor = Color.FromRgb(0x0A, 0x0A, 0x1A);

    public DeckPicker() { InitializeComponent(); }

    public void ApplyTheme(Color bg, Color accent) {
        _bgColor = bg;
        if (Content is Border b) b.Background = new SolidColorBrush(bg);
        UpdateButtonHighlights();
    }

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
        Color active = Lighten(_bgColor, 0.2);
        Color inactive = Lighten(_bgColor, 0.1);
        TabDwellersBtn.Background = new SolidColorBrush(CurrentTab == DeckTab.Dwellers ? active : inactive);
        TabWeaponsBtn.Background = new SolidColorBrush(CurrentTab == DeckTab.Weapons ? active : inactive);
        TabOutfitsBtn.Background = new SolidColorBrush(CurrentTab == DeckTab.Outfits ? active : inactive);
        TabScrapsBtn.Background = new SolidColorBrush(CurrentTab == DeckTab.Scraps ? active : inactive);
    }

    private static Color Lighten(Color c, double amount) {
        if (amount > 0) {
            return Color.FromRgb(
                (byte)Math.Max(0, Math.Min(255, c.R + (255 - c.R) * amount)),
                (byte)Math.Max(0, Math.Min(255, c.G + (255 - c.G) * amount)),
                (byte)Math.Max(0, Math.Min(255, c.B + (255 - c.B) * amount)));
        } else {
            double f = 1.0 + amount;
            return Color.FromRgb(
                (byte)Math.Max(0, Math.Min(255, c.R * f)),
                (byte)Math.Max(0, Math.Min(255, c.G * f)),
                (byte)Math.Max(0, Math.Min(255, c.B * f)));
        }
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
