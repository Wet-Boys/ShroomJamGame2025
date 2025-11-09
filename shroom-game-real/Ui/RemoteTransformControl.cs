using Godot;

namespace ShroomGameReal.Ui;

[Tool]
[GlobalClass]
public partial class RemoteTransformControl : Control
{
    [Export]
    public Control remoteControl;

    public override void _Process(double delta)
    {
        if (remoteControl is null)
            return;

        remoteControl.AnchorsPreset = AnchorsPreset;
        remoteControl.AnchorTop = AnchorTop;
        remoteControl.AnchorBottom = AnchorBottom;
        remoteControl.AnchorLeft = AnchorLeft;
        remoteControl.AnchorRight = AnchorRight;
        remoteControl.GlobalPosition = GlobalPosition;
        remoteControl.Size = Size;
    }
}