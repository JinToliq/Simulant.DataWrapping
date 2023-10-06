using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Simulant.DataWrapping.Repositories;

namespace Simulant.DataWrapping.Data;

public class Entity<TKey, TData> : IEntity<TKey, TData>
  where TData : class, new()
{
  private readonly Logger _log = LogManager.GetCurrentClassLogger();
  private readonly ReaderWriterLockSlim _lockSlim = new();

  public TKey Id { get; }
  public TData Data { get; }

  public Entity(TKey id, TData data)
  {
    Id = id;
    Data = data;
  }

  public bool IsDirty { get; private set; }
  public DateTime LastAccessTime { get; private set; }

  public void Dispose()
  {
    if (_lockSlim.IsWriteLockHeld)
    {
      // Save must be called before dispose. Otherwise, the entity might be disposed because of an exception
      if (IsDirty)
        Repository.Get<TKey, TData>()!.ReloadAsync(this);

      _lockSlim.ExitWriteLock();
    }

    if (_lockSlim.IsReadLockHeld)
      _lockSlim.ExitReadLock();
  }

  public async ValueTask DisposeAsync()
  {
    if (_lockSlim.IsWriteLockHeld)
    {
      // Save must be called before dispose. Otherwise, the entity might be disposed because of an exception
      if (IsDirty)
        await Repository.Get<TKey, TData>()!.ReloadAsync(this);

      _lockSlim.ExitWriteLock();
    }

    if (_lockSlim.IsReadLockHeld)
      _lockSlim.ExitReadLock();
  }

  Entity<TKey, TData> IEntity<TKey, TData>.EnterRead()
  {
    var heldRead = _lockSlim.IsReadLockHeld;
    var heldWrite = _lockSlim.IsWriteLockHeld;
    _lockSlim.EnterReadLock();
    ValidateReadLockRecursion(heldRead);
    ValidateWriteLockRecursion(heldWrite);
    UpdateAccessTime();
    return this;
  }

  Entity<TKey, TData> IEntity<TKey, TData>.EnterWrite()
  {
    var heldRead = _lockSlim.IsReadLockHeld;
    var heldWrite = _lockSlim.IsWriteLockHeld;
    _lockSlim.EnterWriteLock();
    ValidateReadLockRecursion(heldRead);
    ValidateWriteLockRecursion(heldWrite);
    IsDirty = true;
    UpdateAccessTime();
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

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void UpdateAccessTime() => LastAccessTime = DateTime.UtcNow;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void ValidateReadLockRecursion(bool state)
  {
    if (state)
      _log.Warn($"Read lock entered recursively by entity of data: {typeof(TData)}");
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void ValidateWriteLockRecursion(bool state)
  {
    if (state)
      _log.Warn($"Write lock entered recursively by entity of data: {typeof(TData)}");
  }
}