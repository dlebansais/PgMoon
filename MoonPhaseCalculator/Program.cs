namespace MoonPhaseCalculator;

using System;
using System.Globalization;
using System.IO;
using System.Text;

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
        using StreamWriter StreamWriter = new(Stream, Encoding.UTF8);

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
            DateTime[] PhaseDateTimes = new DateTime[typeof(MoonPhase).GetEnumNames().Length];
            for (int i = 0; i < PhaseDateTimes.Length; i++)
                PhaseDateTimes[i] = GetNextTimeForPhase((MoonPhase)i, ref Current);

            DateTime NewMoonTime = PhaseDateTimes[0];
            TimeSpan Duration = (PreviousNewMoonTime != DateTime.MinValue) ? NewMoonTime - PreviousNewMoonTime : TimeSpan.FromDays(29);
            PreviousNewMoonTime = NewMoonTime;

            string Line = $"        new Lunation() {{ Index = {Index}, ";

            for (int i = 0; i < PhaseDateTimes.Length; i++)
                Line += $"{(MoonPhase)i} = ParseDateTime(\"{GetDateString(PhaseDateTimes[i])}\"), ";

            Line += $"Duration = ParseTimeSpan(\"{Duration}\") }},";

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
            current = current.AddHours(1);

        return current;
    }
}
