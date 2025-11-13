using Godot;
using System;
using Godot.Collections;
using ShroomGameReal.Player;
using Array = Godot.Collections.Array;

public partial class CreditsHandler : Label3D
{
    [Export] private PathFollow3D _pathFollow3D;
    [Export] private Camera3D _camera;
    [Export] private Node3D _littleJohn;
    [Export] private Array<Label3D> _credits;
    public static CreditsHandler instance;
    private float _desiredSpot = 0;
    private float _moveSpeed = 1;

    public override void _Ready()
    {
        base._Ready();
        instance = this;
        _littleJohn.Visible = false;
        foreach (var item in _credits)
        {
            item.Visible = false;
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        _pathFollow3D.Progress = Mathf.Lerp(_pathFollow3D.Progress, _desiredSpot, (float)delta) * _moveSpeed;
    }

    public void SetCameraToSpot(int spot)
    {
        PlayerController.instance.Visible = false;
        _littleJohn.Visible = true;
        _camera.Current = true;
        foreach (var item in _credits)
        {
            item.Visible = true;
        }
        switch (spot)
        {
            case 1:
                _desiredSpot = 0f;
                break;
            case 2:
                _desiredSpot = 10.88f;
                break;
            case 3:
                _desiredSpot = 21.23f;
                break;
            case 4:
                _desiredSpot = 43.41f;
                break;
            case 5:
                _camera.RotationDegrees = new Vector3(0, 180, 0);
                _desiredSpot = 70f;
                _moveSpeed = .33f;
                break;
        }
    }
}
