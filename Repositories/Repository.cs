using System;
using System.Collections.Concurrent;

namespace Simulant.DataWrapping.Repositories;

public static class Repository
{
  private static readonly ConcurrentDictionary<Type, IEntityRepository> _repositories = new();

  public static void Register<TKey, TData>(IEntityRepository<TKey, TData> repository) where TData : class, new()
  {
    var added = _repositories.TryAdd(typeof(TData), repository);
    if (!added)
      throw new InvalidOperationException($"Repository for entity of data {typeof(TData)} already registered");
  }

  public static IEntityRepository<TKey, TData> Get<TKey, TData>() where TData : class, new()
  {
    if (_repositories.TryGetValue(typeof(TData), out var repository))
      return repository as IEntityRepository<TKey, TData> ?? throw new InvalidOperationException($"Cannot convert repository {repository.GetType()} to {typeof(IEntityRepository<TKey, TData>)}");

    throw new InvalidOperationException($"Repository for entity of data {typeof(TData)} is not registered");
  }
}