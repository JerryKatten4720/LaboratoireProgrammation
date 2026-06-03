namespace LaboratoireProgrammation.Project.ModernOverseerWars.Helpers;

using System;

public static class RandomProvider {
    public static int Next(int maxValue) => Random.Shared.Next(maxValue);
    public static int Next(int minValue, int maxValue) => Random.Shared.Next(minValue, maxValue);
    public static double NextDouble() => Random.Shared.NextDouble();
}
