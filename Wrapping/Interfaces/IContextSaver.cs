namespace j2DataWrapping.Wrapping.Interfaces
{
  public interface IContextSaver<in TId, in TContext> where TContext : class, new()
  {
    void Save(TId key, TContext context);
  }
}
