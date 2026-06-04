using System;
using LaboratoireProgrammation.Project.ModernOverseerWars.Models.Map;

namespace LaboratoireProgrammation.Project.ModernOverseerWars.Helpers;

public static class GridMath {
    public static int GetDistance(HexTile a, HexTile b) {
        if (a == null || b == null) return 0;
        return Math.Abs(a.Col - b.Col) + Math.Abs(a.Row - b.Row);
    }

    public static bool IsOnLine(HexTile start, HexTile end, HexTile pt) {
        if (start == pt || end == pt) return true;
        
        int x0 = start.Col;
        int y0 = start.Row;
        int x1 = end.Col;
        int y1 = end.Row;
        
        int dx = Math.Abs(x1 - x0);
        int dy = Math.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        
        int x = x0;
        int y = y0;
        
        int err = dx - dy;
        
        while (x != x1 || y != y1) {
            int e2 = 2 * err;
            
            if (e2 > -dy) {
                err -= dy;
                x += sx;
                if (x == pt.Col && y == pt.Row) return true;
                if (x == x1 && y == y1) break;
            }
            if (e2 < dx) {
                err += dx;
                y += sy;
                if (x == pt.Col && y == pt.Row) return true;
            }
        }
        return false;
    }
}
