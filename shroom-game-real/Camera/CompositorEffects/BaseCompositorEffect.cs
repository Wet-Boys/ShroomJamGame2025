using Godot;

namespace ShroomGameReal.Camera.CompositorEffects;

[Tool]
[GlobalClass]
public abstract partial class BaseCompositorEffect : CompositorEffect, ISerializationListener
{
    [ExportToolButton("Rebuild")]
    private Callable RebuildEffectCallable => Callable.From(Rebuild);
    
    protected RenderingDevice Device { get; private set; }

    public BaseCompositorEffect()
    {
        Device = RenderingServer.GetRenderingDevice();
        Callable.From(Construct).CallDeferred();
    }

    protected abstract void ConstructEffect(RenderingDevice device);
    
    protected abstract void DestructEffect(RenderingDevice device);
    
    private void Construct()
    {
        if (Device is null)
        {
            GD.PushError($"RenderingDevice is null! Can't construct effect '{GetType().Name}'!");
            return;
        }

        if (!Enabled)
            return;
        
        RenderingServer.CallOnRenderThread(Callable.From(() => ConstructEffect(Device)));
    }

    protected void Destruct()
    {
        if (Device is null)
        {
            GD.PushError($"RenderingDevice is null! Can't destruct effect '{GetType().Name}'!");
            return;
        }
        
        DestructEffect(Device);
    }

    protected void Rebuild()
    {
        RenderingServer.CallOnRenderThread(Callable.From(Destruct));
        RenderingServer.CallOnRenderThread(Callable.From(Construct));
    }
    
    public void OnBeforeSerialize()
    {
        Destruct();
    }

    public void OnAfterDeserialize()
    {
        Construct();
    }
}