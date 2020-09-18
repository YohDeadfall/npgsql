namespace Npgsql.Expirements.TypeHandling
{
    interface INpgsqlValueConverter<TNative, TRuntime>
    {
        TNative ToNative(TRuntime value);
        TRuntime ToRuntime(TNative value);
    }
}
