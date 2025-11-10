using Godot;

namespace ShroomGameReal.Utilities;

[GlobalClass]
public partial class MouseReleaser : Node
{
    public static MouseReleaser Instance { get; private set; }

    [Export]
    private Input.MouseModeEnum _lockedMode = Input.MouseModeEnum.Captured;
    private int _numberOfRequests;

    public bool IsMouseLocked => _numberOfRequests <= 0;

    public override void _EnterTree()
    {
        Instance = this;
        _numberOfRequests = 0;
    }

    public void RequestFreeMouse()
    {
        GD.Print("free");
        
        _numberOfRequests++;
        UpdateMouseState();
    }

    public void RequestLockedMouse()
    {
        GD.Print("Locked");
        
        _numberOfRequests--;
        UpdateMouseState();
    }

    public void SetLockedMode(Input.MouseModeEnum mouseMode)
    {
        _lockedMode = mouseMode;
        UpdateMouseState();
    }

    public void UpdateMouseState()
    {
        if (_numberOfRequests > 0)
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
        }
        else
        {
            _numberOfRequests = 0;

            Input.MouseMode = _lockedMode;
        }
    }
}