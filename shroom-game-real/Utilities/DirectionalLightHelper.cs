using Godot;

namespace ShroomGameReal.Utilities;

[Tool]
[GlobalClass]
public partial class DirectionalLightHelper : DirectionalLight3D
{
    [ExportGroup("Light Shafts")]
    [Export]
    public Node3D[] LightShafts
    {
        get;
        set
        {
            field = value;
            UpdateLightShafts();
        }
    } = [];

    [Export]
    public float Length
    {
        get;
        set
        {
            field = value;
            UpdateLightShafts();
        }
    } = 4f;
    
    [Export(PropertyHint.Range, "0, 1")]
    public float Opacity
    {
        get;
        set
        {
            field = value;
            UpdateLightShafts();
        }
    } = 0.2f;

    [ExportGroup("Lights")]
    [Export]
    public Light3D[] Lights
    {
        get;
        set
        {
            field = value;
            UpdateLights();
        }

    } = [];
    
    private Color _lastLightColor;
    private Quaternion _lastDirection;

    public override void _Ready()
    {
        _lastLightColor = LightColor;
        _lastDirection = Quaternion;
    }

    public override void _Process(double delta)
    {
        // Only run in Editor
        if (!Engine.IsEditorHint())
            return;

        if (_lastLightColor != LightColor || _lastDirection != Quaternion)
        {
            UpdateLights();
            UpdateLightShafts();
        }
        
        _lastLightColor = LightColor;
        _lastDirection = Quaternion;
    }

    private void UpdateLights()
    {
        foreach (var light in Lights)
        {
            light.LightColor = LightColor;
        }
    }

    private void UpdateLightShafts()
    {
        foreach (var shaft in LightShafts)
        {
            var shaftColor = LightColor;
            shaftColor.A = Opacity;
            
            shaft.Set("color", shaftColor);
            shaft.Set("length", 0);
            shaft.Set("length", Length);
        }
    }
}