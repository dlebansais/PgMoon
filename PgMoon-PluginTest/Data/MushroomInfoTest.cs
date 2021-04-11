namespace PgMoonTest.Data
{
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using PgMoon.Data;

    [TestClass]
    public class MushroomInfoTest
    {
        [DataTestMethod]
        [DynamicData(nameof(EnumIdReturnsMushroomData), DynamicDataSourceType.Method)]
        public void EnumIdReturnsMushroom(MushroomInfo expectedMushroomInfo)
        {
            MushroomInfo actualMushroom = MushroomInfo.From(expectedMushroomInfo.Id);
            Assert.AreEqual(expectedMushroomInfo, actualMushroom);
        }

        public static IEnumerable<object[]> EnumIdReturnsMushroomData()
        {
            yield return new object[] { MushroomInfo.UNKNOWN };
            yield return new object[] { MushroomInfo.PARASOL };
            yield return new object[] { MushroomInfo.MYCENA };
            yield return new object[] { MushroomInfo.BOLETUS };
            yield return new object[] { MushroomInfo.FIELD };
            yield return new object[] { MushroomInfo.BLUSHER };
            yield return new object[] { MushroomInfo.GOBLIN_PUFFBALL };
            yield return new object[] { MushroomInfo.MILK_CAP };
            yield return new object[] { MushroomInfo.BLOOD };
            yield return new object[] { MushroomInfo.CORAL };
            yield return new object[] { MushroomInfo.IOCAINE };
            yield return new object[] { MushroomInfo.GROXMAX };
            yield return new object[] { MushroomInfo.PORCINI };
            yield return new object[] { MushroomInfo.BLACK_FOOT_MOREL };
            yield return new object[] { MushroomInfo.PIXIES_PARASOL };
            yield return new object[] { MushroomInfo.FLY_AMANITA };
            yield return new object[] { MushroomInfo.BLASTCAP };
            yield return new object[] { MushroomInfo.CHARGED_MYCELIUM };
            yield return new object[] { MushroomInfo.FALSE_AGARIC };
            yield return new object[] { MushroomInfo.WIZARDS };
        }

        [DataTestMethod]
        [DynamicData(nameof(MushroomInfoMappingData), DynamicDataSourceType.Method)]
        public void MushroomInfoMapping(MushroomInfo mushroomInfo, bool useLongName, string expectedName)
        {
            Assert.AreEqual(expectedName, mushroomInfo.GetInformation(useLongName));
        }

        public static IEnumerable<object[]> MushroomInfoMappingData()
        {
            yield return new object[] { MushroomInfo.UNKNOWN, true, "Unknown Mushroom (NaNh)" };
            yield return new object[] { MushroomInfo.UNKNOWN, false, "Unknown Mushroom (NaNh)" };

            yield return new object[] { MushroomInfo.PARASOL, true, "Parasol Mushroom (NaNh)" };
            yield return new object[] { MushroomInfo.PARASOL, false, "Parasol (NaNh)" };

            yield return new object[] { MushroomInfo.MYCENA, true, "Mycena Mushroom (24h)" };
            yield return new object[] { MushroomInfo.MYCENA, false, "Mycena (24h)" };

            yield return new object[] { MushroomInfo.BOLETUS, true, "Boletus Mushroom (20h)" };
            yield return new object[] { MushroomInfo.BOLETUS, false, "Boletus (20h)" };

            yield return new object[] { MushroomInfo.FIELD, true, "Field Mushroom (16h)" };
            yield return new object[] { MushroomInfo.FIELD, false, "Field (16h)" };

            yield return new object[] { MushroomInfo.BLUSHER, true, "Blusher Mushroom (12h)" };
            yield return new object[] { MushroomInfo.BLUSHER, false, "Blusher (12h)" };

            yield return new object[] { MushroomInfo.GOBLIN_PUFFBALL, true, "Goblin Puffball (10h)" };
            yield return new object[] { MushroomInfo.GOBLIN_PUFFBALL, false, "Goblin Puffball (10h)" };

            yield return new object[] { MushroomInfo.MILK_CAP, true, "Milk Cap Mushroom (9h)" };
            yield return new object[] { MushroomInfo.MILK_CAP, false, "Milk Cap (9h)" };

            yield return new object[] { MushroomInfo.BLOOD, true, "Blood Mushroom (8h)" };
            yield return new object[] { MushroomInfo.BLOOD, false, "Blood (8h)" };

            yield return new object[] { MushroomInfo.CORAL, true, "Coral Mushroom (7h)" };
            yield return new object[] { MushroomInfo.CORAL, false, "Coral (7h)" };

            yield return new object[] { MushroomInfo.IOCAINE, true, "Iocaine Mushroom (6.5h)" };
            yield return new object[] { MushroomInfo.IOCAINE, false, "Iocaine (6.5h)" };

            yield return new object[] { MushroomInfo.GROXMAX, true, "Groxmax Mushroom (6h)" };
            yield return new object[] { MushroomInfo.GROXMAX, false, "Groxmax (6h)" };

            yield return new object[] { MushroomInfo.PORCINI, true, "Porcini Mushroom (5.5h)" };
            yield return new object[] { MushroomInfo.PORCINI, false, "Porcini (5.5h)" };

            yield return new object[] { MushroomInfo.BLACK_FOOT_MOREL, true, "Black Foot Morel (5h)" };
            yield return new object[] { MushroomInfo.BLACK_FOOT_MOREL, false, "Black Foot Morel (5h)" };

            yield return new object[] { MushroomInfo.PIXIES_PARASOL, true, "Pixie's Parasol (4.5h)" };
            yield return new object[] { MushroomInfo.PIXIES_PARASOL, false, "Pixie's Parasol (4.5h)" };

            yield return new object[] { MushroomInfo.FLY_AMANITA, true, "Fly Amanita (4h)" };
            yield return new object[] { MushroomInfo.FLY_AMANITA, false, "Fly Amanita (4h)" };

            yield return new object[] { MushroomInfo.BLASTCAP, true, "Blastcap Mushroom (3.5h)" };
            yield return new object[] { MushroomInfo.BLASTCAP, false, "Blastcap (3.5h)" };

            yield return new object[] { MushroomInfo.CHARGED_MYCELIUM, true, "Charged Mycelium (3.5h)" };
            yield return new object[] { MushroomInfo.CHARGED_MYCELIUM, false, "Charged Mycelium (3.5h)" };

            yield return new object[] { MushroomInfo.FALSE_AGARIC, true, "False Agaric (2.5h)" };
            yield return new object[] { MushroomInfo.FALSE_AGARIC, false, "False Agaric (2.5h)" };

            yield return new object[] { MushroomInfo.WIZARDS, true, "Wizard's Mushroom (1.5h)" };
            yield return new object[] { MushroomInfo.WIZARDS, false, "Wizard's (1.5h)" };
        }
    }
}