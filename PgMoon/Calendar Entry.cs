using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PgMoon
{
    public class CalendarEntry : INotifyPropertyChanged
    {
        #region Constants
        public static string RahuBoatDestinationLongText { get { return "Rahu Boat Destination"; } }
        public static string RahuBoatDestinationShortText { get { return "Rahu Boat"; } }
        public static string PortToCircleLongText { get { return "Mushroom Circle Recall"; } }
        public static string PortToCircleShortText { get { return "Circle"; } }
        #endregion

        #region Init
        public CalendarEntry(int MoonMonth, MoonPhase MoonPhase, DateTime StartTime, DateTime EndTime, ICollection<MushroomInfo> MushroomInfoList)
        {
            this.MoonMonth = MoonMonth;
            this.MoonPhase = MoonPhase;
            this.StartTime = StartTime;
            this.EndTime = EndTime;
            this.MushroomInfoList = MushroomInfoList;
        }
        #endregion

        #region Properties
        public int MoonMonth { get; private set; }
        public MoonPhase MoonPhase { get; private set; }
        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }
        public ICollection<MushroomInfo> MushroomInfoList { get; private set; }

        public bool IsCurrent { get { return PhaseCalculator.IsCurrent(MoonMonth, MoonPhase); } }

        public string Summary
        {
            get
            {
                string Result = "";

                string GrowingRobustly = "";
                foreach (MushroomInfo Item in MushroomInfoList)
                {
                    if (Item.RobustGrowthPhase1 == MoonPhase || Item.RobustGrowthPhase2 == MoonPhase)
                    {
                        if (GrowingRobustly.Length > 0)
                            GrowingRobustly += ", ";

                        GrowingRobustly += Item.Name;
                    }
                }

                if (GrowingRobustly.Length > 0)
                    Result += "Growing Robustly: " + GrowingRobustly + "\r\n";

                Result += RahuBoatDestinationLongText + ": " + MoonPhase.RahuBoatDestination + "\r\n";
                Result += PortToCircleLongText + ": " + MoonPhase.FastPortMushroomLongText;

                return Result;
            }
        }
        #endregion

        #region Client Interface
        public void Update()
        {
            NotifyPropertyChanged(nameof(IsCurrent));
        }
        #endregion

        #region Implementation
        public override string ToString()
        {
            return MoonPhase.ToString() + " - " + EndTime.ToLocalTime().ToString();
        }
        #endregion

        #region Implementation of INotifyPropertyChanged
        /// <summary>
        ///     Implements the PropertyChanged event.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        internal void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Default parameter is mandatory with [CallerMemberName]")]
        internal void NotifyThisPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
