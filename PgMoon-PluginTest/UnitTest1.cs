using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

namespace PgMoon_PluginTest
{

    public abstract class Enumeration : IComparable
    {
        public string Name { get; private set; }

        public int Id { get; private set; }

        protected Enumeration(int id, string name) => (Id, Name) = (id, name);

        public override string ToString() => Name;

        // override object.Equals
        public override bool Equals(object obj)
        {
            //
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Enumeration that = (Enumeration)obj;
            return this.Id == that.Id;
        }

        public override int GetHashCode() => Id;

        public int CompareTo(object other) => Id.CompareTo(((Enumeration)other).Id);

        // Other utility methods ...
    }

    public class TestMoonPhase : Enumeration
    {
        public static TestMoonPhase UNKNOWN = new TestMoonPhase(-1, "Unknown Moon");
        public static TestMoonPhase FULL_MOON = new TestMoonPhase(0, "Full Moon");
        public static TestMoonPhase WANING_GIBBOUS = new TestMoonPhase(1, "Waning Gibbous");
        public static TestMoonPhase LAST_QUARTER = new TestMoonPhase(2, "Last Quarter");
        public static TestMoonPhase WANING_CRESCENT = new TestMoonPhase(3, "Waning Crescent");
        public static TestMoonPhase NEW_MOON = new TestMoonPhase(4, "New Moon");
        public static TestMoonPhase WAXING_CRESCENT = new TestMoonPhase(5, "Waxing Crescent");
        public static TestMoonPhase FIRST_QUARTER = new TestMoonPhase(6, "First Quarter");
        public static TestMoonPhase WAXING_GIBBOUS = new TestMoonPhase(7, "Waxing Gibbous");

        private TestMoonPhase(int enumId, string enumName) : base(enumId, enumName)
        {

        }

        public static List<TestMoonPhase> GetAll()
        {
            return new List<TestMoonPhase>()
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

        public static TestMoonPhase From(int enumId)
        {
            TestMoonPhase result = GetAll().SingleOrDefault<TestMoonPhase>
            (
                (moonPhase => enumId == moonPhase.Id)
            );

            return (result != null) ? result : TestMoonPhase.UNKNOWN;
        }

        public static TestMoonPhase From(double inputAngle)
        {
            TestMoonPhase result = GetAll().SingleOrDefault<TestMoonPhase>
            (
                moonPhase => {
                    return moonPhase == TestMoonPhase.FULL_MOON;
                }
            );

            return (result != null) ? result : TestMoonPhase.UNKNOWN;
        }
    }

    [TestClass]
    public class CalendarToMoonPhase
    {

        [DataTestMethod]
        [DynamicData(nameof(EnumIdReturnsMoonPhaseData), DynamicDataSourceType.Method)]
        public void EnumIdReturnsMoonPhase(TestMoonPhase expectedMoonPhase)
        {
            TestMoonPhase actualMoonPhase = TestMoonPhase.From(expectedMoonPhase.Id);
            Assert.AreEqual(expectedMoonPhase, actualMoonPhase);
        }

        public static IEnumerable<object[]> EnumIdReturnsMoonPhaseData()
        {
            yield return new object[] { TestMoonPhase.UNKNOWN };
            yield return new object[] { TestMoonPhase.FULL_MOON };
            yield return new object[] { TestMoonPhase.WANING_GIBBOUS };
            yield return new object[] { TestMoonPhase.LAST_QUARTER };
            yield return new object[] { TestMoonPhase.WANING_CRESCENT };
            yield return new object[] { TestMoonPhase.NEW_MOON };
            yield return new object[] { TestMoonPhase.WAXING_CRESCENT };
            yield return new object[] { TestMoonPhase.FIRST_QUARTER };
            yield return new object[] { TestMoonPhase.WAXING_GIBBOUS };
        }

        [DataTestMethod]
        [DynamicData(nameof(AngleReturnsMoonPhaseData), DynamicDataSourceType.Method)]
        public void AngleReturnsMoonPhase(TestMoonPhase expectedMoonPhase, double inputAngle)
        {
            TestMoonPhase actualMoonPhase = TestMoonPhase.From(inputAngle);
            Assert.AreEqual(expectedMoonPhase, actualMoonPhase);
        }

        public static IEnumerable<object[]> AngleReturnsMoonPhaseData()
        {
            yield return new object[] { TestMoonPhase.FULL_MOON, -180.0 };
            yield return new object[] { TestMoonPhase.FULL_MOON, -134.9 };
        }
    }
}
