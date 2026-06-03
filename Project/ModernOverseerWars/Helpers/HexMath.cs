using LaboratoireProgrammation.Project.ModernOverseerWars.Domain;
using LaboratoireProgrammation.Project.ModernOverseerWars.Domain.Entities;
using LaboratoireProgrammation.Project.ModernOverseerWars.Domain.Enums;
using LaboratoireProgrammation.Project.ModernOverseerWars.Domain.Interfaces;
using LaboratoireProgrammation.Project.ModernOverseerWars.Domain.Map;
using LaboratoireProgrammation.Project.ModernOverseerWars.Data.Json;
using System;

namespace LaboratoireProgrammation.Project.ModernOverseerWars.Helpers;

public static class HexMath {
    public static int GetDistance(HexTile a, HexTile b) {
        if (a == null || b == null) return 0;
        int qa = a.Col - (a.Row - (a.Row & 1)) / 2;
        int ra = a.Row;
        int qb = b.Col - (b.Row - (b.Row & 1)) / 2;
        int rb = b.Row;
        return (Math.Abs(qa - qb) + Math.Abs(qa + ra - (qb + rb)) + Math.Abs(ra - rb)) / 2;
    }

    public static bool IsOnLine(HexTile start, HexTile end, HexTile pt) {
        int N = GetDistance(start, end);
        if (N == 0) return start == pt;
        
        var (qa, ra) = OffsetToAxial(start.Col, start.Row);
        var (qb, rb) = OffsetToAxial(end.Col, end.Row);
        var (qp, rp) = OffsetToAxial(pt.Col, pt.Row);
        
        double sa = -qa - ra;
        double sb = -qb - rb;
        
        for (int i = 0; i <= N; i++) {
            double t = 1.0 * i / N;
            double qL = qa + (qb - qa) * t;
            double rL = ra + (rb - ra) * t;
            double sL = sa + (sb - sa) * t;
            
            int qi = (int)Math.Round(qL);
            int ri = (int)Math.Round(rL);
            int si = (int)Math.Round(sL);
            double qDiff = Math.Abs(qi - qL);
            double rDiff = Math.Abs(ri - rL);
            double sDiff = Math.Abs(si - sL);
            if (qDiff > rDiff && qDiff > sDiff) qi = -ri - si;
            else if (rDiff > sDiff) ri = -qi - si;
            
            if (qi == qp && ri == rp) return true;
        }
        return false;
    }
    
    public static (int q, int r) OffsetToAxial(int col, int row) {
        int q = col - (row - (row & 1)) / 2;
        int r = row;
        return (q, r);
    }
}
