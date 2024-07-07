using System;

namespace MoonPhaseCalculator;

public class Moon
{
    public Moon(JulianDay j)
    {
        YearFraction = (j.Value - 2451545.0) / 36525.0; // See Meeus 22.1
        PhaseAngle = YearFractionToPhaseAngle(YearFraction); // the phase angle
    }

    public static double YearFractionToPhaseAngle(double t)
    {
        double YearAngle = YearFractionToYearAngle(t);
        double MonthAngle1 = YearFractionToMonthAngle1(t);
        double MonthAngle2 = YearFractionToMonthAngle2(t);
        double Result = 180.0
                        - YearAngle
                        - 6.289 * sin(MonthAngle2)
                        + 2.100 * sin(MonthAngle1)
                        - 1.274 * sin(2.0 * YearAngle - MonthAngle2)
                        - 0.658 * sin(2.0 * YearAngle)
                        - 0.214 * sin(2.0 * MonthAngle2)
                        - 0.110 * sin(YearAngle);
        return Result;
    }

    private static double YearFractionToYearAngle(double t)
    {
        return 297.8501921 + t * (445267.1114034 + t * (-0.0018819 + t * (1.0 / 545868.0 - t / 113065000.0)));
    }

    private static double YearFractionToMonthAngle1(double t)
    {
        return 357.5291092 + t * (35999.0502909 + t * (-0.0001536 + t / 24490000.0));
    }

    private static double YearFractionToMonthAngle2(double t)
    {
        return 134.9633964 + t * (477198.8675055 + t * (0.0087414 + t * (1.0 / 69699.0 - t / 14712000.0)));
    }

    private static double sin(double x)
    {
        return Math.Sin(ToRadians(x));
    }

    private static double cos(double x)
    {
        return Math.Cos(ToRadians(x));
    }

    private static double ToRadians(double x)
    {
        return (x * Math.PI) / 180.0;
    }

    public bool IsWaning { get => IsWaningPhaseAngle(PhaseAngle); }

    private static bool IsWaningPhaseAngle(double i)
    {
        double s = sin(i);

        if (s < 0) return true; // wanes (shrinks)
        if (s > 0) return false; // waxes (grows)

        return cos(i) > 0;
    }

    public double IlluminatedFraction { get => PhaseAngleToIlluminatedFraction(PhaseAngle); }

    private static double PhaseAngleToIlluminatedFraction(double i)
    {
        return (1 + cos(i)) * 0.5;
    }

    private double YearFraction;
    private double PhaseAngle;
}
