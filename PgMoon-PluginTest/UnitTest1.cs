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

    public class MoonPhaseV2 : Enumeration
    {
       public static readonly MoonPhaseV2 UNKNOWN = new MoonPhaseV2(-1, "Unknown Moon", new Tuple<double, double>(-360, -360));
       public static readonly MoonPhaseV2 FULL_MOON = new MoonPhaseV2(0, "Full Moon", new Tuple<double, double>(-180.0, -135.0));
       public static readonly MoonPhaseV2 WANING_GIBBOUS = new MoonPhaseV2(1, "Waning Gibbous", new Tuple<double, double>(-135.0, -90.0));
       public static readonly MoonPhaseV2 LAST_QUARTER = new MoonPhaseV2(2, "Last Quarter", new Tuple<double, double>(-90.0, -45.0));
       public static readonly MoonPhaseV2 WANING_CRESCENT = new MoonPhaseV2(3, "Waning Crescent", new Tuple<double, double>(-45.0, 0.0));
       public static readonly MoonPhaseV2 NEW_MOON = new MoonPhaseV2(4, "New Moon", new Tuple<double, double>(0.0, 45.0));
       public static readonly MoonPhaseV2 WAXING_CRESCENT = new MoonPhaseV2(5, "Waxing Crescent", new Tuple<double, double>(45.0, 90.0));
       public static readonly MoonPhaseV2 FIRST_QUARTER = new MoonPhaseV2(6, "First Quarter", new Tuple<double, double>(90.0, 135.0));
       public static readonly MoonPhaseV2 WAXING_GIBBOUS = new MoonPhaseV2(7, "Waxing Gibbous", new Tuple<double, double>(135.0, 180.0));

        private readonly Tuple<double, double> angleBounds;

        private MoonPhaseV2
        (
            int enumId,
            string enumName,
            Tuple<double, double> angleBounds
        ) : base(enumId, enumName)
        {
            this.angleBounds = angleBounds;
        }

        public Boolean IsAngleWithinLimits(double inputAngle)
        {
            Boolean result = inputAngle >= angleBounds.Item1 && inputAngle < angleBounds.Item2;
            
            if (this.Equals(MoonPhaseV2.WAXING_GIBBOUS))
            {
                result = inputAngle >= angleBounds.Item1 && inputAngle <= angleBounds.Item2;
            }

            return result;
        }

        public static List<MoonPhaseV2> GetAll()
        {
            return new List<MoonPhaseV2>()
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

        public static MoonPhaseV2 From(int enumId)
        {
            MoonPhaseV2 result = GetAll().SingleOrDefault<MoonPhaseV2>
            (
                moonPhase => enumId == moonPhase.Id
            );

            return (result != null) ? result : MoonPhaseV2.UNKNOWN;
        }

        public static MoonPhaseV2 From(double inputAngle)
        {
            MoonPhaseV2 result = GetAll().SingleOrDefault<MoonPhaseV2>
            (
                moonPhase => moonPhase.IsAngleWithinLimits(inputAngle)
            );

            return (result != null) ? result : MoonPhaseV2.UNKNOWN;
        }
    }

    [TestClass]
    public class CalendarToMoonPhase
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
    }
}
