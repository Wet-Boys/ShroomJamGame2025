using System;
using System.Numerics;
using Godot;

namespace SettingsHelper.SettingsEntries;

public abstract partial class ScalarSettingsEntry<[MustBeVariant] T> : SettingsEntryTyped<T>
    where T : IComparisonOperators<T, T, bool>, IEquatable<T>
{
    public override T Value
    {
        get => base.Value;
        protected set
        {
            if (value > Max)
            {
                base.Value = Max;
                return;
            }

            if (value < Min)
            {
                base.Value = Min;
                return;
            }
            
            base.Value = value;
        }
    }

    public abstract T Min { get; set; }
    
    public abstract T Max { get; set; }
}