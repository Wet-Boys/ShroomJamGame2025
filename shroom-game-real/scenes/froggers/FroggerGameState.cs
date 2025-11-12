using Godot;
using System;
using System.Collections.Generic;
using Godot.Collections;
using ShroomGameReal;
using ShroomGameReal.Tv.GameStates;
using Array = Godot.Collections.Array;

public partial class FroggerGameState : BaseTvGameState
{
    private float _carTimer;
    private float _logTimer;
    public float logResetPoint = 0;
    public float logSpeed = 1;
    public int[] carRows = [-22, -20, -18, -16, -14, -12, -10, -8];//8 rows -22 to -8
    public int[] logRows = [-4, -2, 0, 2, 4, 6, 8, 10, 12, 14];//10 rows -4 to 14

    private List<int> _prevCarRows = new();

    private List<int> _prevLogRows = new();
    //spawn at z +- 19
    private RandomNumberGenerator _rng = new();
    [Export]
    private PackedScene _carScene;
    
    [Export]
    private Array<PackedScene> _logScenes;

    [Export] public Node3D frog;

    public override void _Ready()
    {
        base._Ready();
        CanActivate = true;
        infoText = "Get to the Other Side!";
    }

    public override void OnEnterState()
    {
        IsActive = true;
    }
    public override void _Process(double delta)
    {
        base._Process(delta);
        _carTimer += (float)delta * GlobalGameState.Instance.GameTimeScale;
        _logTimer += (float)delta * GlobalGameState.Instance.GameTimeScale;
        if (_carTimer > .3f)
        {
            _carTimer = 0;
            int carRow = _rng.RandiRange(0, carRows.Length - 1);
            while (_prevCarRows.Contains(carRow))
            {
                carRow = _rng.RandiRange(0, carRows.Length - 1);
            }
            _prevCarRows.Add(carRow);
            if (_prevCarRows.Count > 4)
            {
                _prevCarRows.RemoveAt(0);
            }
            var newCar = _carScene.Instantiate<CarLogMover>();
            AddChild(newCar);
            bool onRight = carRow % 2 == 0;
            newCar.Position = new Vector3(carRows[carRow], 2, onRight ? 19 : -19);
            newCar.speed = onRight ? -1 : 1;
            newCar.speed *= carRow + 4;
;        }

        if (_logTimer > .37f)
        {
            _logTimer = logResetPoint;
            int logRow = _rng.RandiRange(0, logRows.Length - 1);
            while (_prevLogRows.Contains(logRow))
            {
                logRow = _rng.RandiRange(0, carRows.Length - 1);
            }
            _prevLogRows.Add(logRow);
            if (_prevLogRows.Count > 7)
            {
                _prevLogRows.RemoveAt(0);
            }
            var newLog = _logScenes.PickRandom().Instantiate<CarLogMover>();
            AddChild(newLog);
            bool onRight = logRow % 2 == 0;
            newLog.Position = new Vector3(logRows[logRow], 0, onRight ? 19 : -19);
            newLog.speed = onRight ? -1 : 1;
            newLog.speed *= logRow + 3 * logSpeed;
        }

        if (frog.Position.X > 14)
        {
            if (GameFlowHandler.isInDreamSequence)
            {
                GameFlowHandler.instance.FinishMinigame(this, false);
            }
            else
                ExitTv();
            CanActivate = true;
        }
    }
    public override void Failure()
    {
        if (GameFlowHandler.isInDreamSequence)
        {
            GameFlowHandler.instance.FinishMinigame(this, true);
        }
    }
}
