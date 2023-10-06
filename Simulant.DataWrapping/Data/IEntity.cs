using System;

namespace Simulant.DataWrapping.Data;

public interface IEntity : IDisposable, IAsyncDisposable
{ }

public interface IEntity<TKey, TData> : IEntity where TData : class, new()
{
  TKey Id { get; }
  TData Data { get; }
  Entity<TKey, TData> EnterRead();
  Entity<TKey, TData> EnterWrite();
  void Save();
}