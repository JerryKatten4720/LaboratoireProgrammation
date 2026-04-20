namespace LaboratoireProgrammation.Project.ViewModels.Laboratoires.OverseerWars;

public class WorldMap {
    public readonly int Height;
    public readonly int Width;
    public readonly List<Tile> World;
    public PlayerMapView? CurrentView;

    public WorldMap(int width, int height) {
        Width = width;
        Height = height;
        World = new List<Tile>(width * height);
        for (var x = 0; x < height; x++)
        for (var y = 0; y < width; y++)
            World.Add(new Tile { X = x, Y = y });
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
    public int ColumnVisibility = 5;
    public List<Tile> DiscoveredTiles = new();
    public Tile? OriginTile;
    public int RowVisibility = 5;
    public List<Tile> VisibleTiles = new();

    public PlayerMapView(WorldMap world) {
        OriginTile = world.World[0];
        UpdateView(world);
    }

    public void UpdateView(WorldMap world) {
        VisibleTiles.Clear();
        for (var i = 0; i < RowVisibility; i++)
        for (var j = 0; j < ColumnVisibility; j++)
            VisibleTiles.Add(world.World[i * world.Width + j]);
    }
}