using Godot;

namespace ShroomGameReal.scenes.Scoot_Shoot;

[GlobalClass]
public partial class CrtScreenZapper : Node2D
{
    [Export]
    public float maxZapperRange = 100f;
    
    public override void _Process(double delta)
    {
        GlobalPosition = GetViewport().GetMousePosition();
    }
}