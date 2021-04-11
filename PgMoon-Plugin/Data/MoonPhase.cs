namespace PgMoon.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class MoonPhaseV2 : Enumeration
    {
        public static readonly MoonPhaseV2 UNKNOWN = new(-1, "Unknown Moon", new Tuple<double, double>(-360, -360), BoatDestination.UNKNOWN);
        public static readonly MoonPhaseV2 FULL_MOON = new(0, "Full Moon", new Tuple<double, double>(-180.0, -135.0), BoatDestination.KUR_MOUNTAINS);
        public static readonly MoonPhaseV2 WANING_GIBBOUS = new(1, "Waning Gibbous", new Tuple<double, double>(-135.0, -90.0), BoatDestination.KUR_MOUNTAINS);
        public static readonly MoonPhaseV2 LAST_QUARTER = new(2, "Last Quarter", new Tuple<double, double>(-90.0, -45.0), BoatDestination.SUN_VALE);
        public static readonly MoonPhaseV2 WANING_CRESCENT = new(3, "Waning Crescent", new Tuple<double, double>(-45.0, 0.0), BoatDestination.SUN_VALE);
        public static readonly MoonPhaseV2 NEW_MOON = new(4, "New Moon", new Tuple<double, double>(0.0, 45.0), BoatDestination.SERBULE);
        public static readonly MoonPhaseV2 WAXING_CRESCENT = new(5, "Waxing Crescent", new Tuple<double, double>(45.0, 90.0), BoatDestination.SERBULE);
        public static readonly MoonPhaseV2 FIRST_QUARTER = new(6, "First Quarter", new Tuple<double, double>(90.0, 135.0), BoatDestination.SERBULE);
        public static readonly MoonPhaseV2 WAXING_GIBBOUS = new(7, "Waxing Gibbous", new Tuple<double, double>(135.0, 180.0), BoatDestination.KUR_MOUNTAINS);

        private readonly Tuple<double, double> angleBounds;
        private readonly BoatDestination destination;

        private MoonPhaseV2(int enumId, string enumName, Tuple<double, double> angleBounds, BoatDestination destination)
        : base(enumId, enumName)
        {
            this.angleBounds = angleBounds;
            this.destination = destination;
        }

        public bool IsAngleWithinLimits(double inputAngle)
        {
            bool result = inputAngle >= angleBounds.Item1 && inputAngle < angleBounds.Item2;

            if (this.Equals(WAXING_GIBBOUS))
            {
                result = inputAngle >= angleBounds.Item1 && inputAngle <= angleBounds.Item2;
            }

            return result;
        }

        public BoatDestination GetBoatDestination() => this.destination;

        public static List<MoonPhaseV2> GetAll() => new()
        {
            FULL_MOON,
            WANING_GIBBOUS,
            LAST_QUARTER,
            WANING_CRESCENT,
            NEW_MOON,
            WAXING_CRESCENT,
            FIRST_QUARTER,
            WAXING_GIBBOUS,
        };

        public static MoonPhaseV2 From(int enumId)
        {
            MoonPhaseV2 result = GetAll().SingleOrDefault<MoonPhaseV2>(moonPhase => enumId == moonPhase.Id);
            return result ?? UNKNOWN;
        }

        public static MoonPhaseV2 From(double inputAngle)
        {
            MoonPhaseV2 result = GetAll().SingleOrDefault<MoonPhaseV2>(moonPhase => moonPhase.IsAngleWithinLimits(inputAngle));
            return result ?? UNKNOWN;
        }
    }
}