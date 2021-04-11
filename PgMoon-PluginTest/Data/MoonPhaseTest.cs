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
            yield return new object[] { MoonPhaseV2.FULL_MOON, BoatDestination.UNKNOWN };
        }

    }
}
