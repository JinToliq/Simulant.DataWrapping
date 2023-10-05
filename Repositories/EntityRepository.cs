using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Simulant.DataWrapping.Data;

namespace Simulant.DataWrapping.Repositories;

public class EntityRepository<TKey, TData> : IEntityRepository<TKey, TData>
  where TData : class, new()
{
  private readonly ConcurrentDictionary<TKey, Entity<TKey, TData>> _entities;
  private readonly IDataAccess<TKey, TData> _dataAccess;

  public EntityRepository(IDataAccess<TKey, TData> access)
  {
    _entities = new();
    _dataAccess = access;
  }

  public Entity<TKey, TData> GetEntityRead(TKey key)
  {
    return ((IEntity<TKey, TData>)_entities.GetOrAdd(key, k => LoadAsync(k).GetAwaiter().GetResult())).EnterRead();
  }

  public Entity<TKey, TData> GetEntityWrite(TKey key)
  {
    return ((IEntity<TKey, TData>)_entities.GetOrAdd(key, k => LoadAsync(k).GetAwaiter().GetResult())).EnterWrite();
  }

  public void Save(Entity<TKey, TData> entity)
  {
    SaveToStorageAsync(entity).GetAwaiter().GetResult();
  }

  public async Task SaveAsync(Entity<TKey, TData> entity)
  {
    await SaveToStorageAsync(entity);
  }

  public void Reload(Entity<TKey, TData> entity)
  {
    var removed = _entities.TryRemove(entity.Id, out _);
    if (!removed)
      throw new InvalidOperationException("Entity was not found in the repository");

    var loaded = LoadAsync(entity.Id).GetAwaiter().GetResult();
    var added = _entities.TryAdd(entity.Id, loaded);
    if (!added)
      throw new InvalidOperationException("Entity was not added to the repository");
  }

  public async Task ReloadAsync(Entity<TKey, TData> entity)
  {
    var removed = _entities.TryRemove(entity.Id, out _);
    if (!removed)
      throw new InvalidOperationException("Entity was not found in the repository");

    var loaded = await LoadAsync(entity.Id);
    var added = _entities.TryAdd(entity.Id, loaded);
    if (!added)
      throw new InvalidOperationException("Entity was not added to the repository");
  }

  private async Task SaveToStorageAsync(IEntity<TKey, TData> entity)
  {
    await _dataAccess.SaveAsync(entity.Id, entity.Data);
  }

  private async Task<Entity<TKey, TData>> LoadAsync(TKey key)
  {
    var loaded = await _dataAccess.LoadAsync(key);
    return loaded is null ? new(key, await _dataAccess.CreateNewAsync(key)) : new(key, loaded);
  }
}