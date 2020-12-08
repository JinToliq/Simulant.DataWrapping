using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace j2DataWrapping.Wrapping
{
  public interface IWrapper
  {
    Type ContextType { get; }
    IEnumerable<IContextProperty> EnumerateProperties();
  }

  public interface IWrapper<TContext>
    where TContext : class, new()
  {
    TContext Context { get; }
    void SetOnce(IWrapperContainer<TContext> owner, TContext context);
    void ResetDirty();
    void WarmUp<TWarmUpSource>(TWarmUpSource source);
    Wrapper<TContext> EnterRead();
    Wrapper<TContext> EnterWrite();
    Wrapper<TContext> EnterRead<TWarmUpSource>(TWarmUpSource source);
    Wrapper<TContext> EnterWrite<TWarmUpSource>(TWarmUpSource source);
  }

  public abstract class Wrapper<TContext>
    : IWrapper<TContext>,
    IWrapper,
    IDisposable
    where TContext : class, new()
  {
    public TContext Context { get; private set; }
    private IWrapperContainer<TContext> _owner;
    private readonly ReaderWriterLockSlim _lockSlim = new ReaderWriterLockSlim();

    public bool IsDirty { get; private set; }
    public DateTime LastAccessTime { get; private set; }

    public void Dispose()
    {
      if (_lockSlim.IsWriteLockHeld)
      {
        if (IsDirty)
          _owner.AcquireDirtyWrapper(this);
        _lockSlim.ExitWriteLock();
      }

      if (_lockSlim.IsReadLockHeld)
        _lockSlim.ExitReadLock();
    }

    #region IWrapper

    public Type ContextType => Context.GetType();

    public IEnumerable<IContextProperty> EnumerateProperties() => Enumerable.Empty<IContextProperty>();

    #endregion

    #region IWrapper<TContext>

    void IWrapper<TContext>.SetOnce(IWrapperContainer<TContext> owner, TContext context)
    {
      if (_owner == null)
        _owner = owner;
      if (Context == null)
        Context = context;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void IWrapper<TContext>.ResetDirty() => IsDirty = false;

    void IWrapper<TContext>.WarmUp<TWarmUpSource>(TWarmUpSource source) => throw new InvalidOperationException($"No warm up implementation provided for type: {GetType().Name}");

    Wrapper<TContext> IWrapper<TContext>.EnterRead()
    {
      {
        _lockSlim.EnterReadLock();
        LastAccessTime = DateTime.Now;
        return this;
      }
    }

    Wrapper<TContext> IWrapper<TContext>.EnterWrite()
    {
      _lockSlim.EnterWriteLock();
      LastAccessTime = DateTime.Now;
      return this;
    }

    Wrapper<TContext> IWrapper<TContext>.EnterRead<TWarmUpSource>(TWarmUpSource source)
    {
      _lockSlim.EnterReadLock();
      LastAccessTime = DateTime.Now;
      (this as IWrapper<TContext>).WarmUp(source);
      return this;
    }

    Wrapper<TContext> IWrapper<TContext>.EnterWrite<TWarmUpSource>(TWarmUpSource source)
    {
      _lockSlim.EnterWriteLock();
      LastAccessTime = DateTime.Now;
      (this as IWrapper<TContext>).WarmUp(source);
      return this;
    }

    #endregion

    protected void SetProperty<T>(ContextProperty<T> destination, T value)
    {
      if (destination.SetValue(value))
        IsDirty = true;
    }

  }
}
