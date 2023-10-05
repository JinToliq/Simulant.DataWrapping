using System.Threading.Tasks;
using Simulant.DataWrapping.Data;

namespace Simulant.DataWrapping.Repositories;

public interface IEntityRepository { }

public interface IEntityRepository<TKey, TData> : IEntityRepository where TData : class, new()
{
  Entity<TKey, TData> GetEntityRead(TKey key);
  Entity<TKey, TData> GetEntityWrite(TKey key);
  void Save(Entity<TKey, TData> entity);
  Task SaveAsync(Entity<TKey, TData> entity);
  void Reload(Entity<TKey, TData> entity);
  Task ReloadAsync(Entity<TKey, TData> entity);
}