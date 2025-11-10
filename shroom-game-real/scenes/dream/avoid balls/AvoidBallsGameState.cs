using Godot;
using System;
using ShroomGameReal.Tv.GameStates;

public partial class AvoidBallsGameState : BaseTvGameState
{
    public float survivalDuration = 10;
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
        survivalDuration -= (float)delta;
        if (survivalDuration <= 0)
        {
            ExitTv();
            CanActivate = false;
        }
    }
}
