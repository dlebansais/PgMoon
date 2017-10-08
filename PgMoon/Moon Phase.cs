using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace PgMoon
{
    public class MoonPhase : INotifyPropertyChanged
    {
        #region Constants
        public static string ParasolMushroomLongName { get { return "Parasol Mushroom"; } }
        public static string ParasolMushroomShortName { get { return "Parasol"; } }
        public static double ParasolMushroomRefresh { get { return double.NaN; } }
        public static string ParasolMushroomName(bool IsLong) { return IsLong ? ParasolMushroomLongName : ParasolMushroomShortName; }

        public static string MycenaMushroomLongName { get { return "Mycena Mushroom"; } }
        public static string MycenaMushroomShortName { get { return "Mycena"; } }
        public static double MycenaMushroomRefresh { get { return 24.0; } }
        public static string MycenaMushroomName(bool IsLong) { return IsLong ? MycenaMushroomLongName : MycenaMushroomShortName; }

        public static string BoletusMushroomLongName { get { return "Boletus Mushroom"; } }
        public static string BoletusMushroomShortName { get { return "Boletus"; } }
        public static double BoletusMushroomRefresh { get { return 20.0; } }
        public static string BoletusMushroomName(bool IsLong) { return IsLong ? BoletusMushroomLongName : BoletusMushroomShortName; }

        public static string FieldMushroomLongName { get { return "Field Mushroom"; } }
        public static string FieldMushroomShortName { get { return "Field"; } }
        public static double FieldMushroomRefresh { get { return 16.0; } }
        public static string FieldMushroomName(bool IsLong) { return IsLong ? FieldMushroomLongName : FieldMushroomShortName; }

        public static string BlusherMushroomLongName { get { return "Blusher Mushroom"; } }
        public static string BlusherMushroomShortName { get { return "Blusher"; } }
        public static double BlusherMushroomRefresh { get { return 12.0; } }
        public static string BlusherMushroomName(bool IsLong) { return IsLong ? BlusherMushroomLongName : BlusherMushroomShortName; }

        public static string GoblinPuffballLongName { get { return "Goblin Puffball"; } }
        public static string GoblinPuffballShortName { get { return "Goblin Puffball"; } }
        public static double GoblinPuffballRefresh { get { return 10.0; } }
        public static string GoblinPuffballName(bool IsLong) { return IsLong ? GoblinPuffballLongName : GoblinPuffballShortName; }

        public static string MilkCapMushroomLongName { get { return "Milk Cap Mushroom"; } }
        public static string MilkCapMushroomShortName { get { return "Milk Cap"; } }
        public static double MilkCapMushroomRefresh { get { return 9.0; } }
        public static string MilkCapMushroomName(bool IsLong) { return IsLong ? MilkCapMushroomLongName : MilkCapMushroomShortName; }

        public static string BloodMushroomLongName { get { return "Blood Mushroom"; } }
        public static string BloodMushroomShortName { get { return "Blood"; } }
        public static double BloodMushroomRefresh { get { return 8.0; } }
        public static string BloodMushroomName(bool IsLong) { return IsLong ? BloodMushroomLongName : BloodMushroomShortName; }

        public static string CoralMushroomLongName { get { return "Coral Mushroom"; } }
        public static string CoralMushroomShortName { get { return "Coral"; } }
        public static double CoralMushroomRefresh { get { return 7.0; } }
        public static string CoralMushroomName(bool IsLong) { return IsLong ? CoralMushroomLongName : CoralMushroomShortName; }

        public static string IocaineMushroomLongName { get { return "Iocaine Mushroom"; } }
        public static string IocaineMushroomShortName { get { return "Iocaine"; } }
        public static double IocaineMushroomRefresh { get { return 6.5; } }
        public static string IocaineMushroomName(bool IsLong) { return IsLong ? IocaineMushroomLongName : IocaineMushroomShortName; }

        public static string GroxmakMushroomLongName { get { return "Groxmak Mushroom"; } }
        public static string GroxmakMushroomShortName { get { return "Groxmak"; } }
        public static double GroxmakMushroomRefresh { get { return 6.0; } }
        public static string GroxmakMushroomName(bool IsLong) { return IsLong ? GroxmakMushroomLongName : GroxmakMushroomShortName; }

        public static string PorciniMushroomLongName { get { return "Porcini Mushroom"; } }
        public static string PorciniMushroomShortName { get { return "Porcini"; } }
        public static double PorciniMushroomRefresh { get { return 5.5; } }
        public static string PorciniMushroomName(bool IsLong) { return IsLong ? PorciniMushroomLongName : PorciniMushroomShortName; }

        public static string BlackFootMorelLongName { get { return "Black Foot Morel"; } }
        public static string BlackFootMorelShortName { get { return "Black Foot Morel"; } }
        public static double BlackFootMorelRefresh { get { return 5.0; } }
        public static string BlackFootMorelName(bool IsLong) { return IsLong ? BlackFootMorelLongName : BlackFootMorelShortName; }

        public static string PixiesParasolLongName { get { return "Pixie's Parasol"; } }
        public static string PixiesParasolShortName { get { return "Pixie's Parasol"; } }
        public static double PixiesParasolRefresh { get { return 4.5; } }
        public static string PixiesParasolName(bool IsLong) { return IsLong ? PixiesParasolLongName : PixiesParasolShortName; }

        public static string FlyAmanitaLongName { get { return "Fly Amanita"; } }
        public static string FlyAmanitaShortName { get { return "Fly Amanita"; } }
        public static double FlyAmanitaRefresh { get { return 4.0; } }
        public static string FlyAmanitaName(bool IsLong) { return IsLong ? FlyAmanitaLongName : FlyAmanitaShortName; }

        public static string BlastcapMushroomLongName { get { return "Blastcap Mushroom"; } }
        public static string BlastcapMushroomShortName { get { return "Blastcap"; } }
        public static double BlastcapMushroomRefresh { get { return 3.5; } }
        public static string BlastcapMushroomName(bool IsLong) { return IsLong ? BlastcapMushroomLongName : BlastcapMushroomShortName; }

        public static string ChargedMyceliumLongName { get { return "Charged Mycelium"; } }
        public static string ChargedMyceliumShortName { get { return "Charged Mycelium"; } }
        public static double ChargedMyceliumRefresh { get { return 3.5; } }
        public static string ChargedMyceliumName(bool IsLong) { return IsLong ? ChargedMyceliumLongName : ChargedMyceliumShortName; }

        public static string FalseAgaricLongName { get { return "False Agaric"; } }
        public static string FalseAgaricShortName { get { return "False Agaric"; } }
        public static double FalseAgaricRefresh { get { return 2.5; } }
        public static string FalseAgaricName(bool IsLong) { return IsLong ? FalseAgaricLongName : FalseAgaricShortName; }

        public static string WizardsMushroomLongName { get { return "Wizard's Mushroom"; } }
        public static string WizardsMushroomShortName { get { return "Wizard's"; } }
        public static double WizardsMushroomRefresh { get { return 1.5; } }
        public static string WizardsMushroomName(bool IsLong) { return IsLong ? WizardsMushroomLongName : WizardsMushroomShortName; }
        #endregion

        #region Init
        public static MoonPhase NewMoon = new MoonPhase("New Moon", "North of the easternmost lake");
        public static MoonPhase WaxingCrescentMoon = new MoonPhase("Waxing Crescent Moon", "By the meditation pillar");
        public static MoonPhase FirstQuarterMoon = new MoonPhase("First-Quarter Moon", "At spiders");
        public static MoonPhase WaxingGibbousMoon = new MoonPhase("Waxing Gibbous Moon", "North of the portal to Serbule");
        public static MoonPhase FullMoon = new MoonPhase("Full Moon", "By Percy's House");
        public static MoonPhase WaningGibbousMoon = new MoonPhase("Waning Gibbous Moon", "West of Hogan's Keep");
        public static MoonPhase LastQuarterMoon = new MoonPhase("Last-Quarter Moon", "At gnashers");
        public static MoonPhase WaningCrescentMoon = new MoonPhase("Waning Crescent Moon", "North of the waterfall");
        private static MoonPhase NullMoonPhase = new MoonPhase("(Unselect)", null);

        public static List<MoonPhase> MoonPhaseList = new List<MoonPhase>()
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

        public MoonPhase(string Name, string DarkChapelTip)
        {
            this.Name = Name;
            this.DarkChapelTip = DarkChapelTip;
        }
        #endregion

        #region Properties
        public string Name { get; private set; }

        public bool IsCurrent { get { return PhaseCalculator.MoonPhase == this; } }
        public string FastPortMushroomLongText { get { return FastPortMushroomText(true); } }
        public string FastPortMushroomShortText { get { return FastPortMushroomText(false); } }
        public string DarkChapelTip { get; private set; }

        public string FastPortMushroomText(bool IsLong)
        {
            string FastPortMushroomText;

            switch (MoonPhase.MoonPhaseList.IndexOf(this))
            {
                default:
                case 0:
                    FastPortMushroomText = GoblinPuffballName(IsLong) + " (" + NeutralCultureDouble(GoblinPuffballRefresh) + "h), " + CoralMushroomName(IsLong) + " (" + NeutralCultureDouble(CoralMushroomRefresh) + "h)";
                    break;
                case 1:
                    FastPortMushroomText = IocaineMushroomName(IsLong) + " (" + NeutralCultureDouble(IocaineMushroomRefresh) + "h)";
                    break;
                case 2:
                    FastPortMushroomText = MycenaMushroomName(IsLong) + " (" + NeutralCultureDouble(MycenaMushroomRefresh) + "h), " + GroxmakMushroomName(IsLong) + " (" + NeutralCultureDouble(GroxmakMushroomRefresh) + "h), " + BlastcapMushroomName(IsLong) + " (" + NeutralCultureDouble(BlastcapMushroomRefresh) + "h)";
                    break;
                case 3:
                    FastPortMushroomText = BoletusMushroomName(IsLong) + " (" + NeutralCultureDouble(BoletusMushroomRefresh) + "h), " + PorciniMushroomName(IsLong) + " (" + NeutralCultureDouble(PorciniMushroomRefresh) + "h)";
                    break;
                case 4:
                    FastPortMushroomText = FieldMushroomName(IsLong) + " (" + NeutralCultureDouble(FieldMushroomRefresh) + "h), " + BlackFootMorelName(IsLong) + " (" + NeutralCultureDouble(BlackFootMorelRefresh) + "h), " + FalseAgaricName(IsLong) + " (" + NeutralCultureDouble(FalseAgaricRefresh) + "h)";
                    break;
                case 5:
                    FastPortMushroomText = BlusherMushroomName(IsLong) + " (" + NeutralCultureDouble(BlusherMushroomRefresh) + "h), " + PixiesParasolName(IsLong) + " (" + NeutralCultureDouble(PixiesParasolRefresh) + "h)";
                    break;
                case 6:
                    FastPortMushroomText = MilkCapMushroomName(IsLong) + " (" + NeutralCultureDouble(MilkCapMushroomRefresh) + "h), " + FlyAmanitaName(IsLong) + " (" + NeutralCultureDouble(FlyAmanitaRefresh) + "h), " + WizardsMushroomName(IsLong) + " (" + NeutralCultureDouble(WizardsMushroomRefresh) + "h)";
                    break;
                case 7:
                    FastPortMushroomText = BloodMushroomName(IsLong) + " (" + NeutralCultureDouble(BloodMushroomRefresh) + "h), " + ChargedMyceliumName(IsLong) + " (" + NeutralCultureDouble(ChargedMyceliumRefresh) + "h)";
                    break;
            }

            return FastPortMushroomText;
        }

        private string NeutralCultureDouble(double d)
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
