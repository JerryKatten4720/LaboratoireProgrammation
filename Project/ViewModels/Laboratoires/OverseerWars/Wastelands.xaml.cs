using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LaboratoireProgrammation.Project.ViewModels.Laboratoires.OverseerWars
{
    public partial class Wastelands : UserControl {
        private int _gridHeight = 50;
        private int _gridWidth = 50;
        private WorldMap _worldMap = new WorldMap(50, 50);
        private PlayerMapView _playerView;

        private Dictionary<(int x, int y), Dweller> dwellersMap = new();
        public Wastelands() {
            InitializeComponent();
            this.Focusable = true;
            Loaded += (s, e) => {
                _playerView = new PlayerMapView(_worldMap);
                _playerView.OriginTile = _worldMap.World[0];
                _playerView.ColumnVisibility = 5;
                _playerView.RowVisibility = 5;

                uniformGrid1.Rows = _playerView.RowVisibility;
                uniformGrid1.Columns = _playerView.ColumnVisibility;

                var d1 = new Dweller("Place", "Holder", 1, 1, 1, 1, 1, 1, 1);
                d1.Move(2, 3);
                dwellersMap[(d1.X, d1.Y)] = d1;


                GridView(_playerView);
                this.Focus();
            };

            PreviewKeyDown += (s, e) => {
                if (_playerView == null) return;

                int currentX = _playerView.OriginTile.X;
                int currentY = _playerView.OriginTile.Y;
                bool moved = false;
                
                switch (e.Key) {
                    case Key.Z: case Key.Up:
                        MoveCamera(currentX, currentY - 1);
                        moved = true;
                        break;
                    case Key.S: case Key.Down:
                        MoveCamera(currentX, currentY + 1);
                        moved = true;
                        break;
                    case Key.Q: case Key.Left:
                        MoveCamera(currentX - 1, currentY);
                        moved = true;
                        break;
                    case Key.D: case Key.Right:
                        MoveCamera(currentX + 1, currentY);
                        moved = true;
                        break;
                    
                    case Key.M:
                        Dezoom();
                        break;
                    case Key.L:
                        Zoom();
                        break;
                    
                    case Key.K:
                        UpdateDwellerPosition();
                        break;
                    
                } if (moved) { e.Handled = true; }
                
            };
        }

        private void MoveCamera(int newX, int newY) {
            if (newX >= 0 && newX <= _gridWidth - _playerView.ColumnVisibility &&
                newY >= 0 && newY <= _gridHeight - _playerView.RowVisibility) {
                _playerView.OriginTile = _worldMap.GetTile(newX, newY);
                GridView(_playerView);
            }
        }
        
        private void Dezoom() {
            _playerView.ColumnVisibility++;
            _playerView.RowVisibility++;
            uniformGrid1.Rows = _playerView.RowVisibility;
            uniformGrid1.Columns = _playerView.ColumnVisibility;
            _playerView.UpdateView(_worldMap);
            GridView(_playerView);
        }
        
        private void Zoom() {
            _playerView.ColumnVisibility--;
            _playerView.RowVisibility--;
            uniformGrid1.Rows = _playerView.RowVisibility;
            uniformGrid1.Columns = _playerView.ColumnVisibility;
            _playerView.UpdateView(_worldMap);
            GridView(_playerView);
        }

        private void GridView(PlayerMapView view) {
            uniformGrid1.Children.Clear();

            for (int y = 0; y < view.RowVisibility; y++) {
                for (int x = 0; x < view.ColumnVisibility; x++) {
                    
                    int worldX = view.OriginTile.X + x;
                    int worldY = view.OriginTile.Y + y;

                    var currentTile = _worldMap.GetTile(worldX, worldY);

                    Button btn = new Button();
                    btn.Content = $"{worldX}.{worldY}";

                    if (!view.DiscoveredTiles.Contains(currentTile)) { btn.Background = Brushes.Gray; }
                    else { btn.Background = Brushes.Green; }

                    if (dwellersMap.TryGetValue((worldX, worldY), out var dweller)) {
                        btn.Content = $"{worldX}.{worldY} ({dweller.FirstName})";
                    }
                    
                    btn.Click += (s, e) => {
                        if (!view.DiscoveredTiles.Contains(currentTile)) {
                            view.DiscoveredTiles.Add(currentTile);
                            GridView(view);
                            Focus();
                        }
                    };
                    uniformGrid1.Children.Add(btn);
                }
            }
            Focus();
        }
        
        public void UpdateDwellerPosition(Dweller dweller, int newX, int newY) {
            dwellersMap.Remove((dweller.X, dweller.Y));
            dweller.Move(newX, newY);
            dwellersMap[(newX, newY)] = dweller;
        }
        
    }
}