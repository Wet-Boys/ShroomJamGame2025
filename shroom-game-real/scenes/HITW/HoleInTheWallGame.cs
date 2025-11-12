using System.Collections.Generic;
using Godot;
using ShroomGameReal.scenes.HITW.Drawing;
using ShroomGameReal.Tv.GameStates;
using ShroomGameReal.Utilities;

namespace ShroomGameReal.scenes.HITW;

[GlobalClass]
public partial class HoleInTheWallGame : BaseTvGameState
{
    [Export]
    public Vector2I imageSize;
    
    [Export]
    public float budgetMultiplier = 1.25f;

    [Export]
    public float restartOnLoseTime = 4f;
    
    [Export]
    public float nextContestantTime = 4f;
    
    [Export]
    public PackedScene[] contestantPrefabs = [];

    private MouseDraw _mouseDraw;
    private MeshInstance3D _wallQuad;
    private Material _wallCutOutMaterial = ResourceLoader.Load<Material>("res://scenes/HITW/Drawing/Draw Cut Overlay.tres");
    private RigidBody3D _contestantBody;
    private Node3D _contestantRoot;
    private TvGameTimer _gameTimer;
    private Camera3D _wallCamera;
    private Vector3 _wallStartPosition;
    private Node3D _contestantSpawn;
    private int _imagesReady;
    private Tween _wallTween;

    private HitwContestant _currentContestant;
    
    private int _gameProgression;
    public List<int> gameOrder = new();
    
    public override void _Ready()
    {
        _mouseDraw = GetNode<MouseDraw>("%MouseDraw");
        _wallQuad = GetNode<MeshInstance3D>("%Wall Quad");
        _contestantBody = GetNode<RigidBody3D>("%Contestant Body");
        _contestantRoot = GetNode<Node3D>("%Contestant Root");
        _gameTimer = GetNode<TvGameTimer>("%Completion Timer");
        _wallCamera = GetNode<Camera3D>("%HITW Camera");
        _contestantSpawn = GetNode<Node3D>("%Contestant Spawn");

        _wallStartPosition = _wallCamera.GlobalPosition;

        _gameTimer.Timeout += CutOutTimeDone;
        
        _wallCutOutMaterial.Set("shader_parameter/cut_out", _mouseDraw.ImageTexture);
        infoText = "Carve Out!";
    }

    public void SetOrderAndStart(List<int> gameOrder)
    {
        this.gameOrder = gameOrder;
        StartNextRound();
    }

    public bool StartNextRound()
    {
        if (gameOrder.Count != 0)
        {
            SetContestant(contestantPrefabs[gameOrder[0]]);
            gameOrder.RemoveAt(0);
            return true;
        }
        return false;
    }

    public override void OnEnterState()
    {
        MouseReleaser.Instance.SetLockedMode(Input.MouseModeEnum.Confined);
        IsActive = true;
        
        StartGame();
    }

    public void StartGame()
    {
        _wallTween?.Kill();
        _wallCamera.GlobalPosition = _wallStartPosition;

        _wallTween = CreateTween();
        
        _gameTimer.Start();
        _wallTween.TweenProperty(_wallCamera, "position", _wallStartPosition - new Vector3(0, 0, 10f), _currentContestant.timeToComplete + 3f);

        _mouseDraw.ResetImage();
        _mouseDraw.CanDraw = true;
    }

    public override void _Process(double delta)
    {
        _wallTween?.SetSpeedScale(GlobalGameState.Instance.GameTimeScale);
    }

    public void SetContestant(PackedScene contestantPrefab)
    {
        _contestantBody.Freeze = true;
        _contestantBody.AngularVelocity = Vector3.Zero;
        _contestantBody.LinearVelocity = Vector3.Zero;
        _contestantBody.Visible = false;

        foreach (var child in _contestantRoot.GetChildren())
            child.QueueFree();
        
        _wallCutOutMaterial.Set("shader_parameter/show_mistakes", false);

        _contestantBody.GlobalTransform = _contestantSpawn.GlobalTransform;
        
        
        _currentContestant = contestantPrefab.Instantiate<HitwContestant>();
        _contestantRoot.AddChild(_currentContestant);
        
        _currentContestant.ImageReady += ShowContestant;
        _gameTimer.WaitTime = _currentContestant.timeToComplete;
    }

    private void ShowContestant()
    {
        var budget = _currentContestant.NumberPixelsFilled * budgetMultiplier;
        _mouseDraw.maxBudget = Mathf.CeilToInt(budget);

        _wallCutOutMaterial.Set("shader_parameter/overlay_texture", ImageTexture.CreateFromImage(_currentContestant.ShapeImage));
        
        _contestantBody.Visible = true;
        CanActivate = true;
        _gameTimer.WaitTime = _currentContestant.timeToComplete;

        if (IsActive)
            StartGame();
    }

    public bool CanContestantFitCutShape()
    {
        var cutImage = _mouseDraw.ImageTexture.GetImage();
        return _currentContestant.CanFitThrough(cutImage);
    }

    private void CutOutTimeDone()
    {
        _mouseDraw.CanDraw = false;
        
        if (CanContestantFitCutShape())
        {
            // GD.Print("You won");

            _gameProgression++;

            var nextContestantTimer = new TvGameTimer();
            AddChild(nextContestantTimer);
            
            nextContestantTimer.WaitTime = nextContestantTime;
            nextContestantTimer.OneShot = true;
            nextContestantTimer.Timeout += () =>
            {
                if (!StartNextRound())
                {
                    GameWon();
                    return;
                }
                nextContestantTimer.QueueFree();
                StartGame();
            };

            nextContestantTimer.Start();
        }
        else
        {
            // GD.Print("You lost");
            if (GameFlowHandler.isInDreamSequence)
            {
                GameFlowHandler.instance.FailMinigame(this);
            }
            _wallCutOutMaterial.Set("shader_parameter/show_mistakes", true);
            _contestantBody.Freeze = false;

            var restartTimer = new TvGameTimer();
            AddChild(restartTimer);
            
            restartTimer.WaitTime = restartOnLoseTime;
            restartTimer.OneShot = true;
            restartTimer.Timeout += () =>
            {
                SetContestant(contestantPrefabs[_gameProgression]);
                restartTimer.QueueFree();
                StartGame();
            };
            
            restartTimer.Start();
        }
    }

    private void GameWon()
    {
        CanActivate = false;
        ExitTv();
    }
}