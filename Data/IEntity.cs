namespace Simulant.DataWrapping.Data;

public interface IEntity
{ }

public interface IEntity<TKey, TData> : IEntity where TData : class, new()
{
  TKey Id { get; }
  TData Data { get; }
  Entity<TKey, TData> EnterRead();
  Entity<TKey, TData> EnterWrite();
  void Save();
}