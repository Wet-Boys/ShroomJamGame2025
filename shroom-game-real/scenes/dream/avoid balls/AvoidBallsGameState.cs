using Godot;
using System;
using ShroomGameReal;
using ShroomGameReal.Tv.GameStates;

public partial class AvoidBallsGameState : BaseTvGameState
{
    [Export] private AudioStreamPlayer _carCrash;
    public override void _Ready()
    {
        base._Ready();
        CanActivate = true;
        infoText = "Avoid!";
    }
    public override void OnEnterState()
    {
        IsActive = true;
    }
    public override void _Process(double delta)
    {
        base._Process(delta);
    }

    public override void Failure()
    {
        _carCrash.Play();
        if (GameFlowHandler.isInDreamSequence)
        {
            GameFlowHandler.instance.FinishMinigame(this, true);
        }
    }
}
