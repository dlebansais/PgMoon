namespace MoonPhaseCalculator;

using System;
using System.Globalization;
using System.IO;

internal class Program
{
    static void Main(string[] args)
    {
        MoonPhase moonPhase = Calculator.GetMoonPhase(DateTime.UtcNow);
        Console.WriteLine($"MoonPhase: {moonPhase}");

        DateTime Start = DateTime.Parse("01/14/2010 00:00:00", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal).ToUniversalTime();
        DateTime End = DateTime.Parse("01/01/2050 00:00:00", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal).ToUniversalTime();
        DateTime Current = Start;
        int LunationIndex = 1077;
        DateTime PreviousNewMoonTime = DateTime.MinValue;

        using FileStream Stream = new("PhaseCalculator.Lunations.cs", FileMode.Create, FileAccess.Write);
        using StreamWriter StreamWriter = new(Stream);

        StreamWriter.Write("""
namespace PgMoon;

/// <summary>
/// Represents a moon phase calculator.
/// </summary>
public partial class PhaseCalculator
{
    private static Lunation[] LunationTable = new Lunation[]
    {

""");

        for (int Index = LunationIndex; Current < End; Index++)
        {
            DateTime NewMoonTime = GetNextTimeForPhase(MoonPhase.NewMoon, ref Current);
            DateTime QuarterMoonTime = GetNextTimeForPhase(MoonPhase.QuarterMoon, ref Current);
            DateTime FullMoonTime = GetNextTimeForPhase(MoonPhase.FullMoon, ref Current);
            DateTime LastQuarterMoonTime = GetNextTimeForPhase(MoonPhase.LastQuarterMoon, ref Current);
            TimeSpan Duration = (PreviousNewMoonTime != DateTime.MinValue) ? NewMoonTime - PreviousNewMoonTime : TimeSpan.FromDays(29);
            PreviousNewMoonTime = NewMoonTime;

            string Line = $"        new Lunation() {{ Index = {Index}, " +
                          $"NewMoon = ParseDateTime(\"{GetDateString(NewMoonTime)}\"), " +
                          $"FirstQuarterMoon = ParseDateTime(\"{GetDateString(QuarterMoonTime)}\"), " +
                          $"FullMoon = ParseDateTime(\"{GetDateString(FullMoonTime)}\"), " +
                          $"LastQuarterMoon = ParseDateTime(\"{GetDateString(LastQuarterMoonTime)}\"), " +
                          $"Duration = ParseTimeSpan(\"{Duration}\") }},";

            StreamWriter.WriteLine(Line);
        }

        StreamWriter.Write("""
    };
}
""");

    }

    private static string GetDateString(DateTime dateTime)
    {
        return dateTime.ToString(CultureInfo.InvariantCulture);
    }

    private static string GetTimeString(TimeSpan timeSpan)
    {
        return timeSpan.ToString();
    }

    private static DateTime GetNextTimeForPhase(MoonPhase moonPhase, ref DateTime current)
    {
        while (Calculator.GetMoonPhase(current) != moonPhase)
            current = AddOneHour(current);

        return current;
    }

    private static DateTime AddOneHour(DateTime current)
    {
        long TicksInSecond = 1000 * 1000 * 10;
        return new DateTime(current.Ticks + (60 * 60 * TicksInSecond), current.Kind);
    }
}
