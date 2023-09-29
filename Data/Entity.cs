using System;
using System.Threading;
using Simulant.DataWrapping.Repositories;

namespace Simulant.DataWrapping.Data
{
  public class Entity<TKey, TData>(TKey id, TData data) : IEntity<TKey, TData>, IDisposable
    where TData : class, new()
  {
    public TKey Id { get; } = id;
    public TData Data { get; } = data;
    private readonly ReaderWriterLockSlim _lockSlim = new();

    public bool IsDirty { get; private set; }
    public DateTime LastAccessTime { get; private set; }

    public void Dispose()
    {
      if (_lockSlim.IsWriteLockHeld)
      {
        // Save must be called before dispose. Otherwise, the entity might be disposed because of an exception
        if (IsDirty)
          Repository.Get<TKey, TData>()!.Reload(this);

        _lockSlim.ExitWriteLock();
      }

      if (_lockSlim.IsReadLockHeld)
        _lockSlim.ExitReadLock();
    }

    Entity<TKey, TData> IEntity<TKey, TData>.EnterRead()
    {
      _lockSlim.EnterReadLock();
      LastAccessTime = DateTime.Now;
      return this;
    }

    Entity<TKey, TData> IEntity<TKey, TData>.EnterWrite()
    {
      _lockSlim.EnterWriteLock();
      IsDirty = true;
      LastAccessTime = DateTime.Now;
      return this;
    }

    public void Save()
    {
      if (!_lockSlim.IsWriteLockHeld)
      {
        if (IsDirty)
          throw new InvalidOperationException("Entity is marked as dirty but write lock is not held");

        return;
      }

      if (!IsDirty)
        throw new InvalidOperationException("Write lock held but entity is not marked as dirty");

      Repository.Get<TKey, TData>()!.Save(this);
      IsDirty = false;
    }
  }
}