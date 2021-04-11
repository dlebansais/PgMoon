namespace PgMoonTest.Data
{
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using PgMoon.Data;

    [TestClass]
    public class BoatDestinationTest
    {
        [DataTestMethod]
        [DynamicData(nameof(EnumIdReturnsBoatData), DynamicDataSourceType.Method)]
        public void EnumIdReturnsBoatDestination(BoatDestination expectedDestination)
        {
            BoatDestination actualDestination = BoatDestination.From(expectedDestination.Id);
            Assert.AreEqual(expectedDestination, actualDestination);
        }

        public static IEnumerable<object[]> EnumIdReturnsBoatData()
        {
            yield return new object[] { BoatDestination.UNKNOWN };
            yield return new object[] { BoatDestination.SERBULE };
            yield return new object[] { BoatDestination.KUR_MOUNTAINS };
            yield return new object[] { BoatDestination.SUN_VALE };
        }

    }
}
