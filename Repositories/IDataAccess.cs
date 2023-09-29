namespace Simulant.DataWrapping.Repositories;

public interface IDataAccess<TKey, TData>
  where TData : class, new()
{
  void Save(TKey key, TData data);
  void Update(TKey key, TData entity);
  bool Load(TKey key, out TData context);
  TData CreateNew(TKey key);
  void Delete(TKey key);
}