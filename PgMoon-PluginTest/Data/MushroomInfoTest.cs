namespace PgMoonTest.Data
{
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using PgMoon.Data;

    [TestClass]
    public class MushroomInfoTest
    {
        [DataTestMethod]
        [DynamicData(nameof(EnumIdReturnsMoonPhaseData), DynamicDataSourceType.Method)]
        public void EnumIdReturnsMushroom(MushroomInfo expectedMushroomInfo)
        {
            MushroomInfo actualMushroom = MushroomInfo.From(expectedMushroomInfo.Id);
            Assert.AreEqual(expectedMushroomInfo, actualMushroom);
        }

        public static IEnumerable<object[]> EnumIdReturnsMoonPhaseData()
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
    }
}