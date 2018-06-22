namespace PgMoon
{
    public class SharedCalendarEntry
    {
        public SharedCalendarEntry(string Id, string Name, bool CanWrite)
        {
            this.Id = Id;
            this.Name = Name;
            this.CanWrite = CanWrite;
        }

        public string Id { get; private set; }
        public string Name { get; private set; }
        public bool CanWrite { get; private set; }
    }
}
