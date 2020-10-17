namespace j2DataWrapping.Wrapping.Interfaces
{
  public interface IContextUpdater<in TKey, in TWrapper, TContext>
    where TContext : class, new()
    where TWrapper : IWrapper<TContext>
  {
    void Update(TKey key, TWrapper context);
  }
}
