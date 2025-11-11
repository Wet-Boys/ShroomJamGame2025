using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    Time12Am,
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
        ResetPlayerPosition();
        switch (_currentTime)
        {
            case CurrentTime.Time8Am:
                PlayerController.instance.visualHandler.CoughingBaby();
                SetObjectiveText("Put on the cool hole in the wall show");
                LoadScene(CurrentTime.Time10Am);
                break;
            case CurrentTime.Time10Am:
                SetObjectiveText("Cool ads");
                //TODO Don't load frogger yet, have ads play after hole in the wall
                //TODO allow player to interact with the tv to switch to frogger
                LoadScene(CurrentTime.Time12Pm);
                break;
            case CurrentTime.Time12Pm:
                //TODO require player to grab an item before re-entering tv
                LoadScene(CurrentTime.Time3Pm);
                break;
            case CurrentTime.Time3Pm:
                //TODO don't load the obby yet
                //TODO have the tv interact be to switch it to the channel
                LoadScene(CurrentTime.Time6Pm);
                break;
            case CurrentTime.Time6Pm:
                //TODO switch this load scene to a segment to turn the TV off, then have the player interact with the couch
                LoadScene(CurrentTime.Time12Am);
                break;
            case CurrentTime.RandomMinigame:
                LoadScene(CurrentTime.RandomMinigame);
                //TODO check if out of lives, then LoadScene(CurrentTime.Victory);
                break;
            default:
                break;
        }
    }
    private void TvInteracted()
    {
        switch (_currentTime)
        {
            case CurrentTime.Time8Am:
                EnterTv();
                break;
            case CurrentTime.Time10Am:
                if (_canEnterTv)
                {
                    ((HoleInTheWallGame)_currentGame).SetOrderAndStart([0,1]);
                    EnterTv();
                }
                else
                {
                    SetCurrentGame(_currentTime);
                    SetObjectiveText("Watch the show");
                }
                break;
            case CurrentTime.Time12Pm:
                if (_canEnterTv)
                {
                    EnterTv();
                    _canEnterTv = true;
                }
                else
                {
                    SetCurrentGame(_currentTime);
                    SetObjectiveText("Play the game");
                }
                break;
            case CurrentTime.Time3Pm:
                EnterTv();
                break;
            case CurrentTime.Time6Pm:
                if (_canEnterTv)
                {
                    ((ObbyGameState)_currentGame).SpawnLevel(0);
                    EnterTv();
                }
                else
                {
                    SetCurrentGame(_currentTime);
                    SetObjectiveText("Watch the show");
                }
                break;
            case CurrentTime.Time12Am:
                ResetPlayerPosition();
                PlayerController.instance.RotationDegrees = new Vector3(0, 0, 0);
                PlayerController.instance.visualHandler.Succ();
                PlayerController.instance.visualHandler.animationTree.AnimationFinished += AnimationTreeOnAnimationFinished;
                break;
            case CurrentTime.Victory:
                break;
            case CurrentTime.RandomMinigame:
                EnterTv();
                break;
        }
    }

    private void AnimationTreeOnAnimationFinished(StringName animName)
    {
        PlayerController.instance.visualHandler.animationTree.AnimationFinished -= AnimationTreeOnAnimationFinished;
        ResetPlayerPosition();
        LoadScene(CurrentTime.RandomMinigame);
    }

    private static void ResetPlayerPosition()
    {
        PlayerController.instance.Position = new Vector3(1.7f, 0.282f, 2.486f);
        PlayerController.instance.RotationDegrees = new Vector3(0, 180, 0);
        PlayerController.instance.headNode.GetParentNode3D().RotationDegrees = new Vector3(0, 180, 0);
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
            holeInTheWallGame.CanActivate = true;
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
        TvInteractable.instance.Interacted += TvInteracted;
        PlayerController.instance.GetNode<Label>("%TaskLabel").Text = "";
        LoadScene(CurrentTime.Time8Am);
        SetCurrentGame(_currentTime, true);
        TvController.instance.ExitTv += ExitTv;
    }

    private async void SetCurrentGame(CurrentTime gameTime, bool forceInteract = false)
    {
        _canEnterTv = true;
        _currentGame?.QueueFree();
        _currentGame = (Node3D)TvController.instance.SetTvSubWorld(tvGameList[gameTime]);
        ((BaseTvGameState)_currentGame).CanActivate = true;
        if (forceInteract)
        {
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            TvInteractable.instance.OnInteract(PlayerController.instance);
        }
    }

    private bool _canEnterTv = false;
    private void EnterTv()
    {
        _canEnterTv = false;
        var tvGameState = PlayerController.instance.AllPlayerStates.tvGame;
        
        tvGameState.tvController = TvInteractable.instance.TvController;
        
        PlayerController.instance.CurrentState = tvGameState;
    }

    public async void LoadScene(CurrentTime currentTime)
    {
        _currentTime = currentTime;

        if (currentTime == CurrentTime.RandomMinigame)
        {
            await RandomMicroGame();
        }
        else if (sceneList.TryGetValue(currentTime, out PackedScene overworldScene))
        {
            _currentScene?.QueueFree();
            _currentScene = overworldScene.Instantiate<Node3D>();
            GetParent().AddChild(_currentScene);
        }
    }

    private async Task RandomMicroGame()
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
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        SetupRandomGame();
        TvInteractable.instance.OnInteract(PlayerController.instance);
    }
}