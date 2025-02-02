public static class Helper
{
    public static int CalculateHP(int baseStat, int iv = 31, int ev = 252, int level = 100)
    {
        int result = (int)((2 * baseStat + iv + (ev / 4)) * level / 100) + level + 10;
        return result;
    }

    public static int CalculateOtherStat(int baseStat, int iv = 31, int ev = 31, int level = 100, float natureMultiplier = 1)
    {
        int baseStatCalculation = (int)((2 * baseStat + iv + (ev / 4)) * level / 100);
        int result = (int)((baseStatCalculation + 5) * natureMultiplier);
        return result;
    }
}