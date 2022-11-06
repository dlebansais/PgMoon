namespace PgMoon;

using System.Collections.Generic;
using System.ComponentModel;
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
    public static string ParasolMushroomLongName => "Parasol Mushroom";
    public static string ParasolMushroomShortName => "Parasol";
    public static double ParasolMushroomRefresh => double.NaN;
    public static string ParasolMushroomName(bool isLong) { return isLong ? ParasolMushroomLongName : ParasolMushroomShortName; }

    public static string MycenaMushroomLongName => "Mycena Mushroom";
    public static string MycenaMushroomShortName => "Mycena";
    public static double MycenaMushroomRefresh => 24.0;
    public static string MycenaMushroomName(bool isLong) { return isLong ? MycenaMushroomLongName : MycenaMushroomShortName; }

    public static string BoletusMushroomLongName => "Boletus Mushroom";
    public static string BoletusMushroomShortName => "Boletus";
    public static double BoletusMushroomRefresh => 20.0;
    public static string BoletusMushroomName(bool isLong) { return isLong ? BoletusMushroomLongName : BoletusMushroomShortName; }

    public static string FieldMushroomLongName => "Field Mushroom";
    public static string FieldMushroomShortName => "Field";
    public static double FieldMushroomRefresh => 16.0;
    public static string FieldMushroomName(bool isLong) { return isLong ? FieldMushroomLongName : FieldMushroomShortName; }

    public static string BlusherMushroomLongName => "Blusher Mushroom";
    public static string BlusherMushroomShortName => "Blusher";
    public static double BlusherMushroomRefresh => 12.0;
    public static string BlusherMushroomName(bool isLong) { return isLong ? BlusherMushroomLongName : BlusherMushroomShortName; }

    public static string GoblinPuffballLongName => "Goblin Puffball";
    public static string GoblinPuffballShortName => "Goblin Puffball";
    public static double GoblinPuffballRefresh => 10.0;
    public static string GoblinPuffballName(bool isLong) { return isLong ? GoblinPuffballLongName : GoblinPuffballShortName; }

    public static string MilkCapMushroomLongName => "Milk Cap Mushroom";
    public static string MilkCapMushroomShortName => "Milk Cap";
    public static double MilkCapMushroomRefresh => 9.0;
    public static string MilkCapMushroomName(bool isLong) { return isLong ? MilkCapMushroomLongName : MilkCapMushroomShortName; }

    public static string BloodMushroomLongName => "Blood Mushroom";
    public static string BloodMushroomShortName => "Blood";
    public static double BloodMushroomRefresh => 8.0;
    public static string BloodMushroomName(bool isLong) { return isLong ? BloodMushroomLongName : BloodMushroomShortName; }

    public static string CoralMushroomLongName => "Coral Mushroom";
    public static string CoralMushroomShortName => "Coral";
    public static double CoralMushroomRefresh => 7.0;
    public static string CoralMushroomName(bool isLong) { return isLong ? CoralMushroomLongName : CoralMushroomShortName; }

    public static string IocaineMushroomLongName => "Iocaine Mushroom";
    public static string IocaineMushroomShortName => "Iocaine";
    public static double IocaineMushroomRefresh => 6.5;
    public static string IocaineMushroomName(bool isLong) { return isLong ? IocaineMushroomLongName : IocaineMushroomShortName; }

    public static string GroxmakMushroomLongName => "Groxmak Mushroom";
    public static string GroxmakMushroomShortName => "Groxmak";
    public static double GroxmakMushroomRefresh => 6.0;
    public static string GroxmakMushroomName(bool isLong) { return isLong ? GroxmakMushroomLongName : GroxmakMushroomShortName; }

    public static string PorciniMushroomLongName => "Porcini Mushroom";
    public static string PorciniMushroomShortName => "Porcini";
    public static double PorciniMushroomRefresh => 5.5;
    public static string PorciniMushroomName(bool isLong) { return isLong ? PorciniMushroomLongName : PorciniMushroomShortName; }

    public static string BlackFootMorelLongName => "Black Foot Morel";
    public static string BlackFootMorelShortName => "Black Foot Morel";
    public static double BlackFootMorelRefresh => 5.0;
    public static string BlackFootMorelName(bool isLong) { return isLong ? BlackFootMorelLongName : BlackFootMorelShortName; }

    public static string PixiesParasolLongName => "Pixie's Parasol";
    public static string PixiesParasolShortName => "Pixie's Parasol";
    public static double PixiesParasolRefresh => 4.5;
    public static string PixiesParasolName(bool isLong) { return isLong ? PixiesParasolLongName : PixiesParasolShortName; }

    public static string FlyAmanitaLongName => "Fly Amanita";
    public static string FlyAmanitaShortName => "Fly Amanita";
    public static double FlyAmanitaRefresh => 4.0;
    public static string FlyAmanitaName(bool isLong) { return isLong ? FlyAmanitaLongName : FlyAmanitaShortName; }

    public static string BlastcapMushroomLongName => "Blastcap Mushroom";
    public static string BlastcapMushroomShortName => "Blastcap";
    public static double BlastcapMushroomRefresh => 3.5;
    public static string BlastcapMushroomName(bool isLong) { return isLong ? BlastcapMushroomLongName : BlastcapMushroomShortName; }

    public static string ChargedMyceliumLongName => "Charged Mycelium";
    public static string ChargedMyceliumShortName => "Charged Mycelium";
    public static double ChargedMyceliumRefresh => 3.5;
    public static string ChargedMyceliumName(bool isLong) { return isLong ? ChargedMyceliumLongName : ChargedMyceliumShortName; }

    public static string FalseAgaricLongName => "False Agaric";
    public static string FalseAgaricShortName => "False Agaric";
    public static double FalseAgaricRefresh => 2.5;
    public static string FalseAgaricName(bool isLong) { return isLong ? FalseAgaricLongName : FalseAgaricShortName; }

    public static string WizardsMushroomLongName => "Wizard's Mushroom";
    public static string WizardsMushroomShortName => "Wizard's";
    public static double WizardsMushroomRefresh => 1.5;
    public static string WizardsMushroomName(bool isLong) { return isLong ? WizardsMushroomLongName : WizardsMushroomShortName; }
    #endregion

    #region Init
    public static MoonPhase NewMoon { get; } = new("New Moon", "South of the westernmost lake");
    public static MoonPhase WaxingCrescentMoon { get; } = new("Waxing Crescent Moon", "North and east of the portal to Serbule");
    public static MoonPhase FirstQuarterMoon { get; } = new("First-Quarter Moon", "At spiders");
    public static MoonPhase WaxingGibbousMoon { get; } = new("Waxing Gibbous Moon", "By the moutain, north of the lake's edge");
    public static MoonPhase FullMoon { get; } = new("Full Moon", "By Percy's House");
    public static MoonPhase WaningGibbousMoon { get; } = new("Waning Gibbous Moon", "East of Hogan's Keep");
    public static MoonPhase LastQuarterMoon { get; } = new("Last-Quarter Moon", "At gnashers");
    public static MoonPhase WaningCrescentMoon { get; } = new("Waning Crescent Moon", "South of the waterfall");
    public static MoonPhase NullMoonPhase { get; } = new("(Unselect)", string.Empty);

    public static List<MoonPhase> MoonPhaseList { get; } = new()
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

    public bool IsCurrent => PhaseCalculator.MoonPhase == this;
    public string FastPortMushroomLongText => FastPortMushroomText(true);
    public string FastPortMushroomShortText => FastPortMushroomText(false);
    public string DarkChapelTip { get; }

    public string FastPortMushroomText(bool isLong)
    {
        string FastPortMushroomText;

        switch (MoonPhaseList.IndexOf(this))
        {
            default:
            case 0:
                FastPortMushroomText = $"{GoblinPuffballName(isLong)} ({NeutralCultureDouble(GoblinPuffballRefresh)}h), {CoralMushroomName(isLong)} ({NeutralCultureDouble(CoralMushroomRefresh)}h)";
                break;
            case 1:
                FastPortMushroomText = $"{IocaineMushroomName(isLong)} ({NeutralCultureDouble(IocaineMushroomRefresh)}h)";
                break;
            case 2:
                FastPortMushroomText = $"{MycenaMushroomName(isLong)} ({NeutralCultureDouble(MycenaMushroomRefresh)}h), {GroxmakMushroomName(isLong)} ({NeutralCultureDouble(GroxmakMushroomRefresh)}h), {BlastcapMushroomName(isLong)} ({NeutralCultureDouble(BlastcapMushroomRefresh)}h)";
                break;
            case 3:
                FastPortMushroomText = $"{BoletusMushroomName(isLong)} ({NeutralCultureDouble(BoletusMushroomRefresh)}h), {PorciniMushroomName(isLong)} ({NeutralCultureDouble(PorciniMushroomRefresh)}h)";
                break;
            case 4:
                FastPortMushroomText = $"{FieldMushroomName(isLong)} ({NeutralCultureDouble(FieldMushroomRefresh)}h), {BlackFootMorelName(isLong)} ({NeutralCultureDouble(BlackFootMorelRefresh)}h), {FalseAgaricName(isLong)} ({NeutralCultureDouble(FalseAgaricRefresh)}h)";
                break;
            case 5:
                FastPortMushroomText = $"{BlusherMushroomName(isLong)} ({NeutralCultureDouble(BlusherMushroomRefresh)}h), {PixiesParasolName(isLong)} ({NeutralCultureDouble(PixiesParasolRefresh)}h)";
                break;
            case 6:
                FastPortMushroomText = $"{MilkCapMushroomName(isLong)} ({NeutralCultureDouble(MilkCapMushroomRefresh)}h), {FlyAmanitaName(isLong)} ({NeutralCultureDouble(FlyAmanitaRefresh)}h), {WizardsMushroomName(isLong)} ({NeutralCultureDouble(WizardsMushroomRefresh)}h)";
                break;
            case 7:
                FastPortMushroomText = $"{BloodMushroomName(isLong)} ({NeutralCultureDouble(BloodMushroomRefresh)}h), {ChargedMyceliumName(isLong)} ({NeutralCultureDouble(ChargedMyceliumRefresh)}h)";
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
