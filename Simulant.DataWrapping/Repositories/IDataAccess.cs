using System.Threading.Tasks;

namespace Simulant.DataWrapping.Repositories;

public interface IDataAccess<TKey, TData>
  where TData : class, new()
{
  Task SaveAsync(TKey key, TData data);
  Task<TData> LoadAsync(TKey key);
  Task<TData> CreateNewAsync(TKey key);
  Task DeleteAsync(TKey key);
}