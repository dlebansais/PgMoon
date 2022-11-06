namespace PgMoon;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable SA1600 // Elements should be documented
public class SharedCalendarEntry
{
    public static SharedCalendarEntry None { get; } = new();

    private SharedCalendarEntry()
    {
        Id = string.Empty;
        Name = string.Empty;
        CanWrite = false;
    }

    public SharedCalendarEntry(string id, string name, bool canWrite)
    {
        Id = id;
        Name = name;
        CanWrite = canWrite;
    }

    public string Id { get; }
    public string Name { get; }
    public bool CanWrite { get; }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning restore SA1600 // Elements should be documented
