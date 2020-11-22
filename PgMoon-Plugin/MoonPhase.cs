namespace PgMoon
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents a phase of the Moon.
    /// </summary>
    public class MoonPhase : INotifyPropertyChanged
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable SA1600 // Elements should be documented
        #region Constants
        public static string ParasolMushroomLongName { get { return "Parasol Mushroom"; } }
        public static string ParasolMushroomShortName { get { return "Parasol"; } }
        public static double ParasolMushroomRefresh { get { return double.NaN; } }
        public static string ParasolMushroomName(bool isLong) { return isLong ? ParasolMushroomLongName : ParasolMushroomShortName; }

        public static string MycenaMushroomLongName { get { return "Mycena Mushroom"; } }
        public static string MycenaMushroomShortName { get { return "Mycena"; } }
        public static double MycenaMushroomRefresh { get { return 24.0; } }
        public static string MycenaMushroomName(bool isLong) { return isLong ? MycenaMushroomLongName : MycenaMushroomShortName; }

        public static string BoletusMushroomLongName { get { return "Boletus Mushroom"; } }
        public static string BoletusMushroomShortName { get { return "Boletus"; } }
        public static double BoletusMushroomRefresh { get { return 20.0; } }
        public static string BoletusMushroomName(bool isLong) { return isLong ? BoletusMushroomLongName : BoletusMushroomShortName; }

        public static string FieldMushroomLongName { get { return "Field Mushroom"; } }
        public static string FieldMushroomShortName { get { return "Field"; } }
        public static double FieldMushroomRefresh { get { return 16.0; } }
        public static string FieldMushroomName(bool isLong) { return isLong ? FieldMushroomLongName : FieldMushroomShortName; }

        public static string BlusherMushroomLongName { get { return "Blusher Mushroom"; } }
        public static string BlusherMushroomShortName { get { return "Blusher"; } }
        public static double BlusherMushroomRefresh { get { return 12.0; } }
        public static string BlusherMushroomName(bool isLong) { return isLong ? BlusherMushroomLongName : BlusherMushroomShortName; }

        public static string GoblinPuffballLongName { get { return "Goblin Puffball"; } }
        public static string GoblinPuffballShortName { get { return "Goblin Puffball"; } }
        public static double GoblinPuffballRefresh { get { return 10.0; } }
        public static string GoblinPuffballName(bool isLong) { return isLong ? GoblinPuffballLongName : GoblinPuffballShortName; }

        public static string MilkCapMushroomLongName { get { return "Milk Cap Mushroom"; } }
        public static string MilkCapMushroomShortName { get { return "Milk Cap"; } }
        public static double MilkCapMushroomRefresh { get { return 9.0; } }
        public static string MilkCapMushroomName(bool isLong) { return isLong ? MilkCapMushroomLongName : MilkCapMushroomShortName; }

        public static string BloodMushroomLongName { get { return "Blood Mushroom"; } }
        public static string BloodMushroomShortName { get { return "Blood"; } }
        public static double BloodMushroomRefresh { get { return 8.0; } }
        public static string BloodMushroomName(bool isLong) { return isLong ? BloodMushroomLongName : BloodMushroomShortName; }

        public static string CoralMushroomLongName { get { return "Coral Mushroom"; } }
        public static string CoralMushroomShortName { get { return "Coral"; } }
        public static double CoralMushroomRefresh { get { return 7.0; } }
        public static string CoralMushroomName(bool isLong) { return isLong ? CoralMushroomLongName : CoralMushroomShortName; }

        public static string IocaineMushroomLongName { get { return "Iocaine Mushroom"; } }
        public static string IocaineMushroomShortName { get { return "Iocaine"; } }
        public static double IocaineMushroomRefresh { get { return 6.5; } }
        public static string IocaineMushroomName(bool isLong) { return isLong ? IocaineMushroomLongName : IocaineMushroomShortName; }

        public static string GroxmakMushroomLongName { get { return "Groxmak Mushroom"; } }
        public static string GroxmakMushroomShortName { get { return "Groxmak"; } }
        public static double GroxmakMushroomRefresh { get { return 6.0; } }
        public static string GroxmakMushroomName(bool isLong) { return isLong ? GroxmakMushroomLongName : GroxmakMushroomShortName; }

        public static string PorciniMushroomLongName { get { return "Porcini Mushroom"; } }
        public static string PorciniMushroomShortName { get { return "Porcini"; } }
        public static double PorciniMushroomRefresh { get { return 5.5; } }
        public static string PorciniMushroomName(bool isLong) { return isLong ? PorciniMushroomLongName : PorciniMushroomShortName; }

        public static string BlackFootMorelLongName { get { return "Black Foot Morel"; } }
        public static string BlackFootMorelShortName { get { return "Black Foot Morel"; } }
        public static double BlackFootMorelRefresh { get { return 5.0; } }
        public static string BlackFootMorelName(bool isLong) { return isLong ? BlackFootMorelLongName : BlackFootMorelShortName; }

        public static string PixiesParasolLongName { get { return "Pixie's Parasol"; } }
        public static string PixiesParasolShortName { get { return "Pixie's Parasol"; } }
        public static double PixiesParasolRefresh { get { return 4.5; } }
        public static string PixiesParasolName(bool isLong) { return isLong ? PixiesParasolLongName : PixiesParasolShortName; }

        public static string FlyAmanitaLongName { get { return "Fly Amanita"; } }
        public static string FlyAmanitaShortName { get { return "Fly Amanita"; } }
        public static double FlyAmanitaRefresh { get { return 4.0; } }
        public static string FlyAmanitaName(bool isLong) { return isLong ? FlyAmanitaLongName : FlyAmanitaShortName; }

        public static string BlastcapMushroomLongName { get { return "Blastcap Mushroom"; } }
        public static string BlastcapMushroomShortName { get { return "Blastcap"; } }
        public static double BlastcapMushroomRefresh { get { return 3.5; } }
        public static string BlastcapMushroomName(bool isLong) { return isLong ? BlastcapMushroomLongName : BlastcapMushroomShortName; }

        public static string ChargedMyceliumLongName { get { return "Charged Mycelium"; } }
        public static string ChargedMyceliumShortName { get { return "Charged Mycelium"; } }
        public static double ChargedMyceliumRefresh { get { return 3.5; } }
        public static string ChargedMyceliumName(bool isLong) { return isLong ? ChargedMyceliumLongName : ChargedMyceliumShortName; }

        public static string FalseAgaricLongName { get { return "False Agaric"; } }
        public static string FalseAgaricShortName { get { return "False Agaric"; } }
        public static double FalseAgaricRefresh { get { return 2.5; } }
        public static string FalseAgaricName(bool isLong) { return isLong ? FalseAgaricLongName : FalseAgaricShortName; }

        public static string WizardsMushroomLongName { get { return "Wizard's Mushroom"; } }
        public static string WizardsMushroomShortName { get { return "Wizard's"; } }
        public static double WizardsMushroomRefresh { get { return 1.5; } }
        public static string WizardsMushroomName(bool isLong) { return isLong ? WizardsMushroomLongName : WizardsMushroomShortName; }
        #endregion

        #region Init
        public static MoonPhase NewMoon { get; } = new MoonPhase("New Moon", "South of the westernmost lake");
        public static MoonPhase WaxingCrescentMoon { get; } = new MoonPhase("Waxing Crescent Moon", "North and east of the portal to Serbule");
        public static MoonPhase FirstQuarterMoon { get; } = new MoonPhase("First-Quarter Moon", "At spiders");
        public static MoonPhase WaxingGibbousMoon { get; } = new MoonPhase("Waxing Gibbous Moon", "By the moutain, north of the lake's edge");
        public static MoonPhase FullMoon { get; } = new MoonPhase("Full Moon", "By Percy's House");
        public static MoonPhase WaningGibbousMoon { get; } = new MoonPhase("Waning Gibbous Moon", "East of Hogan's Keep");
        public static MoonPhase LastQuarterMoon { get; } = new MoonPhase("Last-Quarter Moon", "At gnashers");
        public static MoonPhase WaningCrescentMoon { get; } = new MoonPhase("Waning Crescent Moon", "South of the waterfall");
        public static MoonPhase NullMoonPhase { get; } = new MoonPhase("(Unselect)", string.Empty);

        public static List<MoonPhase> MoonPhaseList { get; } = new List<MoonPhase>()
        {
            NewMoon,
            WaxingCrescentMoon,
            FirstQuarterMoon,
            WaxingGibbousMoon,
            FullMoon,
            WaningGibbousMoon,
            LastQuarterMoon,
            WaningCrescentMoon,
            NullMoonPhase,
        };

        public MoonPhase(string name, string darkChapelTip)
        {
            Name = name;
            DarkChapelTip = darkChapelTip;
        }
        #endregion

        #region Properties
        public string Name { get; private set; }

        public bool IsCurrent { get { return PhaseCalculator.MoonPhase == this; } }
        public string FastPortMushroomLongText { get { return FastPortMushroomText(true); } }
        public string FastPortMushroomShortText { get { return FastPortMushroomText(false); } }
        public string DarkChapelTip { get; private set; }

        public string FastPortMushroomText(bool isLong)
        {
            string FastPortMushroomText;

            switch (MoonPhase.MoonPhaseList.IndexOf(this))
            {
                default:
                case 0:
                    FastPortMushroomText = GoblinPuffballName(isLong) + " (" + NeutralCultureDouble(GoblinPuffballRefresh) + "h), " + CoralMushroomName(isLong) + " (" + NeutralCultureDouble(CoralMushroomRefresh) + "h)";
                    break;
                case 1:
                    FastPortMushroomText = IocaineMushroomName(isLong) + " (" + NeutralCultureDouble(IocaineMushroomRefresh) + "h)";
                    break;
                case 2:
                    FastPortMushroomText = MycenaMushroomName(isLong) + " (" + NeutralCultureDouble(MycenaMushroomRefresh) + "h), " + GroxmakMushroomName(isLong) + " (" + NeutralCultureDouble(GroxmakMushroomRefresh) + "h), " + BlastcapMushroomName(isLong) + " (" + NeutralCultureDouble(BlastcapMushroomRefresh) + "h)";
                    break;
                case 3:
                    FastPortMushroomText = BoletusMushroomName(isLong) + " (" + NeutralCultureDouble(BoletusMushroomRefresh) + "h), " + PorciniMushroomName(isLong) + " (" + NeutralCultureDouble(PorciniMushroomRefresh) + "h)";
                    break;
                case 4:
                    FastPortMushroomText = FieldMushroomName(isLong) + " (" + NeutralCultureDouble(FieldMushroomRefresh) + "h), " + BlackFootMorelName(isLong) + " (" + NeutralCultureDouble(BlackFootMorelRefresh) + "h), " + FalseAgaricName(isLong) + " (" + NeutralCultureDouble(FalseAgaricRefresh) + "h)";
                    break;
                case 5:
                    FastPortMushroomText = BlusherMushroomName(isLong) + " (" + NeutralCultureDouble(BlusherMushroomRefresh) + "h), " + PixiesParasolName(isLong) + " (" + NeutralCultureDouble(PixiesParasolRefresh) + "h)";
                    break;
                case 6:
                    FastPortMushroomText = MilkCapMushroomName(isLong) + " (" + NeutralCultureDouble(MilkCapMushroomRefresh) + "h), " + FlyAmanitaName(isLong) + " (" + NeutralCultureDouble(FlyAmanitaRefresh) + "h), " + WizardsMushroomName(isLong) + " (" + NeutralCultureDouble(WizardsMushroomRefresh) + "h)";
                    break;
                case 7:
                    FastPortMushroomText = BloodMushroomName(isLong) + " (" + NeutralCultureDouble(BloodMushroomRefresh) + "h), " + ChargedMyceliumName(isLong) + " (" + NeutralCultureDouble(ChargedMyceliumRefresh) + "h)";
                    break;
            }

            return FastPortMushroomText;
        }

        private static string NeutralCultureDouble(double d)
        {
            return d.ToString(CultureInfo.InvariantCulture);
        }

        public string RahuBoatDestination
        {
            get
            {
                string RahuBoatDestination;

                switch (MoonPhaseList.IndexOf(this))
                {
                    default:
                    case 0:
                    case 1:
                    case 2:
                        RahuBoatDestination = "Serbule";
                        break;
                    case 3:
                    case 4:
                    case 5:
                        RahuBoatDestination = "Kur Moutains";
                        break;
                    case 6:
                    case 7:
                        RahuBoatDestination = "Sun Vale";
                        break;
                }

                return RahuBoatDestination;
            }
        }
        #endregion

        #region Client Interface
        public static void UpdateCurrent()
        {
            foreach (MoonPhase Item in MoonPhaseList)
                Item.Update();
        }

        private void Update()
        {
            NotifyPropertyChanged(nameof(IsCurrent));
        }
        #endregion

        #region Implementation of INotifyPropertyChanged
        /// <summary>
        /// Implements the PropertyChanged event.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Invoke handlers of the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">Name of the property that changed.</param>
        protected void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Invoke handlers of the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">Name of the property that changed.</param>
        protected void NotifyThisPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning restore SA1600 // Elements should be documented
    }
}
