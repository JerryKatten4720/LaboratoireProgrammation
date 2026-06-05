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
        "Diamond City", "Goodneighbor", "Corvega Factory",
        "The Prydwen", "Railroad HQ", "The Institute", "Fort Hagen",
        "Pickman Gallery", "Swan's Pond", "Combat Zone", "Vault 111",
        "The Crater of Atom", "Nuka-World", "Far Harbor", "Acadia",
        "The Nucleus", "Watoga", "The Whitespring Resort", "Foundation",
        "The Crater", "Fort Defiance", "Cranberry Bog", "Harper's Ferry",

        "Megaton", "Rivet City", "Tenpenny Tower", "Underworld",
        "The Citadel", "Project Purity", "Little Lamplight", "Big Town",
        "Paradise Falls", "Raven Rock", "Republic of Dave", "Girdershade",
        "The Pitt", "Point Lookout", "Mothership Zeta", "Vault 101",

        "New Vegas", "The Strip", "Goodsprings", "Hoover Dam",
        "Freeside", "Primm", "Novac", "Helios One",
        "Camp McCarran", "Camp Forlorn Hope", "The Fort", "Cottonwood Cove",
        "Jacobstown", "Red Rock Canyon", "Nellis Air Force Base", "Hidden Valley",
        "Sierra Madre Casino", "Big MT", "The Divide", "Zion Canyon",

        "Shady Sands", "The Glow", "Necropolis", "The Hub",
        "Junktown", "Boneyard", "Cathedral", "Mariposa Military Base",
        "Arroyo", "Klamath", "Den", "Modoc",
        "Vault City", "New Reno", "Redding", "Broken Hills",
        "Gecko", "San Francisco", "Navarro", "Enclave Oil Rig"
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
