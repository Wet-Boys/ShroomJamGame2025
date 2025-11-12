using Godot;
using Godot.Collections;
using ShroomGameReal.Tv.GameStates;
using ShroomGameReal.Utilities;

namespace ShroomGameReal.scenes.obby;

[GlobalClass]
public partial class ObbyGameState : BaseTvGameState
{
    [Export]
    private Array<PackedScene> _potentialLevels;
    public override void _Ready()
    {
        CanActivate = true;
        infoText = "Get to the Finish!";
    }

    public override void OnEnterState()
    {
        MouseReleaser.Instance.SetLockedMode(Input.MouseModeEnum.Captured);
        IsActive = true;
    }

    public void SpawnLevel(int specificLevel = -1)
    {
        RandomNumberGenerator rng = new RandomNumberGenerator();
        if (specificLevel == -1)
        {
            specificLevel = rng.RandiRange(1, _potentialLevels.Count - 1);
        }
        AddChild(_potentialLevels[specificLevel].Instantiate<Node3D>());
    }

    public void FinishLevel()
    {
        if (GameFlowHandler.isInDreamSequence)
        {
            GameFlowHandler.instance.FinishMinigame(this, false);
        }
        else
            ExitTv();
        CanActivate = true;
    }
    public override void Failure()
    {
        if (GameFlowHandler.isInDreamSequence)
        {
            GameFlowHandler.instance.FinishMinigame(this, true);
        }
    }
}