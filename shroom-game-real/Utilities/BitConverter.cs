using System;
using System.Collections;
using System.Linq;
using Godot;

namespace ShroomGameReal.Utilities;

internal static class BitConverter
{
    public static byte[] GetBytes(bool value) => System.BitConverter.GetBytes(value);
    
    public static byte[] GetBytes(char value) => System.BitConverter.GetBytes(value);
    
    public static byte[] GetBytes(float value) => System.BitConverter.GetBytes(value);
    
    public static byte[] GetBytes(double value) => System.BitConverter.GetBytes(value);
    
    public static byte[] GetBytes(short value) => System.BitConverter.GetBytes(value);
    
    public static byte[] GetBytes(int value) => System.BitConverter.GetBytes(value);
    
    public static byte[] GetBytes(long value) => System.BitConverter.GetBytes(value);
    
    public static byte[] GetBytes(ushort value) => System.BitConverter.GetBytes(value);
    
    public static byte[] GetBytes(uint value) => System.BitConverter.GetBytes(value);
    
    public static byte[] GetBytes(ulong value) => System.BitConverter.GetBytes(value);

    public static byte[] GetBytes(Vector2 value)
    {
        float[] values = [value.X, value.Y];
        return GetBytes(values);
    }
    
    public static byte[] GetBytes(Vector3 value)
    {
        float[] values = [value.X, value.Y, value.Z];
        return GetBytes(values);
    }
    
    public static byte[] GetBytes(Vector3I value)
    {
        int[] values = [value.X, value.Y, value.Z];
        return GetBytes(values);
    }
    
    public static byte[] GetBytes(Vector4 value)
    {
        float[] values = [value.X, value.Y, value.Z, value.W];
        return GetBytes(values);
    }
    
    public static byte[] GetBytes(Color value)
    {
        float[] values = [value.R, value.G, value.B, value.A];
        return GetBytes(values);
    }
    
    public static byte[] GetBytes(object value)
    {
        if (value is IEnumerable enumerable)
        {
            return enumerable.Cast<object>()
                .SelectMany(GetBytes)
                .ToArray();
        }
        
        return value switch
        {
            byte b => [b],
            bool v => GetBytes(v),
            char v => GetBytes(v),
            float v => GetBytes(v),
            double v => GetBytes(v),
            short v => GetBytes(v),
            int v => GetBytes(v),
            long v => GetBytes(v),
            ushort v => GetBytes(v),
            uint v => GetBytes(v),
            ulong v => GetBytes(v),
            Vector2 v => GetBytes(v),
            Vector3 v => GetBytes(v),
            Vector3I v => GetBytes(v),
            Vector4 v => GetBytes(v),
            Color v => GetBytes(v),
            _ => throw new NotImplementedException($"Unhandled byte conversion of type {value.GetType()}")
        };
    }
}