using Godot;

namespace ShroomGameReal.scenes.Scoot_Shoot;

[GlobalClass]
public partial class CrtScreenZapper : Node2D
{
    [Export]
    public float maxZapperRange = 100f;
    
    // public override void _Process(double delta)
    // {
    //     var camera = MainCamera.Instance;
    //     GlobalPosition = camera.GetViewport().GetMousePosition();
    // }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseMotion)
        {
            GlobalPosition = mouseMotion.Position;
        }
    }
}