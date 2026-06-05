using LaboratoireProgrammation.Project.ModernOverseerWars.Models;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Entities;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Enums;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Interfaces;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Map;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Data.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LaboratoireProgrammation.Project.ModernOverseerWars.Views;

public partial class DeckPicker : UserControl {
    public enum DeckTab { Dwellers, Weapons, Outfits, Scraps }
    public DeckTab CurrentTab { get; private set; } = DeckTab.Dwellers;

    public event Action<ControlCard>? CardSelected;

    private List<Dweller> _dwellers = new();
    private List<IWeapon> _weapons = new();
    private List<IOutfit> _outfits = new();
    private List<Scrap> _scraps = new();

    public DeckPicker() {
        InitializeComponent();
    }

    public void ApplyTheme(Color bg, Color accent) {
        UpdateButtonHighlights();
    }

    public void LoadDeck(IEnumerable<Dweller> dwellers, IEnumerable<IWeapon> weapons, IEnumerable<IOutfit> outfits, IEnumerable<Scrap> scraps) {
        _dwellers = dwellers.ToList();
        _weapons = weapons.ToList();
        _outfits = outfits.ToList();
        _scraps = scraps.ToList();

        TabDwellersBtn.Content = $"DWELLERS ({_dwellers.Count})";
        TabWeaponsBtn.Content = $"WEAPONS ({_weapons.Count})";
        TabOutfitsBtn.Content = $"OUTFITS ({_outfits.Count})";
        TabScrapsBtn.Content = $"SCRAPS ({_scraps.Count})";

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
        } else if (CurrentTab == DeckTab.Weapons) {
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
        } else if (CurrentTab == DeckTab.Outfits) {
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
        } else if (CurrentTab == DeckTab.Scraps) {
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
        DeckCountText.Text = $"> {CardRow.Children.Count} UNITS DETECTED";
    }

    private void UpdateButtonHighlights() {
        SetTabStyle(TabDwellersBtn, CurrentTab == DeckTab.Dwellers);
        SetTabStyle(TabWeaponsBtn, CurrentTab == DeckTab.Weapons);
        SetTabStyle(TabOutfitsBtn, CurrentTab == DeckTab.Outfits);
        SetTabStyle(TabScrapsBtn, CurrentTab == DeckTab.Scraps);
    }

    private void SetTabStyle(Button btn, bool isActive) {
        if (isActive) {
            btn.SetResourceReference(Control.BackgroundProperty, "PlayerAccentBrush");
            btn.SetResourceReference(Control.ForegroundProperty, "PlayerBgBrush");
            btn.SetResourceReference(Control.BorderBrushProperty, "PlayerAccentBrush");
            btn.SetResourceReference(UIElement.EffectProperty, "PlayerAccentGlow");
        } else {
            btn.Background = Brushes.Transparent;
            btn.SetResourceReference(Control.ForegroundProperty, "PlayerAccentDimBrush");
            btn.SetResourceReference(Control.BorderBrushProperty, "PlayerAccentDimBrush");
            btn.Effect = null;
        }
    }

    public void RefreshAll() {
        foreach (ControlCard card in CardRow.Children) {
            card.RefreshHp();
        }
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