namespace PgMoon.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class MoonPhaseV2 : Enumeration
    {
       public static readonly MoonPhaseV2 UNKNOWN = new MoonPhaseV2(-1, "Unknown Moon", new Tuple<double, double>(-360, -360));
       public static readonly MoonPhaseV2 FULL_MOON = new MoonPhaseV2(0, "Full Moon", new Tuple<double, double>(-180.0, -135.0));
       public static readonly MoonPhaseV2 WANING_GIBBOUS = new MoonPhaseV2(1, "Waning Gibbous", new Tuple<double, double>(-135.0, -90.0));
       public static readonly MoonPhaseV2 LAST_QUARTER = new MoonPhaseV2(2, "Last Quarter", new Tuple<double, double>(-90.0, -45.0));
       public static readonly MoonPhaseV2 WANING_CRESCENT = new MoonPhaseV2(3, "Waning Crescent", new Tuple<double, double>(-45.0, 0.0));
       public static readonly MoonPhaseV2 NEW_MOON = new MoonPhaseV2(4, "New Moon", new Tuple<double, double>(0.0, 45.0));
       public static readonly MoonPhaseV2 WAXING_CRESCENT = new MoonPhaseV2(5, "Waxing Crescent", new Tuple<double, double>(45.0, 90.0));
       public static readonly MoonPhaseV2 FIRST_QUARTER = new MoonPhaseV2(6, "First Quarter", new Tuple<double, double>(90.0, 135.0));
       public static readonly MoonPhaseV2 WAXING_GIBBOUS = new MoonPhaseV2(7, "Waxing Gibbous", new Tuple<double, double>(135.0, 180.0));

        private readonly Tuple<double, double> angleBounds;

        private MoonPhaseV2
        (
            int enumId,
            string enumName,
            Tuple<double, double> angleBounds
        ) : base(enumId, enumName)
        {
            this.angleBounds = angleBounds;
        }

        public Boolean IsAngleWithinLimits(double inputAngle)
        {
            Boolean result = inputAngle >= angleBounds.Item1 && inputAngle < angleBounds.Item2;
            
            if (this.Equals(MoonPhaseV2.WAXING_GIBBOUS))
            {
                result = inputAngle >= angleBounds.Item1 && inputAngle <= angleBounds.Item2;
            }

            return result;
        }

        public static List<MoonPhaseV2> GetAll()
        {
            return new List<MoonPhaseV2>()
            {
                FULL_MOON,
                WANING_GIBBOUS,
                LAST_QUARTER,
                WANING_CRESCENT,
                NEW_MOON,
                WAXING_CRESCENT,
                FIRST_QUARTER,
                WAXING_GIBBOUS
            };
        }

        public static MoonPhaseV2 From(int enumId)
        {
            MoonPhaseV2 result = GetAll().SingleOrDefault<MoonPhaseV2>
            (
                moonPhase => enumId == moonPhase.Id
            );

            return (result != null) ? result : MoonPhaseV2.UNKNOWN;
        }

        public static MoonPhaseV2 From(double inputAngle)
        {
            MoonPhaseV2 result = GetAll().SingleOrDefault<MoonPhaseV2>
            (
                moonPhase => moonPhase.IsAngleWithinLimits(inputAngle)
            );

            return (result != null) ? result : MoonPhaseV2.UNKNOWN;
        }
    }
}