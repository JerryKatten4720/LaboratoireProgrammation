using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace LaboratoireProgrammation.Project.ModernOverseerWars;

public partial class ControlCard : UserControl {

    public static readonly DependencyProperty CardImageProperty =
        DependencyProperty.Register(nameof(CardImage), typeof(string), typeof(ControlCard), new PropertyMetadata("Card"));
    public static readonly DependencyProperty PowerProperty =
        DependencyProperty.Register(nameof(Power), typeof(int), typeof(ControlCard), new PropertyMetadata(0));

    public string CardImage {
        get => (string)GetValue(CardImageProperty);
        set => SetValue(CardImageProperty, value);
    }
    public int Power {
        get => (int)GetValue(PowerProperty);
        set => SetValue(PowerProperty, value);
    }

    public Dweller?  BoundDweller { get; private set; }
    public IWeapon?  BoundWeapon  { get; private set; }
    public IOutfit?  BoundOutfit  { get; private set; }
    public Dweller?  OwnerDweller { get; set; }

    private ControlCard? _weaponCard;
    private ControlCard? _outfitCard;

    private static readonly string[] RarityNames = { "common","uncommon","rare","epic","legendary" };
    private System.Windows.Threading.DispatcherTimer? _hoverTimer;
    private bool _isExpanded;
    private Point _dragStartPoint;

    public static ControlCard? DraggedCard { get; private set; }
    private Point _dragStartMousePos;
    private TranslateTransform _translateTransform = new();
    private bool _isDragging;

    public ControlCard() { InitializeComponent(); }

    public void BindDweller(Dweller d) {
        BoundDweller = d;
        CardImage    = d.Name;
        SupervisorBadge.Visibility = d.IsSupervisor ? Visibility.Visible : Visibility.Collapsed;
        StatsLine.Text = $"S:{d.Special_S} P:{d.Special_P} E:{d.Special_E} C:{d.Special_C}";
        HpBar.Maximum  = d.MaxHp;
        RefreshHp();
        ApplyRarity(d.Rarity);
        ImgDweller.Source = MakeImageSource($"../../Assets/images/overseerWars/dwellers/{d.Texture}") ?? MakeImageSource("../../Assets/images/overseerWars/dwellers/dweller.png");

        if (d.EquippedWeapon != null) {
            WeaponCardContainer.Visibility = Visibility.Visible;
            if (_weaponCard == null) {
                _weaponCard = new ControlCard { Width = 110, Height = 157 };
                WeaponCardContainer.Child = _weaponCard;
            }
            _weaponCard.BindWeapon(d.EquippedWeapon);
            _weaponCard.OwnerDweller = d;
        } else {
            WeaponCardContainer.Visibility = Visibility.Collapsed;
            WeaponCardContainer.Child = null;
            _weaponCard = null;
        }

        if (d.EquippedOutfit != null) {
            OutfitCardContainer.Visibility = Visibility.Visible;
            if (_outfitCard == null) {
                _outfitCard = new ControlCard { Width = 110, Height = 157 };
                OutfitCardContainer.Child = _outfitCard;
            }
            _outfitCard.BindOutfit(d.EquippedOutfit);
            _outfitCard.OwnerDweller = d;
        } else {
            OutfitCardContainer.Visibility = Visibility.Collapsed;
            OutfitCardContainer.Child = null;
            _outfitCard = null;
        }

        Canvas.SetLeft(WeaponCardContainer, 5);
        Canvas.SetTop(WeaponCardContainer, 10);
        Canvas.SetLeft(OutfitCardContainer, 25);
        Canvas.SetTop(OutfitCardContainer, 10);
    }

    public void BindWeapon(IWeapon w) {
        BoundWeapon  = w;
        CardImage    = w.Name;
        StatsLine.Text = $"DMG:{w.Damage} ({w.WeaponType[..Math.Min(3, w.WeaponType.Length)]})";
        HpBar.Visibility = Visibility.Collapsed;
        HpText.Visibility = Visibility.Collapsed;
        ApplyRarity(w.Rarity);
        string tex = w is Weapon wp ? wp.Texture : "placeholder.png";
        ImgDweller.Source = MakeImageSource($"../../Assets/images/overseerWars/weapons/{tex}") ?? MakeImageSource("../../Assets/images/overseerWars/weapons/placeholder.png");
    }

    public void BindOutfit(IOutfit o) {
        BoundOutfit  = o;
        CardImage    = o.Name;
        StatsLine.Text = $"ARM:{o.ArmorValue}";
        HpBar.Visibility  = Visibility.Collapsed;
        HpText.Visibility = Visibility.Collapsed;
        ApplyRarity(o.Rarity);
        string tex = o is Outfit op ? op.Texture : "placeholder.png";
        ImgDweller.Source = MakeImageSource($"../../Assets/images/overseerWars/outfits/{tex}") ?? MakeImageSource("../../Assets/images/overseerWars/outfits/placeholder.png");
    }

    public Scrap? BoundScrap { get; private set; }
    public void BindScrap(Scrap s) {
        BoundScrap = s;
        CardImage = s.Name;
        StatsLine.Text = "Junk item";
        HpBar.Visibility = Visibility.Collapsed;
        HpText.Visibility = Visibility.Collapsed;
        ApplyRarity(s.Rarity);
        ImgDweller.Source = MakeImageSource($"../../Assets/images/overseerWars/scraps/{s.Texture}") ?? MakeImageSource("../../Assets/images/overseerWars/weapons/placeholder.png");
    }

    public void RefreshHp() {
        if (BoundDweller == null) return;
        HpBar.Value = BoundDweller.CurrentHp;
        HpText.Text = $"{BoundDweller.CurrentHp}/{BoundDweller.MaxHp}";
        double ratio = (double)BoundDweller.CurrentHp / BoundDweller.MaxHp;
        HpBar.Foreground = ratio > 0.5
            ? new SolidColorBrush(Color.FromRgb(34, 187, 68))
            : new SolidColorBrush(Color.FromRgb(200, 60, 60));
    }

    private void ApplyRarity(CardRarity rarity) {
        string r = RarityNames[(int)rarity];
        string base_ = "../../Assets/images/overseerWars/cards/";
        ImgBorder.Source = MakeImageSource($"{base_}card_border_{r}.png");
        ImgColor.Source  = MakeImageSource($"{base_}card_color_{r}.png");
        ImgTop.Source    = MakeImageSource($"{base_}card_top_{r}.png");
    }
    
    private ImageSource MakeImageSource(string relativePath) {
        try {
            string cleaned = relativePath.Replace("../", "").Replace("..\\", "");
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string absPath = "";
            while (!string.IsNullOrEmpty(baseDir)) {
                string test = Path.Combine(baseDir, cleaned);
                if (File.Exists(test)) { absPath = test; break; }
                baseDir = Path.GetDirectoryName(baseDir)!;
            }
            if (string.IsNullOrEmpty(absPath)) {
                absPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, cleaned);
            }
            if (File.Exists(absPath)) {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(absPath, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                return bitmap;
            }
            return null!;
        }
        catch {
            return null!;
        }
    }

    private bool _isDeckMode;
    public void SetDeckMode(bool deckMode) {
        _isDeckMode = deckMode;
        if (deckMode) {
            RenderTransform = new ScaleTransform(0.9, 0.9);
            RenderTransformOrigin = new Point(0.5, 0.5);

            EquipmentCanvas.Children.Remove(WeaponCardContainer);
            EquipmentCanvas.Children.Remove(OutfitCardContainer);
            RootGrid.Children.Add(WeaponCardContainer);
            RootGrid.Children.Add(OutfitCardContainer);
            WeaponCardContainer.HorizontalAlignment = HorizontalAlignment.Center;
            WeaponCardContainer.VerticalAlignment = VerticalAlignment.Center;
            OutfitCardContainer.HorizontalAlignment = HorizontalAlignment.Center;
            OutfitCardContainer.VerticalAlignment = VerticalAlignment.Center;
            WeaponCardContainer.Margin = new Thickness(0);
            OutfitCardContainer.Margin = new Thickness(0);
            if (WeaponCardContainer.RenderTransform is RotateTransform rw) rw.Angle = 0;
            if (OutfitCardContainer.RenderTransform is RotateTransform ro) ro.Angle = 0;
            
            Panel.SetZIndex(MainCardGrid, 3);
            Panel.SetZIndex(WeaponCardContainer, 2);
            Panel.SetZIndex(OutfitCardContainer, 1);
        }
    }

    private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e) {
        if (!_isDeckMode || BoundDweller == null) return;
        var elements = new List<UIElement> { MainCardGrid };
        if (BoundDweller.EquippedWeapon != null) elements.Add(WeaponCardContainer);
        if (BoundDweller.EquippedOutfit != null) elements.Add(OutfitCardContainer);
        if (elements.Count <= 1) return;

        var sorted = elements.OrderByDescending(el => Panel.GetZIndex(el)).ToList();
        if (e.Delta < 0) {
            var top = sorted.First();
            Panel.SetZIndex(top, -1);
            foreach(var el in elements) if(el != top) Panel.SetZIndex(el, Panel.GetZIndex(el) + 1);
        } else {
            var bottom = sorted.Last();
            Panel.SetZIndex(bottom, 10);
            foreach(var el in elements) if(el != bottom) Panel.SetZIndex(el, Panel.GetZIndex(el) - 1);
        }
        e.Handled = true;
    }

    private void OnMouseEnter(object s, MouseEventArgs e) {
        Panel.SetZIndex(this, 1000);
        RenderTransformOrigin = new Point(0.5, 0.5);
        var scale = new ScaleTransform(_isDeckMode ? 0.96 : 1.06, _isDeckMode ? 0.96 : 1.06);
        RenderTransform = scale;

        if (_isDeckMode) return;

        if (BoundDweller != null) {
            if (_hoverTimer == null) {
                _hoverTimer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(0.5) };
                _hoverTimer.Tick += (st, ev) => {
                    ExpandEquipment();
                    _hoverTimer.Stop();
                };
            }
            _hoverTimer.Start();
        }
    }

    private void OnMouseLeave(object s, MouseEventArgs e) {
        Panel.SetZIndex(this, 0);
        var scale = new ScaleTransform(_isDeckMode ? 0.9 : 1.0, _isDeckMode ? 0.9 : 1.0);
        RenderTransform = scale;
        _hoverTimer?.Stop();
        if (!_isDeckMode) SlideDownEquipment();
    }

    public void ExpandEquipment() {
        if (_isExpanded) return;
        _isExpanded = true;
        EquipmentPopup.IsOpen = true;
        var animLeftW = new DoubleAnimation(-115, TimeSpan.FromMilliseconds(200));
        var animTopW = new DoubleAnimation(20, TimeSpan.FromMilliseconds(200));
        var rotW = new DoubleAnimation(0, TimeSpan.FromMilliseconds(200));
        var animLeftO = new DoubleAnimation(145, TimeSpan.FromMilliseconds(200));
        var animTopO = new DoubleAnimation(20, TimeSpan.FromMilliseconds(200));
        var rotO = new DoubleAnimation(0, TimeSpan.FromMilliseconds(200));

        WeaponCardContainer.BeginAnimation(Canvas.LeftProperty, animLeftW);
        WeaponCardContainer.BeginAnimation(Canvas.TopProperty, animTopW);
        if (WeaponCardContainer.RenderTransform is RotateTransform rW) rW.BeginAnimation(RotateTransform.AngleProperty, rotW);

        OutfitCardContainer.BeginAnimation(Canvas.LeftProperty, animLeftO);
        OutfitCardContainer.BeginAnimation(Canvas.TopProperty, animTopO);
        if (OutfitCardContainer.RenderTransform is RotateTransform rO) rO.BeginAnimation(RotateTransform.AngleProperty, rotO);
    }

    private void SlideDownEquipment() {
        if (!_isExpanded) return;
        _isExpanded = false;
        var animLeftW = new DoubleAnimation(5, TimeSpan.FromMilliseconds(200));
        var animTopW = new DoubleAnimation(10, TimeSpan.FromMilliseconds(200));
        var rotW = new DoubleAnimation(-5, TimeSpan.FromMilliseconds(200));
        var animLeftO = new DoubleAnimation(25, TimeSpan.FromMilliseconds(200));
        var animTopO = new DoubleAnimation(10, TimeSpan.FromMilliseconds(200));
        var rotO = new DoubleAnimation(5, TimeSpan.FromMilliseconds(200));

        WeaponCardContainer.BeginAnimation(Canvas.LeftProperty, animLeftW);
        WeaponCardContainer.BeginAnimation(Canvas.TopProperty, animTopW);
        if (WeaponCardContainer.RenderTransform is RotateTransform rW) rW.BeginAnimation(RotateTransform.AngleProperty, rotW);

        OutfitCardContainer.BeginAnimation(Canvas.LeftProperty, animLeftO);
        OutfitCardContainer.BeginAnimation(Canvas.TopProperty, animTopO);
        if (OutfitCardContainer.RenderTransform is RotateTransform rO) {
            rO.BeginAnimation(RotateTransform.AngleProperty, rotO);
        }
        
        var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
        timer.Tick += (st, ev) => { EquipmentPopup.IsOpen = false; timer.Stop(); };
        timer.Start();
    }

    private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
        _dragStartPoint = e.GetPosition(null);
        OverseerWarsWindow.heldCard = this;
    }

    private void OnPreviewMouseMove(object sender, MouseEventArgs e) {
        if (e.LeftButton == MouseButtonState.Pressed && !_isDragging && OverseerWarsWindow.heldCard == this) {
            Point pos = e.GetPosition(null);
            if (Math.Abs(pos.X - _dragStartPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(pos.Y - _dragStartPoint.Y) > SystemParameters.MinimumVerticalDragDistance) {
                _isDragging = true;
                DraggedCard = this;
                var win = Window.GetWindow(this) as OverseerWarsWindow;
                if (win != null) {
                    Point winPos = e.GetPosition(win);
                    win.StartDraggingCard(this, winPos);
                }
                CaptureMouse();
            }
        }

        if (_isDragging && IsMouseCaptured) {
            var win = Window.GetWindow(this) as OverseerWarsWindow;
            if (win != null) {
                Point winPos = e.GetPosition(win);
                win.MoveDraggingCard(this, winPos);
            }
        }
    }

    private void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
        if (_isDragging) {
            _isDragging = false;
            DraggedCard = null;
            ReleaseMouseCapture();
            var win = Window.GetWindow(this) as OverseerWarsWindow;
            if (win != null) {
                Point winPos = e.GetPosition(win);
                win.StopDraggingCard(this, winPos);
            }
        }
    }

    private void OnRightClick(object sender, MouseButtonEventArgs e) {
        if (BoundDweller == null) return;
        e.Handled = true;
        var win = Window.GetWindow(this) as OverseerWarsWindow;
        if (win == null) return;

        var cm = new ContextMenu();
        var miWeapon = new MenuItem { Header = "Assign Weapon" };
        miWeapon.Click += (s, ev) => win.OpenEquipmentSelection(BoundDweller, true);
        var miOutfit = new MenuItem { Header = "Assign Outfit" };
        miOutfit.Click += (s, ev) => win.OpenEquipmentSelection(BoundDweller, false);

        cm.Items.Add(miWeapon);
        cm.Items.Add(miOutfit);
        cm.IsOpen = true;
    }
}
