using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using HandyControl.Tools;
using ColorHelper = LaboratoireProgrammation.Project.Helpers.ColorHelper;

namespace LaboratoireProgrammation.Project.ViewModels.Laboratoires.OverseerWars {
    public partial class Wastelands : UserControl {

        private static Team? CurrentTeamView;

        private const int WorldWidth = 100;
        private const int WorldHeight = 100;
        private const double BaseTileSize = 80.0;
        private const double MaxZoom = 8.0;
        private const double MinZoom = 0.5;

        private readonly WorldMap? _worldMap;
        private readonly PlayerMapView? _playerView;

        private Vector _cameraPos = new Vector(0, 0);
        private double _zoom = 1.0;
        private Point _lastMousePos;
        private bool _isDragging;

        private readonly Typeface _typeface = new Typeface("Segoe UI");
        private readonly Pen? _gridPen;
        private readonly Pen? _selectedPen;
        
        private readonly Brush? _discoveredBrush;
        private readonly Brush? _undiscoveredBrush;
        private readonly Brush? _selfDwellerBrush;
        private readonly Brush? _enemyDwellerBrush;
        private readonly Brush? _selectedBrush;
        private readonly Brush? _movePathBrush;

        private readonly DwellerRegistry? _registry;
        private List<Dweller> _deployedDwellers = new List<Dweller>(); 
        private Dweller? _selectedDweller = null!;

        private Tile? _selectedTile = null;
        private Tile? _hoveredTile = null;
        private Tile? _finalTile = null;
        private List<Tile> _currentPath = new List<Tile>();

        private readonly Dictionary<Dweller, UIElement> _dwellerVisuals = new Dictionary<Dweller, UIElement>();
        private readonly DispatcherTimer _hoverTimer = new DispatcherTimer();
        private Dweller? _hoveredDweller = null;
        private UIElement? _activeHoverCard = null;
        private Point _hoverScreenPos;

        private readonly DispatcherTimer _gameLoopTimer = new DispatcherTimer();

        public Wastelands() {
            InitializeComponent();

            if (Visibility == Visibility.Hidden) return;

            _worldMap = new WorldMap(WorldWidth, WorldHeight);
            _playerView = new PlayerMapView(_worldMap);

            _gridPen = new Pen(Brushes.Black, 1.0);
            _gridPen.Freeze();

            _selectedPen = new Pen(Brushes.Yellow, 1.0);
            _selectedPen.Freeze();

            _discoveredBrush = new SolidColorBrush(Color.FromRgb(150, 150, 150));
            _discoveredBrush.Freeze();
            
            _selfDwellerBrush = new SolidColorBrush(Color.FromRgb(86, 166, 224));
            _selfDwellerBrush.Freeze();
            
            _enemyDwellerBrush = new SolidColorBrush(Color.FromRgb(168, 46, 53));
            _enemyDwellerBrush.Freeze();

            _undiscoveredBrush = new SolidColorBrush(Color.FromRgb(30, 30, 30));
            _undiscoveredBrush.Freeze();

            _selectedBrush = new SolidColorBrush(Color.FromRgb(199, 162, 50));
            _selectedBrush.Freeze();
            
            _movePathBrush = new SolidColorBrush(Color.FromArgb(200, 100, 200, 0));
            _movePathBrush.Freeze();
            
            RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
            ClipToBounds = true;
            Focusable = true;

            _hoverTimer.Interval = TimeSpan.FromMilliseconds(1000);
            _hoverTimer.Tick += OnHoverTimerTick!;
            
            _gameLoopTimer.Interval = TimeSpan.FromMilliseconds(1);
            _gameLoopTimer.Tick += OnGameLoopTick!;
            _gameLoopTimer.Start();

            _registry = new DwellerRegistry();
            _registry.Initialize();
            
            var team = new Team(0, "Player");
            var team2 = new Team(0, "Enemy");
            
            CurrentTeamView = team;
            
            var allDwellers = _registry.GetAllDwellers();
            
            var garvey = allDwellers.FirstOrDefault(d => d.FirstName.Equals("Preston", StringComparison.OrdinalIgnoreCase));
            var butch = allDwellers.FirstOrDefault(d => d.FirstName.Equals("Butch", StringComparison.OrdinalIgnoreCase));
            var ghoul = allDwellers.FirstOrDefault(d => d.FirstName.Equals("The", StringComparison.OrdinalIgnoreCase));
            
            garvey.CurrentState = DwellerState.Ally;
            butch.CurrentState = DwellerState.Ally;
            ghoul.CurrentState = DwellerState.Enemy;
            
            team.AddDweller(garvey);
            team.AddDweller(butch);
            team2.AddDweller(ghoul);

            var startTile = _worldMap.GetTile(0, 0); 
                
            if (_playerView.DiscoveredTiles != null && !_playerView.DiscoveredTiles.Contains(startTile)) _playerView.DiscoveredTiles.Add(startTile);
            
            MoveDwellerTo(butch, 0, 0);
            MoveDwellerTo(garvey, 0, 0);
            MoveDwellerTo(ghoul, 0, 2);
            
            _deployedDwellers.Add(garvey);
            _deployedDwellers.Add(butch);
            _deployedDwellers.Add(ghoul);
                
            _cameraPos.X = 0;
            _cameraPos.Y = 0;
            
            Loaded += (s, e) => {
                _playerView.OriginTile = _worldMap.World[0];
                EnforceConstraints();
                InvalidateVisual();
                RefreshDwellerVisuals();
                Focus();
            };

            SizeChanged += (s, e) => {
                EnforceConstraints();
                InvalidateVisual();
                RefreshDwellerVisuals();
            };
        }

        public void MoveDwellerTo(Dweller dweller, int x, int y) {
            dweller.Move(x, y);
            DiscoverTile(x, y);
            if (dweller.P > 3) DiscoverNeighbouringTiles(x, y,1);
            if (dweller.P > 6) DiscoverNeighbouringTiles(x, y,2);
            if (dweller.P > 8) DiscoverNeighbouringTiles(x, y,3);
            if (dweller.P > 11) DiscoverNeighbouringTiles(x, y,4);
            RefreshDwellerVisuals();
            InvalidateVisual();
        }
        
        public void DiscoverTile(int x, int y) {
            var tile = _worldMap.GetTile(x, y);
            if (_playerView.DiscoveredTiles != null && !_playerView.DiscoveredTiles.Contains(tile)) _playerView.DiscoveredTiles.Add(tile);
            RefreshDwellerVisuals();
            InvalidateVisual();
        }

        public void DiscoverNeighbouringTiles(int x, int y, int radius) {
            for (int i = -radius; i <= radius; i++) {
                for (int j = -radius; j <= radius; j++) {
                    if (_worldMap.IsTileInBounds(x + i, y + j)) DiscoverTile(x + i, y + j);
                }
            }
        }
        
        public void ShowMovePath(Dweller dweller) {
            _currentPath.Clear();

            if (dweller == null || _hoveredTile == null || _worldMap == null) return;

            int currentX = dweller.X;
            int currentY = dweller.Y;
            int targetX = _hoveredTile.X;
            int targetY = _hoveredTile.Y;

            if (currentX == targetX && currentY == targetY) return;

            int paRemaining = dweller.IndividuelActionPoints;

            while (paRemaining > 0) {
                if (currentX == targetX && currentY == targetY) break;

                if (currentX < targetX) { currentX++; }
                else if (currentX > targetX) { currentX--; }
                else if (currentY < targetY) { currentY++; }
                else if (currentY > targetY) { currentY--; }

                if (_worldMap.IsTileInBounds(currentX, currentY)) {
                    var nextTile = _worldMap.GetTile(currentX, currentY);
                    _finalTile = nextTile;
                    _currentPath.Add(nextTile);
                }

                paRemaining--;
            }
        }
        
        private void RefreshDwellerVisuals() {
            List<Dweller> toRemove = new List<Dweller>();
            
            foreach (var key in _dwellerVisuals.Keys) {
                if (!_deployedDwellers.Contains(key)) {
                    toRemove.Add(key);
                }
            }
            
            foreach (var dweller in toRemove) {
                RootGrid.Children.Remove(_dwellerVisuals[dweller]);
                _dwellerVisuals.Remove(dweller);
            }

            var dwellersByTile = new Dictionary<string, List<Dweller>>();
            
            foreach (var dweller in _deployedDwellers) {
                string key = $"{dweller.X},{dweller.Y}";
                
                if (!dwellersByTile.ContainsKey(key)) { dwellersByTile[key] = new List<Dweller>(); }
                
                dwellersByTile[key].Add(dweller);
            }

            foreach (var tileGroup in dwellersByTile.Values) {
                int countOnTile = tileGroup.Count;
                
                if (countOnTile > 6) { countOnTile = 6; }

                for (int i = 0; i < tileGroup.Count; i++) {
                    var dweller = tileGroup[i];
                    
                    if (_worldMap == null) {
                        continue;
                    }

                    var tile = _worldMap.GetTile(dweller.X, dweller.Y);
                    
                    if (_playerView != null && _playerView.DiscoveredTiles != null && !_playerView.DiscoveredTiles.Contains(tile)) {
                        if (_dwellerVisuals.ContainsKey(dweller)) {
                            RootGrid.Children.Remove(_dwellerVisuals[dweller]);
                            _dwellerVisuals.Remove(dweller);
                        } 
                        continue;
                    }

                    UIElement visual;
                    
                    if (!_dwellerVisuals.ContainsKey(dweller)) {
                        visual = dweller.CreateCharacterVisual();
                        
                        FrameworkElement fwInit = (FrameworkElement) visual;
                        
                        fwInit.HorizontalAlignment = HorizontalAlignment.Left;
                        fwInit.VerticalAlignment = VerticalAlignment.Top;
                        
                        Panel.SetZIndex(visual, 120);

                        var capturedDweller = dweller;

                        visual.MouseEnter += (s, e) => {
                            _hoveredDweller = capturedDweller;
                            _hoverScreenPos = e.GetPosition(RootGrid);
                            _hoverTimer.Stop();
                            _hoverTimer.Start();
                        };

                        visual.MouseLeave += (s, e) => {
                            _hoverTimer.Stop();
                            _hoveredDweller = null;
                            RemoveHoverCard();
                        };

                        visual.MouseMove += (s, e) => {
                            _hoverScreenPos = e.GetPosition(RootGrid);
                        };
                        
                        visual.MouseDown += (s, e) => {
                            if (dweller.Team != CurrentTeamView) return;
                            var oldSelected = _selectedDweller;
                            
                            if (oldSelected != null && _dwellerVisuals.ContainsKey(oldSelected)) {
                                RootGrid.Children.Remove(_dwellerVisuals[oldSelected]);
                                _dwellerVisuals.Remove(oldSelected);
                                if (oldSelected.Team == CurrentTeamView) { oldSelected.CurrentState = DwellerState.Ally; }
                                else { oldSelected.CurrentState = DwellerState.Enemy; }
                            }
                            
                            dweller.CurrentState = DwellerState.Selected;
                            _selectedDweller = dweller;
                            _selectedTile = _worldMap.GetTile(dweller.X, dweller.Y);
                                
                            if (oldSelected != null && _dwellerVisuals.ContainsKey(oldSelected)) {
                                RootGrid.Children.Remove(_dwellerVisuals[oldSelected]);
                                _dwellerVisuals.Remove(oldSelected);
                            }

                            if (_dwellerVisuals.ContainsKey(_selectedDweller)) {
                                RootGrid.Children.Remove(_dwellerVisuals[_selectedDweller]);
                                _dwellerVisuals.Remove(_selectedDweller);
                            }

                            RefreshDwellerVisuals();
                            InvalidateVisual();
    
                            e.Handled = true; 
                        };

                        RootGrid.Children.Add(visual);
                        _dwellerVisuals.Add(dweller, visual);
                    }
                    
                    visual = _dwellerVisuals[dweller];

                    double currentTileSize = BaseTileSize * _zoom;
                    
                    double baseScreenX = (dweller.X * BaseTileSize - _cameraPos.X) * _zoom;
                    double baseScreenY = (dweller.Y * BaseTileSize - _cameraPos.Y) * _zoom;
                    
                    double finalScreenX = baseScreenX;
                    double finalScreenY = baseScreenY;
                    double finalSize = currentTileSize;

                    if (countOnTile > 1 && i < 6) {

                        int cols = 2;
                        if (countOnTile > 3) { cols = 3; }
                        
                        int rows = 2; 

                        double newSize = currentTileSize / cols;
                        
                        int colPos = i % cols;
                        int rowPos = i / cols;

                        finalScreenX = baseScreenX + (colPos * newSize);
                        finalScreenY = baseScreenY + (rowPos * newSize);
                        finalSize = newSize;
                    }

                    FrameworkElement fwVisual = (FrameworkElement)visual;
                    fwVisual.Width = finalSize - 4;
                    fwVisual.Height = finalSize - 4;
                    
                    if (i < 6) {
                        visual.Visibility = Visibility.Visible;
                        visual.RenderTransform = new TranslateTransform(finalScreenX, finalScreenY);
                    }
                    else { visual.Visibility = Visibility.Collapsed; }
                }
            }
        }

        private void OnHoverTimerTick(object sender, EventArgs e) {
            _hoverTimer.Stop();
            
            RemoveHoverCard();

            if (_hoveredDweller != null) {
                var card = _hoveredDweller.CreateCardVisual();
            
                double cardHeight = ActualHeight * 0.40; 
                double cardWidth = cardHeight * (400.0 / 600.0);

                FrameworkElement fwCard = (FrameworkElement)card;
            
                fwCard.HorizontalAlignment = HorizontalAlignment.Left;
                fwCard.VerticalAlignment = VerticalAlignment.Top;
            
                fwCard.Width = cardWidth;
                fwCard.Height = cardHeight;

                card.RenderTransform = new TranslateTransform(_hoverScreenPos.X + 20, _hoverScreenPos.Y + 20);
                Panel.SetZIndex(card, 300);

                _activeHoverCard = card;
                RootGrid.Children.Add(card);
            }
        }

        private void RemoveHoverCard() {
            RootGrid.Children.Remove(_activeHoverCard);
            _activeHoverCard = null;
        }

        private void EnforceConstraints() {
            if (ActualWidth == 0 || ActualHeight == 0) { return; }

            double totalMapWidth = WorldWidth * BaseTileSize;
            double totalMapHeight = WorldHeight * BaseTileSize;

            double minZoomX = ActualWidth / totalMapWidth;
            double minZoomY = ActualHeight / totalMapHeight;
            double dynamicMinZoom = Math.Max(minZoomX, minZoomY);

            _zoom = Math.Clamp(_zoom, dynamicMinZoom, MaxZoom);
            _zoom = Math.Clamp(_zoom, MinZoom, MaxZoom);

            double viewWidth = ActualWidth / _zoom;
            double viewHeight = ActualHeight / _zoom;

            double maxCamX = totalMapWidth - viewWidth;
            double maxCamY = totalMapHeight - viewHeight;

            _cameraPos.X = Math.Clamp(_cameraPos.X, 0, Math.Max(0, maxCamX));
            _cameraPos.Y = Math.Clamp(_cameraPos.Y, 0, Math.Max(0, maxCamY));
        }

        protected override void OnMouseDown(MouseButtonEventArgs e) {
            Focus();

            if (e.ChangedButton == MouseButton.Middle || e.ChangedButton == MouseButton.Right) {
                _isDragging = true;
                _lastMousePos = e.GetPosition(this);
                CaptureMouse();
            }
            
            else if (e.ChangedButton == MouseButton.Left) {
                var dweller = _selectedDweller;
                if (_selectedDweller != null && _finalTile == _hoveredTile && _finalTile != null) { MoveDwellerTo(dweller, _finalTile.X, _finalTile.Y); }
                if (_selectedDweller != null && _finalTile != _hoveredTile) { _selectedDweller = null; _selectedTile = null; _currentPath.Clear(); }
                HandleClick(e.GetPosition(this));
            }
            
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            var currentPos = e.GetPosition(this);

            if (_isDragging) {
                var delta = _lastMousePos - currentPos;

                _cameraPos.X += delta.X / _zoom;
                _cameraPos.Y += delta.Y / _zoom;
        
                _lastMousePos = currentPos;
        
                EnforceConstraints();
                InvalidateVisual();
                RefreshDwellerVisuals();
            }

            double worldMouseX = _cameraPos.X + (currentPos.X / _zoom);
            double worldMouseY = _cameraPos.Y + (currentPos.Y / _zoom);

            int tileX = (int)Math.Floor(worldMouseX / BaseTileSize);
            int tileY = (int)Math.Floor(worldMouseY / BaseTileSize);

            _hoveredTile = _worldMap.GetTile(tileX, tileY);

            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e) {
            if (_isDragging) {
                _isDragging = false;
                ReleaseMouseCapture();
            } base.OnMouseUp(e);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e) {
            var mousePos = e.GetPosition(this);
            double zoomFactor = 1.0;
            
            if (e.Delta > 0) { zoomFactor = 1.2; }
            else { zoomFactor = 0.8; }
            
            double oldZoom = _zoom;

            _zoom = Math.Clamp(_zoom * zoomFactor, MinZoom, MaxZoom);

            if (Math.Abs(_zoom - oldZoom) > 0.0001) {
                Vector mouseWorldPos = (Vector)mousePos / oldZoom + _cameraPos;
                _cameraPos = mouseWorldPos - (Vector)mousePos / _zoom;
                
                EnforceConstraints();
                InvalidateVisual();
                RefreshDwellerVisuals();
            }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e) {
            double step = BaseTileSize / _zoom;
            bool handled = true;

            if (e.Key == Key.W || e.Key == Key.Up) { _cameraPos.Y -= step; }
            else if (e.Key == Key.S || e.Key == Key.Down) { _cameraPos.Y += step; }
            else if (e.Key == Key.A || e.Key == Key.Left) { _cameraPos.X -= step; }
            else if (e.Key == Key.D || e.Key == Key.Right) { _cameraPos.X += step; }
            else if (e.Key == Key.Escape) { _selectedDweller = null; _selectedTile = null; _currentPath.Clear(); }
            else { handled = false; }

            if (handled) {
                EnforceConstraints();
                InvalidateVisual();
                RefreshDwellerVisuals();
                e.Handled = true;
            }
            base.OnPreviewKeyDown(e);
        }

        private void HandleClick(Point screenPos) {
            Vector worldPos = (Vector)screenPos / _zoom + _cameraPos;
            int x = (int)Math.Floor(worldPos.X / BaseTileSize);
            int y = (int)Math.Floor(worldPos.Y / BaseTileSize);

            if (x >= 0 && x < WorldWidth && y >= 0 && y < WorldHeight) {
                if (_worldMap != null) {
                    var tile = _worldMap.GetTile(x, y);
                    
                    _selectedTile = tile;
                }

                InvalidateVisual();
                RefreshDwellerVisuals();
            }
        }

        protected override void OnRender(DrawingContext dc) {
            dc.DrawRectangle(Brushes.Black, null, new Rect(0, 0, ActualWidth, ActualHeight));

            if (ActualWidth == 0 || ActualHeight == 0) {
                return;
            }

            double currentTileSize = BaseTileSize * _zoom;

            int startX = (int)Math.Floor(_cameraPos.X / BaseTileSize);
            int startY = (int)Math.Floor(_cameraPos.Y / BaseTileSize);

            int visibleCols = (int)Math.Ceiling(ActualWidth / currentTileSize) + 1;
            int visibleRows = (int)Math.Ceiling(ActualHeight / currentTileSize) + 1;

            double pixelOffsetX = (_cameraPos.X % BaseTileSize) * _zoom;
            double pixelOffsetY = (_cameraPos.Y % BaseTileSize) * _zoom;

            for (int y = 0; y <= visibleRows; y++) {
                for (int x = 0; x <= visibleCols; x++) {
                    int tileX = startX + x;
                    int tileY = startY + y;

                    if (tileX < 0 || tileX >= WorldWidth || tileY < 0 || tileY >= WorldHeight) {
                        continue;
                    }

                    double drawX = Math.Floor((x * currentTileSize) - pixelOffsetX);
                    double drawY = Math.Floor((y * currentTileSize) - pixelOffsetY);
                    double drawSize = Math.Ceiling(currentTileSize);

                    Rect rect = new Rect(drawX, drawY, drawSize, drawSize);

                    var tile = _worldMap.GetTile(tileX, tileY);
                    bool discovered = _playerView.DiscoveredTiles != null && _playerView.DiscoveredTiles.Contains(tile);
                    bool isSelected = false;

                    bool isPathTile = false;
                    
                    if (tile == _selectedTile) isSelected = true;
                    if (_currentPath.Contains(tile)) isPathTile = true;


                    
                    Brush tileBrush = _undiscoveredBrush!;
                    
                    if (discovered) { tileBrush = _discoveredBrush!; }
                    if (isSelected) { tileBrush = _selectedBrush!; }

                    if (isPathTile) { tileBrush = _movePathBrush!; }

                    if (_hoveredTile == tile) { tileBrush = ColorHelper.GetLighter(tileBrush, 10); }
                    
                    Pen tilePen = _gridPen!;
                    if (isSelected) tilePen = _selectedPen!;

                    dc.DrawRectangle(tileBrush, tilePen, rect);

                    if (currentTileSize > 25 && discovered) {
                        var text = new FormattedText(
                            $"{tileX},{tileY}",
                            CultureInfo.InvariantCulture,
                            FlowDirection.LeftToRight,
                            _typeface,
                            currentTileSize * 0.1,
                            Brushes.White,
                            VisualTreeHelper.GetDpi(this).PixelsPerDip);

                        dc.DrawText(text, new Point(drawX + 2, drawY + 2));
                    }
                }
            }
        }
        
        private void OnGameLoopTick(object sender, EventArgs e) {
            InvalidateVisual();
            RefreshDwellerVisuals();
            if (_hoveredTile != null && _selectedDweller != null) ShowMovePath(_selectedDweller);
        }
        
    }
}