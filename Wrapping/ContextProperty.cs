using System.Runtime.CompilerServices;

namespace j2DataWrapping.Wrapping
{
  public interface IContextProperty
  {
    string Name { get; }
    bool IsDirty { get; }
    void SetDirty(bool value);
  }

  public class ContextProperty<TValue> : IContextProperty
  {
    private TValue _value;

    public string Name { get; private set; }
    public bool IsDirty { get; private set; }

    public TValue Value
    {
      get => _value;
      set => SetValue(value);
    }

    public ContextProperty(string name) => Name = name;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetDirty(bool value = true) => IsDirty = value;

    public bool SetValue(TValue value)
    {
      if (_value.Equals(value))
        return false;

      SetDirty();
      _value = value;
      return true;
    }
  }
}
