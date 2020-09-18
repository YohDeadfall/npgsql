namespace Npgsql.Expirements
{
    public class NpgsqlParameter<T> : NpgsqlParameter
    {
        public new T Value { get; set; } = default!;
    }
}
