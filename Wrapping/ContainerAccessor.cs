using System;
using j2DataWrapping.Wrapping.Interfaces;

namespace j2DataWrapping.Wrapping
{
  [Obsolete("Use WrapperContainer by itself")]
  public class ContainerAccessor<TKey, TWrapper, TContext>
    where TWrapper : Wrapper<TContext>, new()
    where TContext : class, new()
  {
    private readonly WrapperContainer<TKey, TWrapper, TContext> _container;

    public ContainerAccessor(
      IContextExtractor<TKey, TContext> extractor,
      IContextSaver<TKey, TContext> saver,
      IContextUpdater<TKey, TWrapper, TContext> updater)
    {
      _container = new WrapperContainer<TKey, TWrapper, TContext>(extractor, saver, updater);
    }

    public TWrapper GetOrBypassCacheUnsafe(TKey id) => _container.GetOrBypassDefault(id);
    public TWrapper GetRead(TKey id) => _container.GetEntityRead(id);
    public TWrapper GetWrite(TKey id) => _container.GetEntityWrite(id);
    public TWrapper GetRead<TWarmUpSource>(TKey id, TWarmUpSource warmUpSource) => _container.GetEntityRead(id, warmUpSource);
    public TWrapper GetWrite<TWarmUpSource>(TKey id, TWarmUpSource warmUpSource) => _container.GetEntityWrite(id, warmUpSource);
  }
}
