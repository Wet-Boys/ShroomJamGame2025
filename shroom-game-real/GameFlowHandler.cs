using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using ShroomGameReal.Player;
using ShroomGameReal.scenes.obby;
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
                LoadScene(CurrentTime.Time10Am);
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
                break;
            case CurrentTime.RandomMinigame:
                LoadScene(CurrentTime.RandomMinigame, false);
                if (_currentGame is ObbyGameState obbyGameState)
                {
                    obbyGameState.SpawnLevel();
                }
                //TODO check if out of lives, then LoadScene(CurrentTime.Victory);
                break;
            default:
                break;
        }
    }

    public async void TestStart()
    {
        await ToSignal(GetTree(), "physics_frame");
        LoadScene(CurrentTime.Time8Am, true);
        TvController.instance.ExitTv += ExitTv;
    }

    public void LoadScene(CurrentTime currentTime, bool forceInteract = false)
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
            TvInteractable.instance.OnInteract(PlayerController.instance);
        }
    }
}