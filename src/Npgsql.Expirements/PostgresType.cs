namespace Npgsql.Expirements
{
    public abstract class PostgresType
    {
        public int Oid { get; }
        public int? Length { get; }

        protected PostgresType(int oid, int? length) =>
            (Oid, Length) = (oid, length);
    }
}
