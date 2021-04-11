namespace PgMoon.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class MushroomInfo : Enumeration
    {
        public static readonly MushroomInfo UNKNOWN = new(-1, "Unknown Mushroom", double.NaN);
        public static readonly MushroomInfo PARASOL = new(0, "Parasol Mushroom", "Parasol", double.NaN);
        public static readonly MushroomInfo MYCENA = new(1, "Mycena Mushroom", "Mycena", 24.0);
        public static readonly MushroomInfo BOLETUS = new(2, "Boletus Mushroom", "Boletus", 20.0);
        public static readonly MushroomInfo FIELD = new(3, "Field Mushroom", "Field", 16.0);
        public static readonly MushroomInfo BLUSHER = new(4, "Blusher Mushroom", "Blusher", 12.0);
        public static readonly MushroomInfo GOBLIN_PUFFBALL = new(5, "Goblin Puffball", 10.0);
        public static readonly MushroomInfo MILK_CAP = new(6, "Milk Cap Mushroom", "Milk Cap", 9.0);
        public static readonly MushroomInfo BLOOD = new(7, "Blood Mushroom", "Blood", 8.0);
        public static readonly MushroomInfo CORAL = new(8, "Coral Mushroom", "Coral", 7.0);
        public static readonly MushroomInfo IOCAINE = new(9, "Iocaine Mushroom", "Iocaine", 6.5);
        public static readonly MushroomInfo GROXMAX = new(10, "Groxmax Mushroom", "Groxmax", 6.0);
        public static readonly MushroomInfo PORCINI = new(11, "Porcini Mushroom", "Porcini", 5.5);
        public static readonly MushroomInfo BLACK_FOOT_MOREL = new(12, "Black Foot Morel", 5.0);
        public static readonly MushroomInfo PIXIES_PARASOL = new(13, "Pixie's Parasol", 4.5);
        public static readonly MushroomInfo FLY_AMANITA = new(14, "Fly Amanita", 4.0);
        public static readonly MushroomInfo BLASTCAP = new(15, "Blastcap Mushroom", "Blastcap", 3.5);
        public static readonly MushroomInfo CHARGED_MYCELIUM = new(16, "Charged Mycelium", 3.5);
        public static readonly MushroomInfo FALSE_AGARIC = new(17, "False Agaric", 2.5);
        public static readonly MushroomInfo WIZARDS = new(18, "Wizard's Mushroom", "Wizard's", 1.5);

        private readonly string shortName;
        private readonly double refreshRate;

        private MushroomInfo(int enumId, string enumName, string shortName, double refreshRate)
        : base(enumId, enumName)
        {
            this.shortName = shortName;
            this.refreshRate = refreshRate;
        }

        private MushroomInfo(int enumId, string enumName, double refreshRate)
        : this(enumId, enumName, enumName, refreshRate)
        {
        }

        public static List<MushroomInfo> GetAll() => new()
        {
            PARASOL,
            MYCENA,
            BOLETUS,
            FIELD,
            BLUSHER,
            GOBLIN_PUFFBALL,
            MILK_CAP,
            BLOOD,
            CORAL,
            IOCAINE,
            GROXMAX,
            PORCINI,
            BLACK_FOOT_MOREL,
            PIXIES_PARASOL,
            FLY_AMANITA,
            BLASTCAP,
            CHARGED_MYCELIUM,
            FALSE_AGARIC,
            WIZARDS,
        };

        public static MushroomInfo From(int enumId)
        {
            MushroomInfo result = GetAll().SingleOrDefault<MushroomInfo>(mushroom => enumId == mushroom.Id);
            return result ?? UNKNOWN;
        }

        public string GetInformation(bool useLongName)
        {
            string nameToUse = useLongName ? Name : shortName;
            return $"{nameToUse} ({refreshRate}h)";
        }
    }
}