using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PgMoon
{
    public class PhaseCalculator
    {
        #region Init
        static PhaseCalculator()
        {
            Singleton = new PhaseCalculator();
        }

        private PhaseCalculator()
        {
            CalculateBasicMoonPhase(MainWindow.Now());
        }

        public static PhaseCalculator Singleton { get; private set; }
        #endregion

        #region Client Interface
        public static void Update()
        {
            Singleton.CalculateBasicMoonPhase(MainWindow.Now());
        }

        public static bool IsCurrent(int CheckMoonMonth, MoonPhase CheckMoonPhase)
        {
            return CheckMoonMonth == MoonMonth && CheckMoonPhase == MoonPhase;
        }
        #endregion

        #region Properties
        public static int MoonMonth
        {
            get { return _MoonMonth; }
            set
            {
                if (_MoonMonth != value)
                {
                    _MoonMonth = value;
                    NotifyThisPropertyChanged();
                }
            }
        }
        private static int _MoonMonth;

        public static MoonPhase MoonPhase
        {
            get { return _MoonPhase; }
            set
            {
                if (_MoonPhase != value)
                {
                    _MoonPhase = value;
                    NotifyThisPropertyChanged();

                    MoonPhase.UpdateCurrent();
                }
            }
        }
        private static MoonPhase _MoonPhase;

        public static double ProgressWithinPhase
        {
            get { return _ProgressWithinPhase; }
            set
            {
                if (_ProgressWithinPhase != value)
                {
                    _ProgressWithinPhase = value;
                    NotifyThisPropertyChanged();
                }
            }
        }
        private static double _ProgressWithinPhase;

        public static double ProgressToFullMoon
        {
            get { return _ProgressToFullMoon; }
            set
            {
                if (_ProgressToFullMoon != value)
                {
                    _ProgressToFullMoon = value;
                    NotifyThisPropertyChanged();
                }
            }
        }
        private static double _ProgressToFullMoon;

        public static TimeSpan TimeToNextPhase
        {
            get { return _TimeToNextPhase; }
            set
            {
                if (_TimeToNextPhase != value)
                {
                    _TimeToNextPhase = value;
                    NotifyThisPropertyChanged();
                }
            }
        }
        private static TimeSpan _TimeToNextPhase;

        public static TimeSpan TimeToFullMoon
        {
            get { return _TimeToFullMoon; }
            set
            {
                if (_TimeToFullMoon != value)
                {
                    _TimeToFullMoon = value;
                    NotifyThisPropertyChanged();
                }
            }
        }
        private static TimeSpan _TimeToFullMoon;
        #endregion

        #region Client Interface
        public static void DateTimeToMoonPhase(DateTime Time, out int pMoonMonth, out MoonPhase pMoonPhase, out DateTime pPhaseStartTime, out DateTime pPhaseEndTime, out double pProgressToFullMoon, out DateTime pNextFullMoonTime)
        {
            int ip = corrected_moon_phase(Time);
            TimeSpan time_increment = TimeSpan.FromMinutes(10);

            pMoonPhase = MoonPhase.MoonPhaseList[ip];

            int previp = ip;
            DateTime PhaseStartTime = Time;
            do
            {
                PhaseStartTime -= time_increment;
                previp = corrected_moon_phase(PhaseStartTime);
            }
            while (previp == ip);

            PhaseStartTime += time_increment;
            pPhaseStartTime = PhaseStartTime;

            pMoonMonth = (int)(to_time_t(PhaseStartTime) / 2551442.861);

            int nextip = ip;
            DateTime PhaseEndTime = Time;
            do
            {
                PhaseEndTime += time_increment;
                nextip = corrected_moon_phase(PhaseEndTime);
            }
            while (nextip == ip);

            pPhaseEndTime = PhaseEndTime;

            if (ip != 4)
            {
                DateTime AfterFullMoonStartTime = PhaseStartTime;
                DateTime FullMoonEndTime = PhaseEndTime;

                if (previp != 4)
                {
                    do
                    {
                        AfterFullMoonStartTime -= time_increment;
                        previp = corrected_moon_phase(AfterFullMoonStartTime);
                    }
                    while (previp != 4);

                    AfterFullMoonStartTime += time_increment;
                }

                if (nextip != 4)
                {
                    do
                    {
                        FullMoonEndTime += time_increment;
                        nextip = corrected_moon_phase(FullMoonEndTime);
                    }
                    while (nextip != 4);
                }

                double Total = (FullMoonEndTime.Ticks - AfterFullMoonStartTime.Ticks);
                double Current = (Time.Ticks - AfterFullMoonStartTime.Ticks);

                pProgressToFullMoon = Current / Total;
                pNextFullMoonTime = FullMoonEndTime;
            }
            else
            {
                pProgressToFullMoon = 0;
                pNextFullMoonTime = DateTime.MinValue;
            }
        }
        #endregion

        #region Unix Time
        private static readonly DateTime UnixReferenceTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private static long to_time_t(DateTime Time)
        {
            return (long)((Time - UnixReferenceTime).TotalSeconds);
        }

        private static DateTime from_time_t(long Time_t)
        {
            return UnixReferenceTime + TimeSpan.FromSeconds(Time_t);
        }
        #endregion

        #region Calculator
        static double OrbitalPeriod = 27.321661;

        private static int corrected_moon_phase(DateTime utcTime)
        {
            // Reference is not GMT but GMT-4
            utcTime -= TimeSpan.FromHours(4);

            // Fallback for very early times.
            if (utcTime < LunationTable[0].NewMoon)
                return (7 - (int)((LunationTable[0].NewMoon - utcTime).TotalDays / OrbitalPeriod)) % 8;

            DateTime[] Moons = new DateTime[5];

            for (int i = 0; i < LunationTable.Length; i++)
            {
                // Gets dates for this cycle.
                Moons[0] = LunationTable[i].NewMoon;
                Moons[1] = LunationTable[i].FirstQuarterMoon;
                Moons[2] = LunationTable[i].FullMoon;
                Moons[3] = LunationTable[i].LastQuarterMoon;
                Moons[4] = i + 1 < LunationTable.Length ? LunationTable[i + 1].NewMoon : LunationTable[i].NewMoon + LunationTable[i].Duration;

                // Apply this offset to all moon phases, the table is off for some reason.
                double d = 17.75;
                for (int j = 0; j < 5; j++)
                    Moons[j] -= TimeSpan.FromHours(d);

                for (int j = 0; j < 4; j++)
                {
                    if ((Moons[j].Hour == 0 && Moons[j].Minute < 10) || (Moons[j].Hour == 23 && Moons[j].Minute > 50))
                        WriteLimitMoon(Moons[j]);

                    if (utcTime >= Moons[j] && utcTime < Moons[j + 1])
                    {
                        // If we're in this particulary part of the cycle, gets a phase number. 0 if at start, 2 if at end, 1 if in the middle.
                        int QuarterPhase = QuarterCycleMoonPhase(Moons[j], utcTime, Moons[j + 1]);

                        // Four parts, this gives a value between 0 and 8. Round it to next cycle if 8.
                        return (QuarterPhase + (j * 2)) % 8;
                    }
                }
            }

            // Fallback for very late times.
            return ((int)((utcTime - Moons[4]).TotalDays / OrbitalPeriod)) % 8;
        }

        private static int QuarterCycleMoonPhase(DateTime startTime, DateTime time, DateTime endTime)
        {
            // Gets the end time of the first phase of this quarter cycle.
            DateTime FirstPhaseEndTime = new DateTime(startTime.Year, startTime.Month, startTime.Day) + TimeSpan.FromDays(3);

            // Still within this first phase?
            if (time < FirstPhaseEndTime)
                return 0;
            else
            {
                // Gets the start time of the last phase of this quarter cycle.
                DateTime LastPhaseStartTime = new DateTime(endTime.Year, endTime.Month, endTime.Day);

                // Already within this last phase? If not, we're in between (a Waxing or Waning phase).
                if (time >= LastPhaseStartTime)
                    return 2;
                else
                    return 1;
            }
        }

        private static void WriteLimitMoon(DateTime t)
        {
        }

        private void CalculateBasicMoonPhase(DateTime Time)
        {
            int NewMoonMonth;
            MoonPhase NewMoonPhase;
            DateTime NewPhaseStartTime;
            DateTime NewPhaseEndTime;
            double NewProgressToFullMoon;
            DateTime NewNextFullMoonTime;
            DateTimeToMoonPhase(Time, out NewMoonMonth, out NewMoonPhase, out NewPhaseStartTime, out NewPhaseEndTime, out NewProgressToFullMoon, out NewNextFullMoonTime);

            double NewProgressWithinPhase = (Time - NewPhaseStartTime).TotalSeconds / (NewPhaseEndTime - NewPhaseStartTime).TotalSeconds;
            TimeSpan NewTimeToNextPhase = NewPhaseEndTime - Time;
            TimeSpan NewTimeToFullMoon = NewNextFullMoonTime - Time;

            MoonMonth = NewMoonMonth;
            MoonPhase = NewMoonPhase;
            ProgressWithinPhase = NewProgressWithinPhase;
            TimeToNextPhase = NewTimeToNextPhase;
            ProgressToFullMoon = NewProgressToFullMoon;
            TimeToFullMoon = NewTimeToFullMoon;
        }
        #endregion

        #region Moon Phase Calendar
        class Lunation
        {
            public int Index;
            public DateTime NewMoon;
            public DateTime FirstQuarterMoon;
            public DateTime FullMoon;
            public DateTime LastQuarterMoon;
            public TimeSpan Duration;
        };

        static Lunation[] LunationTable = new Lunation[]
        {
            new Lunation() { Index = 1077, NewMoon = DateTime.Parse("15/01/2010 02:11:00"), FirstQuarterMoon = DateTime.Parse("23/01/2010 05:53:00"), FullMoon = DateTime.Parse("30/01/2010 01:17:00"), LastQuarterMoon = DateTime.Parse("05/02/2010 18:48:00"), Duration = TimeSpan.Parse("29.19:40:00") },
            new Lunation() { Index = 1078, NewMoon = DateTime.Parse("13/02/2010 21:51:00"), FirstQuarterMoon = DateTime.Parse("21/02/2010 19:42:00"), FullMoon = DateTime.Parse("28/02/2010 11:37:00"), LastQuarterMoon = DateTime.Parse("07/03/2010 10:41:00"), Duration = TimeSpan.Parse("29.18:10:00") },
            new Lunation() { Index = 1079, NewMoon = DateTime.Parse("15/03/2010 17:01:00"), FirstQuarterMoon = DateTime.Parse("23/03/2010 07:00:00"), FullMoon = DateTime.Parse("29/03/2010 22:25:00"), LastQuarterMoon = DateTime.Parse("06/04/2010 05:36:00"), Duration = TimeSpan.Parse("29.15:28:00") },
            new Lunation() { Index = 1080, NewMoon = DateTime.Parse("14/04/2010 08:28:00"), FirstQuarterMoon = DateTime.Parse("21/04/2010 14:19:00"), FullMoon = DateTime.Parse("28/04/2010 08:18:00"), LastQuarterMoon = DateTime.Parse("06/05/2010 00:14:00"), Duration = TimeSpan.Parse("29.12:35:00") },
            new Lunation() { Index = 1081, NewMoon = DateTime.Parse("13/05/2010 21:04:00"), FirstQuarterMoon = DateTime.Parse("20/05/2010 19:42:00"), FullMoon = DateTime.Parse("27/05/2010 19:07:00"), LastQuarterMoon = DateTime.Parse("04/06/2010 18:13:00"), Duration = TimeSpan.Parse("29.10:10:00") },
            new Lunation() { Index = 1082, NewMoon = DateTime.Parse("12/06/2010 07:14:00"), FirstQuarterMoon = DateTime.Parse("19/06/2010 00:29:00"), FullMoon = DateTime.Parse("26/06/2010 07:30:00"), LastQuarterMoon = DateTime.Parse("04/07/2010 10:35:00"), Duration = TimeSpan.Parse("29.08:26:00") },
            new Lunation() { Index = 1083, NewMoon = DateTime.Parse("11/07/2010 15:40:00"), FirstQuarterMoon = DateTime.Parse("18/07/2010 06:10:00"), FullMoon = DateTime.Parse("25/07/2010 21:36:00"), LastQuarterMoon = DateTime.Parse("03/08/2010 00:58:00"), Duration = TimeSpan.Parse("29.07:28:00") },
            new Lunation() { Index = 1084, NewMoon = DateTime.Parse("09/08/2010 23:08:00"), FirstQuarterMoon = DateTime.Parse("16/08/2010 14:13:00"), FullMoon = DateTime.Parse("24/08/2010 13:04:00"), LastQuarterMoon = DateTime.Parse("01/09/2010 13:21:00"), Duration = TimeSpan.Parse("29.07:22:00") },
            new Lunation() { Index = 1085, NewMoon = DateTime.Parse("08/09/2010 06:29:00"), FirstQuarterMoon = DateTime.Parse("15/09/2010 01:49:00"), FullMoon = DateTime.Parse("23/09/2010 05:17:00"), LastQuarterMoon = DateTime.Parse("30/09/2010 23:52:00"), Duration = TimeSpan.Parse("29.08:15:00") },
            new Lunation() { Index = 1086, NewMoon = DateTime.Parse("07/10/2010 14:44:00"), FirstQuarterMoon = DateTime.Parse("14/10/2010 17:27:00"), FullMoon = DateTime.Parse("22/10/2010 21:36:00"), LastQuarterMoon = DateTime.Parse("30/10/2010 08:45:00"), Duration = TimeSpan.Parse("29.10:07:00") },
            new Lunation() { Index = 1087, NewMoon = DateTime.Parse("06/11/2010 00:51:00"), FirstQuarterMoon = DateTime.Parse("13/11/2010 11:38:00"), FullMoon = DateTime.Parse("21/11/2010 12:27:00"), LastQuarterMoon = DateTime.Parse("28/11/2010 15:36:00"), Duration = TimeSpan.Parse("29.12:44:00") },
            new Lunation() { Index = 1088, NewMoon = DateTime.Parse("05/12/2010 12:35:00"), FirstQuarterMoon = DateTime.Parse("13/12/2010 08:58:00"), FullMoon = DateTime.Parse("21/12/2010 03:13:00"), LastQuarterMoon = DateTime.Parse("27/12/2010 23:18:00"), Duration = TimeSpan.Parse("29.15:27:00") },
            new Lunation() { Index = 1089, NewMoon = DateTime.Parse("04/01/2011 04:02:00"), FirstQuarterMoon = DateTime.Parse("12/01/2011 06:31:00"), FullMoon = DateTime.Parse("19/01/2011 16:21:00"), LastQuarterMoon = DateTime.Parse("26/01/2011 07:57:00"), Duration = TimeSpan.Parse("29.17:28:00") },
            new Lunation() { Index = 1090, NewMoon = DateTime.Parse("02/02/2011 21:30:00"), FirstQuarterMoon = DateTime.Parse("11/02/2011 02:18:00"), FullMoon = DateTime.Parse("18/02/2011 03:35:00"), LastQuarterMoon = DateTime.Parse("24/02/2011 18:26:00"), Duration = TimeSpan.Parse("29.18:15:00") },
            new Lunation() { Index = 1091, NewMoon = DateTime.Parse("04/03/2011 15:45:00"), FirstQuarterMoon = DateTime.Parse("12/03/2011 18:44:00"), FullMoon = DateTime.Parse("19/03/2011 14:10:00"), LastQuarterMoon = DateTime.Parse("26/03/2011 08:07:00"), Duration = TimeSpan.Parse("29.17:46:00") },
            new Lunation() { Index = 1092, NewMoon = DateTime.Parse("03/04/2011 10:32:00"), FirstQuarterMoon = DateTime.Parse("11/04/2011 08:05:00"), FullMoon = DateTime.Parse("17/04/2011 22:44:00"), LastQuarterMoon = DateTime.Parse("24/04/2011 22:46:00"), Duration = TimeSpan.Parse("29.16:18:00") },
            new Lunation() { Index = 1093, NewMoon = DateTime.Parse("03/05/2011 02:50:00"), FirstQuarterMoon = DateTime.Parse("10/05/2011 16:32:00"), FullMoon = DateTime.Parse("17/05/2011 07:08:00"), LastQuarterMoon = DateTime.Parse("24/05/2011 14:52:00"), Duration = TimeSpan.Parse("29.14:12:00") },
            new Lunation() { Index = 1094, NewMoon = DateTime.Parse("01/06/2011 17:02:00"), FirstQuarterMoon = DateTime.Parse("08/06/2011 22:10:00"), FullMoon = DateTime.Parse("15/06/2011 16:13:00"), LastQuarterMoon = DateTime.Parse("23/06/2011 07:48:00"), Duration = TimeSpan.Parse("29.11:51:00") },
            new Lunation() { Index = 1095, NewMoon = DateTime.Parse("01/07/2011 04:53:00"), FirstQuarterMoon = DateTime.Parse("08/07/2011 02:29:00"), FullMoon = DateTime.Parse("15/07/2011 02:39:00"), LastQuarterMoon = DateTime.Parse("23/07/2011 01:01:00"), Duration = TimeSpan.Parse("29.09:46:00") },
            new Lunation() { Index = 1096, NewMoon = DateTime.Parse("30/07/2011 14:39:00"), FirstQuarterMoon = DateTime.Parse("06/08/2011 07:08:00"), FullMoon = DateTime.Parse("13/08/2011 14:57:00"), LastQuarterMoon = DateTime.Parse("21/08/2011 17:54:00"), Duration = TimeSpan.Parse("29.08:24:00") },
            new Lunation() { Index = 1097, NewMoon = DateTime.Parse("28/08/2011 23:04:00"), FirstQuarterMoon = DateTime.Parse("04/09/2011 13:39:00"), FullMoon = DateTime.Parse("12/09/2011 05:26:00"), LastQuarterMoon = DateTime.Parse("20/09/2011 09:38:00"), Duration = TimeSpan.Parse("29.08:05:00") },
            new Lunation() { Index = 1098, NewMoon = DateTime.Parse("27/09/2011 07:08:00"), FirstQuarterMoon = DateTime.Parse("03/10/2011 23:15:00"), FullMoon = DateTime.Parse("11/10/2011 22:05:00"), LastQuarterMoon = DateTime.Parse("19/10/2011 23:30:00"), Duration = TimeSpan.Parse("29.08:47:00") },
            new Lunation() { Index = 1099, NewMoon = DateTime.Parse("26/10/2011 15:55:00"), FirstQuarterMoon = DateTime.Parse("02/11/2011 12:38:00"), FullMoon = DateTime.Parse("10/11/2011 15:16:00"), LastQuarterMoon = DateTime.Parse("18/11/2011 10:09:00"), Duration = TimeSpan.Parse("29.10:14:00") },
            new Lunation() { Index = 1100, NewMoon = DateTime.Parse("25/11/2011 01:09:00"), FirstQuarterMoon = DateTime.Parse("02/12/2011 04:52:00"), FullMoon = DateTime.Parse("10/12/2011 09:36:00"), LastQuarterMoon = DateTime.Parse("17/12/2011 19:47:00"), Duration = TimeSpan.Parse("29.11:57:00") },
            new Lunation() { Index = 1101, NewMoon = DateTime.Parse("24/12/2011 13:06:00"), FirstQuarterMoon = DateTime.Parse("01/01/2012 01:14:00"), FullMoon = DateTime.Parse("09/01/2012 02:30:00"), LastQuarterMoon = DateTime.Parse("16/01/2012 04:07:00"), Duration = TimeSpan.Parse("29.13:33:00") },
            new Lunation() { Index = 1102, NewMoon = DateTime.Parse("23/01/2012 02:39:00"), FirstQuarterMoon = DateTime.Parse("30/01/2012 23:09:00"), FullMoon = DateTime.Parse("07/02/2012 16:53:00"), LastQuarterMoon = DateTime.Parse("14/02/2012 12:03:00"), Duration = TimeSpan.Parse("29.14:55:00") },
            new Lunation() { Index = 1103, NewMoon = DateTime.Parse("21/02/2012 17:34:00"), FirstQuarterMoon = DateTime.Parse("29/02/2012 20:21:00"), FullMoon = DateTime.Parse("08/03/2012 04:39:00"), LastQuarterMoon = DateTime.Parse("14/03/2012 21:25:00"), Duration = TimeSpan.Parse("29.16:03:00") },
            new Lunation() { Index = 1104, NewMoon = DateTime.Parse("22/03/2012 10:37:00"), FirstQuarterMoon = DateTime.Parse("30/03/2012 15:40:00"), FullMoon = DateTime.Parse("06/04/2012 15:18:00"), LastQuarterMoon = DateTime.Parse("13/04/2012 06:49:00"), Duration = TimeSpan.Parse("29.16:41:00") },
            new Lunation() { Index = 1105, NewMoon = DateTime.Parse("21/04/2012 03:18:00"), FirstQuarterMoon = DateTime.Parse("29/04/2012 05:57:00"), FullMoon = DateTime.Parse("05/05/2012 23:35:00"), LastQuarterMoon = DateTime.Parse("12/05/2012 17:46:00"), Duration = TimeSpan.Parse("29.16:29:00") },
            new Lunation() { Index = 1106, NewMoon = DateTime.Parse("20/05/2012 19:47:00"), FirstQuarterMoon = DateTime.Parse("28/05/2012 16:16:00"), FullMoon = DateTime.Parse("04/06/2012 07:11:00"), LastQuarterMoon = DateTime.Parse("11/06/2012 06:41:00"), Duration = TimeSpan.Parse("29.15:15:00") },
            new Lunation() { Index = 1107, NewMoon = DateTime.Parse("19/06/2012 11:02:00"), FirstQuarterMoon = DateTime.Parse("26/06/2012 23:30:00"), FullMoon = DateTime.Parse("03/07/2012 14:51:00"), LastQuarterMoon = DateTime.Parse("10/07/2012 21:47:00"), Duration = TimeSpan.Parse("29.13:22:00") },
            new Lunation() { Index = 1108, NewMoon = DateTime.Parse("19/07/2012 00:23:00"), FirstQuarterMoon = DateTime.Parse("26/07/2012 04:56:00"), FullMoon = DateTime.Parse("01/08/2012 23:27:00"), LastQuarterMoon = DateTime.Parse("09/08/2012 14:55:00"), Duration = TimeSpan.Parse("29.11:30:00") },
            new Lunation() { Index = 1109, NewMoon = DateTime.Parse("17/08/2012 11:54:00"), FirstQuarterMoon = DateTime.Parse("24/08/2012 09:53:00"), FullMoon = DateTime.Parse("31/08/2012 09:58:00"), LastQuarterMoon = DateTime.Parse("08/09/2012 09:15:00"), Duration = TimeSpan.Parse("29.10:16:00") },
            new Lunation() { Index = 1110, NewMoon = DateTime.Parse("15/09/2012 22:10:00"), FirstQuarterMoon = DateTime.Parse("22/09/2012 15:40:00"), FullMoon = DateTime.Parse("29/09/2012 23:18:00"), LastQuarterMoon = DateTime.Parse("08/10/2012 03:33:00"), Duration = TimeSpan.Parse("29.09:52:00") },
            new Lunation() { Index = 1111, NewMoon = DateTime.Parse("15/10/2012 08:02:00"), FirstQuarterMoon = DateTime.Parse("21/10/2012 23:31:00"), FullMoon = DateTime.Parse("29/10/2012 15:49:00"), LastQuarterMoon = DateTime.Parse("06/11/2012 19:35:00"), Duration = TimeSpan.Parse("29.10:05:00") },
            new Lunation() { Index = 1112, NewMoon = DateTime.Parse("13/11/2012 17:08:00"), FirstQuarterMoon = DateTime.Parse("20/11/2012 09:31:00"), FullMoon = DateTime.Parse("28/11/2012 09:45:00"), LastQuarterMoon = DateTime.Parse("06/12/2012 10:31:00"), Duration = TimeSpan.Parse("29.10:34:00") },
            new Lunation() { Index = 1113, NewMoon = DateTime.Parse("13/12/2012 03:41:00"), FirstQuarterMoon = DateTime.Parse("20/12/2012 00:19:00"), FullMoon = DateTime.Parse("28/12/2012 05:21:00"), LastQuarterMoon = DateTime.Parse("04/01/2013 22:57:00"), Duration = TimeSpan.Parse("29.11:02:00") },
            new Lunation() { Index = 1114, NewMoon = DateTime.Parse("11/01/2013 14:43:00"), FirstQuarterMoon = DateTime.Parse("18/01/2013 18:45:00"), FullMoon = DateTime.Parse("26/01/2013 23:38:00"), LastQuarterMoon = DateTime.Parse("03/02/2013 08:56:00"), Duration = TimeSpan.Parse("29.11:37:00") },
            new Lunation() { Index = 1115, NewMoon = DateTime.Parse("10/02/2013 02:20:00"), FirstQuarterMoon = DateTime.Parse("17/02/2013 15:30:00"), FullMoon = DateTime.Parse("25/02/2013 15:26:00"), LastQuarterMoon = DateTime.Parse("04/03/2013 16:52:00"), Duration = TimeSpan.Parse("29.12:31:00") },
            new Lunation() { Index = 1116, NewMoon = DateTime.Parse("11/03/2013 15:50:00"), FirstQuarterMoon = DateTime.Parse("19/03/2013 13:26:00"), FullMoon = DateTime.Parse("27/03/2013 05:27:00"), LastQuarterMoon = DateTime.Parse("03/04/2013 00:36:00"), Duration = TimeSpan.Parse("29.13:44:00") },
            new Lunation() { Index = 1117, NewMoon = DateTime.Parse("10/04/2013 05:35:00"), FirstQuarterMoon = DateTime.Parse("18/04/2013 08:30:00"), FullMoon = DateTime.Parse("25/04/2013 15:57:00"), LastQuarterMoon = DateTime.Parse("02/05/2013 07:14:00"), Duration = TimeSpan.Parse("29.14:53:00") },
            new Lunation() { Index = 1118, NewMoon = DateTime.Parse("09/05/2013 20:28:00"), FirstQuarterMoon = DateTime.Parse("18/05/2013 00:34:00"), FullMoon = DateTime.Parse("25/05/2013 00:24:00"), LastQuarterMoon = DateTime.Parse("31/05/2013 14:58:00"), Duration = TimeSpan.Parse("29.15:28:00") },
            new Lunation() { Index = 1119, NewMoon = DateTime.Parse("08/06/2013 11:56:00"), FirstQuarterMoon = DateTime.Parse("16/06/2013 13:23:00"), FullMoon = DateTime.Parse("23/06/2013 07:32:00"), LastQuarterMoon = DateTime.Parse("30/06/2013 00:53:00"), Duration = TimeSpan.Parse("29.15:18:00") },
            new Lunation() { Index = 1120, NewMoon = DateTime.Parse("08/07/2013 03:14:00"), FirstQuarterMoon = DateTime.Parse("15/07/2013 23:18:00"), FullMoon = DateTime.Parse("22/07/2013 14:15:00"), LastQuarterMoon = DateTime.Parse("29/07/2013 13:43:00"), Duration = TimeSpan.Parse("29.14:36:00") },
            new Lunation() { Index = 1121, NewMoon = DateTime.Parse("06/08/2013 17:50:00"), FirstQuarterMoon = DateTime.Parse("14/08/2013 06:56:00"), FullMoon = DateTime.Parse("20/08/2013 21:44:00"), LastQuarterMoon = DateTime.Parse("28/08/2013 05:34:00"), Duration = TimeSpan.Parse("29.13:45:00") },
            new Lunation() { Index = 1122, NewMoon = DateTime.Parse("05/09/2013 07:36:00"), FirstQuarterMoon = DateTime.Parse("12/09/2013 13:08:00"), FullMoon = DateTime.Parse("19/09/2013 07:12:00"), LastQuarterMoon = DateTime.Parse("26/09/2013 23:55:00"), Duration = TimeSpan.Parse("29.12:58:00") },
            new Lunation() { Index = 1123, NewMoon = DateTime.Parse("04/10/2013 20:34:00"), FirstQuarterMoon = DateTime.Parse("11/10/2013 19:02:00"), FullMoon = DateTime.Parse("18/10/2013 19:37:00"), LastQuarterMoon = DateTime.Parse("26/10/2013 19:40:00"), Duration = TimeSpan.Parse("29.12:15:00") },
            new Lunation() { Index = 1124, NewMoon = DateTime.Parse("03/11/2013 07:50:00"), FirstQuarterMoon = DateTime.Parse("10/11/2013 00:57:00"), FullMoon = DateTime.Parse("17/11/2013 10:15:00"), LastQuarterMoon = DateTime.Parse("25/11/2013 14:27:00"), Duration = TimeSpan.Parse("29.11:32:00") },
            new Lunation() { Index = 1125, NewMoon = DateTime.Parse("02/12/2013 19:22:00"), FirstQuarterMoon = DateTime.Parse("09/12/2013 10:11:00"), FullMoon = DateTime.Parse("17/12/2013 04:28:00"), LastQuarterMoon = DateTime.Parse("25/12/2013 08:47:00"), Duration = TimeSpan.Parse("29.10:52:00") },
            new Lunation() { Index = 1126, NewMoon = DateTime.Parse("01/01/2014 06:14:00"), FirstQuarterMoon = DateTime.Parse("07/01/2014 22:39:00"), FullMoon = DateTime.Parse("15/01/2014 23:52:00"), LastQuarterMoon = DateTime.Parse("24/01/2014 00:18:00"), Duration = TimeSpan.Parse("29.10:24:00") },
            new Lunation() { Index = 1127, NewMoon = DateTime.Parse("30/01/2014 16:38:00"), FirstQuarterMoon = DateTime.Parse("06/02/2014 14:22:00"), FullMoon = DateTime.Parse("14/02/2014 18:52:00"), LastQuarterMoon = DateTime.Parse("22/02/2014 12:15:00"), Duration = TimeSpan.Parse("29.10:21:00") },
            new Lunation() { Index = 1128, NewMoon = DateTime.Parse("01/03/2014 02:59:00"), FirstQuarterMoon = DateTime.Parse("08/03/2014 08:26:00"), FullMoon = DateTime.Parse("16/03/2014 13:08:00"), LastQuarterMoon = DateTime.Parse("23/03/2014 21:46:00"), Duration = TimeSpan.Parse("29.10:45:00") },
            new Lunation() { Index = 1129, NewMoon = DateTime.Parse("30/03/2014 14:44:00"), FirstQuarterMoon = DateTime.Parse("07/04/2014 04:30:00"), FullMoon = DateTime.Parse("15/04/2014 03:42:00"), LastQuarterMoon = DateTime.Parse("22/04/2014 03:51:00"), Duration = TimeSpan.Parse("29.11:30:00") },
            new Lunation() { Index = 1130, NewMoon = DateTime.Parse("29/04/2014 02:14:00"), FirstQuarterMoon = DateTime.Parse("06/05/2014 23:14:00"), FullMoon = DateTime.Parse("14/05/2014 15:15:00"), LastQuarterMoon = DateTime.Parse("21/05/2014 08:59:00"), Duration = TimeSpan.Parse("29.12:26:00") },
            new Lunation() { Index = 1131, NewMoon = DateTime.Parse("28/05/2014 14:40:00"), FirstQuarterMoon = DateTime.Parse("05/06/2014 16:38:00"), FullMoon = DateTime.Parse("13/06/2014 00:11:00"), LastQuarterMoon = DateTime.Parse("19/06/2014 14:38:00"), Duration = TimeSpan.Parse("29.13:28:00") },
            new Lunation() { Index = 1132, NewMoon = DateTime.Parse("27/06/2014 04:08:00"), FirstQuarterMoon = DateTime.Parse("05/07/2014 07:58:00"), FullMoon = DateTime.Parse("12/07/2014 07:24:00"), LastQuarterMoon = DateTime.Parse("18/07/2014 22:08:00"), Duration = TimeSpan.Parse("29.14:33:00") },
            new Lunation() { Index = 1133, NewMoon = DateTime.Parse("26/07/2014 18:41:00"), FirstQuarterMoon = DateTime.Parse("03/08/2014 20:49:00"), FullMoon = DateTime.Parse("10/08/2014 14:09:00"), LastQuarterMoon = DateTime.Parse("17/08/2014 08:25:00"), Duration = TimeSpan.Parse("29.15:31:00") },
            new Lunation() { Index = 1134, NewMoon = DateTime.Parse("25/08/2014 10:12:00"), FirstQuarterMoon = DateTime.Parse("02/09/2014 07:11:00"), FullMoon = DateTime.Parse("08/09/2014 21:38:00"), LastQuarterMoon = DateTime.Parse("15/09/2014 22:04:00"), Duration = TimeSpan.Parse("29.16:01:00") },
            new Lunation() { Index = 1135, NewMoon = DateTime.Parse("24/09/2014 02:13:00"), FirstQuarterMoon = DateTime.Parse("01/10/2014 15:32:00"), FullMoon = DateTime.Parse("08/10/2014 06:50:00"), LastQuarterMoon = DateTime.Parse("15/10/2014 15:12:00"), Duration = TimeSpan.Parse("29.15:43:00") },
            new Lunation() { Index = 1136, NewMoon = DateTime.Parse("23/10/2014 17:56:00"), FirstQuarterMoon = DateTime.Parse("30/10/2014 22:48:00"), FullMoon = DateTime.Parse("06/11/2014 17:22:00"), LastQuarterMoon = DateTime.Parse("14/11/2014 10:15:00"), Duration = TimeSpan.Parse("29.14:36:00") },
            new Lunation() { Index = 1137, NewMoon = DateTime.Parse("22/11/2014 07:32:00"), FirstQuarterMoon = DateTime.Parse("29/11/2014 05:06:00"), FullMoon = DateTime.Parse("06/12/2014 07:26:00"), LastQuarterMoon = DateTime.Parse("14/12/2014 07:50:00"), Duration = TimeSpan.Parse("29.13:04:00") },
            new Lunation() { Index = 1138, NewMoon = DateTime.Parse("21/12/2014 20:35:00"), FirstQuarterMoon = DateTime.Parse("28/12/2014 13:31:00"), FullMoon = DateTime.Parse("04/01/2015 23:53:00"), LastQuarterMoon = DateTime.Parse("13/01/2015 04:46:00"), Duration = TimeSpan.Parse("29.11:38:00") },
            new Lunation() { Index = 1139, NewMoon = DateTime.Parse("20/01/2015 08:13:00"), FirstQuarterMoon = DateTime.Parse("26/01/2015 23:48:00"), FullMoon = DateTime.Parse("03/02/2015 18:08:00"), LastQuarterMoon = DateTime.Parse("11/02/2015 22:49:00"), Duration = TimeSpan.Parse("29.10:34:00") },
            new Lunation() { Index = 1140, NewMoon = DateTime.Parse("18/02/2015 18:47:00"), FirstQuarterMoon = DateTime.Parse("25/02/2015 12:14:00"), FullMoon = DateTime.Parse("05/03/2015 13:05:00"), LastQuarterMoon = DateTime.Parse("13/03/2015 13:47:00"), Duration = TimeSpan.Parse("29.09:49:00") },
            new Lunation() { Index = 1141, NewMoon = DateTime.Parse("20/03/2015 05:36:00"), FirstQuarterMoon = DateTime.Parse("27/03/2015 03:42:00"), FullMoon = DateTime.Parse("04/04/2015 08:05:00"), LastQuarterMoon = DateTime.Parse("11/04/2015 23:44:00"), Duration = TimeSpan.Parse("29.09:21:00") },
            new Lunation() { Index = 1142, NewMoon = DateTime.Parse("18/04/2015 14:56:00"), FirstQuarterMoon = DateTime.Parse("25/04/2015 19:55:00"), FullMoon = DateTime.Parse("03/05/2015 23:42:00"), LastQuarterMoon = DateTime.Parse("11/05/2015 06:36:00"), Duration = TimeSpan.Parse("29.09:16:00") },
            new Lunation() { Index = 1143, NewMoon = DateTime.Parse("18/05/2015 00:13:00"), FirstQuarterMoon = DateTime.Parse("25/05/2015 13:18:00"), FullMoon = DateTime.Parse("02/06/2015 12:19:00"), LastQuarterMoon = DateTime.Parse("09/06/2015 11:41:00"), Duration = TimeSpan.Parse("29.09:52:00") },
            new Lunation() { Index = 1144, NewMoon = DateTime.Parse("16/06/2015 10:05:00"), FirstQuarterMoon = DateTime.Parse("24/06/2015 07:02:00"), FullMoon = DateTime.Parse("01/07/2015 22:19:00"), LastQuarterMoon = DateTime.Parse("08/07/2015 16:24:00"), Duration = TimeSpan.Parse("29.11:19:00") },
            new Lunation() { Index = 1145, NewMoon = DateTime.Parse("15/07/2015 21:24:00"), FirstQuarterMoon = DateTime.Parse("24/07/2015 00:04:00"), FullMoon = DateTime.Parse("31/07/2015 06:42:00"), LastQuarterMoon = DateTime.Parse("06/08/2015 22:02:00"), Duration = TimeSpan.Parse("29.13:29:00") },
            new Lunation() { Index = 1146, NewMoon = DateTime.Parse("14/08/2015 10:53:00"), FirstQuarterMoon = DateTime.Parse("22/08/2015 15:31:00"), FullMoon = DateTime.Parse("29/08/2015 14:35:00"), LastQuarterMoon = DateTime.Parse("05/09/2015 05:54:00"), Duration = TimeSpan.Parse("29.15:48:00") },
            new Lunation() { Index = 1147, NewMoon = DateTime.Parse("13/09/2015 02:41:00"), FirstQuarterMoon = DateTime.Parse("21/09/2015 04:59:00"), FullMoon = DateTime.Parse("27/09/2015 22:50:00"), LastQuarterMoon = DateTime.Parse("04/10/2015 17:06:00"), Duration = TimeSpan.Parse("29.17:24:00") },
            new Lunation() { Index = 1148, NewMoon = DateTime.Parse("12/10/2015 20:05:00"), FirstQuarterMoon = DateTime.Parse("20/10/2015 16:31:00"), FullMoon = DateTime.Parse("27/10/2015 08:05:00"), LastQuarterMoon = DateTime.Parse("03/11/2015 07:23:00"), Duration = TimeSpan.Parse("29.17:41:00") },
            new Lunation() { Index = 1149, NewMoon = DateTime.Parse("11/11/2015 12:47:00"), FirstQuarterMoon = DateTime.Parse("19/11/2015 01:27:00"), FullMoon = DateTime.Parse("25/11/2015 17:44:00"), LastQuarterMoon = DateTime.Parse("03/12/2015 02:40:00"), Duration = TimeSpan.Parse("29.16:42:00") },
            new Lunation() { Index = 1150, NewMoon = DateTime.Parse("11/12/2015 05:29:00"), FirstQuarterMoon = DateTime.Parse("18/12/2015 10:14:00"), FullMoon = DateTime.Parse("25/12/2015 06:11:00"), LastQuarterMoon = DateTime.Parse("02/01/2016 00:30:00"), Duration = TimeSpan.Parse("29.15:01:00") },
            new Lunation() { Index = 1151, NewMoon = DateTime.Parse("09/01/2016 20:30:00"), FirstQuarterMoon = DateTime.Parse("16/01/2016 18:26:00"), FullMoon = DateTime.Parse("23/01/2016 20:45:00"), LastQuarterMoon = DateTime.Parse("31/01/2016 22:27:00"), Duration = TimeSpan.Parse("29.13:08:00") },
            new Lunation() { Index = 1152, NewMoon = DateTime.Parse("08/02/2016 09:38:00"), FirstQuarterMoon = DateTime.Parse("15/02/2016 02:46:00"), FullMoon = DateTime.Parse("22/02/2016 13:19:00"), LastQuarterMoon = DateTime.Parse("01/03/2016 18:10:00"), Duration = TimeSpan.Parse("29.11:16:00") },
            new Lunation() { Index = 1153, NewMoon = DateTime.Parse("08/03/2016 20:54:00"), FirstQuarterMoon = DateTime.Parse("15/03/2016 13:02:00"), FullMoon = DateTime.Parse("23/03/2016 08:00:00"), LastQuarterMoon = DateTime.Parse("31/03/2016 11:16:00"), Duration = TimeSpan.Parse("29.09:29:00") },
            new Lunation() { Index = 1154, NewMoon = DateTime.Parse("07/04/2016 07:23:00"), FirstQuarterMoon = DateTime.Parse("13/04/2016 23:59:00"), FullMoon = DateTime.Parse("22/04/2016 01:23:00"), LastQuarterMoon = DateTime.Parse("29/04/2016 23:28:00"), Duration = TimeSpan.Parse("29.08:06:00") },
            new Lunation() { Index = 1155, NewMoon = DateTime.Parse("06/05/2016 15:29:00"), FirstQuarterMoon = DateTime.Parse("13/05/2016 13:02:00"), FullMoon = DateTime.Parse("21/05/2016 17:14:00"), LastQuarterMoon = DateTime.Parse("29/05/2016 08:11:00"), Duration = TimeSpan.Parse("29.07:30:00") },
            new Lunation() { Index = 1156, NewMoon = DateTime.Parse("04/06/2016 22:59:00"), FirstQuarterMoon = DateTime.Parse("12/06/2016 04:09:00"), FullMoon = DateTime.Parse("20/06/2016 07:02:00"), LastQuarterMoon = DateTime.Parse("27/06/2016 14:18:00"), Duration = TimeSpan.Parse("29.08:01:00") },
            new Lunation() { Index = 1157, NewMoon = DateTime.Parse("04/07/2016 07:01:00"), FirstQuarterMoon = DateTime.Parse("11/07/2016 20:51:00"), FullMoon = DateTime.Parse("19/07/2016 18:56:00"), LastQuarterMoon = DateTime.Parse("26/07/2016 18:59:00"), Duration = TimeSpan.Parse("29.09:44:00") },
            new Lunation() { Index = 1158, NewMoon = DateTime.Parse("02/08/2016 16:44:00"), FirstQuarterMoon = DateTime.Parse("10/08/2016 14:20:00"), FullMoon = DateTime.Parse("18/08/2016 05:26:00"), LastQuarterMoon = DateTime.Parse("24/08/2016 23:40:00"), Duration = TimeSpan.Parse("29.12:19:00") },
            new Lunation() { Index = 1159, NewMoon = DateTime.Parse("01/09/2016 05:03:00"), FirstQuarterMoon = DateTime.Parse("09/09/2016 07:48:00"), FullMoon = DateTime.Parse("16/09/2016 15:05:00"), LastQuarterMoon = DateTime.Parse("23/09/2016 05:56:00"), Duration = TimeSpan.Parse("29.15:08:00") },
            new Lunation() { Index = 1160, NewMoon = DateTime.Parse("30/09/2016 20:11:00"), FirstQuarterMoon = DateTime.Parse("09/10/2016 00:32:00"), FullMoon = DateTime.Parse("16/10/2016 00:23:00"), LastQuarterMoon = DateTime.Parse("22/10/2016 15:13:00"), Duration = TimeSpan.Parse("29.17:27:00") },
            new Lunation() { Index = 1161, NewMoon = DateTime.Parse("30/10/2016 13:38:00"), FirstQuarterMoon = DateTime.Parse("07/11/2016 14:51:00"), FullMoon = DateTime.Parse("14/11/2016 08:52:00"), LastQuarterMoon = DateTime.Parse("21/11/2016 03:33:00"), Duration = TimeSpan.Parse("29.18:40:00") },
            new Lunation() { Index = 1162, NewMoon = DateTime.Parse("29/11/2016 07:18:00"), FirstQuarterMoon = DateTime.Parse("07/12/2016 04:03:00"), FullMoon = DateTime.Parse("13/12/2016 19:05:00"), LastQuarterMoon = DateTime.Parse("20/12/2016 20:55:00"), Duration = TimeSpan.Parse("29.18:35:00") },
            new Lunation() { Index = 1163, NewMoon = DateTime.Parse("29/12/2016 01:53:00"), FirstQuarterMoon = DateTime.Parse("05/01/2017 14:46:00"), FullMoon = DateTime.Parse("12/01/2017 06:33:00"), LastQuarterMoon = DateTime.Parse("19/01/2017 17:13:00"), Duration = TimeSpan.Parse("29.17:14:00") },
            new Lunation() { Index = 1164, NewMoon = DateTime.Parse("27/01/2017 19:07:00"), FirstQuarterMoon = DateTime.Parse("03/02/2017 23:18:00"), FullMoon = DateTime.Parse("10/02/2017 19:32:00"), LastQuarterMoon = DateTime.Parse("18/02/2017 14:33:00"), Duration = TimeSpan.Parse("29.14:51:00") },
            new Lunation() { Index = 1165, NewMoon = DateTime.Parse("26/02/2017 09:58:00"), FirstQuarterMoon = DateTime.Parse("05/03/2017 06:32:00"), FullMoon = DateTime.Parse("12/03/2017 10:53:00"), LastQuarterMoon = DateTime.Parse("20/03/2017 11:58:00"), Duration = TimeSpan.Parse("29.11:59:00") },
            new Lunation() { Index = 1166, NewMoon = DateTime.Parse("27/03/2017 22:57:00"), FirstQuarterMoon = DateTime.Parse("03/04/2017 14:39:00"), FullMoon = DateTime.Parse("11/04/2017 02:08:00"), LastQuarterMoon = DateTime.Parse("19/04/2017 05:56:00"), Duration = TimeSpan.Parse("29.09:19:00") },
            new Lunation() { Index = 1167, NewMoon = DateTime.Parse("26/04/2017 08:16:00"), FirstQuarterMoon = DateTime.Parse("02/05/2017 22:46:00"), FullMoon = DateTime.Parse("10/05/2017 17:42:00"), LastQuarterMoon = DateTime.Parse("18/05/2017 20:32:00"), Duration = TimeSpan.Parse("29.07:28:00") },
            new Lunation() { Index = 1168, NewMoon = DateTime.Parse("25/05/2017 15:44:00"), FirstQuarterMoon = DateTime.Parse("01/06/2017 08:42:00"), FullMoon = DateTime.Parse("09/06/2017 09:09:00"), LastQuarterMoon = DateTime.Parse("17/06/2017 07:32:00"), Duration = TimeSpan.Parse("29.06:46:00") },
            new Lunation() { Index = 1169, NewMoon = DateTime.Parse("23/06/2017 22:30:00"), FirstQuarterMoon = DateTime.Parse("30/06/2017 20:51:00"), FullMoon = DateTime.Parse("09/07/2017 00:06:00"), LastQuarterMoon = DateTime.Parse("16/07/2017 15:25:00"), Duration = TimeSpan.Parse("29.07:15:00") },
            new Lunation() { Index = 1170, NewMoon = DateTime.Parse("23/07/2017 05:45:00"), FirstQuarterMoon = DateTime.Parse("30/07/2017 11:23:00"), FullMoon = DateTime.Parse("07/08/2017 14:10:00"), LastQuarterMoon = DateTime.Parse("14/08/2017 21:14:00"), Duration = TimeSpan.Parse("29.08:45:00") },
            new Lunation() { Index = 1171, NewMoon = DateTime.Parse("21/08/2017 14:30:00"), FirstQuarterMoon = DateTime.Parse("29/08/2017 04:12:00"), FullMoon = DateTime.Parse("06/09/2017 03:02:00"), LastQuarterMoon = DateTime.Parse("13/09/2017 02:24:00"), Duration = TimeSpan.Parse("29.11:00:00") },
            new Lunation() { Index = 1172, NewMoon = DateTime.Parse("20/09/2017 01:29:00"), FirstQuarterMoon = DateTime.Parse("27/09/2017 22:53:00"), FullMoon = DateTime.Parse("05/10/2017 14:40:00"), LastQuarterMoon = DateTime.Parse("12/10/2017 08:25:00"), Duration = TimeSpan.Parse("29.13:42:00") },
            new Lunation() { Index = 1173, NewMoon = DateTime.Parse("19/10/2017 15:12:00"), FirstQuarterMoon = DateTime.Parse("27/10/2017 18:22:00"), FullMoon = DateTime.Parse("04/11/2017 01:22:00"), LastQuarterMoon = DateTime.Parse("10/11/2017 15:36:00"), Duration = TimeSpan.Parse("29.16:30:00") },
            new Lunation() { Index = 1174, NewMoon = DateTime.Parse("18/11/2017 06:42:00"), FirstQuarterMoon = DateTime.Parse("26/11/2017 12:02:00"), FullMoon = DateTime.Parse("03/12/2017 10:46:00"), LastQuarterMoon = DateTime.Parse("10/12/2017 02:51:00"), Duration = TimeSpan.Parse("29.18:48:00") },
            new Lunation() { Index = 1175, NewMoon = DateTime.Parse("18/12/2017 01:30:00"), FirstQuarterMoon = DateTime.Parse("26/12/2017 04:20:00"), FullMoon = DateTime.Parse("01/01/2018 21:24:00"), LastQuarterMoon = DateTime.Parse("08/01/2018 17:25:00"), Duration = TimeSpan.Parse("29.19:47:00") },
            new Lunation() { Index = 1176, NewMoon = DateTime.Parse("16/01/2018 21:17:00"), FirstQuarterMoon = DateTime.Parse("24/01/2018 17:20:00"), FullMoon = DateTime.Parse("31/01/2018 08:26:00"), LastQuarterMoon = DateTime.Parse("07/02/2018 10:53:00"), Duration = TimeSpan.Parse("29.18:48:00") },
            new Lunation() { Index = 1177, NewMoon = DateTime.Parse("15/02/2018 16:05:00"), FirstQuarterMoon = DateTime.Parse("23/02/2018 03:09:00"), FullMoon = DateTime.Parse("01/03/2018 19:51:00"), LastQuarterMoon = DateTime.Parse("09/03/2018 06:19:00"), Duration = TimeSpan.Parse("29.16:06:00") },
            new Lunation() { Index = 1178, NewMoon = DateTime.Parse("17/03/2018 09:11:00"), FirstQuarterMoon = DateTime.Parse("24/03/2018 11:35:00"), FullMoon = DateTime.Parse("31/03/2018 08:36:00"), LastQuarterMoon = DateTime.Parse("08/04/2018 03:17:00"), Duration = TimeSpan.Parse("29.12:46:00") },
            new Lunation() { Index = 1179, NewMoon = DateTime.Parse("15/04/2018 21:57:00"), FirstQuarterMoon = DateTime.Parse("22/04/2018 17:45:00"), FullMoon = DateTime.Parse("29/04/2018 20:58:00"), LastQuarterMoon = DateTime.Parse("07/05/2018 22:08:00"), Duration = TimeSpan.Parse("29.09:51:00") },
            new Lunation() { Index = 1180, NewMoon = DateTime.Parse("15/05/2018 07:47:00"), FirstQuarterMoon = DateTime.Parse("21/05/2018 23:49:00"), FullMoon = DateTime.Parse("29/05/2018 10:19:00"), LastQuarterMoon = DateTime.Parse("06/06/2018 14:31:00"), Duration = TimeSpan.Parse("29.07:55:00") },
            new Lunation() { Index = 1181, NewMoon = DateTime.Parse("13/06/2018 15:43:00"), FirstQuarterMoon = DateTime.Parse("20/06/2018 06:50:00"), FullMoon = DateTime.Parse("28/06/2018 00:53:00"), LastQuarterMoon = DateTime.Parse("06/07/2018 03:50:00"), Duration = TimeSpan.Parse("29.07:05:00") },
            new Lunation() { Index = 1182, NewMoon = DateTime.Parse("12/07/2018 22:47:00"), FirstQuarterMoon = DateTime.Parse("19/07/2018 15:52:00"), FullMoon = DateTime.Parse("27/07/2018 16:20:00"), LastQuarterMoon = DateTime.Parse("04/08/2018 14:17:00"), Duration = TimeSpan.Parse("29.07:10:00") },
            new Lunation() { Index = 1183, NewMoon = DateTime.Parse("11/08/2018 05:57:00"), FirstQuarterMoon = DateTime.Parse("18/08/2018 03:48:00"), FullMoon = DateTime.Parse("26/08/2018 07:56:00"), LastQuarterMoon = DateTime.Parse("02/09/2018 22:37:00"), Duration = TimeSpan.Parse("29.08:04:00") },
            new Lunation() { Index = 1184, NewMoon = DateTime.Parse("09/09/2018 14:01:00"), FirstQuarterMoon = DateTime.Parse("16/09/2018 19:14:00"), FullMoon = DateTime.Parse("24/09/2018 22:52:00"), LastQuarterMoon = DateTime.Parse("02/10/2018 05:45:00"), Duration = TimeSpan.Parse("29.09:45:00") },
            new Lunation() { Index = 1185, NewMoon = DateTime.Parse("08/10/2018 23:46:00"), FirstQuarterMoon = DateTime.Parse("16/10/2018 14:01:00"), FullMoon = DateTime.Parse("24/10/2018 12:45:00"), LastQuarterMoon = DateTime.Parse("31/10/2018 12:40:00"), Duration = TimeSpan.Parse("29.12:15:00") },
            new Lunation() { Index = 1186, NewMoon = DateTime.Parse("07/11/2018 11:01:00"), FirstQuarterMoon = DateTime.Parse("15/11/2018 09:54:00"), FullMoon = DateTime.Parse("23/11/2018 00:39:00"), LastQuarterMoon = DateTime.Parse("29/11/2018 19:18:00"), Duration = TimeSpan.Parse("29.15:18:00") },
            new Lunation() { Index = 1187, NewMoon = DateTime.Parse("07/12/2018 02:20:00"), FirstQuarterMoon = DateTime.Parse("15/12/2018 06:49:00"), FullMoon = DateTime.Parse("22/12/2018 12:48:00"), LastQuarterMoon = DateTime.Parse("29/12/2018 04:34:00"), Duration = TimeSpan.Parse("29.15:18:00") },
            new Lunation() { Index = 1188, NewMoon = DateTime.Parse("05/01/2019 20:28:00"), FirstQuarterMoon = DateTime.Parse("14/01/2019 01:45:00"), FullMoon = DateTime.Parse("21/01/2019 00:16:00"), LastQuarterMoon = DateTime.Parse("27/01/2019 16:10:00"), Duration = TimeSpan.Parse("29.19:35:00") },
            new Lunation() { Index = 1189, NewMoon = DateTime.Parse("04/02/2019 16:03:00"), FirstQuarterMoon = DateTime.Parse("12/02/2019 17:26:00"), FullMoon = DateTime.Parse("19/02/2019 10:53:00"), LastQuarterMoon = DateTime.Parse("26/02/2019 06:27:00"), Duration = TimeSpan.Parse("29.19:00:00") },
            new Lunation() { Index = 1190, NewMoon = DateTime.Parse("06/03/2019 11:03:00"), FirstQuarterMoon = DateTime.Parse("14/03/2019 06:27:00"), FullMoon = DateTime.Parse("20/03/2019 21:42:00"), LastQuarterMoon = DateTime.Parse("28/03/2019 00:09:00"), Duration = TimeSpan.Parse("29.16:47:00") },
            new Lunation() { Index = 1191, NewMoon = DateTime.Parse("05/04/2019 04:50:00"), FirstQuarterMoon = DateTime.Parse("12/04/2019 15:05:00"), FullMoon = DateTime.Parse("19/04/2019 07:12:00"), LastQuarterMoon = DateTime.Parse("26/04/2019 18:18:00"), Duration = TimeSpan.Parse("29.13:55:00") },
            new Lunation() { Index = 1192, NewMoon = DateTime.Parse("04/05/2019 18:45:00"), FirstQuarterMoon = DateTime.Parse("11/05/2019 21:12:00"), FullMoon = DateTime.Parse("18/05/2019 17:11:00"), LastQuarterMoon = DateTime.Parse("26/05/2019 12:33:00"), Duration = TimeSpan.Parse("29.11:16:00") },
            new Lunation() { Index = 1193, NewMoon = DateTime.Parse("03/06/2019 06:01:00"), FirstQuarterMoon = DateTime.Parse("10/06/2019 01:59:00"), FullMoon = DateTime.Parse("17/06/2019 04:30:00"), LastQuarterMoon = DateTime.Parse("25/06/2019 05:46:00"), Duration = TimeSpan.Parse("29.09:14:00") },
            new Lunation() { Index = 1194, NewMoon = DateTime.Parse("02/07/2019 15:16:00"), FirstQuarterMoon = DateTime.Parse("09/07/2019 06:54:00"), FullMoon = DateTime.Parse("16/07/2019 17:38:00"), LastQuarterMoon = DateTime.Parse("24/07/2019 21:18:00"), Duration = TimeSpan.Parse("29.07:56:00") },
            new Lunation() { Index = 1195, NewMoon = DateTime.Parse("31/07/2019 23:11:00"), FirstQuarterMoon = DateTime.Parse("07/08/2019 13:30:00"), FullMoon = DateTime.Parse("15/08/2019 08:29:00"), LastQuarterMoon = DateTime.Parse("23/08/2019 10:56:00"), Duration = TimeSpan.Parse("29.07:25:00") },
            new Lunation() { Index = 1196, NewMoon = DateTime.Parse("30/08/2019 06:37:00"), FirstQuarterMoon = DateTime.Parse("05/09/2019 23:10:00"), FullMoon = DateTime.Parse("14/09/2019 00:32:00"), LastQuarterMoon = DateTime.Parse("21/09/2019 22:40:00"), Duration = TimeSpan.Parse("29.07:49:00") },
            new Lunation() { Index = 1197, NewMoon = DateTime.Parse("28/09/2019 14:26:00"), FirstQuarterMoon = DateTime.Parse("05/10/2019 12:47:00"), FullMoon = DateTime.Parse("13/10/2019 17:07:00"), LastQuarterMoon = DateTime.Parse("21/10/2019 08:39:00"), Duration = TimeSpan.Parse("29.09:12:00") },
            new Lunation() { Index = 1198, NewMoon = DateTime.Parse("27/10/2019 23:38:00"), FirstQuarterMoon = DateTime.Parse("04/11/2019 05:23:00"), FullMoon = DateTime.Parse("12/11/2019 08:34:00"), LastQuarterMoon = DateTime.Parse("19/11/2019 16:10:00"), Duration = TimeSpan.Parse("29.11:27:00") },
            new Lunation() { Index = 1199, NewMoon = DateTime.Parse("26/11/2019 10:05:00"), FirstQuarterMoon = DateTime.Parse("04/12/2019 01:58:00"), FullMoon = DateTime.Parse("12/12/2019 00:12:00"), LastQuarterMoon = DateTime.Parse("18/12/2019 23:57:00"), Duration = TimeSpan.Parse("29.14:08:00") },
            new Lunation() { Index = 1200, NewMoon = DateTime.Parse("26/12/2019 00:13:00"), FirstQuarterMoon = DateTime.Parse("02/01/2020 23:45:00"), FullMoon = DateTime.Parse("10/01/2020 14:21:00"), LastQuarterMoon = DateTime.Parse("17/01/2020 07:58:00"), Duration = TimeSpan.Parse("29.16:29:00") },
            new Lunation() { Index = 1201, NewMoon = DateTime.Parse("24/01/2020 16:42:00"), FirstQuarterMoon = DateTime.Parse("01/02/2020 20:41:00"), FullMoon = DateTime.Parse("09/02/2020 02:33:00"), LastQuarterMoon = DateTime.Parse("15/02/2020 17:17:00"), Duration = TimeSpan.Parse("29.17:50:00") },
            new Lunation() { Index = 1202, NewMoon = DateTime.Parse("23/02/2020 10:32:00"), FirstQuarterMoon = DateTime.Parse("02/03/2020 14:57:00"), FullMoon = DateTime.Parse("09/03/2020 13:47:00"), LastQuarterMoon = DateTime.Parse("16/03/2020 05:34:00"), Duration = TimeSpan.Parse("29.17:56:00") },
            new Lunation() { Index = 1203, NewMoon = DateTime.Parse("24/03/2020 05:28:00"), FirstQuarterMoon = DateTime.Parse("01/04/2020 06:21:00"), FullMoon = DateTime.Parse("07/04/2020 22:35:00"), LastQuarterMoon = DateTime.Parse("14/04/2020 18:56:00"), Duration = TimeSpan.Parse("29.16:58:00") },
            new Lunation() { Index = 1204, NewMoon = DateTime.Parse("22/04/2020 22:25:00"), FirstQuarterMoon = DateTime.Parse("30/04/2020 16:38:00"), FullMoon = DateTime.Parse("07/05/2020 06:45:00"), LastQuarterMoon = DateTime.Parse("14/05/2020 10:02:00"), Duration = TimeSpan.Parse("29.15:13:00") },
            new Lunation() { Index = 1205, NewMoon = DateTime.Parse("22/05/2020 13:38:00"), FirstQuarterMoon = DateTime.Parse("29/05/2020 23:29:00"), FullMoon = DateTime.Parse("05/06/2020 15:12:00"), LastQuarterMoon = DateTime.Parse("13/06/2020 02:23:00"), Duration = TimeSpan.Parse("29.13:03:00") },
            new Lunation() { Index = 1206, NewMoon = DateTime.Parse("21/06/2020 02:41:00"), FirstQuarterMoon = DateTime.Parse("28/06/2020 04:15:00"), FullMoon = DateTime.Parse("05/07/2020 00:44:00"), LastQuarterMoon = DateTime.Parse("12/07/2020 19:28:00"), Duration = TimeSpan.Parse("29.10:51:00") },
            new Lunation() { Index = 1207, NewMoon = DateTime.Parse("20/07/2020 13:32:00"), FirstQuarterMoon = DateTime.Parse("27/07/2020 08:32:00"), FullMoon = DateTime.Parse("03/08/2020 11:58:00"), LastQuarterMoon = DateTime.Parse("11/08/2020 12:44:00"), Duration = TimeSpan.Parse("29.09:09:00") },
            new Lunation() { Index = 1208, NewMoon = DateTime.Parse("18/08/2020 22:41:00"), FirstQuarterMoon = DateTime.Parse("25/08/2020 13:57:00"), FullMoon = DateTime.Parse("02/09/2020 01:22:00"), LastQuarterMoon = DateTime.Parse("10/09/2020 05:25:00"), Duration = TimeSpan.Parse("29.08:19:00") },
            new Lunation() { Index = 1209, NewMoon = DateTime.Parse("17/09/2020 07:00:00"), FirstQuarterMoon = DateTime.Parse("23/09/2020 21:54:00"), FullMoon = DateTime.Parse("01/10/2020 17:05:00"), LastQuarterMoon = DateTime.Parse("09/10/2020 20:39:00"), Duration = TimeSpan.Parse("29.08:31:00") },
            new Lunation() { Index = 1210, NewMoon = DateTime.Parse("16/10/2020 15:31:00"), FirstQuarterMoon = DateTime.Parse("23/10/2020 09:22:00"), FullMoon = DateTime.Parse("31/10/2020 10:49:00"), LastQuarterMoon = DateTime.Parse("08/11/2020 08:46:00"), Duration = TimeSpan.Parse("29.09:36:00") },
            new Lunation() { Index = 1211, NewMoon = DateTime.Parse("15/11/2020 00:07:00"), FirstQuarterMoon = DateTime.Parse("21/11/2020 23:45:00"), FullMoon = DateTime.Parse("30/11/2020 04:29:00"), LastQuarterMoon = DateTime.Parse("07/12/2020 19:36:00"), Duration = TimeSpan.Parse("29.11:09:00") },
            new Lunation() { Index = 1212, NewMoon = DateTime.Parse("14/12/2020 11:16:00"), FirstQuarterMoon = DateTime.Parse("21/12/2020 18:41:00"), FullMoon = DateTime.Parse("29/12/2020 22:28:00"), LastQuarterMoon = DateTime.Parse("06/01/2021 04:37:00"), Duration = TimeSpan.Parse("29.12:44:00") },
            new Lunation() { Index = 1213, NewMoon = DateTime.Parse("13/01/2021 00:00:00"), FirstQuarterMoon = DateTime.Parse("20/01/2021 16:01:00"), FullMoon = DateTime.Parse("28/01/2021 14:16:00"), LastQuarterMoon = DateTime.Parse("04/02/2021 12:37:00"), Duration = TimeSpan.Parse("29.14:06:00") },
            new Lunation() { Index = 1214, NewMoon = DateTime.Parse("11/02/2021 14:05:00"), FirstQuarterMoon = DateTime.Parse("19/02/2021 13:47:00"), FullMoon = DateTime.Parse("27/02/2021 03:17:00"), LastQuarterMoon = DateTime.Parse("05/03/2021 20:30:00"), Duration = TimeSpan.Parse("29.15:15:00") },
            new Lunation() { Index = 1215, NewMoon = DateTime.Parse("13/03/2021 05:21:00"), FirstQuarterMoon = DateTime.Parse("21/03/2021 10:40:00"), FullMoon = DateTime.Parse("28/03/2021 14:48:00"), LastQuarterMoon = DateTime.Parse("04/04/2021 06:02:00"), Duration = TimeSpan.Parse("29.16:10:00") },
            new Lunation() { Index = 1216, NewMoon = DateTime.Parse("11/04/2021 22:30:00"), FirstQuarterMoon = DateTime.Parse("20/04/2021 02:58:00"), FullMoon = DateTime.Parse("26/04/2021 23:31:00"), LastQuarterMoon = DateTime.Parse("03/05/2021 15:50:00"), Duration = TimeSpan.Parse("29.16:29:00") },
            new Lunation() { Index = 1217, NewMoon = DateTime.Parse("11/05/2021 14:59:00"), FirstQuarterMoon = DateTime.Parse("19/05/2021 15:12:00"), FullMoon = DateTime.Parse("26/05/2021 07:13:00"), LastQuarterMoon = DateTime.Parse("02/06/2021 03:24:00"), Duration = TimeSpan.Parse("29.15:53:00") },
            new Lunation() { Index = 1218, NewMoon = DateTime.Parse("10/06/2021 06:52:00"), FirstQuarterMoon = DateTime.Parse("17/06/2021 23:54:00"), FullMoon = DateTime.Parse("24/06/2021 14:39:00"), LastQuarterMoon = DateTime.Parse("01/07/2021 17:10:00"), Duration = TimeSpan.Parse("29.14:24:00") },
            new Lunation() { Index = 1219, NewMoon = DateTime.Parse("09/07/2021 21:16:00"), FirstQuarterMoon = DateTime.Parse("17/07/2021 06:10:00"), FullMoon = DateTime.Parse("23/07/2021 22:36:00"), LastQuarterMoon = DateTime.Parse("31/07/2021 09:15:00"), Duration = TimeSpan.Parse("29.12:34:00") },
            new Lunation() { Index = 1220, NewMoon = DateTime.Parse("08/08/2021 09:50:00"), FirstQuarterMoon = DateTime.Parse("15/08/2021 11:19:00"), FullMoon = DateTime.Parse("22/08/2021 08:01:00"), LastQuarterMoon = DateTime.Parse("30/08/2021 03:13:00"), Duration = TimeSpan.Parse("29.11:02:00") },
            new Lunation() { Index = 1221, NewMoon = DateTime.Parse("06/09/2021 20:51:00"), FirstQuarterMoon = DateTime.Parse("13/09/2021 16:39:00"), FullMoon = DateTime.Parse("20/09/2021 19:54:00"), LastQuarterMoon = DateTime.Parse("28/09/2021 21:57:00"), Duration = TimeSpan.Parse("29.10:14:00") },
            new Lunation() { Index = 1222, NewMoon = DateTime.Parse("06/10/2021 07:05:00"), FirstQuarterMoon = DateTime.Parse("12/10/2021 23:25:00"), FullMoon = DateTime.Parse("20/10/2021 10:56:00"), LastQuarterMoon = DateTime.Parse("28/10/2021 16:05:00"), Duration = TimeSpan.Parse("29.10:09:00") },
            new Lunation() { Index = 1223, NewMoon = DateTime.Parse("04/11/2021 17:14:00"), FirstQuarterMoon = DateTime.Parse("11/11/2021 07:45:00"), FullMoon = DateTime.Parse("19/11/2021 03:57:00"), LastQuarterMoon = DateTime.Parse("27/11/2021 07:27:00"), Duration = TimeSpan.Parse("29.10:28:00") },
            new Lunation() { Index = 1224, NewMoon = DateTime.Parse("04/12/2021 02:43:00"), FirstQuarterMoon = DateTime.Parse("10/12/2021 20:35:00"), FullMoon = DateTime.Parse("18/12/2021 23:35:00"), LastQuarterMoon = DateTime.Parse("26/12/2021 21:23:00"), Duration = TimeSpan.Parse("29.10:51:00") },
            new Lunation() { Index = 1225, NewMoon = DateTime.Parse("02/01/2022 13:33:00"), FirstQuarterMoon = DateTime.Parse("09/01/2022 13:11:00"), FullMoon = DateTime.Parse("17/01/2022 18:48:00"), LastQuarterMoon = DateTime.Parse("25/01/2022 08:40:00"), Duration = TimeSpan.Parse("29.11:12:00") },
            new Lunation() { Index = 1226, NewMoon = DateTime.Parse("01/02/2022 00:46:00"), FirstQuarterMoon = DateTime.Parse("08/02/2022 08:50:00"), FullMoon = DateTime.Parse("16/02/2022 11:56:00"), LastQuarterMoon = DateTime.Parse("23/02/2022 17:32:00"), Duration = TimeSpan.Parse("29.11:49:00") },
            new Lunation() { Index = 1227, NewMoon = DateTime.Parse("02/03/2022 12:34:00"), FirstQuarterMoon = DateTime.Parse("10/03/2022 05:45:00"), FullMoon = DateTime.Parse("18/03/2022 03:17:00"), LastQuarterMoon = DateTime.Parse("25/03/2022 01:37:00"), Duration = TimeSpan.Parse("29.12:50:00") },
            new Lunation() { Index = 1228, NewMoon = DateTime.Parse("01/04/2022 02:24:00"), FirstQuarterMoon = DateTime.Parse("09/04/2022 02:47:00"), FullMoon = DateTime.Parse("16/04/2022 14:55:00"), LastQuarterMoon = DateTime.Parse("23/04/2022 07:56:00"), Duration = TimeSpan.Parse("29.14:04:00") },
            new Lunation() { Index = 1229, NewMoon = DateTime.Parse("30/04/2022 16:28:00"), FirstQuarterMoon = DateTime.Parse("08/05/2022 20:21:00"), FullMoon = DateTime.Parse("16/05/2022 00:14:00"), LastQuarterMoon = DateTime.Parse("22/05/2022 14:43:00"), Duration = TimeSpan.Parse("29.15:02:00") },
            new Lunation() { Index = 1230, NewMoon = DateTime.Parse("30/05/2022 07:30:00"), FirstQuarterMoon = DateTime.Parse("07/06/2022 10:48:00"), FullMoon = DateTime.Parse("14/06/2022 07:51:00"), LastQuarterMoon = DateTime.Parse("20/06/2022 23:10:00"), Duration = TimeSpan.Parse("29.15:22:00") },
            new Lunation() { Index = 1231, NewMoon = DateTime.Parse("28/06/2022 22:52:00"), FirstQuarterMoon = DateTime.Parse("06/07/2022 22:14:00"), FullMoon = DateTime.Parse("13/07/2022 14:37:00"), LastQuarterMoon = DateTime.Parse("20/07/2022 10:18:00"), Duration = TimeSpan.Parse("29.15:03:00") },
            new Lunation() { Index = 1232, NewMoon = DateTime.Parse("28/07/2022 13:54:00"), FirstQuarterMoon = DateTime.Parse("05/08/2022 07:06:00"), FullMoon = DateTime.Parse("11/08/2022 21:35:00"), LastQuarterMoon = DateTime.Parse("19/08/2022 00:36:00"), Duration = TimeSpan.Parse("29.14:22:00") },
            new Lunation() { Index = 1233, NewMoon = DateTime.Parse("27/08/2022 04:17:00"), FirstQuarterMoon = DateTime.Parse("03/09/2022 14:07:00"), FullMoon = DateTime.Parse("10/09/2022 05:59:00"), LastQuarterMoon = DateTime.Parse("17/09/2022 17:51:00"), Duration = TimeSpan.Parse("29.13:37:00") },
            new Lunation() { Index = 1234, NewMoon = DateTime.Parse("25/09/2022 17:54:00"), FirstQuarterMoon = DateTime.Parse("02/10/2022 20:14:00"), FullMoon = DateTime.Parse("09/10/2022 16:54:00"), LastQuarterMoon = DateTime.Parse("17/10/2022 13:15:00"), Duration = TimeSpan.Parse("29.12:54:00") },
            new Lunation() { Index = 1235, NewMoon = DateTime.Parse("25/10/2022 06:48:00"), FirstQuarterMoon = DateTime.Parse("01/11/2022 02:37:00"), FullMoon = DateTime.Parse("08/11/2022 06:02:00"), LastQuarterMoon = DateTime.Parse("16/11/2022 08:27:00"), Duration = TimeSpan.Parse("29.12:09:00") },
            new Lunation() { Index = 1236, NewMoon = DateTime.Parse("23/11/2022 17:57:00"), FirstQuarterMoon = DateTime.Parse("30/11/2022 09:36:00"), FullMoon = DateTime.Parse("07/12/2022 23:08:00"), LastQuarterMoon = DateTime.Parse("16/12/2022 03:56:00"), Duration = TimeSpan.Parse("29.11:20:00") },
            new Lunation() { Index = 1237, NewMoon = DateTime.Parse("23/12/2022 05:16:00"), FirstQuarterMoon = DateTime.Parse("29/12/2022 20:20:00"), FullMoon = DateTime.Parse("06/01/2023 18:07:00"), LastQuarterMoon = DateTime.Parse("14/01/2023 21:10:00"), Duration = TimeSpan.Parse("29.10:36:00") },
            new Lunation() { Index = 1238, NewMoon = DateTime.Parse("21/01/2023 15:53:00"), FirstQuarterMoon = DateTime.Parse("28/01/2023 10:18:00"), FullMoon = DateTime.Parse("05/02/2023 13:28:00"), LastQuarterMoon = DateTime.Parse("13/02/2023 11:00:00"), Duration = TimeSpan.Parse("29.10:13:00") },
            new Lunation() { Index = 1239, NewMoon = DateTime.Parse("20/02/2023 02:05:00"), FirstQuarterMoon = DateTime.Parse("27/02/2023 03:05:00"), FullMoon = DateTime.Parse("07/03/2023 07:40:00"), LastQuarterMoon = DateTime.Parse("14/03/2023 22:08:00"), Duration = TimeSpan.Parse("29.10:17:00") },
            new Lunation() { Index = 1240, NewMoon = DateTime.Parse("21/03/2023 13:23:00"), FirstQuarterMoon = DateTime.Parse("28/03/2023 22:32:00"), FullMoon = DateTime.Parse("06/04/2023 00:34:00"), LastQuarterMoon = DateTime.Parse("13/04/2023 05:11:00"), Duration = TimeSpan.Parse("29.10:49:00") },
            new Lunation() { Index = 1241, NewMoon = DateTime.Parse("20/04/2023 00:12:00"), FirstQuarterMoon = DateTime.Parse("27/04/2023 17:19:00"), FullMoon = DateTime.Parse("05/05/2023 13:34:00"), LastQuarterMoon = DateTime.Parse("12/05/2023 10:28:00"), Duration = TimeSpan.Parse("29.11:41:00") },
            new Lunation() { Index = 1242, NewMoon = DateTime.Parse("19/05/2023 11:53:00"), FirstQuarterMoon = DateTime.Parse("27/05/2023 11:22:00"), FullMoon = DateTime.Parse("03/06/2023 23:41:00"), LastQuarterMoon = DateTime.Parse("10/06/2023 15:31:00"), Duration = TimeSpan.Parse("29.12:44:00") },
            new Lunation() { Index = 1243, NewMoon = DateTime.Parse("18/06/2023 00:37:00"), FirstQuarterMoon = DateTime.Parse("26/06/2023 03:49:00"), FullMoon = DateTime.Parse("03/07/2023 07:38:00"), LastQuarterMoon = DateTime.Parse("09/07/2023 21:47:00"), Duration = TimeSpan.Parse("29.13:55:00") },
            new Lunation() { Index = 1244, NewMoon = DateTime.Parse("17/07/2023 14:31:00"), FirstQuarterMoon = DateTime.Parse("25/07/2023 18:06:00"), FullMoon = DateTime.Parse("01/08/2023 14:31:00"), LastQuarterMoon = DateTime.Parse("08/08/2023 06:28:00"), Duration = TimeSpan.Parse("29.15:06:00") },
            new Lunation() { Index = 1245, NewMoon = DateTime.Parse("16/08/2023 05:38:00"), FirstQuarterMoon = DateTime.Parse("24/08/2023 05:57:00"), FullMoon = DateTime.Parse("30/08/2023 21:35:00"), LastQuarterMoon = DateTime.Parse("06/09/2023 18:21:00"), Duration = TimeSpan.Parse("29.16:02:00") },
            new Lunation() { Index = 1246, NewMoon = DateTime.Parse("14/09/2023 21:39:00"), FirstQuarterMoon = DateTime.Parse("22/09/2023 15:31:00"), FullMoon = DateTime.Parse("29/09/2023 05:57:00"), LastQuarterMoon = DateTime.Parse("06/10/2023 09:47:00"), Duration = TimeSpan.Parse("29.16:15:00") },
            new Lunation() { Index = 1247, NewMoon = DateTime.Parse("14/10/2023 13:55:00"), FirstQuarterMoon = DateTime.Parse("21/10/2023 23:29:00"), FullMoon = DateTime.Parse("28/10/2023 16:24:00"), LastQuarterMoon = DateTime.Parse("05/11/2023 03:36:00"), Duration = TimeSpan.Parse("29.15:32:00") },
            new Lunation() { Index = 1248, NewMoon = DateTime.Parse("13/11/2023 04:27:00"), FirstQuarterMoon = DateTime.Parse("20/11/2023 05:49:00"), FullMoon = DateTime.Parse("27/11/2023 04:16:00"), LastQuarterMoon = DateTime.Parse("05/12/2023 00:49:00"), Duration = TimeSpan.Parse("29.14:05:00") },
            new Lunation() { Index = 1249, NewMoon = DateTime.Parse("12/12/2023 18:32:00"), FirstQuarterMoon = DateTime.Parse("19/12/2023 13:39:00"), FullMoon = DateTime.Parse("26/12/2023 19:33:00"), LastQuarterMoon = DateTime.Parse("03/01/2024 22:30:00"), Duration = TimeSpan.Parse("29.12:25:00") },
            new Lunation() { Index = 1250, NewMoon = DateTime.Parse("11/01/2024 06:57:00"), FirstQuarterMoon = DateTime.Parse("17/01/2024 22:52:00"), FullMoon = DateTime.Parse("25/01/2024 12:53:00"), LastQuarterMoon = DateTime.Parse("02/02/2024 18:18:00"), Duration = TimeSpan.Parse("29.11:02:00") },
            new Lunation() { Index = 1251, NewMoon = DateTime.Parse("09/02/2024 17:59:00"), FirstQuarterMoon = DateTime.Parse("16/02/2024 10:00:00"), FullMoon = DateTime.Parse("24/02/2024 07:30:00"), LastQuarterMoon = DateTime.Parse("03/03/2024 10:23:00"), Duration = TimeSpan.Parse("29.10:01:00") },
            new Lunation() { Index = 1252, NewMoon = DateTime.Parse("10/03/2024 05:00:00"), FirstQuarterMoon = DateTime.Parse("17/03/2024 00:10:00"), FullMoon = DateTime.Parse("25/03/2024 03:00:00"), LastQuarterMoon = DateTime.Parse("01/04/2024 23:14:00"), Duration = TimeSpan.Parse("29.09:20:00") },
            new Lunation() { Index = 1253, NewMoon = DateTime.Parse("08/04/2024 14:20:00"), FirstQuarterMoon = DateTime.Parse("15/04/2024 15:13:00"), FullMoon = DateTime.Parse("23/04/2024 19:48:00"), LastQuarterMoon = DateTime.Parse("01/05/2024 07:27:00"), Duration = TimeSpan.Parse("29.09:01:00") },
            new Lunation() { Index = 1254, NewMoon = DateTime.Parse("07/05/2024 23:21:00"), FirstQuarterMoon = DateTime.Parse("15/05/2024 07:48:00"), FullMoon = DateTime.Parse("23/05/2024 09:53:00"), LastQuarterMoon = DateTime.Parse("30/05/2024 13:12:00"), Duration = TimeSpan.Parse("29.09:16:00") },
            new Lunation() { Index = 1255, NewMoon = DateTime.Parse("06/06/2024 08:37:00"), FirstQuarterMoon = DateTime.Parse("14/06/2024 01:18:00"), FullMoon = DateTime.Parse("21/06/2024 21:07:00"), LastQuarterMoon = DateTime.Parse("28/06/2024 17:53:00"), Duration = TimeSpan.Parse("29.10:20:00") },
            new Lunation() { Index = 1256, NewMoon = DateTime.Parse("05/07/2024 18:57:00"), FirstQuarterMoon = DateTime.Parse("13/07/2024 18:48:00"), FullMoon = DateTime.Parse("21/07/2024 06:17:00"), LastQuarterMoon = DateTime.Parse("27/07/2024 22:51:00"), Duration = TimeSpan.Parse("29.12:16:00") },
            new Lunation() { Index = 1257, NewMoon = DateTime.Parse("04/08/2024 07:13:00"), FirstQuarterMoon = DateTime.Parse("12/08/2024 11:18:00"), FullMoon = DateTime.Parse("19/08/2024 14:25:00"), LastQuarterMoon = DateTime.Parse("26/08/2024 05:25:00"), Duration = TimeSpan.Parse("29.14:42:00") },
            new Lunation() { Index = 1258, NewMoon = DateTime.Parse("02/09/2024 21:55:00"), FirstQuarterMoon = DateTime.Parse("11/09/2024 02:05:00"), FullMoon = DateTime.Parse("17/09/2024 22:34:00"), LastQuarterMoon = DateTime.Parse("24/09/2024 14:49:00"), Duration = TimeSpan.Parse("29.16:54:00") },
            new Lunation() { Index = 1259, NewMoon = DateTime.Parse("02/10/2024 14:49:00"), FirstQuarterMoon = DateTime.Parse("10/10/2024 14:55:00"), FullMoon = DateTime.Parse("17/10/2024 07:26:00"), LastQuarterMoon = DateTime.Parse("24/10/2024 04:03:00"), Duration = TimeSpan.Parse("29.17:58:00") },
            new Lunation() { Index = 1260, NewMoon = DateTime.Parse("01/11/2024 08:47:00"), FirstQuarterMoon = DateTime.Parse("09/11/2024 00:55:00"), FullMoon = DateTime.Parse("15/11/2024 16:28:00"), LastQuarterMoon = DateTime.Parse("22/11/2024 20:27:00"), Duration = TimeSpan.Parse("29.17:34:00") },
            new Lunation() { Index = 1261, NewMoon = DateTime.Parse("01/12/2024 01:21:00"), FirstQuarterMoon = DateTime.Parse("08/12/2024 10:26:00"), FullMoon = DateTime.Parse("15/12/2024 04:01:00"), LastQuarterMoon = DateTime.Parse("22/12/2024 17:18:00"), Duration = TimeSpan.Parse("29.16:05:00") },
            new Lunation() { Index = 1262, NewMoon = DateTime.Parse("30/12/2024 17:26:00"), FirstQuarterMoon = DateTime.Parse("06/01/2025 18:56:00"), FullMoon = DateTime.Parse("13/01/2025 17:26:00"), LastQuarterMoon = DateTime.Parse("21/01/2025 15:30:00"), Duration = TimeSpan.Parse("29.14:09:00") },
            new Lunation() { Index = 1263, NewMoon = DateTime.Parse("29/01/2025 07:35:00"), FirstQuarterMoon = DateTime.Parse("05/02/2025 03:02:00"), FullMoon = DateTime.Parse("12/02/2025 08:53:00"), LastQuarterMoon = DateTime.Parse("20/02/2025 12:32:00"), Duration = TimeSpan.Parse("29.12:09:00") },
            new Lunation() { Index = 1264, NewMoon = DateTime.Parse("27/02/2025 19:44:00"), FirstQuarterMoon = DateTime.Parse("06/03/2025 11:31:00"), FullMoon = DateTime.Parse("14/03/2025 02:54:00"), LastQuarterMoon = DateTime.Parse("22/03/2025 07:29:00"), Duration = TimeSpan.Parse("29.10:13:00") },
            new Lunation() { Index = 1265, NewMoon = DateTime.Parse("29/03/2025 06:57:00"), FirstQuarterMoon = DateTime.Parse("04/04/2025 22:14:00"), FullMoon = DateTime.Parse("12/04/2025 20:22:00"), LastQuarterMoon = DateTime.Parse("20/04/2025 21:35:00"), Duration = TimeSpan.Parse("29.08:33:00") },
            new Lunation() { Index = 1266, NewMoon = DateTime.Parse("27/04/2025 15:31:00"), FirstQuarterMoon = DateTime.Parse("04/05/2025 09:51:00"), FullMoon = DateTime.Parse("12/05/2025 12:55:00"), LastQuarterMoon = DateTime.Parse("20/05/2025 07:58:00"), Duration = TimeSpan.Parse("29.07:31:00") },
            new Lunation() { Index = 1267, NewMoon = DateTime.Parse("26/05/2025 23:02:00"), FirstQuarterMoon = DateTime.Parse("02/06/2025 23:40:00"), FullMoon = DateTime.Parse("11/06/2025 03:43:00"), LastQuarterMoon = DateTime.Parse("18/06/2025 15:19:00"), Duration = TimeSpan.Parse("29.07:29:00") },
            new Lunation() { Index = 1268, NewMoon = DateTime.Parse("25/06/2025 06:31:00"), FirstQuarterMoon = DateTime.Parse("02/07/2025 15:30:00"), FullMoon = DateTime.Parse("10/07/2025 16:36:00"), LastQuarterMoon = DateTime.Parse("17/07/2025 20:37:00"), Duration = TimeSpan.Parse("29.08:40:00") },
            new Lunation() { Index = 1269, NewMoon = DateTime.Parse("24/07/2025 15:11:00"), FirstQuarterMoon = DateTime.Parse("01/08/2025 08:41:00"), FullMoon = DateTime.Parse("09/08/2025 03:55:00"), LastQuarterMoon = DateTime.Parse("16/08/2025 01:12:00"), Duration = TimeSpan.Parse("29.10:55:00") },
            new Lunation() { Index = 1270, NewMoon = DateTime.Parse("23/08/2025 02:06:00"), FirstQuarterMoon = DateTime.Parse("31/08/2025 02:25:00"), FullMoon = DateTime.Parse("07/09/2025 14:08:00"), LastQuarterMoon = DateTime.Parse("14/09/2025 06:32:00"), Duration = TimeSpan.Parse("29.13:48:00") },
            new Lunation() { Index = 1271, NewMoon = DateTime.Parse("21/09/2025 15:54:00"), FirstQuarterMoon = DateTime.Parse("29/09/2025 19:53:00"), FullMoon = DateTime.Parse("06/10/2025 23:47:00"), LastQuarterMoon = DateTime.Parse("13/10/2025 14:12:00"), Duration = TimeSpan.Parse("29.16:31:00") },
            new Lunation() { Index = 1272, NewMoon = DateTime.Parse("21/10/2025 08:25:00"), FirstQuarterMoon = DateTime.Parse("29/10/2025 12:20:00"), FullMoon = DateTime.Parse("05/11/2025 08:19:00"), LastQuarterMoon = DateTime.Parse("12/11/2025 00:28:00"), Duration = TimeSpan.Parse("29.18:22:00") },
            new Lunation() { Index = 1273, NewMoon = DateTime.Parse("20/11/2025 01:47:00"), FirstQuarterMoon = DateTime.Parse("28/11/2025 01:58:00"), FullMoon = DateTime.Parse("04/12/2025 18:14:00"), LastQuarterMoon = DateTime.Parse("11/12/2025 15:51:00"), Duration = TimeSpan.Parse("29.18:56:00") },
            new Lunation() { Index = 1274, NewMoon = DateTime.Parse("19/12/2025 20:43:00"), FirstQuarterMoon = DateTime.Parse("27/12/2025 14:09:00"), FullMoon = DateTime.Parse("03/01/2026 05:02:00"), LastQuarterMoon = DateTime.Parse("10/01/2026 10:48:00"), Duration = TimeSpan.Parse("29.18:09:00") },
            new Lunation() { Index = 1275, NewMoon = DateTime.Parse("18/01/2026 14:51:00"), FirstQuarterMoon = DateTime.Parse("25/01/2026 23:47:00"), FullMoon = DateTime.Parse("01/02/2026 17:09:00"), LastQuarterMoon = DateTime.Parse("09/02/2026 07:43:00"), Duration = TimeSpan.Parse("29.16:09:00") },
            new Lunation() { Index = 1276, NewMoon = DateTime.Parse("17/02/2026 07:01:00"), FirstQuarterMoon = DateTime.Parse("24/02/2026 07:27:00"), FullMoon = DateTime.Parse("03/03/2026 06:37:00"), LastQuarterMoon = DateTime.Parse("11/03/2026 05:38:00"), Duration = TimeSpan.Parse("29.13:22:00") },
            new Lunation() { Index = 1277, NewMoon = DateTime.Parse("18/03/2026 21:23:00"), FirstQuarterMoon = DateTime.Parse("25/03/2026 15:17:00"), FullMoon = DateTime.Parse("01/04/2026 22:11:00"), LastQuarterMoon = DateTime.Parse("10/04/2026 00:51:00"), Duration = TimeSpan.Parse("29.10:28:00") },
            new Lunation() { Index = 1278, NewMoon = DateTime.Parse("17/04/2026 07:51:00"), FirstQuarterMoon = DateTime.Parse("23/04/2026 22:31:00"), FullMoon = DateTime.Parse("01/05/2026 13:23:00"), LastQuarterMoon = DateTime.Parse("09/05/2026 17:10:00"), Duration = TimeSpan.Parse("29.08:09:00") },
            new Lunation() { Index = 1279, NewMoon = DateTime.Parse("16/05/2026 16:01:00"), FirstQuarterMoon = DateTime.Parse("23/05/2026 07:10:00"), FullMoon = DateTime.Parse("31/05/2026 04:45:00"), LastQuarterMoon = DateTime.Parse("08/06/2026 06:00:00"), Duration = TimeSpan.Parse("29.06:53:00") },
            new Lunation() { Index = 1280, NewMoon = DateTime.Parse("14/06/2026 22:54:00"), FirstQuarterMoon = DateTime.Parse("21/06/2026 17:55:00"), FullMoon = DateTime.Parse("29/06/2026 19:56:00"), LastQuarterMoon = DateTime.Parse("07/07/2026 15:28:00"), Duration = TimeSpan.Parse("29.06:50:00") },
            new Lunation() { Index = 1281, NewMoon = DateTime.Parse("14/07/2026 05:43:00"), FirstQuarterMoon = DateTime.Parse("21/07/2026 07:05:00"), FullMoon = DateTime.Parse("29/07/2026 10:35:00"), LastQuarterMoon = DateTime.Parse("05/08/2026 22:21:00"), Duration = TimeSpan.Parse("29.07:53:00") },
            new Lunation() { Index = 1282, NewMoon = DateTime.Parse("12/08/2026 13:36:00"), FirstQuarterMoon = DateTime.Parse("19/08/2026 22:46:00"), FullMoon = DateTime.Parse("28/08/2026 00:18:00"), LastQuarterMoon = DateTime.Parse("04/09/2026 03:51:00"), Duration = TimeSpan.Parse("29.09:50:00") },
            new Lunation() { Index = 1283, NewMoon = DateTime.Parse("10/09/2026 23:26:00"), FirstQuarterMoon = DateTime.Parse("18/09/2026 16:43:00"), FullMoon = DateTime.Parse("26/09/2026 12:49:00"), LastQuarterMoon = DateTime.Parse("03/10/2026 09:25:00"), Duration = TimeSpan.Parse("29.12:23:00") },
            new Lunation() { Index = 1284, NewMoon = DateTime.Parse("10/10/2026 11:50:00"), FirstQuarterMoon = DateTime.Parse("18/10/2026 12:12:00"), FullMoon = DateTime.Parse("26/10/2026 00:11:00"), LastQuarterMoon = DateTime.Parse("01/11/2026 15:28:00"), Duration = TimeSpan.Parse("29.15:12:00") },
            new Lunation() { Index = 1285, NewMoon = DateTime.Parse("09/11/2026 02:02:00"), FirstQuarterMoon = DateTime.Parse("17/11/2026 06:47:00"), FullMoon = DateTime.Parse("24/11/2026 09:53:00"), LastQuarterMoon = DateTime.Parse("01/12/2026 01:08:00"), Duration = TimeSpan.Parse("29.17:50:00") },
            new Lunation() { Index = 1286, NewMoon = DateTime.Parse("08/12/2026 19:51:00"), FirstQuarterMoon = DateTime.Parse("17/12/2026 00:42:00"), FullMoon = DateTime.Parse("23/12/2026 20:28:00"), LastQuarterMoon = DateTime.Parse("30/12/2026 13:59:00"), Duration = TimeSpan.Parse("29.19:32:00") },
            new Lunation() { Index = 1287, NewMoon = DateTime.Parse("07/01/2027 15:24:00"), FirstQuarterMoon = DateTime.Parse("15/01/2027 15:34:00"), FullMoon = DateTime.Parse("22/01/2027 07:17:00"), LastQuarterMoon = DateTime.Parse("29/01/2027 05:55:00"), Duration = TimeSpan.Parse("29.19:32:00") },
            new Lunation() { Index = 1288, NewMoon = DateTime.Parse("06/02/2027 10:56:00"), FirstQuarterMoon = DateTime.Parse("14/02/2027 02:58:00"), FullMoon = DateTime.Parse("20/02/2027 18:23:00"), LastQuarterMoon = DateTime.Parse("28/02/2027 00:16:00"), Duration = TimeSpan.Parse("29.17:33:00") },
            new Lunation() { Index = 1289, NewMoon = DateTime.Parse("08/03/2027 04:29:00"), FirstQuarterMoon = DateTime.Parse("15/03/2027 12:25:00"), FullMoon = DateTime.Parse("22/03/2027 06:43:00"), LastQuarterMoon = DateTime.Parse("29/03/2027 20:53:00"), Duration = TimeSpan.Parse("29.14:22:00") },
            new Lunation() { Index = 1290, NewMoon = DateTime.Parse("06/04/2027 19:51:00"), FirstQuarterMoon = DateTime.Parse("13/04/2027 18:56:00"), FullMoon = DateTime.Parse("20/04/2027 18:27:00"), LastQuarterMoon = DateTime.Parse("28/04/2027 16:17:00"), Duration = TimeSpan.Parse("29.11:07:00") },
            new Lunation() { Index = 1291, NewMoon = DateTime.Parse("06/05/2027 06:58:00"), FirstQuarterMoon = DateTime.Parse("13/05/2027 00:43:00"), FullMoon = DateTime.Parse("20/05/2027 06:58:00"), LastQuarterMoon = DateTime.Parse("28/05/2027 09:57:00"), Duration = TimeSpan.Parse("29.08:42:00") },
            new Lunation() { Index = 1292, NewMoon = DateTime.Parse("04/06/2027 15:40:00"), FirstQuarterMoon = DateTime.Parse("11/06/2027 06:56:00"), FullMoon = DateTime.Parse("18/06/2027 20:44:00"), LastQuarterMoon = DateTime.Parse("27/06/2027 00:54:00"), Duration = TimeSpan.Parse("29.07:22:00") },
            new Lunation() { Index = 1293, NewMoon = DateTime.Parse("03/07/2027 23:02:00"), FirstQuarterMoon = DateTime.Parse("10/07/2027 14:39:00"), FullMoon = DateTime.Parse("18/07/2027 11:44:00"), LastQuarterMoon = DateTime.Parse("26/07/2027 12:54:00"), Duration = TimeSpan.Parse("29.07:03:00") },
            new Lunation() { Index = 1294, NewMoon = DateTime.Parse("02/08/2027 06:05:00"), FirstQuarterMoon = DateTime.Parse("09/08/2027 00:54:00"), FullMoon = DateTime.Parse("17/08/2027 03:28:00"), LastQuarterMoon = DateTime.Parse("24/08/2027 22:27:00"), Duration = TimeSpan.Parse("29.07:36:00") },
            new Lunation() { Index = 1295, NewMoon = DateTime.Parse("31/08/2027 13:41:00"), FirstQuarterMoon = DateTime.Parse("07/09/2027 14:31:00"), FullMoon = DateTime.Parse("15/09/2027 19:03:00"), LastQuarterMoon = DateTime.Parse("23/09/2027 06:20:00"), Duration = TimeSpan.Parse("29.08:55:00") },
            new Lunation() { Index = 1296, NewMoon = DateTime.Parse("29/09/2027 22:36:00"), FirstQuarterMoon = DateTime.Parse("07/10/2027 07:47:00"), FullMoon = DateTime.Parse("15/10/2027 09:47:00"), LastQuarterMoon = DateTime.Parse("22/10/2027 13:29:00"), Duration = TimeSpan.Parse("29.11:00:00") },
            new Lunation() { Index = 1297, NewMoon = DateTime.Parse("29/10/2027 09:36:00"), FirstQuarterMoon = DateTime.Parse("06/11/2027 03:59:00"), FullMoon = DateTime.Parse("13/11/2027 22:25:00"), LastQuarterMoon = DateTime.Parse("20/11/2027 19:48:00"), Duration = TimeSpan.Parse("29.13:48:00") },
            new Lunation() { Index = 1298, NewMoon = DateTime.Parse("27/11/2027 22:24:00"), FirstQuarterMoon = DateTime.Parse("06/12/2027 00:22:00"), FullMoon = DateTime.Parse("13/12/2027 11:08:00"), LastQuarterMoon = DateTime.Parse("20/12/2027 04:10:00"), Duration = TimeSpan.Parse("29.16:48:00") },
            new Lunation() { Index = 1299, NewMoon = DateTime.Parse("27/12/2027 15:12:00"), FirstQuarterMoon = DateTime.Parse("04/01/2028 20:40:00"), FullMoon = DateTime.Parse("11/01/2028 23:03:00"), LastQuarterMoon = DateTime.Parse("18/01/2028 14:25:00"), Duration = TimeSpan.Parse("29.19:00:00") },
            new Lunation() { Index = 1300, NewMoon = DateTime.Parse("26/01/2028 10:12:00"), FirstQuarterMoon = DateTime.Parse("03/02/2028 14:10:00"), FullMoon = DateTime.Parse("10/02/2028 10:03:00"), LastQuarterMoon = DateTime.Parse("17/02/2028 03:07:00"), Duration = TimeSpan.Parse("29.19:25:00") },
            new Lunation() { Index = 1301, NewMoon = DateTime.Parse("25/02/2028 05:37:00"), FirstQuarterMoon = DateTime.Parse("04/03/2028 04:02:00"), FullMoon = DateTime.Parse("10/03/2028 20:06:00"), LastQuarterMoon = DateTime.Parse("17/03/2028 19:22:00"), Duration = TimeSpan.Parse("29.17:54:00") },
            new Lunation() { Index = 1302, NewMoon = DateTime.Parse("26/03/2028 00:31:00"), FirstQuarterMoon = DateTime.Parse("02/04/2028 15:15:00"), FullMoon = DateTime.Parse("09/04/2028 06:26:00"), LastQuarterMoon = DateTime.Parse("16/04/2028 12:36:00"), Duration = TimeSpan.Parse("29.15:16:00") },
            new Lunation() { Index = 1303, NewMoon = DateTime.Parse("24/04/2028 15:46:00"), FirstQuarterMoon = DateTime.Parse("01/05/2028 22:25:00"), FullMoon = DateTime.Parse("08/05/2028 15:48:00"), LastQuarterMoon = DateTime.Parse("16/05/2028 06:43:00"), Duration = TimeSpan.Parse("29.12:29:00") },
            new Lunation() { Index = 1304, NewMoon = DateTime.Parse("24/05/2028 04:16:00"), FirstQuarterMoon = DateTime.Parse("31/05/2028 03:36:00"), FullMoon = DateTime.Parse("07/06/2028 02:08:00"), LastQuarterMoon = DateTime.Parse("15/06/2028 00:27:00"), Duration = TimeSpan.Parse("29.10:11:00") },
            new Lunation() { Index = 1305, NewMoon = DateTime.Parse("22/06/2028 14:27:00"), FirstQuarterMoon = DateTime.Parse("29/06/2028 08:10:00"), FullMoon = DateTime.Parse("06/07/2028 14:10:00"), LastQuarterMoon = DateTime.Parse("14/07/2028 16:56:00"), Duration = TimeSpan.Parse("29.08:34:00") },
            new Lunation() { Index = 1306, NewMoon = DateTime.Parse("21/07/2028 23:01:00"), FirstQuarterMoon = DateTime.Parse("28/07/2028 13:40:00"), FullMoon = DateTime.Parse("05/08/2028 04:09:00"), LastQuarterMoon = DateTime.Parse("13/08/2028 07:45:00"), Duration = TimeSpan.Parse("29.07:42:00") },
            new Lunation() { Index = 1307, NewMoon = DateTime.Parse("20/08/2028 06:43:00"), FirstQuarterMoon = DateTime.Parse("26/08/2028 21:35:00"), FullMoon = DateTime.Parse("03/09/2028 19:47:00"), LastQuarterMoon = DateTime.Parse("11/09/2028 20:45:00"), Duration = TimeSpan.Parse("29.07:40:00") },
            new Lunation() { Index = 1308, NewMoon = DateTime.Parse("18/09/2028 14:23:00"), FirstQuarterMoon = DateTime.Parse("25/09/2028 09:10:00"), FullMoon = DateTime.Parse("03/10/2028 12:25:00"), LastQuarterMoon = DateTime.Parse("11/10/2028 07:56:00"), Duration = TimeSpan.Parse("29.08:33:00") },
            new Lunation() { Index = 1309, NewMoon = DateTime.Parse("17/10/2028 22:56:00"), FirstQuarterMoon = DateTime.Parse("25/10/2028 00:53:00"), FullMoon = DateTime.Parse("02/11/2028 05:17:00"), LastQuarterMoon = DateTime.Parse("09/11/2028 16:25:00"), Duration = TimeSpan.Parse("29.10:21:00") },
            new Lunation() { Index = 1310, NewMoon = DateTime.Parse("16/11/2028 08:17:00"), FirstQuarterMoon = DateTime.Parse("23/11/2028 19:14:00"), FullMoon = DateTime.Parse("01/12/2028 20:40:00"), LastQuarterMoon = DateTime.Parse("09/12/2028 00:38:00"), Duration = TimeSpan.Parse("29.12:48:00") },
            new Lunation() { Index = 1311, NewMoon = DateTime.Parse("15/12/2028 21:06:00"), FirstQuarterMoon = DateTime.Parse("23/12/2028 16:44:00"), FullMoon = DateTime.Parse("31/12/2028 11:48:00"), LastQuarterMoon = DateTime.Parse("07/01/2029 08:26:00"), Duration = TimeSpan.Parse("29.15:18:00") },
            new Lunation() { Index = 1312, NewMoon = DateTime.Parse("14/01/2029 12:24:00"), FirstQuarterMoon = DateTime.Parse("22/01/2029 14:23:00"), FullMoon = DateTime.Parse("30/01/2029 01:03:00"), LastQuarterMoon = DateTime.Parse("05/02/2029 16:52:00"), Duration = TimeSpan.Parse("29.17:07:00") },
            new Lunation() { Index = 1313, NewMoon = DateTime.Parse("13/02/2029 05:31:00"), FirstQuarterMoon = DateTime.Parse("21/02/2029 10:09:00"), FullMoon = DateTime.Parse("28/02/2029 12:10:00"), LastQuarterMoon = DateTime.Parse("07/03/2029 02:51:00"), Duration = TimeSpan.Parse("29.17:48:00") },
            new Lunation() { Index = 1314, NewMoon = DateTime.Parse("15/03/2029 00:19:00"), FirstQuarterMoon = DateTime.Parse("23/03/2029 03:33:00"), FullMoon = DateTime.Parse("29/03/2029 22:26:00"), LastQuarterMoon = DateTime.Parse("05/04/2029 15:51:00"), Duration = TimeSpan.Parse("29.17:21:00") },
            new Lunation() { Index = 1315, NewMoon = DateTime.Parse("13/04/2029 17:40:00"), FirstQuarterMoon = DateTime.Parse("21/04/2029 15:50:00"), FullMoon = DateTime.Parse("28/04/2029 06:36:00"), LastQuarterMoon = DateTime.Parse("05/05/2029 05:48:00"), Duration = TimeSpan.Parse("29.16:02:00") },
            new Lunation() { Index = 1316, NewMoon = DateTime.Parse("13/05/2029 09:42:00"), FirstQuarterMoon = DateTime.Parse("21/05/2029 00:16:00"), FullMoon = DateTime.Parse("27/05/2029 14:37:00"), LastQuarterMoon = DateTime.Parse("03/06/2029 21:18:00"), Duration = TimeSpan.Parse("29.14:08:00") },
            new Lunation() { Index = 1317, NewMoon = DateTime.Parse("11/06/2029 23:50:00"), FirstQuarterMoon = DateTime.Parse("19/06/2029 05:54:00"), FullMoon = DateTime.Parse("25/06/2029 23:22:00"), LastQuarterMoon = DateTime.Parse("03/07/2029 13:57:00"), Duration = TimeSpan.Parse("29.12:01:00") },
            new Lunation() { Index = 1318, NewMoon = DateTime.Parse("11/07/2029 11:51:00"), FirstQuarterMoon = DateTime.Parse("18/07/2029 10:14:00"), FullMoon = DateTime.Parse("25/07/2029 09:35:00"), LastQuarterMoon = DateTime.Parse("02/08/2029 07:15:00"), Duration = TimeSpan.Parse("29.10:05:00") },
            new Lunation() { Index = 1319, NewMoon = DateTime.Parse("09/08/2029 21:55:00"), FirstQuarterMoon = DateTime.Parse("16/08/2029 14:55:00"), FullMoon = DateTime.Parse("23/08/2029 21:51:00"), LastQuarterMoon = DateTime.Parse("01/09/2029 00:33:00"), Duration = TimeSpan.Parse("29.08:49:00") },
            new Lunation() { Index = 1320, NewMoon = DateTime.Parse("08/09/2029 06:44:00"), FirstQuarterMoon = DateTime.Parse("14/09/2029 21:29:00"), FullMoon = DateTime.Parse("22/09/2029 12:29:00"), LastQuarterMoon = DateTime.Parse("30/09/2029 16:56:00"), Duration = TimeSpan.Parse("29.08:30:00") },
            new Lunation() { Index = 1321, NewMoon = DateTime.Parse("07/10/2029 15:14:00"), FirstQuarterMoon = DateTime.Parse("14/10/2029 07:08:00"), FullMoon = DateTime.Parse("22/10/2029 05:27:00"), LastQuarterMoon = DateTime.Parse("30/10/2029 07:32:00"), Duration = TimeSpan.Parse("29.09:10:00") },
            new Lunation() { Index = 1322, NewMoon = DateTime.Parse("05/11/2029 23:24:00"), FirstQuarterMoon = DateTime.Parse("12/11/2029 19:35:00"), FullMoon = DateTime.Parse("20/11/2029 23:02:00"), LastQuarterMoon = DateTime.Parse("28/11/2029 18:47:00"), Duration = TimeSpan.Parse("29.10:28:00") },
            new Lunation() { Index = 1323, NewMoon = DateTime.Parse("05/12/2029 09:52:00"), FirstQuarterMoon = DateTime.Parse("12/12/2029 12:49:00"), FullMoon = DateTime.Parse("20/12/2029 17:46:00"), LastQuarterMoon = DateTime.Parse("28/12/2029 04:49:00"), Duration = TimeSpan.Parse("29.11:57:00") },
            new Lunation() { Index = 1324, NewMoon = DateTime.Parse("03/01/2030 21:49:00"), FirstQuarterMoon = DateTime.Parse("11/01/2030 09:06:00"), FullMoon = DateTime.Parse("19/01/2030 10:54:00"), LastQuarterMoon = DateTime.Parse("26/01/2030 13:14:00"), Duration = TimeSpan.Parse("29.13:18:00") },
            new Lunation() { Index = 1325, NewMoon = DateTime.Parse("02/02/2030 11:07:00"), FirstQuarterMoon = DateTime.Parse("10/02/2030 06:49:00"), FullMoon = DateTime.Parse("18/02/2030 01:19:00"), LastQuarterMoon = DateTime.Parse("24/02/2030 20:57:00"), Duration = TimeSpan.Parse("29.14:27:00") },
            new Lunation() { Index = 1326, NewMoon = DateTime.Parse("04/03/2030 01:34:00"), FirstQuarterMoon = DateTime.Parse("12/03/2030 04:47:00"), FullMoon = DateTime.Parse("19/03/2030 13:56:00"), LastQuarterMoon = DateTime.Parse("26/03/2030 05:51:00"), Duration = TimeSpan.Parse("29.15:28:00") },
            new Lunation() { Index = 1327, NewMoon = DateTime.Parse("02/04/2030 18:02:00"), FirstQuarterMoon = DateTime.Parse("10/04/2030 22:56:00"), FullMoon = DateTime.Parse("17/04/2030 23:19:00"), LastQuarterMoon = DateTime.Parse("24/04/2030 14:38:00"), Duration = TimeSpan.Parse("29.16:10:00") },
            new Lunation() { Index = 1328, NewMoon = DateTime.Parse("02/05/2030 10:12:00"), FirstQuarterMoon = DateTime.Parse("10/05/2030 13:11:00"), FullMoon = DateTime.Parse("17/05/2030 07:19:00"), LastQuarterMoon = DateTime.Parse("24/05/2030 00:57:00"), Duration = TimeSpan.Parse("29.16:09:00") },
            new Lunation() { Index = 1329, NewMoon = DateTime.Parse("01/06/2030 02:21:00"), FirstQuarterMoon = DateTime.Parse("08/06/2030 23:35:00"), FullMoon = DateTime.Parse("15/06/2030 14:40:00"), LastQuarterMoon = DateTime.Parse("22/06/2030 13:19:00"), Duration = TimeSpan.Parse("29.15:13:00") },
            new Lunation() { Index = 1330, NewMoon = DateTime.Parse("30/06/2030 17:34:00"), FirstQuarterMoon = DateTime.Parse("08/07/2030 07:01:00"), FullMoon = DateTime.Parse("14/07/2030 22:11:00"), LastQuarterMoon = DateTime.Parse("22/07/2030 04:07:00"), Duration = TimeSpan.Parse("29.13:36:00") },
            new Lunation() { Index = 1331, NewMoon = DateTime.Parse("30/07/2030 07:10:00"), FirstQuarterMoon = DateTime.Parse("06/08/2030 12:42:00"), FullMoon = DateTime.Parse("13/08/2030 06:44:00"), LastQuarterMoon = DateTime.Parse("20/08/2030 21:15:00"), Duration = TimeSpan.Parse("29.11:56:00") },
            new Lunation() { Index = 1332, NewMoon = DateTime.Parse("28/08/2030 19:07:00"), FirstQuarterMoon = DateTime.Parse("04/09/2030 17:55:00"), FullMoon = DateTime.Parse("11/09/2030 17:17:00"), LastQuarterMoon = DateTime.Parse("19/09/2030 15:56:00"), Duration = TimeSpan.Parse("29.10:47:00") },
            new Lunation() { Index = 1333, NewMoon = DateTime.Parse("27/09/2030 05:54:00"), FirstQuarterMoon = DateTime.Parse("03/10/2030 23:56:00"), FullMoon = DateTime.Parse("11/10/2030 06:46:00"), LastQuarterMoon = DateTime.Parse("19/10/2030 10:50:00"), Duration = TimeSpan.Parse("29.10:22:00") },
            new Lunation() { Index = 1334, NewMoon = DateTime.Parse("26/10/2030 16:16:00"), FirstQuarterMoon = DateTime.Parse("02/11/2030 07:55:00"), FullMoon = DateTime.Parse("09/11/2030 22:30:00"), LastQuarterMoon = DateTime.Parse("18/11/2030 03:32:00"), Duration = TimeSpan.Parse("29.10:29:00") },
            new Lunation() { Index = 1335, NewMoon = DateTime.Parse("25/11/2030 01:46:00"), FirstQuarterMoon = DateTime.Parse("01/12/2030 17:56:00"), FullMoon = DateTime.Parse("09/12/2030 17:40:00"), LastQuarterMoon = DateTime.Parse("17/12/2030 19:01:00"), Duration = TimeSpan.Parse("29.10:46:00") },
            new Lunation() { Index = 1336, NewMoon = DateTime.Parse("24/12/2030 12:32:00"), FirstQuarterMoon = DateTime.Parse("31/12/2030 08:36:00"), FullMoon = DateTime.Parse("08/01/2031 13:25:00"), LastQuarterMoon = DateTime.Parse("16/01/2031 07:47:00"), Duration = TimeSpan.Parse("29.10:59:00") },
            new Lunation() { Index = 1337, NewMoon = DateTime.Parse("22/01/2031 23:30:00"), FirstQuarterMoon = DateTime.Parse("30/01/2031 02:43:00"), FullMoon = DateTime.Parse("07/02/2031 07:46:00"), LastQuarterMoon = DateTime.Parse("14/02/2031 17:49:00"), Duration = TimeSpan.Parse("29.11:18:00") },
            new Lunation() { Index = 1338, NewMoon = DateTime.Parse("21/02/2031 10:48:00"), FirstQuarterMoon = DateTime.Parse("28/02/2031 23:02:00"), FullMoon = DateTime.Parse("08/03/2031 23:29:00"), LastQuarterMoon = DateTime.Parse("16/03/2031 02:35:00"), Duration = TimeSpan.Parse("29.12:00:00") },
            new Lunation() { Index = 1339, NewMoon = DateTime.Parse("22/03/2031 23:49:00"), FirstQuarterMoon = DateTime.Parse("30/03/2031 20:32:00"), FullMoon = DateTime.Parse("07/04/2031 13:21:00"), LastQuarterMoon = DateTime.Parse("14/04/2031 08:57:00"), Duration = TimeSpan.Parse("29.13:08:00") },
            new Lunation() { Index = 1340, NewMoon = DateTime.Parse("21/04/2031 12:57:00"), FirstQuarterMoon = DateTime.Parse("29/04/2031 15:19:00"), FullMoon = DateTime.Parse("06/05/2031 23:39:00"), LastQuarterMoon = DateTime.Parse("13/05/2031 15:06:00"), Duration = TimeSpan.Parse("29.14:20:00") },
            new Lunation() { Index = 1341, NewMoon = DateTime.Parse("21/05/2031 03:17:00"), FirstQuarterMoon = DateTime.Parse("29/05/2031 07:19:00"), FullMoon = DateTime.Parse("05/06/2031 07:58:00"), LastQuarterMoon = DateTime.Parse("11/06/2031 22:20:00"), Duration = TimeSpan.Parse("29.15:07:00") },
            new Lunation() { Index = 1342, NewMoon = DateTime.Parse("19/06/2031 18:24:00"), FirstQuarterMoon = DateTime.Parse("27/06/2031 20:18:00"), FullMoon = DateTime.Parse("04/07/2031 15:01:00"), LastQuarterMoon = DateTime.Parse("11/07/2031 07:49:00"), Duration = TimeSpan.Parse("29.15:16:00") },
            new Lunation() { Index = 1343, NewMoon = DateTime.Parse("19/07/2031 09:40:00"), FirstQuarterMoon = DateTime.Parse("27/07/2031 06:34:00"), FullMoon = DateTime.Parse("02/08/2031 21:45:00"), LastQuarterMoon = DateTime.Parse("09/08/2031 20:23:00"), Duration = TimeSpan.Parse("29.14:52:00") },
            new Lunation() { Index = 1344, NewMoon = DateTime.Parse("18/08/2031 00:32:00"), FirstQuarterMoon = DateTime.Parse("25/08/2031 14:39:00"), FullMoon = DateTime.Parse("01/09/2031 05:20:00"), LastQuarterMoon = DateTime.Parse("08/09/2031 12:14:00"), Duration = TimeSpan.Parse("29.14:15:00") },
            new Lunation() { Index = 1345, NewMoon = DateTime.Parse("16/09/2031 14:46:00"), FirstQuarterMoon = DateTime.Parse("23/09/2031 21:19:00"), FullMoon = DateTime.Parse("30/09/2031 14:57:00"), LastQuarterMoon = DateTime.Parse("08/10/2031 06:50:00"), Duration = TimeSpan.Parse("29.13:34:00") },
            new Lunation() { Index = 1346, NewMoon = DateTime.Parse("16/10/2031 04:20:00"), FirstQuarterMoon = DateTime.Parse("23/10/2031 03:36:00"), FullMoon = DateTime.Parse("30/10/2031 03:32:00"), LastQuarterMoon = DateTime.Parse("07/11/2031 02:02:00"), Duration = TimeSpan.Parse("29.12:49:00") },
            new Lunation() { Index = 1347, NewMoon = DateTime.Parse("14/11/2031 16:09:00"), FirstQuarterMoon = DateTime.Parse("21/11/2031 09:44:00"), FullMoon = DateTime.Parse("28/11/2031 18:18:00"), LastQuarterMoon = DateTime.Parse("06/12/2031 22:19:00"), Duration = TimeSpan.Parse("29.11:56:00") },
            new Lunation() { Index = 1348, NewMoon = DateTime.Parse("14/12/2031 04:05:00"), FirstQuarterMoon = DateTime.Parse("20/12/2031 19:00:00"), FullMoon = DateTime.Parse("28/12/2031 12:32:00"), LastQuarterMoon = DateTime.Parse("05/01/2032 17:04:00"), Duration = TimeSpan.Parse("29.11:01:00") },
            new Lunation() { Index = 1349, NewMoon = DateTime.Parse("12/01/2032 15:06:00"), FirstQuarterMoon = DateTime.Parse("19/01/2032 07:14:00"), FullMoon = DateTime.Parse("27/01/2032 07:52:00"), LastQuarterMoon = DateTime.Parse("04/02/2032 08:48:00"), Duration = TimeSpan.Parse("29.10:18:00") },
            new Lunation() { Index = 1350, NewMoon = DateTime.Parse("11/02/2032 01:24:00"), FirstQuarterMoon = DateTime.Parse("17/02/2032 22:28:00"), FullMoon = DateTime.Parse("26/02/2032 02:43:00"), LastQuarterMoon = DateTime.Parse("04/03/2032 20:46:00"), Duration = TimeSpan.Parse("29.10:00:00") },
            new Lunation() { Index = 1351, NewMoon = DateTime.Parse("11/03/2032 11:24:00"), FirstQuarterMoon = DateTime.Parse("18/03/2032 16:56:00"), FullMoon = DateTime.Parse("26/03/2032 20:46:00"), LastQuarterMoon = DateTime.Parse("03/04/2032 06:10:00"), Duration = TimeSpan.Parse("29.10:15:00") },
            new Lunation() { Index = 1352, NewMoon = DateTime.Parse("09/04/2032 22:39:00"), FirstQuarterMoon = DateTime.Parse("17/04/2032 11:24:00"), FullMoon = DateTime.Parse("25/04/2032 11:09:00"), LastQuarterMoon = DateTime.Parse("02/05/2032 12:01:00"), Duration = TimeSpan.Parse("29.10:56:00") },
            new Lunation() { Index = 1353, NewMoon = DateTime.Parse("09/05/2032 09:35:00"), FirstQuarterMoon = DateTime.Parse("17/05/2032 05:43:00"), FullMoon = DateTime.Parse("24/05/2032 22:37:00"), LastQuarterMoon = DateTime.Parse("31/05/2032 16:51:00"), Duration = TimeSpan.Parse("29.11:56:00") },
            new Lunation() { Index = 1354, NewMoon = DateTime.Parse("07/06/2032 21:32:00"), FirstQuarterMoon = DateTime.Parse("15/06/2032 22:59:00"), FullMoon = DateTime.Parse("23/06/2032 07:32:00"), LastQuarterMoon = DateTime.Parse("29/06/2032 22:11:00"), Duration = TimeSpan.Parse("29.13:09:00") },
            new Lunation() { Index = 1355, NewMoon = DateTime.Parse("07/07/2032 10:41:00"), FirstQuarterMoon = DateTime.Parse("15/07/2032 14:32:00"), FullMoon = DateTime.Parse("22/07/2032 14:51:00"), LastQuarterMoon = DateTime.Parse("29/07/2032 05:25:00"), Duration = TimeSpan.Parse("29.14:30:00") },
            new Lunation() { Index = 1356, NewMoon = DateTime.Parse("06/08/2032 01:11:00"), FirstQuarterMoon = DateTime.Parse("14/08/2032 03:50:00"), FullMoon = DateTime.Parse("20/08/2032 21:46:00"), LastQuarterMoon = DateTime.Parse("27/08/2032 15:33:00"), Duration = TimeSpan.Parse("29.15:45:00") },
            new Lunation() { Index = 1357, NewMoon = DateTime.Parse("04/09/2032 16:56:00"), FirstQuarterMoon = DateTime.Parse("12/09/2032 14:49:00"), FullMoon = DateTime.Parse("19/09/2032 05:30:00"), LastQuarterMoon = DateTime.Parse("26/09/2032 05:12:00"), Duration = TimeSpan.Parse("29.16:30:00") },
            new Lunation() { Index = 1358, NewMoon = DateTime.Parse("04/10/2032 09:26:00"), FirstQuarterMoon = DateTime.Parse("11/10/2032 23:47:00"), FullMoon = DateTime.Parse("18/10/2032 14:58:00"), LastQuarterMoon = DateTime.Parse("25/10/2032 22:28:00"), Duration = TimeSpan.Parse("29.16:19:00") },
            new Lunation() { Index = 1359, NewMoon = DateTime.Parse("03/11/2032 01:44:00"), FirstQuarterMoon = DateTime.Parse("10/11/2032 06:33:00"), FullMoon = DateTime.Parse("17/11/2032 01:42:00"), LastQuarterMoon = DateTime.Parse("24/11/2032 17:47:00"), Duration = TimeSpan.Parse("29.15:08:00") },
            new Lunation() { Index = 1360, NewMoon = DateTime.Parse("02/12/2032 15:52:00"), FirstQuarterMoon = DateTime.Parse("09/12/2032 14:08:00"), FullMoon = DateTime.Parse("16/12/2032 15:49:00"), LastQuarterMoon = DateTime.Parse("24/12/2032 15:39:00"), Duration = TimeSpan.Parse("29.13:24:00") },
            new Lunation() { Index = 1361, NewMoon = DateTime.Parse("01/01/2033 05:16:00"), FirstQuarterMoon = DateTime.Parse("07/01/2033 22:34:00"), FullMoon = DateTime.Parse("15/01/2033 08:07:00"), LastQuarterMoon = DateTime.Parse("23/01/2033 12:45:00"), Duration = TimeSpan.Parse("29.11:43:00") },
            new Lunation() { Index = 1362, NewMoon = DateTime.Parse("30/01/2033 16:59:00"), FirstQuarterMoon = DateTime.Parse("06/02/2033 08:34:00"), FullMoon = DateTime.Parse("14/02/2033 02:04:00"), LastQuarterMoon = DateTime.Parse("22/02/2033 06:53:00"), Duration = TimeSpan.Parse("29.10:24:00") },
            new Lunation() { Index = 1363, NewMoon = DateTime.Parse("01/03/2033 03:23:00"), FirstQuarterMoon = DateTime.Parse("07/03/2033 20:27:00"), FullMoon = DateTime.Parse("15/03/2033 21:37:00"), LastQuarterMoon = DateTime.Parse("23/03/2033 21:49:00"), Duration = TimeSpan.Parse("29.09:28:00") },
            new Lunation() { Index = 1364, NewMoon = DateTime.Parse("30/03/2033 13:51:00"), FirstQuarterMoon = DateTime.Parse("06/04/2033 11:13:00"), FullMoon = DateTime.Parse("14/04/2033 15:17:00"), LastQuarterMoon = DateTime.Parse("22/04/2033 07:42:00"), Duration = TimeSpan.Parse("29.08:55:00") },
            new Lunation() { Index = 1365, NewMoon = DateTime.Parse("28/04/2033 22:46:00"), FirstQuarterMoon = DateTime.Parse("06/05/2033 02:45:00"), FullMoon = DateTime.Parse("14/05/2033 06:42:00"), LastQuarterMoon = DateTime.Parse("21/05/2033 14:29:00"), Duration = TimeSpan.Parse("29.08:50:00") },
            new Lunation() { Index = 1366, NewMoon = DateTime.Parse("28/05/2033 07:36:00"), FirstQuarterMoon = DateTime.Parse("04/06/2033 19:38:00"), FullMoon = DateTime.Parse("12/06/2033 19:19:00"), LastQuarterMoon = DateTime.Parse("19/06/2033 19:29:00"), Duration = TimeSpan.Parse("29.09:31:00") },
            new Lunation() { Index = 1367, NewMoon = DateTime.Parse("26/06/2033 17:07:00"), FirstQuarterMoon = DateTime.Parse("04/07/2033 13:12:00"), FullMoon = DateTime.Parse("12/07/2033 05:28:00"), LastQuarterMoon = DateTime.Parse("19/07/2033 00:07:00"), Duration = TimeSpan.Parse("29.11:06:00") },
            new Lunation() { Index = 1368, NewMoon = DateTime.Parse("26/07/2033 04:12:00"), FirstQuarterMoon = DateTime.Parse("03/08/2033 06:25:00"), FullMoon = DateTime.Parse("10/08/2033 14:07:00"), LastQuarterMoon = DateTime.Parse("17/08/2033 05:42:00"), Duration = TimeSpan.Parse("29.13:27:00") },
            new Lunation() { Index = 1369, NewMoon = DateTime.Parse("24/08/2033 17:39:00"), FirstQuarterMoon = DateTime.Parse("01/09/2033 22:23:00"), FullMoon = DateTime.Parse("08/09/2033 22:20:00"), LastQuarterMoon = DateTime.Parse("15/09/2033 13:33:00"), Duration = TimeSpan.Parse("29.16:00:00") },
            new Lunation() { Index = 1370, NewMoon = DateTime.Parse("23/09/2033 09:39:00"), FirstQuarterMoon = DateTime.Parse("01/10/2033 12:32:00"), FullMoon = DateTime.Parse("08/10/2033 06:58:00"), LastQuarterMoon = DateTime.Parse("15/10/2033 00:47:00"), Duration = TimeSpan.Parse("29.17:49:00") },
            new Lunation() { Index = 1371, NewMoon = DateTime.Parse("23/10/2033 03:28:00"), FirstQuarterMoon = DateTime.Parse("31/10/2033 00:46:00"), FullMoon = DateTime.Parse("06/11/2033 15:32:00"), LastQuarterMoon = DateTime.Parse("13/11/2033 15:08:00"), Duration = TimeSpan.Parse("29.18:11:00") },
            new Lunation() { Index = 1372, NewMoon = DateTime.Parse("21/11/2033 20:39:00"), FirstQuarterMoon = DateTime.Parse("29/11/2033 10:15:00"), FullMoon = DateTime.Parse("06/12/2033 02:22:00"), LastQuarterMoon = DateTime.Parse("13/12/2033 10:28:00"), Duration = TimeSpan.Parse("29.17:07:00") },
            new Lunation() { Index = 1373, NewMoon = DateTime.Parse("21/12/2033 13:46:00"), FirstQuarterMoon = DateTime.Parse("28/12/2033 19:20:00"), FullMoon = DateTime.Parse("04/01/2034 14:47:00"), LastQuarterMoon = DateTime.Parse("12/01/2034 08:17:00"), Duration = TimeSpan.Parse("29.15:15:00") },
            new Lunation() { Index = 1374, NewMoon = DateTime.Parse("20/01/2034 05:01:00"), FirstQuarterMoon = DateTime.Parse("27/01/2034 03:31:00"), FullMoon = DateTime.Parse("03/02/2034 05:04:00"), LastQuarterMoon = DateTime.Parse("11/02/2034 06:08:00"), Duration = TimeSpan.Parse("29.13:09:00") },
            new Lunation() { Index = 1375, NewMoon = DateTime.Parse("18/02/2034 18:10:00"), FirstQuarterMoon = DateTime.Parse("25/02/2034 11:34:00"), FullMoon = DateTime.Parse("04/03/2034 21:10:00"), LastQuarterMoon = DateTime.Parse("13/03/2034 02:44:00"), Duration = TimeSpan.Parse("29.11:04:00") },
            new Lunation() { Index = 1376, NewMoon = DateTime.Parse("20/03/2034 06:14:00"), FirstQuarterMoon = DateTime.Parse("26/03/2034 21:18:00"), FullMoon = DateTime.Parse("03/04/2034 15:18:00"), LastQuarterMoon = DateTime.Parse("11/04/2034 18:45:00"), Duration = TimeSpan.Parse("29.09:11:00") },
            new Lunation() { Index = 1377, NewMoon = DateTime.Parse("18/04/2034 15:25:00"), FirstQuarterMoon = DateTime.Parse("25/04/2034 07:34:00"), FullMoon = DateTime.Parse("03/05/2034 08:15:00"), LastQuarterMoon = DateTime.Parse("11/05/2034 06:56:00"), Duration = TimeSpan.Parse("29.07:47:00") },
            new Lunation() { Index = 1378, NewMoon = DateTime.Parse("17/05/2034 23:12:00"), FirstQuarterMoon = DateTime.Parse("24/05/2034 19:57:00"), FullMoon = DateTime.Parse("01/06/2034 23:54:00"), LastQuarterMoon = DateTime.Parse("09/06/2034 15:43:00"), Duration = TimeSpan.Parse("29.07:13:00") },
            new Lunation() { Index = 1379, NewMoon = DateTime.Parse("16/06/2034 06:25:00"), FirstQuarterMoon = DateTime.Parse("23/06/2034 10:35:00"), FullMoon = DateTime.Parse("01/07/2034 13:44:00"), LastQuarterMoon = DateTime.Parse("08/07/2034 21:59:00"), Duration = TimeSpan.Parse("29.07:49:00") },
            new Lunation() { Index = 1380, NewMoon = DateTime.Parse("15/07/2034 14:15:00"), FirstQuarterMoon = DateTime.Parse("23/07/2034 03:05:00"), FullMoon = DateTime.Parse("31/07/2034 01:54:00"), LastQuarterMoon = DateTime.Parse("07/08/2034 02:50:00"), Duration = TimeSpan.Parse("29.09:38:00") },
            new Lunation() { Index = 1381, NewMoon = DateTime.Parse("13/08/2034 23:53:00"), FirstQuarterMoon = DateTime.Parse("21/08/2034 20:43:00"), FullMoon = DateTime.Parse("29/08/2034 12:49:00"), LastQuarterMoon = DateTime.Parse("05/09/2034 07:41:00"), Duration = TimeSpan.Parse("29.12:21:00") },
            new Lunation() { Index = 1382, NewMoon = DateTime.Parse("12/09/2034 12:13:00"), FirstQuarterMoon = DateTime.Parse("20/09/2034 14:39:00"), FullMoon = DateTime.Parse("27/09/2034 22:56:00"), LastQuarterMoon = DateTime.Parse("04/10/2034 14:04:00"), Duration = TimeSpan.Parse("29.15:19:00") },
            new Lunation() { Index = 1383, NewMoon = DateTime.Parse("12/10/2034 03:32:00"), FirstQuarterMoon = DateTime.Parse("20/10/2034 08:02:00"), FullMoon = DateTime.Parse("27/10/2034 08:42:00"), LastQuarterMoon = DateTime.Parse("02/11/2034 23:27:00"), Duration = TimeSpan.Parse("29.17:44:00") },
            new Lunation() { Index = 1384, NewMoon = DateTime.Parse("10/11/2034 20:16:00"), FirstQuarterMoon = DateTime.Parse("18/11/2034 23:01:00"), FullMoon = DateTime.Parse("25/11/2034 17:32:00"), LastQuarterMoon = DateTime.Parse("02/12/2034 11:46:00"), Duration = TimeSpan.Parse("29.18:58:00") },
            new Lunation() { Index = 1385, NewMoon = DateTime.Parse("10/12/2034 15:14:00"), FirstQuarterMoon = DateTime.Parse("18/12/2034 12:44:00"), FullMoon = DateTime.Parse("25/12/2034 03:54:00"), LastQuarterMoon = DateTime.Parse("01/01/2035 05:00:00"), Duration = TimeSpan.Parse("29.18:49:00") },
            new Lunation() { Index = 1386, NewMoon = DateTime.Parse("09/01/2035 10:03:00"), FirstQuarterMoon = DateTime.Parse("16/01/2035 23:45:00"), FullMoon = DateTime.Parse("23/01/2035 15:16:00"), LastQuarterMoon = DateTime.Parse("31/01/2035 01:02:00"), Duration = TimeSpan.Parse("29.17:19:00") },
            new Lunation() { Index = 1387, NewMoon = DateTime.Parse("08/02/2035 03:22:00"), FirstQuarterMoon = DateTime.Parse("15/02/2035 08:16:00"), FullMoon = DateTime.Parse("22/02/2035 03:53:00"), LastQuarterMoon = DateTime.Parse("01/03/2035 22:01:00"), Duration = TimeSpan.Parse("29.14:47:00") },
            new Lunation() { Index = 1388, NewMoon = DateTime.Parse("09/03/2035 18:09:00"), FirstQuarterMoon = DateTime.Parse("16/03/2035 16:14:00"), FullMoon = DateTime.Parse("23/03/2035 18:42:00"), LastQuarterMoon = DateTime.Parse("31/03/2035 19:06:00"), Duration = TimeSpan.Parse("29.11:48:00") },
            new Lunation() { Index = 1389, NewMoon = DateTime.Parse("08/04/2035 06:57:00"), FirstQuarterMoon = DateTime.Parse("14/04/2035 22:54:00"), FullMoon = DateTime.Parse("22/04/2035 09:20:00"), LastQuarterMoon = DateTime.Parse("30/04/2035 12:53:00"), Duration = TimeSpan.Parse("29.09:06:00") },
            new Lunation() { Index = 1390, NewMoon = DateTime.Parse("07/05/2035 16:03:00"), FirstQuarterMoon = DateTime.Parse("14/05/2035 06:28:00"), FullMoon = DateTime.Parse("22/05/2035 00:25:00"), LastQuarterMoon = DateTime.Parse("30/05/2035 03:30:00"), Duration = TimeSpan.Parse("29.07:17:00") },
            new Lunation() { Index = 1391, NewMoon = DateTime.Parse("05/06/2035 23:20:00"), FirstQuarterMoon = DateTime.Parse("12/06/2035 15:50:00"), FullMoon = DateTime.Parse("20/06/2035 15:37:00"), LastQuarterMoon = DateTime.Parse("28/06/2035 14:42:00"), Duration = TimeSpan.Parse("29.06:39:00") },
            new Lunation() { Index = 1392, NewMoon = DateTime.Parse("05/07/2035 05:59:00"), FirstQuarterMoon = DateTime.Parse("12/07/2035 03:32:00"), FullMoon = DateTime.Parse("20/07/2035 06:36:00"), LastQuarterMoon = DateTime.Parse("27/07/2035 22:55:00"), Duration = TimeSpan.Parse("29.07:13:00") },
            new Lunation() { Index = 1393, NewMoon = DateTime.Parse("03/08/2035 13:11:00"), FirstQuarterMoon = DateTime.Parse("10/08/2035 17:52:00"), FullMoon = DateTime.Parse("18/08/2035 21:00:00"), LastQuarterMoon = DateTime.Parse("26/08/2035 05:07:00"), Duration = TimeSpan.Parse("29.08:48:00") },
            new Lunation() { Index = 1394, NewMoon = DateTime.Parse("01/09/2035 21:59:00"), FirstQuarterMoon = DateTime.Parse("09/09/2035 10:47:00"), FullMoon = DateTime.Parse("17/09/2035 10:23:00"), LastQuarterMoon = DateTime.Parse("24/09/2035 10:39:00"), Duration = TimeSpan.Parse("29.11:07:00") },
            new Lunation() { Index = 1395, NewMoon = DateTime.Parse("01/10/2035 09:06:00"), FirstQuarterMoon = DateTime.Parse("09/10/2035 05:49:00"), FullMoon = DateTime.Parse("16/10/2035 22:35:00"), LastQuarterMoon = DateTime.Parse("23/10/2035 16:56:00"), Duration = TimeSpan.Parse("29.13:52:00") },
            new Lunation() { Index = 1396, NewMoon = DateTime.Parse("30/10/2035 22:58:00"), FirstQuarterMoon = DateTime.Parse("08/11/2035 00:50:00"), FullMoon = DateTime.Parse("15/11/2035 08:48:00"), LastQuarterMoon = DateTime.Parse("22/11/2035 00:16:00"), Duration = TimeSpan.Parse("29.16:39:00") },
            new Lunation() { Index = 1397, NewMoon = DateTime.Parse("29/11/2035 14:37:00"), FirstQuarterMoon = DateTime.Parse("07/12/2035 20:05:00"), FullMoon = DateTime.Parse("14/12/2035 19:33:00"), LastQuarterMoon = DateTime.Parse("21/12/2035 11:28:00"), Duration = TimeSpan.Parse("29.18:53:00") },
            new Lunation() { Index = 1398, NewMoon = DateTime.Parse("29/12/2035 09:30:00"), FirstQuarterMoon = DateTime.Parse("06/01/2036 12:48:00"), FullMoon = DateTime.Parse("13/01/2036 06:16:00"), LastQuarterMoon = DateTime.Parse("20/01/2036 01:46:00"), Duration = TimeSpan.Parse("29.19:46:00") },
            new Lunation() { Index = 1399, NewMoon = DateTime.Parse("28/01/2036 05:17:00"), FirstQuarterMoon = DateTime.Parse("05/02/2036 02:00:00"), FullMoon = DateTime.Parse("11/02/2036 17:08:00"), LastQuarterMoon = DateTime.Parse("18/02/2036 18:46:00"), Duration = TimeSpan.Parse("29.18:42:00") },
            new Lunation() { Index = 1400, NewMoon = DateTime.Parse("26/02/2036 23:59:00"), FirstQuarterMoon = DateTime.Parse("05/03/2036 11:48:00"), FullMoon = DateTime.Parse("12/03/2036 05:09:00"), LastQuarterMoon = DateTime.Parse("19/03/2036 14:38:00"), Duration = TimeSpan.Parse("29.15:57:00") },
            new Lunation() { Index = 1401, NewMoon = DateTime.Parse("27/03/2036 16:56:00"), FirstQuarterMoon = DateTime.Parse("03/04/2036 20:03:00"), FullMoon = DateTime.Parse("10/04/2036 16:22:00"), LastQuarterMoon = DateTime.Parse("18/04/2036 10:05:00"), Duration = TimeSpan.Parse("29.12:37:00") },
            new Lunation() { Index = 1402, NewMoon = DateTime.Parse("26/04/2036 05:33:00"), FirstQuarterMoon = DateTime.Parse("03/05/2036 01:54:00"), FullMoon = DateTime.Parse("10/05/2036 04:09:00"), LastQuarterMoon = DateTime.Parse("18/05/2036 04:39:00"), Duration = TimeSpan.Parse("29.09:44:00") },
            new Lunation() { Index = 1403, NewMoon = DateTime.Parse("25/05/2036 15:16:00"), FirstQuarterMoon = DateTime.Parse("01/06/2036 07:34:00"), FullMoon = DateTime.Parse("08/06/2036 17:01:00"), LastQuarterMoon = DateTime.Parse("16/06/2036 21:03:00"), Duration = TimeSpan.Parse("29.07:53:00") },
            new Lunation() { Index = 1404, NewMoon = DateTime.Parse("23/06/2036 23:09:00"), FirstQuarterMoon = DateTime.Parse("30/06/2036 14:13:00"), FullMoon = DateTime.Parse("08/07/2036 07:19:00"), LastQuarterMoon = DateTime.Parse("16/07/2036 10:39:00"), Duration = TimeSpan.Parse("29.07:07:00") },
            new Lunation() { Index = 1405, NewMoon = DateTime.Parse("23/07/2036 06:17:00"), FirstQuarterMoon = DateTime.Parse("29/07/2036 22:56:00"), FullMoon = DateTime.Parse("06/08/2036 22:48:00"), LastQuarterMoon = DateTime.Parse("14/08/2036 21:35:00"), Duration = TimeSpan.Parse("29.07:18:00") },
            new Lunation() { Index = 1406, NewMoon = DateTime.Parse("21/08/2036 13:35:00"), FirstQuarterMoon = DateTime.Parse("28/08/2036 10:43:00"), FullMoon = DateTime.Parse("05/09/2036 14:45:00"), LastQuarterMoon = DateTime.Parse("13/09/2036 06:29:00"), Duration = TimeSpan.Parse("29.08:16:00") },
            new Lunation() { Index = 1407, NewMoon = DateTime.Parse("19/09/2036 21:51:00"), FirstQuarterMoon = DateTime.Parse("27/09/2036 02:12:00"), FullMoon = DateTime.Parse("05/10/2036 06:15:00"), LastQuarterMoon = DateTime.Parse("12/10/2036 14:09:00"), Duration = TimeSpan.Parse("29.09:58:00") },
            new Lunation() { Index = 1408, NewMoon = DateTime.Parse("19/10/2036 07:49:00"), FirstQuarterMoon = DateTime.Parse("26/10/2036 21:13:00"), FullMoon = DateTime.Parse("03/11/2036 19:44:00"), LastQuarterMoon = DateTime.Parse("10/11/2036 20:28:00"), Duration = TimeSpan.Parse("29.12:24:00") },
            new Lunation() { Index = 1409, NewMoon = DateTime.Parse("17/11/2036 19:14:00"), FirstQuarterMoon = DateTime.Parse("25/11/2036 17:28:00"), FullMoon = DateTime.Parse("03/12/2036 09:08:00"), LastQuarterMoon = DateTime.Parse("10/12/2036 04:18:00"), Duration = TimeSpan.Parse("29.15:20:00") },
            new Lunation() { Index = 1410, NewMoon = DateTime.Parse("17/12/2036 10:34:00"), FirstQuarterMoon = DateTime.Parse("25/12/2036 14:44:00"), FullMoon = DateTime.Parse("01/01/2037 21:35:00"), LastQuarterMoon = DateTime.Parse("08/01/2037 13:29:00"), Duration = TimeSpan.Parse("29.18:00:00") },
            new Lunation() { Index = 1411, NewMoon = DateTime.Parse("16/01/2037 04:34:00"), FirstQuarterMoon = DateTime.Parse("24/01/2037 09:55:00"), FullMoon = DateTime.Parse("31/01/2037 09:04:00"), LastQuarterMoon = DateTime.Parse("07/02/2037 00:43:00"), Duration = TimeSpan.Parse("29.19:20:00") },
            new Lunation() { Index = 1412, NewMoon = DateTime.Parse("14/02/2037 23:54:00"), FirstQuarterMoon = DateTime.Parse("23/02/2037 01:40:00"), FullMoon = DateTime.Parse("01/03/2037 19:28:00"), LastQuarterMoon = DateTime.Parse("08/03/2037 15:25:00"), Duration = TimeSpan.Parse("29.18:42:00") },
            new Lunation() { Index = 1413, NewMoon = DateTime.Parse("16/03/2037 19:36:00"), FirstQuarterMoon = DateTime.Parse("24/03/2037 14:39:00"), FullMoon = DateTime.Parse("31/03/2037 05:53:00"), LastQuarterMoon = DateTime.Parse("07/04/2037 07:25:00"), Duration = TimeSpan.Parse("29.16:32:00") },
            new Lunation() { Index = 1414, NewMoon = DateTime.Parse("15/04/2037 12:07:00"), FirstQuarterMoon = DateTime.Parse("22/04/2037 23:11:00"), FullMoon = DateTime.Parse("29/04/2037 14:53:00"), LastQuarterMoon = DateTime.Parse("07/05/2037 00:56:00"), Duration = TimeSpan.Parse("29.13:47:00") },
            new Lunation() { Index = 1415, NewMoon = DateTime.Parse("15/05/2037 01:54:00"), FirstQuarterMoon = DateTime.Parse("22/05/2037 05:08:00"), FullMoon = DateTime.Parse("29/05/2037 00:24:00"), LastQuarterMoon = DateTime.Parse("05/06/2037 18:49:00"), Duration = TimeSpan.Parse("29.11:16:00") },
            new Lunation() { Index = 1416, NewMoon = DateTime.Parse("13/06/2037 13:10:00"), FirstQuarterMoon = DateTime.Parse("20/06/2037 09:45:00"), FullMoon = DateTime.Parse("27/06/2037 11:19:00"), LastQuarterMoon = DateTime.Parse("05/07/2037 12:00:00"), Duration = TimeSpan.Parse("29.09:22:00") },
            new Lunation() { Index = 1417, NewMoon = DateTime.Parse("12/07/2037 22:31:00"), FirstQuarterMoon = DateTime.Parse("19/07/2037 14:31:00"), FullMoon = DateTime.Parse("27/07/2037 00:15:00"), LastQuarterMoon = DateTime.Parse("04/08/2037 03:51:00"), Duration = TimeSpan.Parse("29.08:10:00") },
            new Lunation() { Index = 1418, NewMoon = DateTime.Parse("11/08/2037 06:41:00"), FirstQuarterMoon = DateTime.Parse("17/08/2037 20:59:00"), FullMoon = DateTime.Parse("25/08/2037 15:09:00"), LastQuarterMoon = DateTime.Parse("02/09/2037 18:03:00"), Duration = TimeSpan.Parse("29.07:44:00") },
            new Lunation() { Index = 1419, NewMoon = DateTime.Parse("09/09/2037 14:25:00"), FirstQuarterMoon = DateTime.Parse("16/09/2037 06:36:00"), FullMoon = DateTime.Parse("24/09/2037 07:31:00"), LastQuarterMoon = DateTime.Parse("02/10/2037 06:29:00"), Duration = TimeSpan.Parse("29.08:09:00") },
            new Lunation() { Index = 1420, NewMoon = DateTime.Parse("08/10/2037 22:34:00"), FirstQuarterMoon = DateTime.Parse("15/10/2037 20:15:00"), FullMoon = DateTime.Parse("24/10/2037 00:36:00"), LastQuarterMoon = DateTime.Parse("31/10/2037 17:06:00"), Duration = TimeSpan.Parse("29.09:29:00") },
            new Lunation() { Index = 1421, NewMoon = DateTime.Parse("07/11/2037 07:03:00"), FirstQuarterMoon = DateTime.Parse("14/11/2037 12:58:00"), FullMoon = DateTime.Parse("22/11/2037 16:35:00"), LastQuarterMoon = DateTime.Parse("30/11/2037 01:06:00"), Duration = TimeSpan.Parse("29.11:35:00") },
            new Lunation() { Index = 1422, NewMoon = DateTime.Parse("06/12/2037 18:38:00"), FirstQuarterMoon = DateTime.Parse("14/12/2037 09:42:00"), FullMoon = DateTime.Parse("22/12/2037 08:38:00"), LastQuarterMoon = DateTime.Parse("29/12/2037 09:04:00"), Duration = TimeSpan.Parse("29.14:03:00") },
            new Lunation() { Index = 1423, NewMoon = DateTime.Parse("05/01/2038 08:41:00"), FirstQuarterMoon = DateTime.Parse("13/01/2038 07:33:00"), FullMoon = DateTime.Parse("20/01/2038 22:59:00"), LastQuarterMoon = DateTime.Parse("27/01/2038 17:00:00"), Duration = TimeSpan.Parse("29.16:11:00") },
            new Lunation() { Index = 1424, NewMoon = DateTime.Parse("04/02/2038 00:52:00"), FirstQuarterMoon = DateTime.Parse("12/02/2038 04:29:00"), FullMoon = DateTime.Parse("19/02/2038 11:09:00"), LastQuarterMoon = DateTime.Parse("26/02/2038 01:55:00"), Duration = TimeSpan.Parse("29.17:23:00") },
            new Lunation() { Index = 1425, NewMoon = DateTime.Parse("05/03/2038 18:14:00"), FirstQuarterMoon = DateTime.Parse("13/03/2038 22:41:00"), FullMoon = DateTime.Parse("20/03/2038 22:09:00"), LastQuarterMoon = DateTime.Parse("27/03/2038 13:36:00"), Duration = TimeSpan.Parse("29.17:28:00") },
            new Lunation() { Index = 1426, NewMoon = DateTime.Parse("04/04/2038 12:42:00"), FirstQuarterMoon = DateTime.Parse("12/04/2038 14:01:00"), FullMoon = DateTime.Parse("19/04/2038 06:35:00"), LastQuarterMoon = DateTime.Parse("26/04/2038 02:15:00"), Duration = TimeSpan.Parse("29.16:37:00") },
            new Lunation() { Index = 1427, NewMoon = DateTime.Parse("04/05/2038 05:19:00"), FirstQuarterMoon = DateTime.Parse("12/05/2038 00:18:00"), FullMoon = DateTime.Parse("18/05/2038 14:23:00"), LastQuarterMoon = DateTime.Parse("25/05/2038 16:43:00"), Duration = TimeSpan.Parse("29.15:05:00") },
            new Lunation() { Index = 1428, NewMoon = DateTime.Parse("02/06/2038 20:24:00"), FirstQuarterMoon = DateTime.Parse("10/06/2038 07:11:00"), FullMoon = DateTime.Parse("16/06/2038 22:30:00"), LastQuarterMoon = DateTime.Parse("24/06/2038 08:39:00"), Duration = TimeSpan.Parse("29.13:08:00") },
            new Lunation() { Index = 1429, NewMoon = DateTime.Parse("02/07/2038 09:32:00"), FirstQuarterMoon = DateTime.Parse("09/07/2038 12:00:00"), FullMoon = DateTime.Parse("16/07/2038 07:48:00"), LastQuarterMoon = DateTime.Parse("24/07/2038 01:39:00"), Duration = TimeSpan.Parse("29.11:08:00") },
            new Lunation() { Index = 1430, NewMoon = DateTime.Parse("31/07/2038 20:40:00"), FirstQuarterMoon = DateTime.Parse("07/08/2038 16:21:00"), FullMoon = DateTime.Parse("14/08/2038 18:56:00"), LastQuarterMoon = DateTime.Parse("22/08/2038 19:12:00"), Duration = TimeSpan.Parse("29.09:32:00") },
            new Lunation() { Index = 1431, NewMoon = DateTime.Parse("30/08/2038 06:12:00"), FirstQuarterMoon = DateTime.Parse("05/09/2038 21:50:00"), FullMoon = DateTime.Parse("13/09/2038 08:24:00"), LastQuarterMoon = DateTime.Parse("21/09/2038 12:27:00"), Duration = TimeSpan.Parse("29.08:45:00") },
            new Lunation() { Index = 1432, NewMoon = DateTime.Parse("28/09/2038 14:57:00"), FirstQuarterMoon = DateTime.Parse("05/10/2038 05:52:00"), FullMoon = DateTime.Parse("13/10/2038 00:21:00"), LastQuarterMoon = DateTime.Parse("21/10/2038 04:23:00"), Duration = TimeSpan.Parse("29.08:55:00") },
            new Lunation() { Index = 1433, NewMoon = DateTime.Parse("27/10/2038 23:52:00"), FirstQuarterMoon = DateTime.Parse("03/11/2038 17:23:00"), FullMoon = DateTime.Parse("11/11/2038 17:27:00"), LastQuarterMoon = DateTime.Parse("19/11/2038 17:10:00"), Duration = TimeSpan.Parse("29.09:54:00") },
            new Lunation() { Index = 1434, NewMoon = DateTime.Parse("26/11/2038 08:46:00"), FirstQuarterMoon = DateTime.Parse("03/12/2038 07:46:00"), FullMoon = DateTime.Parse("11/12/2038 12:30:00"), LastQuarterMoon = DateTime.Parse("19/12/2038 04:28:00"), Duration = TimeSpan.Parse("29.11:15:00") },
            new Lunation() { Index = 1435, NewMoon = DateTime.Parse("25/12/2038 20:01:00"), FirstQuarterMoon = DateTime.Parse("02/01/2039 02:36:00"), FullMoon = DateTime.Parse("10/01/2039 06:45:00"), LastQuarterMoon = DateTime.Parse("17/01/2039 13:41:00"), Duration = TimeSpan.Parse("29.12:34:00") },
            new Lunation() { Index = 1436, NewMoon = DateTime.Parse("24/01/2039 08:36:00"), FirstQuarterMoon = DateTime.Parse("31/01/2039 23:45:00"), FullMoon = DateTime.Parse("08/02/2039 22:39:00"), LastQuarterMoon = DateTime.Parse("15/02/2039 21:35:00"), Duration = TimeSpan.Parse("29.13:41:00") },
            new Lunation() { Index = 1437, NewMoon = DateTime.Parse("22/02/2039 22:17:00"), FirstQuarterMoon = DateTime.Parse("02/03/2039 21:15:00"), FullMoon = DateTime.Parse("10/03/2039 11:35:00"), LastQuarterMoon = DateTime.Parse("17/03/2039 06:07:00"), Duration = TimeSpan.Parse("29.14:42:00") },
            new Lunation() { Index = 1438, NewMoon = DateTime.Parse("24/03/2039 13:59:00"), FirstQuarterMoon = DateTime.Parse("01/04/2039 17:54:00"), FullMoon = DateTime.Parse("08/04/2039 22:52:00"), LastQuarterMoon = DateTime.Parse("15/04/2039 14:07:00"), Duration = TimeSpan.Parse("29.15:35:00") },
            new Lunation() { Index = 1439, NewMoon = DateTime.Parse("23/04/2039 05:34:00"), FirstQuarterMoon = DateTime.Parse("01/05/2039 10:07:00"), FullMoon = DateTime.Parse("08/05/2039 07:19:00"), LastQuarterMoon = DateTime.Parse("14/05/2039 23:16:00"), Duration = TimeSpan.Parse("29.16:03:00") },
            new Lunation() { Index = 1440, NewMoon = DateTime.Parse("22/05/2039 21:38:00"), FirstQuarterMoon = DateTime.Parse("30/05/2039 22:24:00"), FullMoon = DateTime.Parse("06/06/2039 14:47:00"), LastQuarterMoon = DateTime.Parse("13/06/2039 10:16:00"), Duration = TimeSpan.Parse("29.15:43:00") },
            new Lunation() { Index = 1441, NewMoon = DateTime.Parse("21/06/2039 13:21:00"), FirstQuarterMoon = DateTime.Parse("29/06/2039 07:17:00"), FullMoon = DateTime.Parse("05/07/2039 22:03:00"), LastQuarterMoon = DateTime.Parse("12/07/2039 23:38:00"), Duration = TimeSpan.Parse("29.14:33:00") },
            new Lunation() { Index = 1442, NewMoon = DateTime.Parse("21/07/2039 03:54:00"), FirstQuarterMoon = DateTime.Parse("28/07/2039 13:49:00"), FullMoon = DateTime.Parse("04/08/2039 05:56:00"), LastQuarterMoon = DateTime.Parse("11/08/2039 15:35:00"), Duration = TimeSpan.Parse("29.12:56:00") },
            new Lunation() { Index = 1443, NewMoon = DateTime.Parse("19/08/2039 16:50:00"), FirstQuarterMoon = DateTime.Parse("26/08/2039 19:16:00"), FullMoon = DateTime.Parse("02/09/2039 15:23:00"), LastQuarterMoon = DateTime.Parse("10/09/2039 09:45:00"), Duration = TimeSpan.Parse("29.11:32:00") },
            new Lunation() { Index = 1444, NewMoon = DateTime.Parse("18/09/2039 04:22:00"), FirstQuarterMoon = DateTime.Parse("25/09/2039 00:52:00"), FullMoon = DateTime.Parse("02/10/2039 03:23:00"), LastQuarterMoon = DateTime.Parse("10/10/2039 04:59:00"), Duration = TimeSpan.Parse("29.10:46:00") },
            new Lunation() { Index = 1445, NewMoon = DateTime.Parse("17/10/2039 15:08:00"), FirstQuarterMoon = DateTime.Parse("24/10/2039 07:50:00"), FullMoon = DateTime.Parse("31/10/2039 18:36:00"), LastQuarterMoon = DateTime.Parse("08/11/2039 22:46:00"), Duration = TimeSpan.Parse("29.10:37:00") },
            new Lunation() { Index = 1446, NewMoon = DateTime.Parse("16/11/2039 00:46:00"), FirstQuarterMoon = DateTime.Parse("22/11/2039 16:16:00"), FullMoon = DateTime.Parse("30/11/2039 11:49:00"), LastQuarterMoon = DateTime.Parse("08/12/2039 15:44:00"), Duration = TimeSpan.Parse("29.10:46:00") },
            new Lunation() { Index = 1447, NewMoon = DateTime.Parse("15/12/2039 11:32:00"), FirstQuarterMoon = DateTime.Parse("22/12/2039 05:01:00"), FullMoon = DateTime.Parse("30/12/2039 07:37:00"), LastQuarterMoon = DateTime.Parse("07/01/2040 06:05:00"), Duration = TimeSpan.Parse("29.10:53:00") },
            new Lunation() { Index = 1448, NewMoon = DateTime.Parse("13/01/2040 22:25:00"), FirstQuarterMoon = DateTime.Parse("20/01/2040 21:21:00"), FullMoon = DateTime.Parse("29/01/2040 02:54:00"), LastQuarterMoon = DateTime.Parse("05/02/2040 17:32:00"), Duration = TimeSpan.Parse("29.10:59:00") },
            new Lunation() { Index = 1449, NewMoon = DateTime.Parse("12/02/2040 09:24:00"), FirstQuarterMoon = DateTime.Parse("19/02/2040 16:33:00"), FullMoon = DateTime.Parse("27/02/2040 19:59:00"), LastQuarterMoon = DateTime.Parse("06/03/2040 02:18:00"), Duration = TimeSpan.Parse("29.11:22:00") },
            new Lunation() { Index = 1450, NewMoon = DateTime.Parse("12/03/2040 21:46:00"), FirstQuarterMoon = DateTime.Parse("20/03/2040 13:59:00"), FullMoon = DateTime.Parse("28/03/2040 11:11:00"), LastQuarterMoon = DateTime.Parse("04/04/2040 10:06:00"), Duration = TimeSpan.Parse("29.12:14:00") },
            new Lunation() { Index = 1451, NewMoon = DateTime.Parse("11/04/2040 10:00:00"), FirstQuarterMoon = DateTime.Parse("19/04/2040 09:37:00"), FullMoon = DateTime.Parse("26/04/2040 22:37:00"), LastQuarterMoon = DateTime.Parse("03/05/2040 15:59:00"), Duration = TimeSpan.Parse("29.13:28:00") },
            new Lunation() { Index = 1452, NewMoon = DateTime.Parse("10/05/2040 23:27:00"), FirstQuarterMoon = DateTime.Parse("19/05/2040 03:00:00"), FullMoon = DateTime.Parse("26/05/2040 07:47:00"), LastQuarterMoon = DateTime.Parse("01/06/2040 22:17:00"), Duration = TimeSpan.Parse("29.14:35:00") },
            new Lunation() { Index = 1453, NewMoon = DateTime.Parse("09/06/2040 14:03:00"), FirstQuarterMoon = DateTime.Parse("17/06/2040 17:32:00"), FullMoon = DateTime.Parse("24/06/2040 15:19:00"), LastQuarterMoon = DateTime.Parse("01/07/2040 06:17:00"), Duration = TimeSpan.Parse("29.15:12:00") },
            new Lunation() { Index = 1454, NewMoon = DateTime.Parse("09/07/2040 05:14:00"), FirstQuarterMoon = DateTime.Parse("17/07/2040 05:16:00"), FullMoon = DateTime.Parse("23/07/2040 22:05:00"), LastQuarterMoon = DateTime.Parse("30/07/2040 17:05:00"), Duration = TimeSpan.Parse("29.15:12:00") },
            new Lunation() { Index = 1455, NewMoon = DateTime.Parse("07/08/2040 20:26:00"), FirstQuarterMoon = DateTime.Parse("15/08/2040 14:35:00"), FullMoon = DateTime.Parse("22/08/2040 05:09:00"), LastQuarterMoon = DateTime.Parse("29/08/2040 07:16:00"), Duration = TimeSpan.Parse("29.14:47:00") },
            new Lunation() { Index = 1456, NewMoon = DateTime.Parse("06/09/2040 11:13:00"), FirstQuarterMoon = DateTime.Parse("13/09/2040 22:07:00"), FullMoon = DateTime.Parse("20/09/2040 13:42:00"), LastQuarterMoon = DateTime.Parse("28/09/2040 00:41:00"), Duration = TimeSpan.Parse("29.14:12:00") },
            new Lunation() { Index = 1457, NewMoon = DateTime.Parse("06/10/2040 01:25:00"), FirstQuarterMoon = DateTime.Parse("13/10/2040 04:41:00"), FullMoon = DateTime.Parse("20/10/2040 00:49:00"), LastQuarterMoon = DateTime.Parse("27/10/2040 20:26:00"), Duration = TimeSpan.Parse("29.13:30:00") },
            new Lunation() { Index = 1458, NewMoon = DateTime.Parse("04/11/2040 13:56:00"), FirstQuarterMoon = DateTime.Parse("11/11/2040 10:23:00"), FullMoon = DateTime.Parse("18/11/2040 14:06:00"), LastQuarterMoon = DateTime.Parse("26/11/2040 16:07:00"), Duration = TimeSpan.Parse("29.12:37:00") },
            new Lunation() { Index = 1459, NewMoon = DateTime.Parse("04/12/2040 02:33:00"), FirstQuarterMoon = DateTime.Parse("10/12/2040 18:29:00"), FullMoon = DateTime.Parse("18/12/2040 07:15:00"), LastQuarterMoon = DateTime.Parse("26/12/2040 12:02:00"), Duration = TimeSpan.Parse("29.11:35:00") },
            new Lunation() { Index = 1460, NewMoon = DateTime.Parse("02/01/2041 14:07:00"), FirstQuarterMoon = DateTime.Parse("09/01/2041 05:05:00"), FullMoon = DateTime.Parse("17/01/2041 02:11:00"), LastQuarterMoon = DateTime.Parse("25/01/2041 05:33:00"), Duration = TimeSpan.Parse("29.10:35:00") },
            new Lunation() { Index = 1461, NewMoon = DateTime.Parse("01/02/2041 00:42:00"), FirstQuarterMoon = DateTime.Parse("07/02/2041 18:40:00"), FullMoon = DateTime.Parse("15/02/2041 21:21:00"), LastQuarterMoon = DateTime.Parse("23/02/2041 19:28:00"), Duration = TimeSpan.Parse("29.09:56:00") },
            new Lunation() { Index = 1462, NewMoon = DateTime.Parse("02/03/2041 10:39:00"), FirstQuarterMoon = DateTime.Parse("09/03/2041 10:51:00"), FullMoon = DateTime.Parse("17/03/2041 16:19:00"), LastQuarterMoon = DateTime.Parse("25/03/2041 06:31:00"), Duration = TimeSpan.Parse("29.09:50:00") },
            new Lunation() { Index = 1463, NewMoon = DateTime.Parse("31/03/2041 21:29:00"), FirstQuarterMoon = DateTime.Parse("08/04/2041 05:38:00"), FullMoon = DateTime.Parse("16/04/2041 08:00:00"), LastQuarterMoon = DateTime.Parse("23/04/2041 13:24:00"), Duration = TimeSpan.Parse("29.10:17:00") },
            new Lunation() { Index = 1464, NewMoon = DateTime.Parse("30/04/2041 07:46:00"), FirstQuarterMoon = DateTime.Parse("07/05/2041 23:54:00"), FullMoon = DateTime.Parse("15/05/2041 20:52:00"), LastQuarterMoon = DateTime.Parse("22/05/2041 18:25:00"), Duration = TimeSpan.Parse("29.11:10:00") },
            new Lunation() { Index = 1465, NewMoon = DateTime.Parse("29/05/2041 18:55:00"), FirstQuarterMoon = DateTime.Parse("06/06/2041 17:40:00"), FullMoon = DateTime.Parse("14/06/2041 06:58:00"), LastQuarterMoon = DateTime.Parse("20/06/2041 23:12:00"), Duration = TimeSpan.Parse("29.12:21:00") },
            new Lunation() { Index = 1466, NewMoon = DateTime.Parse("28/06/2041 07:16:00"), FirstQuarterMoon = DateTime.Parse("06/07/2041 10:12:00"), FullMoon = DateTime.Parse("13/07/2041 15:00:00"), LastQuarterMoon = DateTime.Parse("20/07/2041 05:13:00"), Duration = TimeSpan.Parse("29.13:45:00") },
            new Lunation() { Index = 1467, NewMoon = DateTime.Parse("27/07/2041 21:02:00"), FirstQuarterMoon = DateTime.Parse("05/08/2041 00:52:00"), FullMoon = DateTime.Parse("11/08/2041 22:04:00"), LastQuarterMoon = DateTime.Parse("18/08/2041 13:43:00"), Duration = TimeSpan.Parse("29.15:14:00") },
            new Lunation() { Index = 1468, NewMoon = DateTime.Parse("26/08/2041 12:16:00"), FirstQuarterMoon = DateTime.Parse("03/09/2041 13:18:00"), FullMoon = DateTime.Parse("10/09/2041 05:23:00"), LastQuarterMoon = DateTime.Parse("17/09/2041 01:32:00"), Duration = TimeSpan.Parse("29.16:25:00") },
            new Lunation() { Index = 1469, NewMoon = DateTime.Parse("25/09/2041 04:41:00"), FirstQuarterMoon = DateTime.Parse("02/10/2041 23:32:00"), FullMoon = DateTime.Parse("09/10/2041 14:02:00"), LastQuarterMoon = DateTime.Parse("16/10/2041 17:05:00"), Duration = TimeSpan.Parse("29.16:49:00") },
            new Lunation() { Index = 1470, NewMoon = DateTime.Parse("24/10/2041 21:30:00"), FirstQuarterMoon = DateTime.Parse("01/11/2041 08:04:00"), FullMoon = DateTime.Parse("07/11/2041 23:43:00"), LastQuarterMoon = DateTime.Parse("15/11/2041 11:06:00"), Duration = TimeSpan.Parse("29.16:06:00") },
            new Lunation() { Index = 1471, NewMoon = DateTime.Parse("23/11/2041 12:36:00"), FirstQuarterMoon = DateTime.Parse("30/11/2041 14:48:00"), FullMoon = DateTime.Parse("07/12/2041 12:42:00"), LastQuarterMoon = DateTime.Parse("15/12/2041 08:32:00"), Duration = TimeSpan.Parse("29.14:30:00") },
            new Lunation() { Index = 1472, NewMoon = DateTime.Parse("23/12/2041 03:06:00"), FirstQuarterMoon = DateTime.Parse("29/12/2041 22:45:00"), FullMoon = DateTime.Parse("06/01/2042 03:53:00"), LastQuarterMoon = DateTime.Parse("14/01/2042 06:24:00"), Duration = TimeSpan.Parse("29.12:36:00") },
            new Lunation() { Index = 1473, NewMoon = DateTime.Parse("21/01/2042 15:42:00"), FirstQuarterMoon = DateTime.Parse("28/01/2042 07:48:00"), FullMoon = DateTime.Parse("04/02/2042 20:57:00"), LastQuarterMoon = DateTime.Parse("13/02/2042 02:16:00"), Duration = TimeSpan.Parse("29.10:57:00") },
            new Lunation() { Index = 1474, NewMoon = DateTime.Parse("20/02/2042 02:38:00"), FirstQuarterMoon = DateTime.Parse("26/02/2042 18:29:00"), FullMoon = DateTime.Parse("06/03/2042 15:09:00"), LastQuarterMoon = DateTime.Parse("14/03/2042 19:21:00"), Duration = TimeSpan.Parse("29.09:44:00") },
            new Lunation() { Index = 1475, NewMoon = DateTime.Parse("21/03/2042 13:22:00"), FirstQuarterMoon = DateTime.Parse("28/03/2042 07:59:00"), FullMoon = DateTime.Parse("05/04/2042 10:15:00"), LastQuarterMoon = DateTime.Parse("13/04/2042 07:09:00"), Duration = TimeSpan.Parse("29.08:56:00") },
            new Lunation() { Index = 1476, NewMoon = DateTime.Parse("19/04/2042 22:19:00"), FirstQuarterMoon = DateTime.Parse("26/04/2042 22:19:00"), FullMoon = DateTime.Parse("05/05/2042 02:48:00"), LastQuarterMoon = DateTime.Parse("12/05/2042 15:18:00"), Duration = TimeSpan.Parse("29.08:36:00") },
            new Lunation() { Index = 1477, NewMoon = DateTime.Parse("19/05/2042 06:54:00"), FirstQuarterMoon = DateTime.Parse("26/05/2042 14:18:00"), FullMoon = DateTime.Parse("03/06/2042 16:48:00"), LastQuarterMoon = DateTime.Parse("10/06/2042 21:00:00"), Duration = TimeSpan.Parse("29.08:53:00") },
            new Lunation() { Index = 1478, NewMoon = DateTime.Parse("17/06/2042 15:48:00"), FirstQuarterMoon = DateTime.Parse("25/06/2042 07:28:00"), FullMoon = DateTime.Parse("03/07/2042 04:09:00"), LastQuarterMoon = DateTime.Parse("10/07/2042 01:38:00"), Duration = TimeSpan.Parse("29.10:04:00") },
            new Lunation() { Index = 1479, NewMoon = DateTime.Parse("17/07/2042 01:51:00"), FirstQuarterMoon = DateTime.Parse("25/07/2042 01:01:00"), FullMoon = DateTime.Parse("01/08/2042 13:33:00"), LastQuarterMoon = DateTime.Parse("08/08/2042 06:34:00"), Duration = TimeSpan.Parse("29.12:09:00") },
            new Lunation() { Index = 1480, NewMoon = DateTime.Parse("15/08/2042 14:01:00"), FirstQuarterMoon = DateTime.Parse("23/08/2042 17:55:00"), FullMoon = DateTime.Parse("30/08/2042 22:02:00"), LastQuarterMoon = DateTime.Parse("06/09/2042 13:08:00"), Duration = TimeSpan.Parse("29.14:49:00") },
            new Lunation() { Index = 1481, NewMoon = DateTime.Parse("14/09/2042 04:50:00"), FirstQuarterMoon = DateTime.Parse("22/09/2042 09:20:00"), FullMoon = DateTime.Parse("29/09/2042 06:34:00"), LastQuarterMoon = DateTime.Parse("05/10/2042 22:34:00"), Duration = TimeSpan.Parse("29.17:13:00") },
            new Lunation() { Index = 1482, NewMoon = DateTime.Parse("13/10/2042 22:03:00"), FirstQuarterMoon = DateTime.Parse("21/10/2042 22:53:00"), FullMoon = DateTime.Parse("28/10/2042 15:48:00"), LastQuarterMoon = DateTime.Parse("04/11/2042 10:51:00"), Duration = TimeSpan.Parse("29.18:25:00") },
            new Lunation() { Index = 1483, NewMoon = DateTime.Parse("12/11/2042 15:28:00"), FirstQuarterMoon = DateTime.Parse("20/11/2042 09:31:00"), FullMoon = DateTime.Parse("27/11/2042 01:06:00"), LastQuarterMoon = DateTime.Parse("04/12/2042 04:18:00"), Duration = TimeSpan.Parse("29.18:01:00") },
            new Lunation() { Index = 1484, NewMoon = DateTime.Parse("12/12/2042 09:29:00"), FirstQuarterMoon = DateTime.Parse("19/12/2042 19:27:00"), FullMoon = DateTime.Parse("26/12/2042 12:42:00"), LastQuarterMoon = DateTime.Parse("03/01/2043 01:08:00"), Duration = TimeSpan.Parse("29.16:24:00") },
            new Lunation() { Index = 1485, NewMoon = DateTime.Parse("11/01/2043 01:53:00"), FirstQuarterMoon = DateTime.Parse("18/01/2043 04:04:00"), FullMoon = DateTime.Parse("25/01/2043 01:56:00"), LastQuarterMoon = DateTime.Parse("01/02/2043 23:14:00"), Duration = TimeSpan.Parse("29.14:14:00") },
            new Lunation() { Index = 1486, NewMoon = DateTime.Parse("09/02/2043 16:07:00"), FirstQuarterMoon = DateTime.Parse("16/02/2043 12:00:00"), FullMoon = DateTime.Parse("23/02/2043 16:57:00"), LastQuarterMoon = DateTime.Parse("03/03/2043 20:07:00"), Duration = TimeSpan.Parse("29.12:02:00") },
            new Lunation() { Index = 1487, NewMoon = DateTime.Parse("11/03/2043 05:09:00"), FirstQuarterMoon = DateTime.Parse("17/03/2043 21:03:00"), FullMoon = DateTime.Parse("25/03/2043 10:26:00"), LastQuarterMoon = DateTime.Parse("02/04/2043 14:56:00"), Duration = TimeSpan.Parse("29.09:57:00") },
            new Lunation() { Index = 1488, NewMoon = DateTime.Parse("09/04/2043 15:06:00"), FirstQuarterMoon = DateTime.Parse("16/04/2043 06:08:00"), FullMoon = DateTime.Parse("24/04/2043 03:23:00"), LastQuarterMoon = DateTime.Parse("02/05/2043 04:58:00"), Duration = TimeSpan.Parse("29.08:15:00") },
            new Lunation() { Index = 1489, NewMoon = DateTime.Parse("08/05/2043 23:21:00"), FirstQuarterMoon = DateTime.Parse("15/05/2043 17:05:00"), FullMoon = DateTime.Parse("23/05/2043 19:36:00"), LastQuarterMoon = DateTime.Parse("31/05/2043 15:24:00"), Duration = TimeSpan.Parse("29.07:14:00") },
            new Lunation() { Index = 1490, NewMoon = DateTime.Parse("07/06/2043 06:35:00"), FirstQuarterMoon = DateTime.Parse("14/06/2043 06:18:00"), FullMoon = DateTime.Parse("22/06/2043 10:20:00"), LastQuarterMoon = DateTime.Parse("29/06/2043 22:53:00"), Duration = TimeSpan.Parse("29.07:16:00") },
            new Lunation() { Index = 1491, NewMoon = DateTime.Parse("06/07/2043 13:50:00"), FirstQuarterMoon = DateTime.Parse("13/07/2043 21:46:00"), FullMoon = DateTime.Parse("21/07/2043 23:24:00"), LastQuarterMoon = DateTime.Parse("29/07/2043 04:22:00"), Duration = TimeSpan.Parse("29.08:32:00") },
            new Lunation() { Index = 1492, NewMoon = DateTime.Parse("04/08/2043 22:22:00"), FirstQuarterMoon = DateTime.Parse("12/08/2043 14:57:00"), FullMoon = DateTime.Parse("20/08/2043 11:04:00"), LastQuarterMoon = DateTime.Parse("27/08/2043 09:09:00"), Duration = TimeSpan.Parse("29.10:55:00") },
            new Lunation() { Index = 1493, NewMoon = DateTime.Parse("03/09/2043 09:17:00"), FirstQuarterMoon = DateTime.Parse("11/09/2043 09:01:00"), FullMoon = DateTime.Parse("18/09/2043 21:47:00"), LastQuarterMoon = DateTime.Parse("25/09/2043 14:40:00"), Duration = TimeSpan.Parse("29.13:55:00") },
            new Lunation() { Index = 1494, NewMoon = DateTime.Parse("02/10/2043 23:12:00"), FirstQuarterMoon = DateTime.Parse("11/10/2043 03:04:00"), FullMoon = DateTime.Parse("18/10/2043 07:55:00"), LastQuarterMoon = DateTime.Parse("24/10/2043 22:27:00"), Duration = TimeSpan.Parse("29.16:45:00") },
            new Lunation() { Index = 1495, NewMoon = DateTime.Parse("01/11/2043 14:57:00"), FirstQuarterMoon = DateTime.Parse("09/11/2043 19:13:00"), FullMoon = DateTime.Parse("16/11/2043 16:52:00"), LastQuarterMoon = DateTime.Parse("23/11/2043 08:45:00"), Duration = TimeSpan.Parse("29.18:40:00") },
            new Lunation() { Index = 1496, NewMoon = DateTime.Parse("01/12/2043 09:36:00"), FirstQuarterMoon = DateTime.Parse("09/12/2043 10:27:00"), FullMoon = DateTime.Parse("16/12/2043 03:01:00"), LastQuarterMoon = DateTime.Parse("23/12/2043 00:04:00"), Duration = TimeSpan.Parse("29.19:11:00") },
            new Lunation() { Index = 1497, NewMoon = DateTime.Parse("31/12/2043 04:48:00"), FirstQuarterMoon = DateTime.Parse("07/01/2044 23:01:00"), FullMoon = DateTime.Parse("14/01/2044 13:51:00"), LastQuarterMoon = DateTime.Parse("21/01/2044 18:47:00"), Duration = TimeSpan.Parse("29.18:16:00") },
            new Lunation() { Index = 1498, NewMoon = DateTime.Parse("29/01/2044 23:04:00"), FirstQuarterMoon = DateTime.Parse("06/02/2044 08:46:00"), FullMoon = DateTime.Parse("13/02/2044 01:41:00"), LastQuarterMoon = DateTime.Parse("20/02/2044 15:20:00"), Duration = TimeSpan.Parse("29.16:08:00") },
            new Lunation() { Index = 1499, NewMoon = DateTime.Parse("28/02/2044 15:12:00"), FirstQuarterMoon = DateTime.Parse("06/03/2044 16:17:00"), FullMoon = DateTime.Parse("13/03/2044 15:41:00"), LastQuarterMoon = DateTime.Parse("21/03/2044 12:52:00"), Duration = TimeSpan.Parse("29.13:14:00") },
            new Lunation() { Index = 1500, NewMoon = DateTime.Parse("29/03/2044 05:25:00"), FirstQuarterMoon = DateTime.Parse("04/04/2044 23:44:00"), FullMoon = DateTime.Parse("12/04/2044 05:39:00"), LastQuarterMoon = DateTime.Parse("20/04/2044 07:48:00"), Duration = TimeSpan.Parse("29.10:16:00") },
            new Lunation() { Index = 1501, NewMoon = DateTime.Parse("27/04/2044 15:42:00"), FirstQuarterMoon = DateTime.Parse("04/05/2044 06:28:00"), FullMoon = DateTime.Parse("11/05/2044 20:16:00"), LastQuarterMoon = DateTime.Parse("20/05/2044 00:01:00"), Duration = TimeSpan.Parse("29.07:58:00") },
            new Lunation() { Index = 1502, NewMoon = DateTime.Parse("26/05/2044 23:39:00"), FirstQuarterMoon = DateTime.Parse("02/06/2044 14:33:00"), FullMoon = DateTime.Parse("10/06/2044 11:16:00"), LastQuarterMoon = DateTime.Parse("18/06/2044 12:59:00"), Duration = TimeSpan.Parse("29.06:45:00") },
            new Lunation() { Index = 1503, NewMoon = DateTime.Parse("25/06/2044 06:24:00"), FirstQuarterMoon = DateTime.Parse("02/07/2044 00:48:00"), FullMoon = DateTime.Parse("10/07/2044 02:21:00"), LastQuarterMoon = DateTime.Parse("17/07/2044 22:46:00"), Duration = TimeSpan.Parse("29.06:46:00") },
            new Lunation() { Index = 1504, NewMoon = DateTime.Parse("24/07/2044 13:10:00"), FirstQuarterMoon = DateTime.Parse("31/07/2044 13:40:00"), FullMoon = DateTime.Parse("08/08/2044 17:13:00"), LastQuarterMoon = DateTime.Parse("16/08/2044 06:03:00"), Duration = TimeSpan.Parse("29.07:55:00") },
            new Lunation() { Index = 1505, NewMoon = DateTime.Parse("22/08/2044 21:05:00"), FirstQuarterMoon = DateTime.Parse("30/08/2044 05:18:00"), FullMoon = DateTime.Parse("07/09/2044 07:24:00"), LastQuarterMoon = DateTime.Parse("14/09/2044 11:57:00"), Duration = TimeSpan.Parse("29.09:58:00") },
            new Lunation() { Index = 1506, NewMoon = DateTime.Parse("21/09/2044 07:03:00"), FirstQuarterMoon = DateTime.Parse("28/09/2044 23:30:00"), FullMoon = DateTime.Parse("06/10/2044 20:30:00"), LastQuarterMoon = DateTime.Parse("13/10/2044 17:52:00"), Duration = TimeSpan.Parse("29.12:33:00") },
            new Lunation() { Index = 1507, NewMoon = DateTime.Parse("20/10/2044 19:36:00"), FirstQuarterMoon = DateTime.Parse("28/10/2044 19:27:00"), FullMoon = DateTime.Parse("05/11/2044 08:26:00"), LastQuarterMoon = DateTime.Parse("12/11/2044 00:09:00"), Duration = TimeSpan.Parse("29.15:21:00") },
            new Lunation() { Index = 1508, NewMoon = DateTime.Parse("19/11/2044 09:57:00"), FirstQuarterMoon = DateTime.Parse("27/11/2044 14:36:00"), FullMoon = DateTime.Parse("04/12/2044 18:33:00"), LastQuarterMoon = DateTime.Parse("11/12/2044 09:52:00"), Duration = TimeSpan.Parse("29.17:55:00") },
            new Lunation() { Index = 1509, NewMoon = DateTime.Parse("19/12/2044 03:53:00"), FirstQuarterMoon = DateTime.Parse("27/12/2044 08:59:00"), FullMoon = DateTime.Parse("03/01/2045 05:20:00"), LastQuarterMoon = DateTime.Parse("09/01/2045 22:32:00"), Duration = TimeSpan.Parse("29.19:32:00") },
            new Lunation() { Index = 1510, NewMoon = DateTime.Parse("17/01/2045 23:25:00"), FirstQuarterMoon = DateTime.Parse("26/01/2045 00:09:00"), FullMoon = DateTime.Parse("01/02/2045 16:05:00"), LastQuarterMoon = DateTime.Parse("08/02/2045 14:03:00"), Duration = TimeSpan.Parse("29.19:26:00") },
            new Lunation() { Index = 1511, NewMoon = DateTime.Parse("16/02/2045 18:51:00"), FirstQuarterMoon = DateTime.Parse("24/02/2045 11:36:00"), FullMoon = DateTime.Parse("03/03/2045 02:52:00"), LastQuarterMoon = DateTime.Parse("10/03/2045 07:49:00"), Duration = TimeSpan.Parse("29.17:24:00") },
            new Lunation() { Index = 1512, NewMoon = DateTime.Parse("18/03/2045 13:14:00"), FirstQuarterMoon = DateTime.Parse("25/03/2045 20:56:00"), FullMoon = DateTime.Parse("01/04/2045 14:42:00"), LastQuarterMoon = DateTime.Parse("09/04/2045 03:52:00"), Duration = TimeSpan.Parse("29.14:12:00") },
            new Lunation() { Index = 1513, NewMoon = DateTime.Parse("17/04/2045 03:26:00"), FirstQuarterMoon = DateTime.Parse("24/04/2045 03:12:00"), FullMoon = DateTime.Parse("01/05/2045 01:52:00"), LastQuarterMoon = DateTime.Parse("08/05/2045 22:50:00"), Duration = TimeSpan.Parse("29.11:00:00") },
            new Lunation() { Index = 1514, NewMoon = DateTime.Parse("16/05/2045 14:26:00"), FirstQuarterMoon = DateTime.Parse("23/05/2045 08:38:00"), FullMoon = DateTime.Parse("30/05/2045 13:52:00"), LastQuarterMoon = DateTime.Parse("07/06/2045 16:23:00"), Duration = TimeSpan.Parse("29.08:38:00") },
            new Lunation() { Index = 1515, NewMoon = DateTime.Parse("14/06/2045 23:04:00"), FirstQuarterMoon = DateTime.Parse("21/06/2045 14:28:00"), FullMoon = DateTime.Parse("29/06/2045 03:15:00"), LastQuarterMoon = DateTime.Parse("07/07/2045 07:30:00"), Duration = TimeSpan.Parse("29.07:24:00") },
            new Lunation() { Index = 1516, NewMoon = DateTime.Parse("14/07/2045 06:28:00"), FirstQuarterMoon = DateTime.Parse("20/07/2045 21:52:00"), FullMoon = DateTime.Parse("28/07/2045 18:10:00"), LastQuarterMoon = DateTime.Parse("05/08/2045 19:57:00"), Duration = TimeSpan.Parse("29.07:11:00") },
            new Lunation() { Index = 1517, NewMoon = DateTime.Parse("12/08/2045 13:39:00"), FirstQuarterMoon = DateTime.Parse("19/08/2045 07:55:00"), FullMoon = DateTime.Parse("27/08/2045 10:07:00"), LastQuarterMoon = DateTime.Parse("04/09/2045 06:03:00"), Duration = TimeSpan.Parse("29.07:48:00") },
            new Lunation() { Index = 1518, NewMoon = DateTime.Parse("10/09/2045 21:27:00"), FirstQuarterMoon = DateTime.Parse("17/09/2045 21:30:00"), FullMoon = DateTime.Parse("26/09/2045 02:11:00"), LastQuarterMoon = DateTime.Parse("03/10/2045 14:31:00"), Duration = TimeSpan.Parse("29.09:09:00") },
            new Lunation() { Index = 1519, NewMoon = DateTime.Parse("10/10/2045 06:36:00"), FirstQuarterMoon = DateTime.Parse("17/10/2045 14:55:00"), FullMoon = DateTime.Parse("25/10/2045 17:31:00"), LastQuarterMoon = DateTime.Parse("01/11/2045 22:09:00"), Duration = TimeSpan.Parse("29.11:12:00") },
            new Lunation() { Index = 1520, NewMoon = DateTime.Parse("08/11/2045 16:48:00"), FirstQuarterMoon = DateTime.Parse("16/11/2045 10:25:00"), FullMoon = DateTime.Parse("24/11/2045 06:43:00"), LastQuarterMoon = DateTime.Parse("01/12/2045 04:46:00"), Duration = TimeSpan.Parse("29.13:52:00") },
            new Lunation() { Index = 1521, NewMoon = DateTime.Parse("08/12/2045 06:41:00"), FirstQuarterMoon = DateTime.Parse("16/12/2045 08:08:00"), FullMoon = DateTime.Parse("23/12/2045 19:49:00"), LastQuarterMoon = DateTime.Parse("30/12/2045 13:11:00"), Duration = TimeSpan.Parse("29.16:43:00") },
            new Lunation() { Index = 1522, NewMoon = DateTime.Parse("06/01/2046 23:24:00"), FirstQuarterMoon = DateTime.Parse("15/01/2046 04:42:00"), FullMoon = DateTime.Parse("22/01/2046 07:51:00"), LastQuarterMoon = DateTime.Parse("28/01/2046 23:11:00"), Duration = TimeSpan.Parse("29.18:46:00") },
            new Lunation() { Index = 1523, NewMoon = DateTime.Parse("05/02/2046 18:09:00"), FirstQuarterMoon = DateTime.Parse("13/02/2046 22:20:00"), FullMoon = DateTime.Parse("20/02/2046 18:44:00"), LastQuarterMoon = DateTime.Parse("27/02/2046 11:22:00"), Duration = TimeSpan.Parse("29.19:06:00") },
            new Lunation() { Index = 1524, NewMoon = DateTime.Parse("07/03/2046 13:15:00"), FirstQuarterMoon = DateTime.Parse("15/03/2046 13:12:00"), FullMoon = DateTime.Parse("22/03/2046 05:26:00"), LastQuarterMoon = DateTime.Parse("29/03/2046 02:57:00"), Duration = TimeSpan.Parse("29.17:36:00") },
            new Lunation() { Index = 1525, NewMoon = DateTime.Parse("06/04/2046 07:51:00"), FirstQuarterMoon = DateTime.Parse("13/04/2046 23:21:00"), FullMoon = DateTime.Parse("20/04/2046 14:21:00"), LastQuarterMoon = DateTime.Parse("27/04/2046 19:30:00"), Duration = TimeSpan.Parse("29.15:04:00") },
            new Lunation() { Index = 1526, NewMoon = DateTime.Parse("05/05/2046 22:56:00"), FirstQuarterMoon = DateTime.Parse("13/05/2046 06:24:00"), FullMoon = DateTime.Parse("19/05/2046 23:15:00"), LastQuarterMoon = DateTime.Parse("27/05/2046 13:06:00"), Duration = TimeSpan.Parse("29.12:26:00") },
            new Lunation() { Index = 1527, NewMoon = DateTime.Parse("04/06/2046 11:22:00"), FirstQuarterMoon = DateTime.Parse("11/06/2046 11:27:00"), FullMoon = DateTime.Parse("18/06/2046 09:09:00"), LastQuarterMoon = DateTime.Parse("26/06/2046 06:39:00"), Duration = TimeSpan.Parse("29.10:16:00") },
            new Lunation() { Index = 1528, NewMoon = DateTime.Parse("03/07/2046 21:38:00"), FirstQuarterMoon = DateTime.Parse("10/07/2046 15:53:00"), FullMoon = DateTime.Parse("17/07/2046 20:55:00"), LastQuarterMoon = DateTime.Parse("25/07/2046 23:19:00"), Duration = TimeSpan.Parse("29.08:47:00") },
            new Lunation() { Index = 1529, NewMoon = DateTime.Parse("02/08/2046 06:25:00"), FirstQuarterMoon = DateTime.Parse("08/08/2046 21:15:00"), FullMoon = DateTime.Parse("16/08/2046 10:50:00"), LastQuarterMoon = DateTime.Parse("24/08/2046 14:36:00"), Duration = TimeSpan.Parse("29.08:00:00") },
            new Lunation() { Index = 1530, NewMoon = DateTime.Parse("31/08/2046 14:25:00"), FirstQuarterMoon = DateTime.Parse("07/09/2046 05:06:00"), FullMoon = DateTime.Parse("15/09/2046 02:39:00"), LastQuarterMoon = DateTime.Parse("23/09/2046 04:15:00"), Duration = TimeSpan.Parse("29.08:00:00") },
            new Lunation() { Index = 1531, NewMoon = DateTime.Parse("29/09/2046 22:25:00"), FirstQuarterMoon = DateTime.Parse("06/10/2046 16:41:00"), FullMoon = DateTime.Parse("14/10/2046 19:41:00"), LastQuarterMoon = DateTime.Parse("22/10/2046 16:07:00"), Duration = TimeSpan.Parse("29.08:52:00") },
            new Lunation() { Index = 1532, NewMoon = DateTime.Parse("29/10/2046 07:17:00"), FirstQuarterMoon = DateTime.Parse("05/11/2046 07:28:00"), FullMoon = DateTime.Parse("13/11/2046 12:04:00"), LastQuarterMoon = DateTime.Parse("21/11/2046 01:10:00"), Duration = TimeSpan.Parse("29.10:33:00") },
            new Lunation() { Index = 1533, NewMoon = DateTime.Parse("27/11/2046 16:50:00"), FirstQuarterMoon = DateTime.Parse("05/12/2046 02:56:00"), FullMoon = DateTime.Parse("13/12/2046 04:55:00"), LastQuarterMoon = DateTime.Parse("20/12/2046 09:43:00"), Duration = TimeSpan.Parse("29.12:49:00") },
            new Lunation() { Index = 1534, NewMoon = DateTime.Parse("27/12/2046 05:38:00"), FirstQuarterMoon = DateTime.Parse("04/01/2047 00:30:00"), FullMoon = DateTime.Parse("11/01/2047 20:21:00"), LastQuarterMoon = DateTime.Parse("18/01/2047 17:32:00"), Duration = TimeSpan.Parse("29.15:05:00") },
            new Lunation() { Index = 1535, NewMoon = DateTime.Parse("25/01/2047 20:43:00"), FirstQuarterMoon = DateTime.Parse("02/02/2047 22:09:00"), FullMoon = DateTime.Parse("10/02/2047 09:39:00"), LastQuarterMoon = DateTime.Parse("17/02/2047 01:42:00"), Duration = TimeSpan.Parse("29.16:42:00") },
            new Lunation() { Index = 1536, NewMoon = DateTime.Parse("24/02/2047 13:25:00"), FirstQuarterMoon = DateTime.Parse("04/03/2047 17:51:00"), FullMoon = DateTime.Parse("11/03/2047 21:36:00"), LastQuarterMoon = DateTime.Parse("18/03/2047 12:10:00"), Duration = TimeSpan.Parse("29.17:18:00") },
            new Lunation() { Index = 1537, NewMoon = DateTime.Parse("26/03/2047 07:44:00"), FirstQuarterMoon = DateTime.Parse("03/04/2047 11:10:00"), FullMoon = DateTime.Parse("10/04/2047 06:35:00"), LastQuarterMoon = DateTime.Parse("16/04/2047 23:30:00"), Duration = TimeSpan.Parse("29.16:56:00") },
            new Lunation() { Index = 1538, NewMoon = DateTime.Parse("25/04/2047 00:39:00"), FirstQuarterMoon = DateTime.Parse("02/05/2047 23:26:00"), FullMoon = DateTime.Parse("09/05/2047 14:24:00"), LastQuarterMoon = DateTime.Parse("16/05/2047 12:45:00"), Duration = TimeSpan.Parse("29.15:48:00") },
            new Lunation() { Index = 1539, NewMoon = DateTime.Parse("24/05/2047 16:27:00"), FirstQuarterMoon = DateTime.Parse("01/06/2047 07:54:00"), FullMoon = DateTime.Parse("07/06/2047 22:04:00"), LastQuarterMoon = DateTime.Parse("15/06/2047 03:45:00"), Duration = TimeSpan.Parse("29.14:08:00") },
            new Lunation() { Index = 1540, NewMoon = DateTime.Parse("23/06/2047 06:35:00"), FirstQuarterMoon = DateTime.Parse("30/06/2047 13:36:00"), FullMoon = DateTime.Parse("07/07/2047 06:33:00"), LastQuarterMoon = DateTime.Parse("14/07/2047 20:09:00"), Duration = TimeSpan.Parse("29.12:14:00") },
            new Lunation() { Index = 1541, NewMoon = DateTime.Parse("22/07/2047 18:49:00"), FirstQuarterMoon = DateTime.Parse("29/07/2047 18:02:00"), FullMoon = DateTime.Parse("05/08/2047 16:38:00"), LastQuarterMoon = DateTime.Parse("13/08/2047 13:34:00"), Duration = TimeSpan.Parse("29.10:27:00") },
            new Lunation() { Index = 1542, NewMoon = DateTime.Parse("21/08/2047 05:16:00"), FirstQuarterMoon = DateTime.Parse("27/08/2047 22:49:00"), FullMoon = DateTime.Parse("04/09/2047 04:54:00"), LastQuarterMoon = DateTime.Parse("12/09/2047 07:18:00"), Duration = TimeSpan.Parse("29.09:15:00") },
            new Lunation() { Index = 1543, NewMoon = DateTime.Parse("19/09/2047 14:31:00"), FirstQuarterMoon = DateTime.Parse("26/09/2047 05:28:00"), FullMoon = DateTime.Parse("03/10/2047 19:42:00"), LastQuarterMoon = DateTime.Parse("12/10/2047 00:21:00"), Duration = TimeSpan.Parse("29.08:57:00") },
            new Lunation() { Index = 1544, NewMoon = DateTime.Parse("18/10/2047 23:27:00"), FirstQuarterMoon = DateTime.Parse("25/10/2047 15:12:00"), FullMoon = DateTime.Parse("02/11/2047 12:58:00"), LastQuarterMoon = DateTime.Parse("10/11/2047 14:39:00"), Duration = TimeSpan.Parse("29.09:31:00") },
            new Lunation() { Index = 1545, NewMoon = DateTime.Parse("17/11/2047 07:58:00"), FirstQuarterMoon = DateTime.Parse("24/11/2047 03:40:00"), FullMoon = DateTime.Parse("02/12/2047 06:55:00"), LastQuarterMoon = DateTime.Parse("10/12/2047 03:28:00"), Duration = TimeSpan.Parse("29.10:39:00") },
            new Lunation() { Index = 1546, NewMoon = DateTime.Parse("16/12/2047 18:38:00"), FirstQuarterMoon = DateTime.Parse("23/12/2047 20:50:00"), FullMoon = DateTime.Parse("01/01/2048 01:57:00"), LastQuarterMoon = DateTime.Parse("08/01/2048 13:49:00"), Duration = TimeSpan.Parse("29.11:54:00") },
            new Lunation() { Index = 1547, NewMoon = DateTime.Parse("15/01/2048 06:32:00"), FirstQuarterMoon = DateTime.Parse("22/01/2048 16:56:00"), FullMoon = DateTime.Parse("30/01/2048 19:14:00"), LastQuarterMoon = DateTime.Parse("06/02/2048 22:16:00"), Duration = TimeSpan.Parse("29.12:59:00") },
            new Lunation() { Index = 1548, NewMoon = DateTime.Parse("13/02/2048 19:31:00"), FirstQuarterMoon = DateTime.Parse("21/02/2048 14:22:00"), FullMoon = DateTime.Parse("29/02/2048 09:38:00"), LastQuarterMoon = DateTime.Parse("07/03/2048 05:45:00"), Duration = TimeSpan.Parse("29.13:56:00") },
            new Lunation() { Index = 1549, NewMoon = DateTime.Parse("14/03/2048 10:27:00"), FirstQuarterMoon = DateTime.Parse("22/03/2048 12:03:00"), FullMoon = DateTime.Parse("29/03/2048 22:04:00"), LastQuarterMoon = DateTime.Parse("05/04/2048 14:10:00"), Duration = TimeSpan.Parse("29.14:52:00") },
            new Lunation() { Index = 1550, NewMoon = DateTime.Parse("13/04/2048 01:19:00"), FirstQuarterMoon = DateTime.Parse("21/04/2048 06:02:00"), FullMoon = DateTime.Parse("28/04/2048 07:12:00"), LastQuarterMoon = DateTime.Parse("04/05/2048 22:22:00"), Duration = TimeSpan.Parse("29.15:38:00") },
            new Lunation() { Index = 1551, NewMoon = DateTime.Parse("12/05/2048 16:58:00"), FirstQuarterMoon = DateTime.Parse("20/05/2048 20:16:00"), FullMoon = DateTime.Parse("27/05/2048 14:57:00"), LastQuarterMoon = DateTime.Parse("03/06/2048 08:04:00"), Duration = TimeSpan.Parse("29.15:52:00") },
            new Lunation() { Index = 1552, NewMoon = DateTime.Parse("11/06/2048 08:49:00"), FirstQuarterMoon = DateTime.Parse("19/06/2048 06:49:00"), FullMoon = DateTime.Parse("25/06/2048 22:08:00"), LastQuarterMoon = DateTime.Parse("02/07/2048 19:57:00"), Duration = TimeSpan.Parse("29.15:14:00") },
            new Lunation() { Index = 1553, NewMoon = DateTime.Parse("11/07/2048 00:04:00"), FirstQuarterMoon = DateTime.Parse("18/07/2048 14:31:00"), FullMoon = DateTime.Parse("25/07/2048 05:33:00"), LastQuarterMoon = DateTime.Parse("01/08/2048 10:30:00"), Duration = TimeSpan.Parse("29.13:55:00") },
            new Lunation() { Index = 1554, NewMoon = DateTime.Parse("09/08/2048 13:58:00"), FirstQuarterMoon = DateTime.Parse("16/08/2048 20:31:00"), FullMoon = DateTime.Parse("23/08/2048 14:07:00"), LastQuarterMoon = DateTime.Parse("31/08/2048 03:41:00"), Duration = TimeSpan.Parse("29.12:26:00") },
            new Lunation() { Index = 1555, NewMoon = DateTime.Parse("08/09/2048 02:24:00"), FirstQuarterMoon = DateTime.Parse("15/09/2048 02:03:00"), FullMoon = DateTime.Parse("22/09/2048 00:46:00"), LastQuarterMoon = DateTime.Parse("29/09/2048 22:45:00"), Duration = TimeSpan.Parse("29.11:21:00") },
            new Lunation() { Index = 1556, NewMoon = DateTime.Parse("07/10/2048 13:45:00"), FirstQuarterMoon = DateTime.Parse("14/10/2048 08:20:00"), FullMoon = DateTime.Parse("21/10/2048 14:24:00"), LastQuarterMoon = DateTime.Parse("29/10/2048 18:14:00"), Duration = TimeSpan.Parse("29.10:53:00") },
            new Lunation() { Index = 1557, NewMoon = DateTime.Parse("05/11/2048 23:38:00"), FirstQuarterMoon = DateTime.Parse("12/11/2048 15:28:00"), FullMoon = DateTime.Parse("20/11/2048 06:19:00"), LastQuarterMoon = DateTime.Parse("28/11/2048 11:33:00"), Duration = TimeSpan.Parse("29.10:52:00") },
            new Lunation() { Index = 1558, NewMoon = DateTime.Parse("05/12/2048 10:30:00"), FirstQuarterMoon = DateTime.Parse("12/12/2048 02:29:00"), FullMoon = DateTime.Parse("20/12/2048 01:39:00"), LastQuarterMoon = DateTime.Parse("28/12/2048 03:31:00"), Duration = TimeSpan.Parse("29.10:54:00") },
        };
        #endregion

        #region Implementation of STATIC INotifyPropertyChanged
        /// <summary>
        ///     Implements the PropertyChanged event.
        /// </summary>
        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

        internal static void NotifyPropertyChanged(string propertyName)
        {
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Default parameter is mandatory with [CallerMemberName]")]
        internal static void NotifyThisPropertyChanged([CallerMemberName] string propertyName = "")
        {
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
