namespace LaboratoireProgrammation.Project.ModernOverseerWars;

public enum TileType { Empty, Location, Player1Vault, Player2Vault }

public class HexTile {
    public int Col { get; set; }
    public int Row { get; set; }
    public bool IsRevealedByP1 { get; set; } = false;
    public bool IsRevealedByP2 { get; set; } = false;
    public TileType Type { get; set; } = TileType.Empty;
    public string LocationName { get; set; } = "";
    public bool IsNavigable { get; set; } = true;
    
    public List<Dweller> Player1Dwellers { get; } = new();
    public List<Dweller> Player2Dwellers { get; } = new();

    public bool HasConflict => Player1Dwellers.Any(d => d.IsAlive) && Player2Dwellers.Any(d => d.IsAlive);

    public bool IsRevealedFor(bool isP1) {
        if (!GameConfigManager.Config.EnableFogOfWar) return true;
        return isP1 ? IsRevealedByP1 : IsRevealedByP2;
    }

    public void RevealFor(bool isP1) {
        if (isP1) IsRevealedByP1 = true;
        else IsRevealedByP2 = true;
    }
}

public class HexMap {
    public int Cols => GameConfigManager.Config.MapCols;
    public int Rows => GameConfigManager.Config.MapRows;
    public HexTile[,] Tiles { get; private set; }

    private static readonly string[] LocationNames = {
        "Super Duper Mart","Red Rocket","Sanctuary Hills",
        "Diamond City","Goodneighbor","Corvega Factory"
    };

    public HexMap() {
        Tiles = new HexTile[Cols, Rows];
        var rng = new Random();

        var candidates = Enumerable.Range(0, Cols * Rows)
            .Where(i => i % Cols != 0 && i % Cols != Cols - 1)
            .OrderBy(_ => rng.Next()).Take(4).ToHashSet();
        
        int mid = Rows / 2;

        int idx = 0;
        for (int c = 0; c < Cols; c++)
            for (int r = 0; r < Rows; r++) {
                Tiles[c, r] = new HexTile { Col = c, Row = r };
                
                bool isVault = (c == 0 && r == mid) || (c == Cols - 1 && r == mid);

                if (candidates.Contains(idx) && Tiles[c, r].IsNavigable) 
                    Tiles[c, r].Type = TileType.Location;
                idx++;
            }

        Tiles[0, mid].Type = TileType.Player1Vault;
        Tiles[0, mid].IsRevealedByP1 = true;
        Tiles[0, mid].IsRevealedByP2 = true;
        Tiles[Cols - 1, mid].Type = TileType.Player2Vault;
        Tiles[Cols - 1, mid].IsRevealedByP1 = true;
        Tiles[Cols - 1, mid].IsRevealedByP2 = true;

        int li = 0;
        for (int c = 0; c < Cols; c++)
            for (int r = 0; r < Rows; r++)
                if (Tiles[c, r].Type == TileType.Location)
                    Tiles[c, r].LocationName = LocationNames[li++ % LocationNames.Length];
    }

    public HexTile Get(int col, int row) => Tiles[col, row];
}
