namespace PgMoon
{
    using System;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable SA1600 // Elements should be documented
    public class Lunation
    {
        public int Index { get; set; }
        public DateTime NewMoon { get; set; }
        public DateTime FirstQuarterMoon { get; set; }
        public DateTime FullMoon { get; set; }
        public DateTime LastQuarterMoon { get; set; }
        public TimeSpan Duration { get; set; }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning restore SA1600 // Elements should be documented
}
