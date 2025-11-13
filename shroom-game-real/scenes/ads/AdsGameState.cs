using Godot;
using System;
using ShroomGameReal.Tv.GameStates;

public partial class AdsGameState : BaseTvGameState
{
    [Export] private VideoStreamPlayer _vendingMachines;
    [Export] private VideoStreamPlayer _pain;
    [Export] private VideoStreamPlayer _kitchenGun;
    [Export] private bool _interactable;
    public override void _Ready()
    {
        base._Ready();
        CanActivate = _interactable;
        if (_vendingMachines is not null)
        {
            _vendingMachines.Finished += VendingMachinesOnFinished;
            _pain.Finished += PainOnFinished;
            _kitchenGun.Finished += KitchenGunOnFinished;   
        }
    }

    private void KitchenGunOnFinished()
    {
        _pain.Visible = true;
        _pain.Play();
        _kitchenGun.Visible = false;
    }

    private void PainOnFinished()
    {
        _vendingMachines.Visible = true;
        _vendingMachines.Play();
        _pain.Visible = false;
    }

    private void VendingMachinesOnFinished()
    {
        _kitchenGun.Visible = true;
        _kitchenGun.Play();
        _vendingMachines.Visible = false;
    }

    public override void OnEnterState()
    {
        IsActive = true;
    }
}
