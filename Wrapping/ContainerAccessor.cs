namespace j2DataWrapping.Wrapping
{
  public class ContainerAccessor<TKey, TWrapper, TContext, TContainer>
    where TWrapper : Wrapper<TContext>, new()
    where TContext : class, new()
    where TContainer : WrapperContainer<TKey, TWrapper, TContext>, new()
  {
    private readonly WrapperContainer<TKey, TWrapper, TContext> _container = new TContainer();

    public TWrapper GetOrBypassCacheUnsafe(TKey id) => _container.GetOrBypassDefault(id);
    public TWrapper GetRead(TKey id) => _container.GetEntityRead(id);
    public TWrapper GetWrite(TKey id) => _container.GetEntityWrite(id);
    public TWrapper GetRead<TWarmUpSource>(TKey id, TWarmUpSource warmUpSource) => _container.GetEntityRead(id, warmUpSource);
    public TWrapper GetWrite<TWarmUpSource>(TKey id, TWarmUpSource warmUpSource) => _container.GetEntityWrite(id, warmUpSource);
  }
}
