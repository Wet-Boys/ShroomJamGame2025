using Godot;

namespace ShroomGameReal.Interactables;

public partial class InteractLabel : PanelContainer
{
    public override void _Ready()
    {
        UpdatePivot();
    }

    public void UpdatePivot()
    {
        PivotOffset = Size / 2f;
    }
}