using System;
using System.Collections.Concurrent;
using Simulant.DataWrapping.Data;

namespace Simulant.DataWrapping.Repositories;

public class EntityRepository<TKey, TData> : IEntityRepository<TKey, TData>
  where TData : class, new()
{
  private readonly ConcurrentDictionary<TKey, Entity<TKey, TData>> _entities;
  private readonly IDataAccess<TKey, TData> _dataAccess;

  public EntityRepository(IDataAccess<TKey, TData> access) => _dataAccess = access;

  public Entity<TKey, TData> GetEntityRead(TKey key) => ((IEntity<TKey, TData>)_entities.GetOrAdd(key, Load)).EnterRead();

  public Entity<TKey, TData> GetEntityWrite(TKey key) => ((IEntity<TKey, TData>)_entities.GetOrAdd(key, Load)).EnterWrite();

  public void Save(Entity<TKey, TData> entity)
  {
    SaveToStorage(entity);
  }

  public void Reload(Entity<TKey, TData> entity)
  {
    var removed = _entities.TryRemove(entity.Id, out var _);
    if (!removed)
      throw new InvalidOperationException("Entity was not found in the repository");

    var loaded = Load(entity.Id);
    var added = _entities.TryAdd(entity.Id, loaded);
    if (!added)
      throw new InvalidOperationException("Entity was not added to the repository");
  }

  private void SaveToStorage(Entity<TKey, TData> entity)
  {
    _dataAccess.Save(entity.Id, entity.Data);
  }

  private Entity<TKey, TData> Load(TKey key)
  {
    var loaded = _dataAccess.Load(key, out var data);
    return loaded ? new(key, data) : new(key, _dataAccess.CreateNew(key));
  }
}