using System;
using System.Linq;
using Godot;

namespace ShroomGameReal.Utilities;

public static class RenderingDeviceExtensions
{
    public static Rid UniformBufferCreate(this RenderingDevice device, params object[] uniformData)
    {
        var data = BitConverter.GetBytes(uniformData);
        
        if (data.Length % 16 != 0)
        {
            var remainingLength = 16 - data.Length % 16;
            var padding = new byte[remainingLength];
            
            data = data.Concat(padding).ToArray();
        }
        
        return device.UniformBufferCreate((uint)data.Length, data);
    }
}