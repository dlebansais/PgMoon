namespace PgMoonTest.Data
{
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using PgMoon.Data;

    [TestClass]
    public class MoonPhaseTest
    {
        [DataTestMethod]
        [DynamicData(nameof(EnumIdReturnsMoonPhaseData), DynamicDataSourceType.Method)]
        public void EnumIdReturnsMoonPhase(MoonPhaseV2 expectedMoonPhase)
        {
            MoonPhaseV2 actualMoonPhase = MoonPhaseV2.From(expectedMoonPhase.Id);
            Assert.AreEqual(expectedMoonPhase, actualMoonPhase);
        }

        public static IEnumerable<object[]> EnumIdReturnsMoonPhaseData()
        {
            yield return new object[] { MoonPhaseV2.UNKNOWN };
            yield return new object[] { MoonPhaseV2.FULL_MOON };
            yield return new object[] { MoonPhaseV2.WANING_GIBBOUS };
            yield return new object[] { MoonPhaseV2.LAST_QUARTER };
            yield return new object[] { MoonPhaseV2.WANING_CRESCENT };
            yield return new object[] { MoonPhaseV2.NEW_MOON };
            yield return new object[] { MoonPhaseV2.WAXING_CRESCENT };
            yield return new object[] { MoonPhaseV2.FIRST_QUARTER };
            yield return new object[] { MoonPhaseV2.WAXING_GIBBOUS };
        }

        [DataTestMethod]
        [DynamicData(nameof(MoonPhaseNameMapping), DynamicDataSourceType.Method)]
        public void MoonPhaseName(MoonPhaseV2 expectedMoonPhase, string expectedMoonName)
        {
            string actualMoonName = expectedMoonPhase.Name;
            Assert.AreEqual(expectedMoonName, actualMoonName);
        }

        public static IEnumerable<object[]> MoonPhaseNameMapping()
        {
            yield return new object[] { MoonPhaseV2.UNKNOWN, "Unknown Moon" };
            yield return new object[] { MoonPhaseV2.FULL_MOON, "Full Moon" };
            yield return new object[] { MoonPhaseV2.WANING_GIBBOUS, "Waning Gibbous Moon" };
            yield return new object[] { MoonPhaseV2.LAST_QUARTER, "Last Quarter Moon" };
            yield return new object[] { MoonPhaseV2.WANING_CRESCENT, "Waning Crescent Moon" };
            yield return new object[] { MoonPhaseV2.NEW_MOON, "New Moon" };
            yield return new object[] { MoonPhaseV2.WAXING_CRESCENT, "Waxing Crescent Moon" };
            yield return new object[] { MoonPhaseV2.FIRST_QUARTER, "First Quarter Moon" };
            yield return new object[] { MoonPhaseV2.WAXING_GIBBOUS, "Waxing Gibbous Moon" };
        }

        [DataTestMethod]
        [DynamicData(nameof(AngleReturnsMoonPhaseData), DynamicDataSourceType.Method)]
        public void AngleReturnsMoonPhase(MoonPhaseV2 expectedMoonPhase, double inputAngle)
        {
            MoonPhaseV2 actualMoonPhase = MoonPhaseV2.From(inputAngle);
            Assert.AreEqual(expectedMoonPhase, actualMoonPhase);
        }

        public static IEnumerable<object[]> AngleReturnsMoonPhaseData()
        {
            yield return new object[] { MoonPhaseV2.FULL_MOON, -180.0 };
            yield return new object[] { MoonPhaseV2.FULL_MOON, -135.1 };
            yield return new object[] { MoonPhaseV2.WANING_GIBBOUS, -135.0 };
            yield return new object[] { MoonPhaseV2.WANING_GIBBOUS, -90.1 };
            yield return new object[] { MoonPhaseV2.LAST_QUARTER, -90.0 };
            yield return new object[] { MoonPhaseV2.LAST_QUARTER, -45.1 };
            yield return new object[] { MoonPhaseV2.WANING_CRESCENT, -45.0 };
            yield return new object[] { MoonPhaseV2.WANING_CRESCENT, -0.1 };
            yield return new object[] { MoonPhaseV2.NEW_MOON, 0.0 };
            yield return new object[] { MoonPhaseV2.NEW_MOON, 44.9 };
            yield return new object[] { MoonPhaseV2.WAXING_CRESCENT, 45.0 };
            yield return new object[] { MoonPhaseV2.WAXING_CRESCENT, 89.9 };
            yield return new object[] { MoonPhaseV2.FIRST_QUARTER, 90.0 };
            yield return new object[] { MoonPhaseV2.FIRST_QUARTER, 134.9 };
            yield return new object[] { MoonPhaseV2.WAXING_GIBBOUS, 135.0 };
            yield return new object[] { MoonPhaseV2.WAXING_GIBBOUS, 180.0 };
        }

        [DataTestMethod]
        [DynamicData(nameof(MoonPhaseMappingData), DynamicDataSourceType.Method)]
        public void MoonPhaseContainsBoatDestination(MoonPhaseV2 moonPhase, BoatDestination expectedDestination)
        {
            BoatDestination actualBoatDestination = moonPhase.GetBoatDestination();
            Assert.AreEqual(expectedDestination, actualBoatDestination);
        }

        public static IEnumerable<object[]> MoonPhaseMappingData()
        {
            yield return new object[] { MoonPhaseV2.UNKNOWN, BoatDestination.UNKNOWN };
            yield return new object[] { MoonPhaseV2.FULL_MOON, BoatDestination.KUR_MOUNTAINS };
            yield return new object[] { MoonPhaseV2.WANING_GIBBOUS, BoatDestination.KUR_MOUNTAINS };
            yield return new object[] { MoonPhaseV2.LAST_QUARTER, BoatDestination.SUN_VALE };
            yield return new object[] { MoonPhaseV2.WANING_CRESCENT, BoatDestination.SUN_VALE };
            yield return new object[] { MoonPhaseV2.NEW_MOON, BoatDestination.SERBULE };
            yield return new object[] { MoonPhaseV2.WAXING_CRESCENT, BoatDestination.SERBULE };
            yield return new object[] { MoonPhaseV2.FIRST_QUARTER, BoatDestination.SERBULE };
            yield return new object[] { MoonPhaseV2.WAXING_GIBBOUS, BoatDestination.KUR_MOUNTAINS };
        }

        [DataTestMethod]
        [DynamicData(nameof(MoonPhaseMushroomPreferenceData), DynamicDataSourceType.Method)]
        public void MoonPhaseContainsPreferentialMushrooms(MoonPhaseV2 moonPhase, List<MushroomInfo> preferentialMushrooms)
        {
            foreach (var mushroom in MushroomInfo.GetAll())
            {
                Assert.AreEqual(preferentialMushrooms.Contains(mushroom), moonPhase.isPreferable(mushroom));
            }
        }

        public static IEnumerable<object[]> MoonPhaseMushroomPreferenceData()
        {
            yield return new object[] { MoonPhaseV2.UNKNOWN, new List<MushroomInfo>() };
            yield return new object[] { MoonPhaseV2.FULL_MOON, new List<MushroomInfo>() };
            yield return new object[] { MoonPhaseV2.WANING_GIBBOUS, new List<MushroomInfo>() };
            yield return new object[] { MoonPhaseV2.LAST_QUARTER, new List<MushroomInfo>() };
            yield return new object[] { MoonPhaseV2.WANING_CRESCENT, new List<MushroomInfo>() };
            yield return new object[] { MoonPhaseV2.NEW_MOON, new List<MushroomInfo>() };
            yield return new object[] { MoonPhaseV2.WAXING_CRESCENT, new List<MushroomInfo>() };
            yield return new object[] { MoonPhaseV2.FIRST_QUARTER, new List<MushroomInfo>() };
            yield return new object[] { MoonPhaseV2.WAXING_GIBBOUS, new List<MushroomInfo>() };
        }
    }
}
