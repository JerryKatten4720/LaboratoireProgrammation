using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace LaboratoireProgrammation.Project.ModernOverseerWars;

public partial class OverseerWarsWindow : Window {

    public static ControlCard? heldCard;

    private GameState       _state   = null!;
    private TurnManager     _turns   = null!;
    private ResourceManager _res     = null!;
    private CombatManager   _combat  = null!;
    private HexTileControl? _selectedTile;
    private bool _showingVault = false;

    private Color _p1Color;
    private Color _p2Color;
    private Color _p1Bg;
    private Color _p2Bg;

    private Room? _targetRoomForAssignment;
    private HexTile? _targetTileForDeployment;
    private Dweller? _selectedDwellerForAction;
    private Border? _selectedDwellerRowBorder;

    private Dweller? _equippingDweller;
    private bool _isEquippingWeapon;
    private ICard? _selectedEquipment;
    private Border? _selectedEquipmentRowBorder;

    private Dweller? _dwellerInAssignmentSlot;

    private Panel? _originalCardParent;
    private int _originalCardIndex;
    private UIElement? _dragPlaceholder;

    public OverseerWarsWindow(PlayerProfile p1, PlayerProfile p2) {
        InitializeComponent();
        _p1Color = (Color)ColorConverter.ConvertFromString(p1.Theme.Color);
        _p2Color = (Color)ColorConverter.ConvertFromString(p2.Theme.Color);
        _p1Bg    = (Color)ColorConverter.ConvertFromString(p1.Theme.BgColor);
        _p2Bg    = (Color)ColorConverter.ConvertFromString(p2.Theme.BgColor);

        Loaded += (s, e) => StartNewGame(p1, p2);
    }

    private void StartNewGame(PlayerProfile p1, PlayerProfile p2) {
        GameConfigManager.Load(FindJsonFile("config.json"));
        _state = new GameState { Player1 = p1, Player2 = p2 };
        _state.InitVaults();
        _turns  = new TurnManager(_state);
        _res    = new ResourceManager(_state);
        _combat = new CombatManager(_state);

        _turns.TurnTick    += OnTick;
        _turns.TurnExpired += () => Dispatcher.InvokeAsync(OnEndTurn_Internal);
        _combat.CombatEvent += msg => Dispatcher.InvokeAsync(() => AppendLog(msg));
        _combat.DamagePopup += (text, _, _, _) =>
            Dispatcher.InvokeAsync(() => SpawnDamagePopup(text));

        SetupPlayers();
        BuildMapUI();
        BuildVaultUI();
        
        DeckPickerControl.LoadDeck(GetIdleDwellers(), _state.ActiveVault.UnusedWeapons, _state.ActiveVault.UnusedOutfits, _state.ActiveVault.Scraps);
        DeckPickerControl.CardSelected += card => { heldCard = card; UpdateInfoPanel(); };
        GameOverOverlay.Visibility = Visibility.Collapsed;
        _turns.StartTurn(isPlayer1: true);
        ApplyTheme(isP1: true);
        RefreshAll();
        double cW = GameConfigManager.Config.MapCols * 100;
        double cH = GameConfigManager.Config.MapRows * 100;
        MapView.ScrollToVerticalOffset(Math.Max(0, (cH - 480) / 2));
        MapView.ScrollToHorizontalOffset(Math.Max(0, (cW - 850) / 2));
    }

    private static string FindJsonFile(string filename) {
        string[] paths = {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "data", filename),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "Assets", "data", filename),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Assets", "data", filename),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "Assets", "data", filename),
            Path.Combine("Assets", "data", filename),
            Path.Combine("..", "Assets", "data", filename),
            Path.Combine("..", "..", "Assets", "data", filename),
            Path.Combine("..", "..", "..", "Assets", "data", filename),
            @"C:\Users\antoi\Documents\GitHub\LaboratoireProgrammation\Assets\data\" + filename
        };
        foreach (var p in paths) {
            if (File.Exists(p)) return p;
        }
        throw new FileNotFoundException($"Could not find JSON file: {filename}");
    }

    private void SetupPlayers() {
        try {
            string dp = FindJsonFile("dwellers.json");
            string wp = FindJsonFile("weapons.json");
            string op = FindJsonFile("outfits.json");
            string sp = FindJsonFile("scraps.json");

            GameDataRepository.Dwellers = JsonSerializer.Deserialize<List<DwellerJson>>(File.ReadAllText(dp)) ?? new();
            GameDataRepository.Weapons = JsonSerializer.Deserialize<List<WeaponJson>>(File.ReadAllText(wp)) ?? new();
            GameDataRepository.Outfits = JsonSerializer.Deserialize<List<OutfitJson>>(File.ReadAllText(op)) ?? new();
            GameDataRepository.Scraps = JsonSerializer.Deserialize<List<ScrapJson>>(File.ReadAllText(sp)) ?? new();

            var rng = Random.Shared;
            var sh = GameDataRepository.Dwellers.OrderBy(_ => rng.Next()).ToList();
            int count = Math.Min(GameConfigManager.Config.StarterDwellersCount, sh.Count / 2);

            for (int i = 0; i < count; i++) {
                var d1 = CreateDwellerFromParsed(sh[i], i == 0);
                _state.Vault1.Dwellers.Add(d1);
                var d2 = CreateDwellerFromParsed(sh[i + count], i == 0);
                _state.Vault2.Dwellers.Add(d2);
            }

            if (GameDataRepository.Weapons.Count > 0) {
                var w1 = CreateWeaponFromParsed(GameDataRepository.Weapons[rng.Next(GameDataRepository.Weapons.Count)]);
                _state.Vault1.Weapons.Add(w1);
                var w2 = CreateWeaponFromParsed(GameDataRepository.Weapons[rng.Next(GameDataRepository.Weapons.Count)]);
                _state.Vault2.Weapons.Add(w2);
            }

            if (GameDataRepository.Outfits.Count > 0) {
                var o1 = CreateOutfitFromParsed(GameDataRepository.Outfits[rng.Next(GameDataRepository.Outfits.Count)]);
                _state.Vault1.Outfits.Add(o1);
                var o2 = CreateOutfitFromParsed(GameDataRepository.Outfits[rng.Next(GameDataRepository.Outfits.Count)]);
                _state.Vault2.Outfits.Add(o2);
            }

            EquipStarter(_state.Vault1);
            EquipStarter(_state.Vault2);
        }
        catch (Exception ex) {
            MessageBox.Show($"Error loading data: {ex.Message}");
        }
    }

    private Dweller CreateDwellerFromParsed(DwellerJson dj, bool sup) {
        var d = new Dweller {
            FirstName = dj.FirstName, LastName = dj.LastName,
            Special_S = dj.S, Special_P = dj.P, Special_E = dj.E, Special_C = dj.C, Special_I = dj.I, Special_A = dj.A, Special_L = dj.L,
            RarityVal = dj.Rarity, Texture = dj.Texture, IsSupervisor = sup
        };
        d.CurrentHp = d.MaxHp;
        return d;
    }

    private Weapon CreateWeaponFromParsed(WeaponJson wj) {
        var w = new Weapon { Name = wj.WeaponName, Texture = wj.Texture, Damages = wj.Damages, CriticalChance = wj.CriticalChance, Rarity = CardRarity.Common };
        w.WeaponType = (w.Name.Contains("Bat") || w.Name.Contains("Knife") || w.Name.Contains("Sword")) ? "Melee" : "Ranged";
        return w;
    }

    private Outfit CreateOutfitFromParsed(OutfitJson oj) {
        return new Outfit { Name = oj.OutfitName, Texture = oj.Texture, Defense = oj.Defense, S = oj.S, P = oj.P, E = oj.E, C = oj.C, I = oj.I, A = oj.A, L = oj.L, Rarity = CardRarity.Common };
    }

    private void EquipStarter(Vault v) {
        var s = v.Supervisor;
        if (s != null) {
            var w = v.Weapons.FirstOrDefault();
            if (w != null) s.EquippedWeapon = w;
            var o = v.Outfits.FirstOrDefault();
            if (o != null) s.EquippedOutfit = o;
        }
    }

    private void ApplyTheme(bool isP1) {
        Color accent = isP1 ? _p1Color : _p2Color;
        Color bg     = isP1 ? _p1Bg   : _p2Bg;
        
        Color darkAccent = Color.FromRgb((byte)(accent.R * 0.15), (byte)(accent.G * 0.15), (byte)(accent.B * 0.15));
        
        RootGrid.Background = new SolidColorBrush(darkAccent);
        MapBorder.Background = new SolidColorBrush(Color.FromArgb((byte)(255 * GameConfigManager.Config.MapTransparency), darkAccent.R, darkAccent.G, darkAccent.B));
        VaultView.Opacity = GameConfigManager.Config.VaultUITransparency;

        TopBar.Background   = new SolidColorBrush(
            Color.FromRgb((byte)(bg.R / 2), (byte)(bg.G / 2), (byte)(bg.B / 2)));
        TimerBar.Fill = new SolidColorBrush(accent);
        ActivePlayerLabel.Foreground = new SolidColorBrush(accent);
        ActivePlayerLabel.Text = isP1
            ? $"🎯 {_state.Player1.Pseudo}'s Turn"
            : $"🎯 {_state.Player2.Pseudo}'s Turn";
        HexTileControl.ActivePlayerColor = accent;

        foreach (VaultCardHolder h in RoomsPanel.Children.OfType<VaultCardHolder>())
            ((SolidColorBrush)h.FindName("HolderBorderColor") ?? new SolidColorBrush()).Color = accent;
        EndTurnButton.BorderBrush = new SolidColorBrush(accent);
        EndTurnButton.Foreground  = new SolidColorBrush(accent);

        string pseudo = isP1 ? _state.Player1.Pseudo : _state.Player2.Pseudo;
        BgUsernameText.Fill = new SolidColorBrush(accent);

        _dwellerInAssignmentSlot = null;
        UpdateAssignmentSlotUI();
    }

    private void BuildMapUI() {
        MapCanvas.Children.Clear();
        double tileW = 103, tileH = 103, offsetX = 0, offsetY = 0;
        for (int c = 0; c < _state.Map.Cols; c++)
            for (int r = 0; r < _state.Map.Rows; r++) {
                var ctrl = new HexTileControl();
                ctrl.BindTile(_state.Map.Get(c, r));
                ctrl.Refresh(isP1: _state.IsPlayer1Turn);
                ctrl.CardDropped  += OnCardDroppedOnTile;
                double x = offsetX + c * tileW;
                double y = offsetY + r * tileH;
                Canvas.SetLeft(ctrl, x);
                Canvas.SetTop(ctrl, y);
                MapCanvas.Children.Add(ctrl);
            }
        MapCanvas.Width  = offsetX + _state.Map.Cols * tileW;
        MapCanvas.Height = offsetY + _state.Map.Rows * tileH;
    }

    private void RefreshMapUI() {
        bool isP1 = _state.IsPlayer1Turn;
        foreach (HexTileControl ctrl in MapCanvas.Children.OfType<HexTileControl>())
            ctrl.Refresh(isP1);
    }

    private void BuildVaultUI() {
        RoomsPanel.Children.Clear();
        var vault = _state.ActiveVault;
        foreach (var room in vault.Rooms) {
            var holder = new VaultCardHolder();
            holder.BindRoom(room, vault);
            holder.CardAssigned += OnCardAssignedToRoom;
            holder.AssignClicked += h => {
                _targetRoomForAssignment = h.BoundRoom;
                _targetTileForDeployment = null;
                OpenDwellerSelection();
            };
            holder.TechBonusRequested += OnTechBonus;
            RoomsPanel.Children.Add(holder);
        }
    }

    private void RefreshVaultUI() {
        foreach (VaultCardHolder h in RoomsPanel.Children.OfType<VaultCardHolder>())
            h.Refresh();
    }

    private void OnTileClicked(HexTileControl ctrl) {
        if (_selectedTile != null && _selectedTile != ctrl) {
            var activeDwellers = _state.IsPlayer1Turn ? _selectedTile.BoundTile.Player1Dwellers : _selectedTile.BoundTile.Player2Dwellers;
            var dw = activeDwellers.LastOrDefault(d => d.IsAlive);
            if (dw != null) {
                int cost = GetDeploymentCost(dw, ctrl.BoundTile);
                if (cost <= _state.ActiveVault.ActionPoints) {
                    _state.ActiveVault.ActionPoints -= cost;
                    RemoveDwellerFromMap(dw);
                    bool isHomeVault = (_state.IsPlayer1Turn && ctrl.BoundTile.Type == TileType.Player1Vault) || (!_state.IsPlayer1Turn && ctrl.BoundTile.Type == TileType.Player2Vault);
                    if (!isHomeVault) {
                        if (_state.IsPlayer1Turn) ctrl.BoundTile.Player1Dwellers.Add(dw);
                        else ctrl.BoundTile.Player2Dwellers.Add(dw);
                    }
                    ctrl.BoundTile.RevealFor(_state.IsPlayer1Turn);
                    _state.AddLog($"{dw.Name} moved to ({ctrl.BoundTile.Col},{ctrl.BoundTile.Row}) [{cost} PA]");
                    if (ctrl.BoundTile.HasConflict) { _combat.ResolveCombatsOnMap(); CheckGameOver(); }
                    _selectedTile.SetSelected(false);
                    _selectedTile = null;
                    RefreshAll();
                    return;
                } else {
                    ShowWarning("Not enough PA to move there!");
                }
            }
        }
        
        if (_selectedTile == ctrl) {
            _selectedTile.SetSelected(false);
            _selectedTile = null;
            UpdateInfoPanel();
            return;
        }

        if (_selectedTile != null && _selectedTile != ctrl)
            _selectedTile.SetSelected(false);
        _selectedTile = ctrl;
        ctrl.SetSelected(true);
        UpdateInfoPanel();
    }

    private void OnCardDroppedOnTile(HexTileControl ctrl, ControlCard card) {
        if (_selectedTile != null && _selectedTile != ctrl) _selectedTile.SetSelected(false);
        _selectedTile = ctrl;
        ctrl.SetSelected(true);
        DeployCard(card);
    }

    private void UpdateInfoPanel() {
        DeployButton.Visibility = Visibility.Collapsed;
        if (_selectedTile?.BoundTile == null) return;
        var tile = _selectedTile.BoundTile;
        bool isP1 = _state.IsPlayer1Turn;
        bool revealed = tile.IsRevealedFor(isP1);
        InfoTitle.Text = revealed ? (tile.LocationName != "" ? tile.LocationName
            : tile.Type == TileType.Player1Vault ? "P1 Vault"
            : tile.Type == TileType.Player2Vault ? "P2 Vault"
            : "Wasteland") : "Fog of War";
        InfoBody.Text = revealed
            ? $"P1 dwellers: {tile.Player1Dwellers.Count(d => d.IsAlive)}\nP2 dwellers: {tile.Player2Dwellers.Count(d => d.IsAlive)}"
            : "Unexplored";
                var canAct = _state.ActiveVault.ActionPoints > 0;
        if (canAct) {
            DeployButton.Visibility = Visibility.Visible;
        }
    }

    private int GetDeploymentCost(Dweller d, HexTile target) {
        var current = _state.Map.Tiles.Cast<HexTile>().FirstOrDefault(t => t.Player1Dwellers.Contains(d) || t.Player2Dwellers.Contains(d));
        if (current == null) {
            current = _state.IsPlayer1Turn 
                ? _state.Map.Get(0, _state.Map.Rows / 2)
                : _state.Map.Get(_state.Map.Cols - 1, _state.Map.Rows / 2);
        }
        return GetHexDistance(current, target);
    }

    private int GetHexDistance(HexTile a, HexTile b) {
        int qa = a.Col - (a.Row - (a.Row & 1)) / 2;
        int ra = a.Row;
        int qb = b.Col - (b.Row - (b.Row & 1)) / 2;
        int rb = b.Row;
        return (Math.Abs(qa - qb) + Math.Abs(qa + ra - (qb + rb)) + Math.Abs(ra - rb)) / 2;
    }

    private HexTile? FindDwellerTile(Dweller d) {
        for (int c = 0; c < _state.Map.Cols; c++)
            for (int r = 0; r < _state.Map.Rows; r++) {
                var t = _state.Map.Get(c, r);
                if (t.Player1Dwellers.Contains(d) || t.Player2Dwellers.Contains(d)) return t;
            }
        return null;
    }

    private void OnDeployDweller(object s, RoutedEventArgs e) {
        if (_selectedTile?.BoundTile == null) return;
        _targetRoomForAssignment = null;
        _targetTileForDeployment = _selectedTile.BoundTile;
        OpenDwellerSelection();
    }

    private void DeployCard(ControlCard? card) {
        if (card?.BoundDweller == null || _selectedTile?.BoundTile == null) return;
        if (!_selectedTile.BoundTile.IsNavigable) { ShowWarning("Tile is unnavigable!"); return; }
        var vault = _state.ActiveVault;
        var tile  = _selectedTile.BoundTile;
        int cost  = GetDeploymentCost(card.BoundDweller, tile);
        if (cost > vault.ActionPoints) { ShowWarning("Tile too far away, you need more PA"); return; }
        
        if (card.OwnerDweller != null) {
            if (card.BoundWeapon != null) card.OwnerDweller.EquippedWeapon = null;
            if (card.BoundOutfit != null) card.OwnerDweller.EquippedOutfit = null;
        }

        vault.ActionPoints -= cost;
        RemoveDwellerFromMap(card.BoundDweller);
        bool isP1 = _state.IsPlayer1Turn;
        bool isHomeVault = (isP1 && tile.Type == TileType.Player1Vault) || (!isP1 && tile.Type == TileType.Player2Vault);
        if (!isHomeVault) {
            var dwellerList = isP1 ? tile.Player1Dwellers : tile.Player2Dwellers;
            dwellerList.Add(card.BoundDweller);
        }
        tile.IsRevealedByP1 = isP1 ? true : tile.IsRevealedByP1;
        tile.IsRevealedByP2 = isP1 ? tile.IsRevealedByP2 : true;

        if (tile.HasConflict) { _combat.ResolveCombatsOnMap(); CheckGameOver(); }
        _state.AddLog($"{card.BoundDweller.Name} → ({tile.Col},{tile.Row}) [{cost} PA]");
        heldCard = null;
        if (_selectedTile != null) _selectedTile.SetSelected(false);
        _selectedTile = null;
        RefreshAll();
    }

    private void ShowWarning(string message) {
        WarningText.Text = message;
        WarningPanel.Visibility = Visibility.Visible;
        var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
        timer.Tick += (s, e) => {
            WarningPanel.Visibility = Visibility.Collapsed;
            timer.Stop();
        };
        timer.Start();
    }



    private void OnCardAssignedToRoom(VaultCardHolder holder, ControlCard card) {
        if (card.BoundDweller == null || holder.BoundRoom == null) return;
        if (holder.BoundRoom.AssignedDwellers.Contains(card.BoundDweller)) return; // Already in room, no PA cost
        if (_state.ActiveVault.ActionPoints <= 0) { ShowWarning("No action points left!"); return; }
        _res.TryAssignDweller(_state.ActiveVault, card.BoundDweller, holder.BoundRoom);
        _state.ActiveVault.ActionPoints--;
        RefreshAll();
    }

    private void OnTechBonus(string bonus) {
        var (ok, err) = _res.TryUnlockTechBonus(_state.ActiveVault, bonus);
        if (!ok) AppendLog(err);
        RefreshAll();
    }

    private void OnEndTurn(object s, RoutedEventArgs e) => OnEndTurn_Internal();
    private void OnEndTurn_Internal() {
        if (_state.Phase == GamePhase.GameOver) return;
        
        bool isP1 = _state.IsPlayer1Turn;
        for (int c = 0; c < _state.Map.Cols; c++) {
            for (int r = 0; r < _state.Map.Rows; r++) {
                var t = _state.Map.Get(c, r);
                if (t.IsNavigable) {
                    var dweller = (isP1 ? t.Player1Dwellers : t.Player2Dwellers).FirstOrDefault(d => d.IsAlive);
                    if (dweller != null) {
                        var vault = isP1 ? _state.Vault1 : _state.Vault2;
                        var (card, msg) = LootManager.SearchTile(t, dweller, isP1, vault);
                        AppendLog($"[Auto-Loot ({c},{r})] {msg}");
                        if (card is Dweller nd) vault.Dwellers.Add(nd);
                        else if (card is IWeapon w) vault.Weapons.Add(w);
                        else if (card is IOutfit o) vault.Outfits.Add(o);
                    }
                }
            }
        }
        
        _turns.EndTurn();
        _combat.ResolveCombatsOnMap();
        CheckGameOver();
        if (_state.Phase != GamePhase.GameOver) {
            ApplyTheme(isP1: _state.IsPlayer1Turn);
            if (_showingVault) BuildVaultUI();
        }
        RefreshAll();
    }

    private void OnToggleView(object s, RoutedEventArgs e) {
        _showingVault = !_showingVault;
        MapView.Visibility   = _showingVault ? Visibility.Collapsed : Visibility.Visible;
        MapBorder.Visibility = _showingVault ? Visibility.Collapsed : Visibility.Visible;
        VaultView.Visibility = _showingVault ? Visibility.Visible   : Visibility.Collapsed;
        PhaseIndicator.Content = _showingVault ? "🗺 MAP" : "🏠 VAULT";
        if (_showingVault) { BuildVaultUI(); RefreshVaultUI(); }
    }

    private void OnBuildRoom(object s, RoutedEventArgs e) {
        var vault    = _state.ActiveVault;
        var existing = vault.Rooms.Select(r => r.Type);
        var dlg      = new BuildRoomDialog(existing) { Owner = this };
        if (dlg.ShowDialog() == true && dlg.ChosenRoom.HasValue) {
            var (ok, err) = _res.TryBuildRoom(vault, dlg.ChosenRoom.Value);
            if (!ok) AppendLog(err);
            else { BuildVaultUI(); RefreshAll(); }
        }
    }

    private void OnPlayAgain(object s, RoutedEventArgs e) {
        var menu = new StartMenuWindow { Owner = this };
        if (menu.ShowDialog() == true)
            StartNewGame(menu.Profile1, menu.Profile2);
    }

    private void OnTick() {
        double pct = (double)_turns.SecondsRemaining / _turns.TurnDurationSeconds;
        double fullW = ActualWidth > 0 ? ActualWidth : 1280;
        TimerBar.Width = fullW * pct;

        if (_turns.SecondsRemaining <= 10) {
            Color warn = Color.FromRgb(200, 60, 60);
            TimerBar.Fill = new SolidColorBrush(warn);
        }
    }

    private void SpawnDamagePopup(string text) {
        var rng = Random.Shared;
        double x = rng.Next(50, (int)PopupCanvas.ActualWidth - 50);
        double y = rng.Next(50, (int)PopupCanvas.ActualHeight - 80);
        Color col = _state.IsPlayer1Turn ? _p1Color : _p2Color;
        DamagePopupHelper.Spawn(text, PopupCanvas, x, y, col);
    }

    private void RefreshHUD() {
        var vault = _state.ActiveVault;
        ElecText.Text  = $"⚡ {vault.Electricity}";
        WaterText.Text = $"💧 {vault.Water}";
        FoodText.Text  = $"🍅 {vault.Food}";
        ApText.Text    = $"PA: {vault.ActionPoints}/{vault.MaxActionPoints}";
        TurnText.Text  = $"Turn {_state.TurnNumber}";
    }

    private void RefreshAll() {
        RefreshHUD();
        RefreshMapUI();
        if (_showingVault) RefreshVaultUI();
        RefreshDeckUI();
        DeckPickerControl.RefreshAll();
        UpdateInfoPanel();
    }

    private void RefreshDeckUI() {
        DeckPickerControl.LoadDeck(GetIdleDwellers(), _state.ActiveVault.UnusedWeapons, _state.ActiveVault.UnusedOutfits, _state.ActiveVault.Scraps);
    }

    private IEnumerable<Dweller> GetIdleDwellers() {
        return _state.ActiveVault.Dwellers.Where(d => d.IsAlive && GetDwellerStatus(d) == "Idle in Deck");
    }

    private void RemoveDwellerFromMap(Dweller d) {
        for (int c = 0; c < _state.Map.Cols; c++)
            for (int r = 0; r < _state.Map.Rows; r++) {
                _state.Map.Get(c, r).Player1Dwellers.Remove(d);
                _state.Map.Get(c, r).Player2Dwellers.Remove(d);
            }
    }

    private void AppendLog(string msg) {
        if (string.IsNullOrEmpty(msg)) return;
        LogText.Text += msg + "\n";
        LogScroller.ScrollToEnd();
    }

    private void CheckGameOver() {
        if (_state.Phase != GamePhase.GameOver) return;
        GameOverOverlay.Visibility = Visibility.Visible;
        bool p1Won = _state.WinnerName == _state.Player1.Pseudo;
        GameOverTitle.Text    = p1Won ? "🏆 VICTORY!" : "💀 DEFEAT";
        GameOverWinner.Text   = $"{_state.WinnerName} eliminated the enemy Overseer!";
        GameOverTitle.Foreground = new SolidColorBrush(p1Won ? _p1Color : _p2Color);
    }

    private void OnRoomsPreviewMouseWheel(object sender, MouseWheelEventArgs e) {
        var hit = VisualTreeHelper.HitTest(RoomsScrollViewer, e.GetPosition(RoomsScrollViewer));
        var holder = FindParent<VaultCardHolder>(hit?.VisualHit);
        if (holder != null && holder.BoundRoom?.AssignedDwellers.Count > 1) {
            return;
        }
        if (e.Delta > 0) RoomsScrollViewer.LineLeft();
        else RoomsScrollViewer.LineRight();
        e.Handled = true;
    }

    private static T? FindParent<T>(DependencyObject? child) where T : DependencyObject {
        while (child != null) {
            if (child is T parent) return parent;
            child = VisualTreeHelper.GetParent(child);
        }
        return null;
    }

    private void OpenDwellerSelection() {
        DwellerSelectionList.Children.Clear();
        _selectedDwellerForAction = null;
        _selectedDwellerRowBorder = null;

        var vault = _state.ActiveVault;
        foreach (var d in vault.Dwellers) {
            if (_targetTileForDeployment != null) {
                int cost = GetDeploymentCost(d, _targetTileForDeployment);
                if (cost > vault.ActionPoints) continue;
            }
            var row = CreateDwellerRow(d);
            DwellerSelectionList.Children.Add(row);
        }
        DwellerSelectionOverlay.Visibility = Visibility.Visible;
    }

    private UIElement CreateDwellerRow(Dweller d) {
        var border = new Border {
            Background = new SolidColorBrush(Color.FromRgb(0x15, 0x15, 0x30)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0x25, 0x25, 0x45)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Margin = new Thickness(0, 0, 0, 6),
            Padding = new Thickness(8),
            Cursor = Cursors.Hand
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.2, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.0, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.8, GridUnitType.Star) });

        var nameStack = new StackPanel();
        nameStack.Children.Add(new TextBlock { Text = d.Name, Foreground = Brushes.White, FontWeight = FontWeights.Bold, FontSize = 12 });
        var rc = new[] { "#969696", "#64C864", "#6496FA", "#B450DC", "#FFB400" };
        nameStack.Children.Add(new TextBlock { Text = d.Rarity.ToString(), Foreground = (Brush)new BrushConverter().ConvertFromString(rc[(int)d.Rarity])!, FontSize = 9, FontWeight = FontWeights.Bold });
        Grid.SetColumn(nameStack, 0);
        grid.Children.Add(nameStack);

        grid.Children.Add(new TextBlock { Text = $"S:{d.Special_S} P:{d.Special_P} E:{d.Special_E} C:{d.Special_C}\nI:{d.Special_I} A:{d.Special_A} L:{d.Special_L}", Foreground = new SolidColorBrush(Color.FromRgb(0xAA, 0xAA, 0xDD)), FontSize = 10, VerticalAlignment = VerticalAlignment.Center });
        Grid.SetColumn(grid.Children[^1], 1);

        grid.Children.Add(new TextBlock { Text = $"W: {d.EquippedWeapon?.Name ?? "None"}\nO: {d.EquippedOutfit?.Name ?? "None"}", Foreground = new SolidColorBrush(Color.FromRgb(0x99, 0xAA, 0x99)), FontSize = 10, VerticalAlignment = VerticalAlignment.Center });
        Grid.SetColumn(grid.Children[^1], 2);

        grid.Children.Add(new TextBlock { Text = $"HP: {d.CurrentHp}/{d.MaxHp}", Foreground = d.CurrentHp > d.MaxHp / 2 ? Brushes.LightGreen : Brushes.OrangeRed, FontSize = 10, FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center });
        Grid.SetColumn(grid.Children[^1], 3);

        grid.Children.Add(new TextBlock { Text = GetDwellerStatus(d), Foreground = new SolidColorBrush(Color.FromRgb(0xBB, 0xBB, 0xBB)), FontSize = 10, TextWrapping = TextWrapping.Wrap, VerticalAlignment = VerticalAlignment.Center });
        Grid.SetColumn(grid.Children[^1], 4);

        border.Child = grid;
        border.MouseDown += (s, e) => {
            if (_selectedDwellerRowBorder != null) {
                _selectedDwellerRowBorder.Background = new SolidColorBrush(Color.FromRgb(0x15, 0x15, 0x30));
                _selectedDwellerRowBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(0x25, 0x25, 0x45));
            }
            border.Background = new SolidColorBrush(Color.FromRgb(0x2A, 0x3A, 0x60));
            border.BorderBrush = new SolidColorBrush(Color.FromRgb(0x55, 0x88, 0xFF));
            _selectedDwellerRowBorder = border;
            _selectedDwellerForAction = d;
        };

        return border;
    }

    private string GetDwellerStatus(Dweller d) {
        foreach (var room in _state.ActiveVault.Rooms)
            if (room.AssignedDwellers.Contains(d)) return $"Working in {room.Name}";
        for (int c = 0; c < _state.Map.Cols; c++)
            for (int r = 0; r < _state.Map.Rows; r++) {
                var t = _state.Map.Get(c, r);
                if (t.Player1Dwellers.Contains(d) || t.Player2Dwellers.Contains(d))
                    return $"At ({c},{r}) {(t.LocationName != "" ? t.LocationName : "Wasteland")}";
            }
        return "Idle in Deck";
    }

    private void OnCancelDwellerSelection(object s, RoutedEventArgs e) => DwellerSelectionOverlay.Visibility = Visibility.Collapsed;
    private void OnConfirmDwellerSelection(object s, RoutedEventArgs e) {
        if (_selectedDwellerForAction == null) return;
        DwellerSelectionOverlay.Visibility = Visibility.Collapsed;
        var dw = _selectedDwellerForAction;

        if (_targetRoomForAssignment != null) {
            if (_state.ActiveVault.ActionPoints <= 0) { ShowWarning("No action points left!"); return; }
            _res.TryAssignDweller(_state.ActiveVault, dw, _targetRoomForAssignment);
            _state.ActiveVault.ActionPoints--;
            RemoveDwellerFromMap(dw);
            if (dw == _dwellerInAssignmentSlot) _dwellerInAssignmentSlot = null;
            RefreshAll();
        }
        else if (_targetTileForDeployment != null) {
            if (!_targetTileForDeployment.IsNavigable) { ShowWarning("Tile is unnavigable!"); return; }
            var vault = _state.ActiveVault;
            var tile = _targetTileForDeployment;
            int cost = GetDeploymentCost(dw, tile);
            if (cost > vault.ActionPoints) { ShowWarning("Tile too far away, you need more PA"); return; }
            vault.ActionPoints -= cost;
            RemoveDwellerFromMap(dw);
            if (dw == _dwellerInAssignmentSlot) _dwellerInAssignmentSlot = null;
            bool isP1 = _state.IsPlayer1Turn;
            bool isHomeVault = (isP1 && tile.Type == TileType.Player1Vault) || (!isP1 && tile.Type == TileType.Player2Vault);
            if (!isHomeVault) {
                var dwellerList = isP1 ? tile.Player1Dwellers : tile.Player2Dwellers;
                dwellerList.Add(dw);
            }
            tile.IsRevealedByP1 = isP1 ? true : tile.IsRevealedByP1;
            tile.IsRevealedByP2 = isP1 ? tile.IsRevealedByP2 : true;
            if (tile.HasConflict) { _combat.ResolveCombatsOnMap(); CheckGameOver(); }
            _state.AddLog($"{dw.Name} → ({tile.Col},{tile.Row}) [{cost} PA]");
            if (_selectedTile != null) _selectedTile.SetSelected(false);
            _selectedTile = null;
            RefreshAll();
        }
    }

    public void OpenEquipmentSelection(Dweller dw, bool isW) {
        _equippingDweller = dw;
        _isEquippingWeapon = isW;
        _selectedEquipment = null;
        _selectedEquipmentRowBorder = null;

        EquipmentSelectionTitle.Text = isW ? "SELECT WEAPON" : "SELECT OUTFIT";
        EquipmentSelectionList.Children.Clear();

        var vault = _state.ActiveVault;
        if (isW) {
            foreach (var weapon in vault.UnusedWeapons) {
                var row = CreateEquipmentRow(weapon);
                EquipmentSelectionList.Children.Add(row);
            }
        } else {
            foreach (var outfit in vault.UnusedOutfits) {
                var row = CreateEquipmentRow(outfit);
                EquipmentSelectionList.Children.Add(row);
            }
        }
        EquipmentSelectionOverlay.Visibility = Visibility.Visible;
    }

    private UIElement CreateEquipmentRow(ICard item) {
        var border = new Border {
            Background = new SolidColorBrush(Color.FromRgb(0x15, 0x15, 0x30)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0x25, 0x25, 0x45)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Margin = new Thickness(0, 0, 0, 6),
            Padding = new Thickness(8),
            Cursor = Cursors.Hand
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });

        var nameStack = new StackPanel();
        nameStack.Children.Add(new TextBlock { Text = item.Name, Foreground = Brushes.White, FontWeight = FontWeights.Bold, FontSize = 12 });
        var rc = new[] { "#969696", "#64C864", "#6496FA", "#B450DC", "#FFB400" };
        nameStack.Children.Add(new TextBlock { Text = item.Rarity.ToString(), Foreground = (Brush)new BrushConverter().ConvertFromString(rc[(int)item.Rarity])!, FontSize = 9, FontWeight = FontWeights.Bold });
        Grid.SetColumn(nameStack, 0);
        grid.Children.Add(nameStack);

        string statsStr = "";
        if (item is IWeapon w) {
            statsStr = $"Damage: {w.Damage} ({w.WeaponType})";
        } else if (item is IOutfit o) {
            statsStr = $"Armor: {o.ArmorValue}";
            if (o is Outfit op) {
                statsStr += $" (S:{op.S} P:{op.P} E:{op.E} L:{op.L})";
            }
        }
        grid.Children.Add(new TextBlock { Text = statsStr, Foreground = new SolidColorBrush(Color.FromRgb(0xAA, 0xAA, 0xDD)), FontSize = 10, VerticalAlignment = VerticalAlignment.Center });
        Grid.SetColumn(grid.Children[^1], 1);

        border.Child = grid;
        border.MouseDown += (s, e) => {
            if (_selectedEquipmentRowBorder != null) {
                _selectedEquipmentRowBorder.Background = new SolidColorBrush(Color.FromRgb(0x15, 0x15, 0x30));
                _selectedEquipmentRowBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(0x25, 0x25, 0x45));
            }
            border.Background = new SolidColorBrush(Color.FromRgb(0x2A, 0x3A, 0x60));
            border.BorderBrush = new SolidColorBrush(Color.FromRgb(0x55, 0x88, 0xFF));
            _selectedEquipmentRowBorder = border;
            _selectedEquipment = item;
        };

        return border;
    }

    private void OnCancelEquipmentSelection(object s, RoutedEventArgs e) => EquipmentSelectionOverlay.Visibility = Visibility.Collapsed;
    private void OnConfirmEquipmentSelection(object s, RoutedEventArgs e) {
        if (_equippingDweller == null || _selectedEquipment == null) return;
        EquipmentSelectionOverlay.Visibility = Visibility.Collapsed;

        if (_isEquippingWeapon && _selectedEquipment is IWeapon w) {
            _equippingDweller.EquippedWeapon = w;
            _state.AddLog($"{_equippingDweller.Name} equipped {w.Name}");
        } else if (!_isEquippingWeapon && _selectedEquipment is IOutfit o) {
            _equippingDweller.EquippedOutfit = o;
            _state.AddLog($"{_equippingDweller.Name} equipped {o.Name}");
        }
        RefreshAll();
    }

    private void OnAssignmentSlotDragOver(object sender, DragEventArgs e) {
        e.Effects = DragDropEffects.Move;
        e.Handled = true;
    }

    private void OnAssignmentSlotDrop(object sender, DragEventArgs e) {
        var held = heldCard;
        if (held != null && held.BoundDweller != null) {
            _selectedDwellerForAction = held.BoundDweller;
            UpdateAssignmentSlotUI();
            heldCard = null;
        }
    }

    private void OnReturnCardDrop(object sender, DragEventArgs e) {
        var held = heldCard;
        if (held != null) {
            if (held.BoundWeapon != null && held.OwnerDweller != null) {
                held.OwnerDweller.EquippedWeapon = null;
                _state.AddLog($"{held.OwnerDweller.Name} unequipped {held.BoundWeapon.Name}");
                RefreshAll();
                UpdateAssignmentSlotUI();
            }
            else if (held.BoundOutfit != null && held.OwnerDweller != null) {
                held.OwnerDweller.EquippedOutfit = null;
                _state.AddLog($"{held.OwnerDweller.Name} unequipped {held.BoundOutfit.Name}");
                RefreshAll();
                UpdateAssignmentSlotUI();
            }
            heldCard = null;
        }
    }

    public void OnAssignmentSlotDropDirect(ControlCard card) {
        if (card.BoundDweller != null) {
            _dwellerInAssignmentSlot = card.BoundDweller;
            UpdateAssignmentSlotUI();
        }
        else if (_dwellerInAssignmentSlot != null) {
            if (card.BoundWeapon != null) {
                if (card.OwnerDweller != null) card.OwnerDweller.EquippedWeapon = null;
                _dwellerInAssignmentSlot.EquippedWeapon = card.BoundWeapon;
                _state.AddLog($"{_dwellerInAssignmentSlot.Name} equipped {card.BoundWeapon.Name}");
                RefreshAll();
                UpdateAssignmentSlotUI();
            }
            else if (card.BoundOutfit != null) {
                if (card.OwnerDweller != null) card.OwnerDweller.EquippedOutfit = null;
                _dwellerInAssignmentSlot.EquippedOutfit = card.BoundOutfit;
                _state.AddLog($"{_dwellerInAssignmentSlot.Name} equipped {card.BoundOutfit.Name}");
                RefreshAll();
                UpdateAssignmentSlotUI();
            }
        }
    }

    private void UpdateAssignmentSlotUI() {
        AssignmentSlotContainer.Children.Clear();
        if (_dwellerInAssignmentSlot == null) {
            var placeholder = new Border {
                BorderBrush = new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x44)),
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(8),
                Width = 140, Height = 200,
                Background = new SolidColorBrush(Color.FromRgb(0x11, 0x11, 0x22))
            };
            placeholder.Child = new TextBlock {
                Text = "Drag dweller card here",
                Foreground = new SolidColorBrush(Color.FromRgb(0x44, 0x44, 0x55)),
                FontSize = 11, FontStyle = FontStyles.Italic,
                VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center, TextWrapping = TextWrapping.Wrap
            };
            AssignmentSlotContainer.Children.Add(placeholder);
        } else {
            var card = new ControlCard();
            card.BindDweller(_dwellerInAssignmentSlot);
            AssignmentSlotContainer.Children.Add(card);
        }
    }

    private bool IsPointOverElement(FrameworkElement element, Point windowPoint) {
        if (element.Visibility != Visibility.Visible) return false;
        try {
            Point rel = this.TranslatePoint(windowPoint, element);
            return rel.X >= 0 && rel.X <= element.ActualWidth && rel.Y >= 0 && rel.Y <= element.ActualHeight;
        } catch {
            return false;
        }
    }

    private Point _dragStartWinPos;
    public void StartDraggingCard(ControlCard card, Point initialMousePos) {
        _dragStartWinPos = card.TranslatePoint(new Point(0, 0), this);
        _originalCardParent = VisualTreeHelper.GetParent(card) as Panel;
        if (_originalCardParent != null) {
            int idx = _originalCardParent.Children.IndexOf(card);
            _dragPlaceholder = new Border { Width = card.ActualWidth, Height = card.ActualHeight, Margin = card.Margin };
            _originalCardParent.Children.Insert(idx, _dragPlaceholder);
            _originalCardParent.Children.Remove(card);
        }
        
        DragOverlayCanvas.Children.Clear();
        DragOverlayCanvas.Children.Add(card);
        Canvas.SetLeft(card, initialMousePos.X - 70);
        Canvas.SetTop(card, initialMousePos.Y - 100);

        if (card.BoundDweller != null) {
            foreach (HexTileControl tileCtrl in MapCanvas.Children.OfType<HexTileControl>()) {
                if (tileCtrl.BoundTile != null && tileCtrl.BoundTile.IsNavigable) {
                    if (GetDeploymentCost(card.BoundDweller, tileCtrl.BoundTile) <= _state.ActiveVault.ActionPoints) {
                        tileCtrl.SetReachableHighlight(true);
                    }
                }
            }
        }
    }

    private HexTileControl? GetPreciseHoveredTile(Point windowPoint) {
        HexTileControl? bestTile = null;
        double minDistance = double.MaxValue;
        foreach (HexTileControl tileCtrl in MapCanvas.Children.OfType<HexTileControl>()) {
            if (tileCtrl.Visibility != Visibility.Visible || tileCtrl.BoundTile == null) continue;
            try {
                double w = tileCtrl.ActualWidth > 0 ? tileCtrl.ActualWidth : 100;
                double h = tileCtrl.ActualHeight > 0 ? tileCtrl.ActualHeight : 100;
                Point center = tileCtrl.TranslatePoint(new Point(w / 2, h / 2), this);
                double dx = windowPoint.X - center.X;
                double dy = windowPoint.Y - center.Y;
                double dist = Math.Sqrt(dx * dx + dy * dy);
                if (dist < minDistance) {
                    minDistance = dist;
                    bestTile = tileCtrl;
                }
            } catch {}
        }
        if (minDistance < 50) return bestTile;
        return null;
    }

    public void MoveDraggingCard(ControlCard card, Point currentMousePos) {
        double targetScale = 1.0;
        
        if (MapView.Visibility == Visibility.Visible && MapBorder.IsVisible) {
            Point mapRel = MapBorder.TranslatePoint(new Point(0, 0), this);
            double minX = mapRel.X;
            double minY = mapRel.Y;
            double maxX = mapRel.X + MapBorder.ActualWidth;
            double maxY = mapRel.Y + MapBorder.ActualHeight;

            double dx = 0;
            if (currentMousePos.X < minX) dx = minX - currentMousePos.X;
            else if (currentMousePos.X > maxX) dx = currentMousePos.X - maxX;

            double dy = 0;
            if (currentMousePos.Y < minY) dy = minY - currentMousePos.Y;
            else if (currentMousePos.Y > maxY) dy = currentMousePos.Y - maxY;

            double dist = Math.Max(0, Math.Sqrt(dx * dx + dy * dy));

            if (dist < 200) {
                double factor = dist / 200.0;
                double tileScale = 100.0 * (MapScale?.ScaleX ?? 1.0) / 140.0;
                targetScale = tileScale + factor * (1.0 - tileScale);
            }

            var hoveredTile = GetPreciseHoveredTile(currentMousePos);
            foreach (HexTileControl tileCtrl in MapCanvas.Children.OfType<HexTileControl>()) {
                tileCtrl.SetHovered(tileCtrl == hoveredTile);
            }
        }
        
        card.RenderTransform = new ScaleTransform(targetScale, targetScale);
        Canvas.SetLeft(card, currentMousePos.X - (140 * targetScale / 2));
        Canvas.SetTop(card, currentMousePos.Y - (200 * targetScale / 2));
    }

    public void StopDraggingCard(ControlCard card, Point dropPoint) {
        bool handled = false;
        foreach (HexTileControl tileCtrl in MapCanvas.Children.OfType<HexTileControl>()) {
            tileCtrl.SetReachableHighlight(false);
        }

        if (MapView.Visibility == Visibility.Visible) {
            var hoveredTile = GetPreciseHoveredTile(dropPoint);
            if (hoveredTile != null) {
                hoveredTile.SetHovered(false);
                OnCardDroppedOnTile(hoveredTile, card);
                handled = true;
            }
            foreach (HexTileControl tileCtrl in MapCanvas.Children.OfType<HexTileControl>()) {
                tileCtrl.SetHovered(false);
            }
        }
        
        if (!handled && VaultView.Visibility == Visibility.Visible) {
            foreach (VaultCardHolder holder in RoomsPanel.Children.OfType<VaultCardHolder>()) {
                if (IsPointOverElement(holder, dropPoint)) {
                    OnCardAssignedToRoom(holder, card);
                    handled = true;
                    break;
                }
            }
        }
        
        if (!handled && IsPointOverElement(AssignmentSlotContainer, dropPoint)) {
            OnAssignmentSlotDropDirect(card);
            handled = true;
        }
        
        if (!handled && IsPointOverElement(DeckPickerControl, dropPoint)) {
            if (card.BoundDweller != null) {
                foreach (var r in _state.ActiveVault.Rooms) {
                    if (r.AssignedDwellers.Contains(card.BoundDweller)) {
                        r.AssignedDwellers.Remove(card.BoundDweller);
                        handled = true;
                        _state.AddLog($"{card.BoundDweller.Name} returned to Deck");
                        break;
                    }
                }
            }
        }
        
        if (!handled && _originalCardParent != null) {
            double curLeft = Canvas.GetLeft(card);
            if (double.IsNaN(curLeft)) curLeft = _dragStartWinPos.X;
            double curTop = Canvas.GetTop(card);
            if (double.IsNaN(curTop)) curTop = _dragStartWinPos.Y;

            var leftAnim = new DoubleAnimation(curLeft, _dragStartWinPos.X, TimeSpan.FromSeconds(0.4)) { EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut } };
            var topAnim = new DoubleAnimation(curTop, _dragStartWinPos.Y, TimeSpan.FromSeconds(0.4)) { EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut } };
            var scale = card.RenderTransform as ScaleTransform ?? new ScaleTransform(1, 1);
            card.RenderTransform = scale;
            var scaleAnim = new DoubleAnimation(scale.ScaleX, 1.0, TimeSpan.FromSeconds(1)) { EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut } };
            
            leftAnim.Completed += (s, e) => {
                DragOverlayCanvas.Children.Remove(card);
                if (_dragPlaceholder != null && _originalCardParent.Children.Contains(_dragPlaceholder)) {
                    int idx = _originalCardParent.Children.IndexOf(_dragPlaceholder);
                    _originalCardParent.Children.Insert(idx, card);
                    _originalCardParent.Children.Remove(_dragPlaceholder);
                } else if (!_originalCardParent.Children.Contains(card)) {
                    _originalCardParent.Children.Add(card);
                }
                _dragPlaceholder = null;
                card.RenderTransform = new ScaleTransform(1, 1);
                card.BeginAnimation(Canvas.LeftProperty, null);
                card.BeginAnimation(Canvas.TopProperty, null);
            };
            
            card.BeginAnimation(Canvas.LeftProperty, leftAnim);
            card.BeginAnimation(Canvas.TopProperty, topAnim);
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);
            return;
        }
        
        DragOverlayCanvas.Children.Remove(card);
        card.RenderTransform = new ScaleTransform(1, 1);
        if (_dragPlaceholder != null && _originalCardParent != null) {
            if (!handled) {
                int idx = _originalCardParent.Children.IndexOf(_dragPlaceholder);
                if (idx >= 0) _originalCardParent.Children.Insert(idx, card);
                else _originalCardParent.Children.Add(card);
            }
            _originalCardParent.Children.Remove(_dragPlaceholder);
        } else if (handled && _originalCardParent != null) {
            if (_dragPlaceholder != null) _originalCardParent.Children.Remove(_dragPlaceholder);
        }
        _dragPlaceholder = null;

        RefreshAll();
    }
    
    private Point _mapDragStart;
    private Point _mapScrollStart;
    private bool _isDraggingMap;

    private void MapCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
        if (e.OriginalSource is not FrameworkElement el || el.TemplatedParent is not HexTileControl) {
            _isDraggingMap = true;
            _mapDragStart = e.GetPosition(this);
            _mapScrollStart = new Point(MapView.HorizontalOffset, MapView.VerticalOffset);
            MapCanvas.CaptureMouse();
        }
    }
    
    private void MapCanvas_MouseMove(object sender, MouseEventArgs e) {
        if (_isDraggingMap) {
            Point current = e.GetPosition(this);
            double dx = current.X - _mapDragStart.X;
            double dy = current.Y - _mapDragStart.Y;
            MapView.ScrollToHorizontalOffset(_mapScrollStart.X - dx);
            MapView.ScrollToVerticalOffset(_mapScrollStart.Y - dy);
        } else {
            var hoveredTile = GetPreciseHoveredTile(e.GetPosition(this));
            bool isMoveMode = _selectedTile != null && (_state.IsPlayer1Turn ? _selectedTile.BoundTile.Player1Dwellers : _selectedTile.BoundTile.Player2Dwellers).Any(d => d.IsAlive);
            
            foreach (HexTileControl tileCtrl in MapCanvas.Children.OfType<HexTileControl>()) {
                if (hoveredTile != null && isMoveMode && tileCtrl.BoundTile != null) {
                    var activeDwellers = _state.IsPlayer1Turn ? _selectedTile.BoundTile.Player1Dwellers : _selectedTile.BoundTile.Player2Dwellers;
                    var dw = activeDwellers.LastOrDefault(d => d.IsAlive);
                    if (dw != null && GetDeploymentCost(dw, hoveredTile.BoundTile) <= _state.ActiveVault.ActionPoints && hoveredTile.BoundTile.IsNavigable) {
                        int distTotal = GetDeploymentCost(dw, hoveredTile.BoundTile);
                        var startTile = _state.Map.Tiles.Cast<HexTile>().FirstOrDefault(t => t.Player1Dwellers.Contains(dw) || t.Player2Dwellers.Contains(dw));
                        if (startTile == null) {
                            startTile = _state.IsPlayer1Turn ? _state.Map.Get(0, _state.Map.Rows / 2) : _state.Map.Get(_state.Map.Cols - 1, _state.Map.Rows / 2);
                        }

                        bool onPath = false;
                        int distA = GetHexDistance(startTile, tileCtrl.BoundTile);
                        int distB = GetHexDistance(tileCtrl.BoundTile, hoveredTile.BoundTile);
                        if (distA + distB == distTotal) onPath = true;
                        
                        tileCtrl.SetPath(onPath);
                    } else {
                        tileCtrl.SetPath(false);
                    }
                } else {
                    tileCtrl.SetPath(false);
                }
                tileCtrl.SetHovered(tileCtrl == hoveredTile);
            }
        }
    }
    
    private void MapCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
        if (_isDraggingMap) {
            _isDraggingMap = false;
            MapCanvas.ReleaseMouseCapture();
            Point current = e.GetPosition(this);
            double dist = Math.Sqrt(Math.Pow(current.X - _mapDragStart.X, 2) + Math.Pow(current.Y - _mapDragStart.Y, 2));
            if (dist < 5) {
                var clickedTile = GetPreciseHoveredTile(current);
                if (clickedTile != null) OnTileClicked(clickedTile);
            }
        }
    }

    private void MapView_MouseWheel(object sender, MouseWheelEventArgs e) {
        double zoomDelta = e.Delta > 0 ? 0.1 : -0.1;
        double newScale = (MapScale?.ScaleX ?? 1.0) + zoomDelta;
        if (newScale >= 0.5 && newScale <= 2.0 && MapScale != null) {
            MapScale.ScaleX = newScale;
            MapScale.ScaleY = newScale;
        }
        e.Handled = true;
    }
}
