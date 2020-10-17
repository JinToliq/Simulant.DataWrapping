namespace j2DataWrapping.Wrapping.Interfaces
{
  public interface IContextExtractor<in TId, TContext> where TContext : class
  {
    bool TryGetExternal(TId key, out TContext context);
  }
}
