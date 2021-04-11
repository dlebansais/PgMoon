namespace PgMoon.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class MoonPhaseV2 : Enumeration
    {
        public static readonly MoonPhaseV2 UNKNOWN = new(-1, "Unknown", new Tuple<double, double>(-360, -360), string.Empty, BoatDestination.UNKNOWN, new List<MushroomInfo>());
        public static readonly MoonPhaseV2 FULL_MOON = new(0, "Full", new Tuple<double, double>(-180.0, -135.0), "By Percy's House", BoatDestination.KUR_MOUNTAINS, new List<MushroomInfo>() { MushroomInfo.FIELD, MushroomInfo.BLACK_FOOT_MOREL, MushroomInfo.FALSE_AGARIC });
        public static readonly MoonPhaseV2 WANING_GIBBOUS = new(1, "Waning Gibbous", new Tuple<double, double>(-135.0, -90.0), "East of Hogan's Keep", BoatDestination.KUR_MOUNTAINS, new List<MushroomInfo>() {MushroomInfo.BLUSHER, MushroomInfo.PIXIES_PARASOL });
        public static readonly MoonPhaseV2 LAST_QUARTER = new(2, "Last Quarter", new Tuple<double, double>(-90.0, -45.0), "At Gnashers", BoatDestination.SUN_VALE, new List<MushroomInfo>() {MushroomInfo.MILK_CAP, MushroomInfo.FLY_AMANITA, MushroomInfo.WIZARDS });
        public static readonly MoonPhaseV2 WANING_CRESCENT = new(3, "Waning Crescent", new Tuple<double, double>(-45.0, 0.0), "South of the Waterfall", BoatDestination.SUN_VALE, new List<MushroomInfo>() {MushroomInfo.BLOOD, MushroomInfo.CHARGED_MYCELIUM });
        public static readonly MoonPhaseV2 NEW_MOON = new(4, "New", new Tuple<double, double>(0.0, 45.0), "South of the westernmost Lake", BoatDestination.SERBULE, new List<MushroomInfo>() {MushroomInfo.GOBLIN_PUFFBALL, MushroomInfo.CORAL });
        public static readonly MoonPhaseV2 WAXING_CRESCENT = new(5, "Waxing Crescent", new Tuple<double, double>(45.0, 90.0), "North and east of the Portal to Serbule", BoatDestination.SERBULE, new List<MushroomInfo>() { MushroomInfo.IOCAINE });
        public static readonly MoonPhaseV2 FIRST_QUARTER = new(6, "First Quarter", new Tuple<double, double>(90.0, 135.0), "At Spiders", BoatDestination.SERBULE, new List<MushroomInfo>() {MushroomInfo.MYCENA, MushroomInfo.GROXMAX, MushroomInfo.BLASTCAP });
        public static readonly MoonPhaseV2 WAXING_GIBBOUS = new(7, "Waxing Gibbous", new Tuple<double, double>(135.0, 180.0), "By the Mountain, north of the Lake's edge", BoatDestination.KUR_MOUNTAINS, new List<MushroomInfo>() {MushroomInfo.BOLETUS, MushroomInfo.PORCINI });

        private readonly Tuple<double, double> angleBounds;
        private readonly string darkChapelTip;
        private readonly BoatDestination destination;
        private readonly List<MushroomInfo> mushroomList;

        private MoonPhaseV2(int enumId, string enumName, Tuple<double, double> angleBounds, string darkChapelTip, BoatDestination destination, List<MushroomInfo> mushroomList)
        : base(enumId, $"{enumName} Moon")
        {
            this.angleBounds = angleBounds;
            this.darkChapelTip = darkChapelTip;
            this.destination = destination;
            this.mushroomList = mushroomList;
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

        public string GetTip() => this.darkChapelTip;
        public BoatDestination GetBoatDestination() => this.destination;
        public bool isPreferable(MushroomInfo mushroom) => mushroomList.Contains(mushroom);

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