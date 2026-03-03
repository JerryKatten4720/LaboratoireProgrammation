namespace LaboratoireProgrammation.Project.ViewModels.Laboratoires.OverseerWars;

public class WorldMap {
    public PlayerMapView? CurrentView;
    
    public int Height;
    public int Width;
    public List<Tile> World;

    public WorldMap(int H, int W) {
        Height = H;
        Width = W;
        
        World = new List<Tile>();
        CurrentView = null;
        for (int i = 0; i < H; i++) {
            for (int j = 0; j < W; j++) {
                World.Add(new Tile { X = i, Y = j });
            }
        }
    }
    public Tile GetTile(int x, int y) {
        return World[x * Width + y];
    }
    public bool IsTileInBounds(int x, int y) {
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }
    
}

public class Tile {
    public int X;
    public int Y;
}

public class PlayerMapView {
    public int RowVisibility;
    public int ColumnVisibility;
    
    public Tile? OriginTile;
    public List<Tile>? DiscoveredTiles;
    public List<Tile>? VisibleTiles;

    public PlayerMapView(WorldMap world) {
        OriginTile = world.World[0];
        RowVisibility = 5;
        ColumnVisibility = 5;
        DiscoveredTiles = new List<Tile>();
        VisibleTiles = new List<Tile>();
        UpdateView(world);
    }
    
    public void UpdateView(WorldMap world) {
        VisibleTiles?.Clear();
        for (int i = 0; i < RowVisibility; i++) {
            for (int j = 0; j < ColumnVisibility; j++) {
                VisibleTiles?.Add(world.World[i * world.Width + j]);
            }
        }
    }
}