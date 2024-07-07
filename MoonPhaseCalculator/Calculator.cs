namespace MoonPhaseCalculator;

using System;

internal class Calculator
{
    /// <summary>
    /// Gets the moon phase.
    /// </summary>
    /// <param name="date">The date in UTC time.</param>
    /// <returns></returns>
    public static MoonPhase GetMoonPhase(DateTime date)
    {
        DateTime LocalTime = date.ToLocalTime();
        DateTime EST = ToUnitedStatesEasternTime(LocalTime);
        DateTime ESTMidnight = WithTimeAtStartOfDay(EST);

        MoonPhaseInfo[] phases = CalculatePhasesForDate(ESTMidnight);
        MoonPhase moonPhase = (MoonPhase)phases[1].Phase!;

        return moonPhase;
    }

    private static DateTime ToUnitedStatesEasternTime(DateTime dateTime)
    {
        return TimeZoneInfo.ConvertTime(dateTime, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
    }

    private static DateTime WithTimeAtStartOfDay(DateTime dateTime)
    {
        return dateTime - dateTime.TimeOfDay;
    }

    private static MoonPhaseInfo[] CalculatePhasesForDate(DateTime date)
    {
        MoonPhaseInfo[] phases = new MoonPhaseInfo[31];
        for (int loop = 0; loop<phases.Length; ++loop)
        {
            DateTime thisDate = date.AddDays(loop - 1);
            phases[loop] = new MoonPhaseInfo(thisDate);
        }

        // we want the "full moon" to be EXACTLY 3 days. So we find the 
        // most full day, and declare that day to be full, as well as the day
        // before and after it. 
        // We do the same for the "new moon" and the two quarter-moons, also exactly 3 days.

        // full moon
        for (int loop = 0; loop<phases.Length-1; ++loop)
        {
            if (!phases[loop].Moon.IsWaning && phases[loop + 1].Moon.IsWaning)
            {
                phases[loop].Phase = MoonPhase.FullMoon;
                phases[loop + 1].Phase = MoonPhase.FullMoon;
                if (loop > 0)
                    phases[loop - 1].Phase = MoonPhase.FullMoon;
                break;
            }
        }

        // new moon
        for (int loop = 0; loop < phases.Length - 1; ++loop)
        {
            if (phases[loop].Moon.IsWaning && !phases[loop + 1].Moon.IsWaning)
            {
                phases[loop].Phase = MoonPhase.NewMoon;
                phases[loop + 1].Phase = MoonPhase.NewMoon;
                if (loop > 0)
                    phases[loop - 1].Phase = MoonPhase.NewMoon;
                break;
            }
        }

        // first quarter moon
        for (int loop = 0; loop < phases.Length - 1; ++loop)
        {
            if (!phases[loop].Moon.IsWaning
                && !phases[loop + 1].Moon.IsWaning
                && phases[loop].Moon.IlluminatedFraction <= .5
                && phases[loop + 1].Moon.IlluminatedFraction > .5
                )
            {
                phases[loop].Phase = MoonPhase.QuarterMoon;
                phases[loop + 1].Phase = MoonPhase.QuarterMoon;
                if (loop > 0)
                    phases[loop - 1].Phase = MoonPhase.QuarterMoon;
                break;
            }
        }


        // last quarter moon
        for (int loop = 0; loop < phases.Length - 1; ++loop)
        {
            if (phases[loop].Moon.IsWaning
                && phases[loop + 1].Moon.IsWaning
                && phases[loop].Moon.IlluminatedFraction >= .5
                && phases[loop + 1].Moon.IlluminatedFraction < .5
                )
            {
                phases[loop].Phase = MoonPhase.LastQuarterMoon;
                phases[loop + 1].Phase = MoonPhase.LastQuarterMoon;
                if (loop > 0)
                    phases[loop - 1].Phase = MoonPhase.LastQuarterMoon;
                break;
            }
        }

        // the dates that don't have phases yet are the "leftover" phases. 
        // Their length varies between 3 and 5 days depending on the month
        foreach (MoonPhaseInfo mpi in phases)
        {
            if (mpi.Phase != null)
                continue; // already handled!

            if (!mpi.Moon.IsWaning)
            {
                if (mpi.Moon.IlluminatedFraction <= .5)
                    mpi.Phase = MoonPhase.WaxingCrescentMoon;
                else
                    mpi.Phase = MoonPhase.WaxingGibbousMoon;
            }
            else
            {
                if (mpi.Moon.IlluminatedFraction >= .5)
                    mpi.Phase = MoonPhase.WaningGibbousMoon;
                else
                    mpi.Phase = MoonPhase.WaningCrescentMoon;
            }
        }

        return phases;
    }
}
