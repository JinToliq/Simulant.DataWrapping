using System;
using System.Collections.Concurrent;
using j2DataWrapping.Wrapping.Interfaces;

namespace j2DataWrapping.Wrapping
{
  public interface IWrapperContainer<TContext> where TContext : class, new()
  {
    void AcquireDirtyWrapper(Wrapper<TContext> wrapper);
  }

  public interface IWrapperContainer<in TKey, out TWrapper, TContext>
    : IWrapperContainer<TContext>
    where TWrapper : Wrapper<TContext>, new()
    where TContext : class, new()
  {
    TWrapper GetEntityRead(TKey key);
    TWrapper GetEntityWrite(TKey key);
    TWrapper GetEntityRead<TWarmUpSource>(TKey key, TWarmUpSource warmupSource);
    TWrapper GetEntityWrite<TWarmUpSource>(TKey key, TWarmUpSource warmupSource);
    TWrapper GetOrBypassDefault(TKey key);
  }

  public abstract class WrapperContainer<TKey, TWrapper, TContext>
    : IWrapperContainer<TKey, TWrapper, TContext>
    where TWrapper : Wrapper<TContext>, IWrapper<TContext>, new()
    where TContext : class, new()
  {
    public const int DefaultObsolescenceTimeSec = 5 * 60;

    private readonly ConcurrentDictionary<TKey, TWrapper> _entities;
    private readonly IContextExtractor<TKey, TContext> _contextExtractor;
    private readonly IContextSaver<TKey, TContext> _contextSaver;
    private readonly IContextUpdater<TKey, TWrapper, TContext> _contextUpdater;
    private object _locker = new object();

    protected virtual int ObsolescenceTimeSec => DefaultObsolescenceTimeSec;

    protected WrapperContainer(
      IContextExtractor<TKey, TContext> extractor,
      IContextSaver<TKey, TContext> saver,
      IContextUpdater<TKey, TWrapper, TContext> updater)
    {
      _entities = new ConcurrentDictionary<TKey, TWrapper>();
      _contextExtractor = extractor;
      _contextSaver = saver;
      _contextUpdater = updater;
    }

    public TWrapper GetEntityRead(TKey key)
    {
      lock (_locker)
      {
        var value = _entities.GetOrAdd(key, ProvideEntity(key));
        return value.EnterRead() as TWrapper;
      }
    }

    public TWrapper GetEntityWrite(TKey key)
    {
      lock (_locker)
      {
        var value = _entities.GetOrAdd(key, ProvideEntity(key));
        return value.EnterWrite() as TWrapper;
      }
    }

    public TWrapper GetEntityRead<TWarmUpSource>(TKey key, TWarmUpSource warmUpSource)
    {
      lock (_locker)
      {
        var value = _entities.GetOrAdd(key, ProvideEntity(key));
        return value.EnterRead(warmUpSource) as TWrapper;
      }
    }

    public TWrapper GetEntityWrite<TWarmUpSource>(TKey key, TWarmUpSource warmUpSource)
    {
      lock (_locker)
      {
        var value = _entities.GetOrAdd(key, ProvideEntity(key));
        return value.EnterWrite(warmUpSource) as TWrapper;
      }
    }

    public TWrapper GetOrBypassDefault(TKey key)
    {
      lock (_locker)
      {
        if (_entities.TryGetValue(key, out var value))
          return value;

        if (!_contextExtractor.TryGetExternal(key, out var context))
          return null;

        value = new TWrapper();
        value.SetOnce(this, context);
        _entities.TryAdd(key, value);
        return value;
      }
    }

    public void AcquireDirtyWrapper(Wrapper<TContext> wrapper) => UnloadWrappers();

    private TWrapper ProvideEntity(TKey key)
    {
      if (!_contextExtractor.TryGetExternal(key, out var context))
      {
        context = new TContext();
        _contextSaver.Save(key, context);
      }

      var wrapper = new TWrapper();
      wrapper.SetOnce(this, context);
      return wrapper;
    }

    private void UnloadWrappers()
    {
      lock (_locker)
      {
        var now = DateTime.Now;
        var keys = _entities.Keys;
        foreach (var key in keys)
        {
          var value = _entities[key];
          if (value.IsDirty)
          {
            _contextUpdater.Update(key, value);
            value.ResetDirty();
          }

          if ((now - value.LastAccessTime).TotalSeconds < ObsolescenceTimeSec)
            continue;

          _entities.TryRemove(key, out _);
        }
      }
    }
  }
}
