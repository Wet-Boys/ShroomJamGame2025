using System;
using Godot;

namespace SettingsHelper.SettingsEntries;

public abstract partial class SettingsEntryTyped<[MustBeVariant] T> : SettingsEntry
    where T : IEquatable<T>
{
    public virtual T Value
    {
        get
        {
            if (BoxedValue is null)
                return DefaultValue;
            
            return (T)BoxedValue;
        }
        protected set
        {
            var originalValue = Value;
            BoxedValue = value;
            
            if (originalValue.Equals(value))
                OnValueChanged?.Invoke(value);
        }
    }

    protected abstract T DefaultValue { get; set; }
    
    public event Action<T> OnValueChanged;
}