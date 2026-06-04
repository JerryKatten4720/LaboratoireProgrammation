namespace LaboratoireProgrammation.Project.ModernOverseerWars.Models.Map;

using System.Linq;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Enums;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Data;
using LaboratoireProgrammation.Project.ModernOverseerWars.Helpers;

public class HexMap {
    public int Cols => GameConfigRepository.Config.MapCols;
    public int Rows => GameConfigRepository.Config.MapRows;
    public HexTile[,] Tiles { get; private set; }

    private static readonly string[] LocationNames = {
        "Super Duper Mart", "Red Rocket", "Sanctuary Hills",
        "Diamond City", "Goodneighbor", "Corvega Factory"
    };

    public HexMap() {
        Tiles = new HexTile[Cols, Rows];

        var candidates = Enumerable.Range(0, Cols * Rows)
            .Where(i => i % Cols != 0 && i % Cols != Cols - 1)
            .OrderBy(_ => RandomProvider.Next(1000))
            .Take(4).ToHashSet();
        
        int mid = Rows / 2;
        int idx = 0;
        
        for (int c = 0; c < Cols; c++) {
            for (int r = 0; r < Rows; r++) {
                Tiles[c, r] = new HexTile { Col = c, Row = r };
                
                if (candidates.Contains(idx) && Tiles[c, r].IsNavigable) 
                    Tiles[c, r].Type = TileType.Location;
                idx++;
            }
        }

        Tiles[0, mid].Type = TileType.Player1Vault;
        Tiles[0, mid].IsRevealedByP1 = true;
        Tiles[0, mid].IsRevealedByP2 = true;
        
        Tiles[Cols - 1, mid].Type = TileType.Player2Vault;
        Tiles[Cols - 1, mid].IsRevealedByP1 = true;
        Tiles[Cols - 1, mid].IsRevealedByP2 = true;

        int li = 0;
        for (int c = 0; c < Cols; c++) {
            for (int r = 0; r < Rows; r++) {
                if (Tiles[c, r].Type == TileType.Location) {
                    Tiles[c, r].LocationName = LocationNames[li++ % LocationNames.Length];
                }
            }
        }
    }

    public HexTile Get(int col, int row) => Tiles[col, row];
}
