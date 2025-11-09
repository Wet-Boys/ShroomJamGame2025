using Godot;

namespace ShroomGameReal;

[GlobalClass]
public partial class GlobalGameState : Node
{
    public static GlobalGameState Instance { get; private set; }
    
    [Export]
    public bool InBaitMode { get; set; } = true;
    
    [Export]
    public float MainTimeScale { get; set; } = 1f;
    
    [Export]
    public float GameTimeScale { get; set; } = 1f;

    public bool IsMainPaused => Instance.MainTimeScale <= 0;

    public override void _EnterTree()
    {
        Instance = this;
    }
}