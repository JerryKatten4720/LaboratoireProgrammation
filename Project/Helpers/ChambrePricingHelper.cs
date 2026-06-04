using System;

namespace LaboratoireProgrammation.Project.ModernHospital.Helpers;

public static class ChambrePricingHelper {
    public const decimal FraisFixesCreation = 15000m;
    public const decimal MultiplicateurCoutLit = 5.5m;
    public const decimal FacteurCapacite = 20m;

    public static decimal CalculerCoutTotal(decimal coutEntretien, int capaciteLits) {
        return FraisFixesCreation + coutEntretien + CalculerCoutLitsTotal(coutEntretien, capaciteLits);
    }

    public static decimal CalculerCoutLitsTotal(decimal coutEntretien, int capaciteLits) {
        var multiplicateur = 1m + (capaciteLits / FacteurCapacite);
        
        return MultiplicateurCoutLit * coutEntretien * capaciteLits * multiplicateur;
    }

    public static decimal CalculerCoutUnitaireLit(decimal coutEntretien, int capaciteLits) {
        var multiplicateur = 1m + (capaciteLits / FacteurCapacite);
        
        return MultiplicateurCoutLit * coutEntretien * multiplicateur;
    }
}