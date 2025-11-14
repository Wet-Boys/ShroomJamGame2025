using Godot;
using System;
using Godot.Collections;
using ShroomGameReal;
using ShroomGameReal.Player;
using Array = Godot.Collections.Array;

public partial class CreditsHandler : Label3D
{
    [Export] private PathFollow3D _pathFollow3D;
    [Export] private Camera3D _camera;
    [Export] private Node3D _littleJohn;
    [Export] private Array<Label3D> _credits;
    public static CreditsHandler instance;
    private int _currentSegment = 0;
    private bool _finalSegment = false;

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
        switch (_currentSegment)
        {
            case 1:
                _pathFollow3D.Progress = Mathf.Lerp(_pathFollow3D.Progress, 0f, (float)delta);
                break;
            case 2:
                _pathFollow3D.Progress = Mathf.Lerp(_pathFollow3D.Progress, 10.88f, (float)delta);
                break;
            case 3:
                _pathFollow3D.Progress = Mathf.Lerp(_pathFollow3D.Progress, 21.23f, (float)delta);
                break;
            case 4:
                _pathFollow3D.Progress = Mathf.Lerp(_pathFollow3D.Progress, 43.41f, (float)delta);
                break;
            case 5:
                _pathFollow3D.Progress = Mathf.Lerp(_pathFollow3D.Progress, 70f, (float)delta * .33f);
                break;
        }
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
        _credits[4].Visible = false;

        _currentSegment = spot;
        if (spot == 1)
        {
            _pathFollow3D.Progress = 0;
        }
        _camera.RotationDegrees = new Vector3(0, 90, 0);
        if (spot == 5)
        {
            _finalSegment = true;
            _camera.RotationDegrees = new Vector3(0, 180, 0);
            for (int i = 0; i < 4; i++)
            {
                _credits[i].Visible = false;
            }

            _credits[4].Visible = true;
            _credits[4].Text = $"Dream sequence score: {GameFlowHandler.completedDreamLevels}";
        }
    }
}
