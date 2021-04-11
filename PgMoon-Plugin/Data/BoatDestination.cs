namespace PgMoon.Data
{
    using System.Collections.Generic;
    using System.Linq;

    public class BoatDestination : Enumeration
    {
        public static readonly BoatDestination UNKNOWN = new(-1, "Unknown Destination");
        public static readonly BoatDestination SERBULE = new(0, "Serbule");
        public static readonly BoatDestination KUR_MOUNTAINS = new(1, "Kur Mountains");
        public static readonly BoatDestination SUN_VALE = new(2, "Sun Vale");

        private BoatDestination(int enumId, string enumName)
        : base(enumId, enumName)
        {
        }

        public static List<BoatDestination> GetAll() => new()
        {
            SERBULE,
            KUR_MOUNTAINS,
            SUN_VALE,
        };

        public static BoatDestination From(int enumId)
        {
            BoatDestination result = GetAll().SingleOrDefault<BoatDestination>(destination => enumId == destination.Id);

            return result ?? UNKNOWN;
        }
    }
}