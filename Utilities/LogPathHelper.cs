using System.Globalization;

public static class LogPathHelper
{
    private static string basePath = @"D:\ud\logs";

    public static string GetPath(string type)
    {
        var now = DateTime.Now;

        var year = now.Year.ToString();
        var month = now.Month.ToString("D2");
        var week = GetWeekOfMonth(now);
        var day = now.Day.ToString("D2");

        var folder = Path.Combine(basePath, year, month, $"Week-{week}", day, type);

        Directory.CreateDirectory(folder);

        return Path.Combine(folder, $"{type}.txt");
    }

    private static int GetWeekOfMonth(DateTime date)
    {
        var firstDay = new DateTime(date.Year, date.Month, 1);
        return ((date.Day + (int)firstDay.DayOfWeek - 1) / 7) + 1;
    }
}