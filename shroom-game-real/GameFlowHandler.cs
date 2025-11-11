using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using ShroomGameReal.Player;
using ShroomGameReal.scenes.HITW;
using ShroomGameReal.scenes.obby;
using ShroomGameReal.scenes.Scoot_Shoot;
using ShroomGameReal.Tv;
using ShroomGameReal.Tv.GameStates;
using Array = Godot.Collections.Array;

namespace ShroomGameReal;

public enum CurrentTime
{
    Time8Am,
    Time10Am,
    Time12Pm,
    Time3Pm,
    Time6Pm,
    Time9Pm,
    Time12Am,
    Time6Am,
    Victory,
    RandomMinigame,
}
[GlobalClass]
public partial class GameFlowHandler : Node
{
    public Godot.Collections.Dictionary<CurrentTime, PackedScene> sceneList = new()
    {
        { CurrentTime.Time8Am,  (PackedScene)GD.Load("res://scenes/8AM/8am.tscn")},//res://scenes/8AM/8AM.tscn
        { CurrentTime.Time10Am,  (PackedScene)GD.Load("res://scenes/10AM/10am.tscn")},
        { CurrentTime.Time12Pm,  (PackedScene)GD.Load("res://scenes/12PM/12pm.tscn")},
        { CurrentTime.Time3Pm,  (PackedScene)GD.Load("res://scenes/3PM/3pm.tscn")},
        { CurrentTime.Time6Pm,  (PackedScene)GD.Load("res://scenes/6PM/6pm.tscn")},
        { CurrentTime.Time12Am,  (PackedScene)GD.Load("res://scenes/12AM/12am.tscn")},
        { CurrentTime.Victory,  (PackedScene)GD.Load("res://scenes/8AM_Victory/8am_victory.tscn")},
    };
    public Godot.Collections.Dictionary<CurrentTime, PackedScene> tvGameList = new()
    {
        { CurrentTime.Time8Am,  (PackedScene)GD.Load("res://scenes/Scoot Shoot/Scoot Shoot Game.tscn")},
        { CurrentTime.Time10Am,  (PackedScene)GD.Load("res://scenes/HITW/Hole In The Wall Game.tscn")},
        { CurrentTime.Time12Pm,  (PackedScene)GD.Load("res://scenes/froggers/frogger.tscn")},
        { CurrentTime.Time6Pm,  (PackedScene)GD.Load("res://scenes/obby/obby.tscn")},
    };

    [Export]
    public Array<PackedScene> microGames;
    public List<int> lastMicrogames = new();
    private RandomNumberGenerator _rng = new();
    public static GameFlowHandler instance;
    private Node3D _currentScene;
    private Node3D _currentGame;
    private CurrentTime _currentTime;
    [Export] private Label _label;
    public override void _EnterTree()
    {
        instance = this;
    }

    public override void _Ready()
    {
        base._Ready();
        TestStart();
    }

    private void ExitTv()
    {
        switch (_currentTime)
        {
            case CurrentTime.Time8Am:
                PlayerController.instance.visualHandler.CoughingBaby();
                SetObjectiveText("Put on the cool hole in the wall show");
                LoadScene(CurrentTime.Time10Am);
                ((HoleInTheWallGame)_currentGame).SetOrderAndStart([0,1]);
                break;
            case CurrentTime.Time10Am:
                LoadScene(CurrentTime.Time12Pm);
                break;
            case CurrentTime.Time12Pm:
                LoadScene(CurrentTime.Time3Pm);
                break;
            case CurrentTime.Time3Pm:
                LoadScene(CurrentTime.Time6Pm);
                ((ObbyGameState)_currentGame).SpawnLevel(0);
                break;
            case CurrentTime.Time6Pm:
                LoadScene(CurrentTime.Time12Am);
                break;
            case CurrentTime.Time12Am:
                LoadScene(CurrentTime.RandomMinigame);
                SetupRandomGame();
                break;
            case CurrentTime.RandomMinigame:
                //TODO check if out of lives, then LoadScene(CurrentTime.Victory);
                LoadScene(CurrentTime.RandomMinigame, true);
                SetupRandomGame();
                break;
            default:
                break;
        }
    }

    private static void SetObjectiveText(string text)
    {
        PlayerController.instance.GetNode<Label>("%TaskLabel").Text = text;
    }

    private void SetupRandomGame()
    {
        if (_currentGame is ObbyGameState obbyGameState)
        {
            obbyGameState.SpawnLevel();
        }
        else if (_currentGame is HoleInTheWallGame holeInTheWallGame)
        {
            holeInTheWallGame.SetOrderAndStart([_rng.RandiRange(0, holeInTheWallGame.contestantPrefabs.Length-1)]);
        }
        else if (_currentGame is FroggerGameState froggerGameState)
        {
            froggerGameState.logResetPoint = .2f;
            froggerGameState.frog.Position += new Vector3(18, 0, 0);
        }
        else if (_currentGame is ScootShootOnRailsGame scootShootOnRailsGame)
        {
            scootShootOnRailsGame.PlaySingleStage(_rng.RandiRange(0, scootShootOnRailsGame.stages.Length - 1));
        }
    }

    public async void TestStart()
    {
        await ToSignal(GetTree(), "physics_frame");
        PlayerController.instance.GetNode<Label>("%TaskLabel").Text = "";
        LoadScene(CurrentTime.Time8Am, true);
        TvController.instance.ExitTv += ExitTv;
    }

    public async void LoadScene(CurrentTime currentTime, bool forceInteract = false)
    {
        _currentTime = currentTime;

        if (currentTime == CurrentTime.RandomMinigame)
        {
            _currentGame?.QueueFree();
            int randomGame = _rng.RandiRange(0, microGames.Count-1);
            while (lastMicrogames.Contains(randomGame))
            {
                randomGame = _rng.RandiRange(0, microGames.Count-1);
            }
            lastMicrogames.Add(randomGame);
            if (lastMicrogames.Count > 1)
            {
                lastMicrogames.RemoveAt(0);
            }
            _currentGame = (Node3D)TvController.instance.SetTvSubWorld(microGames[randomGame]);
            _label.Text = ((BaseTvGameState)_currentGame).infoText;
        }
        else
        {
            if (sceneList.TryGetValue(currentTime, out PackedScene overworldScene))
            {
                _currentScene?.QueueFree();
                _currentScene = overworldScene.Instantiate<Node3D>();
                GetParent().AddChild(_currentScene);
            }
        
            if (tvGameList.TryGetValue(currentTime, out var tvGameScene))
            {
                _currentGame?.QueueFree();
                _currentGame = (Node3D)TvController.instance.SetTvSubWorld(tvGameScene);
            }   
        }

        if (forceInteract)
        {
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            TvInteractable.instance.OnInteract(PlayerController.instance);
        }
    }
    
}