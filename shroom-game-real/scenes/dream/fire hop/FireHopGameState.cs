using Godot;
using System;
using ShroomGameReal;
using ShroomGameReal.Tv.GameStates;

public partial class FireHopGameState : BaseTvGameState
{
    public override void _Ready()
    {
        base._Ready();
        CanActivate = true;
        infoText = "Jump Over!";
    }
    public override void OnEnterState()
    {
        IsActive = true;
    }
}
