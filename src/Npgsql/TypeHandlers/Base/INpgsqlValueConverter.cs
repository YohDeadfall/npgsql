namespace Npgsql.TypeHandlers.Base
{
    internal interface INpgsqlValueConverter<TSource, TTarget>
    {
        TSource ToSource(TTarget value);
        TTarget ToTarget(TSource value);
    }
}
