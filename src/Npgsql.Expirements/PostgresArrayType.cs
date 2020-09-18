namespace Npgsql.Expirements
{
    public sealed class PostgresArrayType : PostgresType
    {
        public PostgresArrayType(int oid, int? length) : base(oid, length)
        {
        }

        public PostgresType ElementType { get; set; } = default!;
    }
}
