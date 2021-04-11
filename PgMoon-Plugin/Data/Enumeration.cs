namespace PgMoon.Data
{
    using System;

    public abstract class Enumeration : IComparable
    {
        public string Name { get; private set; }

        public int Id { get; private set; }

        protected Enumeration(int id, string name) => (Id, Name) = (id, name);

        public override string ToString() => Name;

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Enumeration that = (Enumeration)obj;
            return this.Id == that.Id;
        }

        public override int GetHashCode() => Id;

        public int CompareTo(object other) => Id.CompareTo(((Enumeration)other).Id);
    }
}