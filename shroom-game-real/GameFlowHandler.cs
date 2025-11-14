using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using ShroomGameReal.Music;
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
    Time12AmStatic,
    Victory,
    RandomMinigame,
}
[GlobalClass]
public partial class GameFlowHandler : Node
{
    public Godot.Collections.Dictionary<CurrentTime, PackedScene> sceneList = new()
    {
        { CurrentTime.Time8Am,  (PackedScene)GD.Load("res://scenes/8AM/8am.tscn")},
        { CurrentTime.Time10Am,  (PackedScene)GD.Load("res://scenes/10AM/10am.tscn")},
        { CurrentTime.Time12Pm,  (PackedScene)GD.Load("res://scenes/12PM/12pm.tscn")},
        { CurrentTime.Time3Pm,  (PackedScene)GD.Load("res://scenes/3PM/3pm.tscn")},
        { CurrentTime.Time6Pm,  (PackedScene)GD.Load("res://scenes/6PM/6pm.tscn")},
        { CurrentTime.Time12Am,  (PackedScene)GD.Load("res://scenes/12AM/12am.tscn")},
        { CurrentTime.Victory,  (PackedScene)GD.Load("res://scenes/8AM/8am.tscn")},
    };
    public Godot.Collections.Dictionary<CurrentTime, string> sceneNameList = new()
    {
        { CurrentTime.Time8Am,  "8am"},
        { CurrentTime.Time10Am, "10am"},
        { CurrentTime.Time12Pm, "12pm"},
        { CurrentTime.Time3Pm,  "3pm"},
        { CurrentTime.Time6Pm,  "6pm"},
        { CurrentTime.Time12Am, "12am"},
        { CurrentTime.Victory,  "8am_victory"},
    };
    public Godot.Collections.Dictionary<CurrentTime, PackedScene> tvGameList = new()
    {
        { CurrentTime.Time8Am,  (PackedScene)GD.Load("res://scenes/Scoot Shoot/Scoot Shoot Game.tscn")},
        { CurrentTime.Time10Am,  (PackedScene)GD.Load("res://scenes/HITW/Hole In The Wall Game.tscn")},
        { CurrentTime.Time3Pm,  (PackedScene)GD.Load("res://scenes/froggers/frogger.tscn")},
        { CurrentTime.Time6Pm,  (PackedScene)GD.Load("res://scenes/obby/obby.tscn")},
        { CurrentTime.Time12Pm,  (PackedScene)GD.Load("res://scenes/ads/ads.tscn")},
        { CurrentTime.Time12Am,  (PackedScene)GD.Load("res://scenes/off/off.tscn")},
        { CurrentTime.Time12AmStatic,  (PackedScene)GD.Load("res://scenes/off/static.tscn")},
    };

    private Godot.Collections.Dictionary<CurrentTime, Node3D> _loadedScenes = new();

    [Export]
    public Array<PackedScene> microGames;
    public List<int> lastMicrogames = new();
    private RandomNumberGenerator _rng = new();
    public static GameFlowHandler instance;
    private Node3D _currentScene;
    private Node3D _currentGame;
    public static CurrentTime currentTime;
    public static bool isInDreamSequence = false;
    [Export] private Label _label;
    private double _macrogameTimer = 0;
    [Export] private DreamTimer _dreamTimer;
    private bool _lowerIsBad;
    public static int completedDreamLevels = 0;
    private double _timerMultiplier = 1;
    [Export] public AudioStreamPlayer cough;
    [Export] public AudioStreamPlayer yawn;
    [Export] public AudioStreamPlayer3D tvSucc;
    [Export] public GpuParticles3D succParticles;

    public static int Lives
    {
        get => field;
        set
        {
            field = value;
            if (value == 0)
            {
                GD.Print("game over");
            }
        }
    }
    public override void _EnterTree()
    {
        instance = this;
    }

    public override void _Ready()
    {
        base._Ready();
        TestStart();
        Lives = 4;
    }

    private void ExitTv()
    {
        _foodHasWorkedThisTimeslot = false;
        ResetPlayerPosition();
        switch (currentTime)
        {
            case CurrentTime.Time8Am:
                MusicManager.Instance.StartIntroSong();
                PlayerController.instance.visualHandler.CoughingBaby();
                cough.Play();
                SetObjectiveText("Put on the game shows");
                TvInteractable.instance.InteractText = "Change Channel";
                LoadScene(CurrentTime.Time10Am);
                break;
            case CurrentTime.Time10Am:
                SetObjectiveText("Get some lunch");
                LoadScene(CurrentTime.Time12Pm);
                break;
            case CurrentTime.Time12Pm:
                MusicManager.Instance.UnDuckOverworldMusic();
                MusicManager.Instance.StartMiddleSong();
                SetObjectiveText("");
                LoadScene(CurrentTime.Time3Pm);
                SetCurrentGame(currentTime);
                TvInteractable.instance.InteractText = "Watch Channel";
                break;
            case CurrentTime.Time3Pm:
                LoadScene(CurrentTime.Time6Pm);
                break;
            case CurrentTime.Time6Pm:
                LoadScene(CurrentTime.Time12Am);
                SetObjectiveText("Turn off the TV and head to bed");
                TvInteractable.instance.InteractText = "Turn Off Tv";
                PlayerController.instance.visualHandler.Yawn();
                yawn.Play();
                MusicManager.Instance.DuckOverworldMusic();
                break;
            case CurrentTime.RandomMinigame:
                if (Lives != 0)
                {
                    completedDreamLevels++;
                    LoadScene(CurrentTime.RandomMinigame);
                    ChangeSpeed(((float)Engine.TimeScale) + .025f);
                }
                else
                {
                    EndDream();
                }
                break;
            default:
                break;
        }
    }

    private async void EndDream()
    {
        ChangeSpeed(1);
        SetCurrentGame(CurrentTime.Time12Am);
        ChangeSpeed(1);
        GD.Print($"Final score: {completedDreamLevels}");
        LoadScene(CurrentTime.Victory);
        PlayerController.instance.Position = new Vector3(0.879f, 8.3f, 7.5f);
        PlayerController.instance.RotationDegrees = new Vector3(0, 180, 0);
        PlayerController.instance.headNode.GetParentNode3D().RotationDegrees = new Vector3(0, 180, 0);
        _dreamTimer.HideIt();
        ChangeSpeed(1);
        PlayerController.instance.visualHandler.WakeUp();
        ChangeSpeed(1);
        await ToSignal(GetTree().CreateTimer(5f), "timeout");
        MusicManager.Instance.StartDreamSong();
        CreditsHandler.instance.SetCameraToSpot(1);
        await ToSignal(GetTree().CreateTimer(6f), "timeout");
        CreditsHandler.instance.SetCameraToSpot(2);
        await ToSignal(GetTree().CreateTimer(6f), "timeout");
        CreditsHandler.instance.SetCameraToSpot(3);
        await ToSignal(GetTree().CreateTimer(6f), "timeout");
        CreditsHandler.instance.SetCameraToSpot(4);
        await ToSignal(GetTree().CreateTimer(6f), "timeout");
        CreditsHandler.instance.SetCameraToSpot(5);
        await ToSignal(GetTree().CreateTimer(15f), "timeout");
        FadeOut.instance.FadeOutAnimation();
    }

    private bool _timeToSleep;
    private async void TvInteracted()
    {
        if (currentTime != CurrentTime.Time12Am)
        {
            TvInteractable.instance.click.Play();
        }
        else
        {
            TvInteractable.instance.shutdown.Play();
            TvInteractable.instance.InteractOverride = true;
        }
        switch (currentTime)
        {
            case CurrentTime.Time8Am:
                EnterTv();
                break;
            case CurrentTime.Time10Am:
                if (_canEnterTv)
                {
                    ((HoleInTheWallGame)_currentGame).SetOrderAndStart([0,1,2,3]);
                    EnterTv();
                }
                else
                {
                    SetObjectiveText("");
                    SetCurrentGame(currentTime);
                }
                break;
            case CurrentTime.Time12Pm:
                if (_canEnterTv)
                {
                    MusicManager.Instance.DuckOverworldMusic();
                    SetObjectiveText("Interact to stop watching");
                    EnterTv();
                    _canEnterTv = true;
                }
                else
                {
                    SetCurrentGame(currentTime);
                }
                break;
            case CurrentTime.Time3Pm:
                EnterTv();
                break;
            case CurrentTime.Time6Pm:
                if (_canEnterTv)
                {
                    EnterTv();
                }
                else
                {
                    SetCurrentGame(currentTime);
                }
                break;
            case CurrentTime.Time12Am:
                SetObjectiveText("Head to bed");
                SetCurrentGame(currentTime);
                _timeToSleep = true;
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
        PlayerController.instance.Position = new Vector3(0.879f, 8.3f, 4.5f);
        PlayerController.instance.RotationDegrees = new Vector3(0, 180, 0);
        PlayerController.instance.headNode.GetParentNode3D().RotationDegrees = new Vector3(0, 180, 0);
    }

    private static void SetObjectiveText(string text)
    {
        PlayerController.instance.GetNode<Label>("%TaskLabel").Text = text;
    }

    private void SetupRandomGame()
    {
        _lowerIsBad = true;
        _macrogameTimer = 10;
        DreamTransition.instance.text.Text = ((BaseTvGameState)_currentGame).infoText;
        DreamTransition.instance.text.PivotOffset = DreamTransition.instance.text.Size * .5f;
        _timerMultiplier = 1;
        if (_currentGame is ObbyGameState obbyGameState)
        {
            obbyGameState.SpawnLevel();
            _timerMultiplier = .5;
        }
        else if (_currentGame is HoleInTheWallGame holeInTheWallGame)
        {
            holeInTheWallGame.SetOrderAndStart([_rng.RandiRange(0, holeInTheWallGame.contestantPrefabs.Length-1)]);
            holeInTheWallGame.CanActivate = true;
            _timerMultiplier = .8;
        }
        else if (_currentGame is FroggerGameState froggerGameState)
        {
            froggerGameState.logResetPoint = .2f;
            froggerGameState.logSpeed = 1.5f;
            froggerGameState.frog.Position += new Vector3(18, 0, 0);
            _timerMultiplier = .75;
        }
        else if (_currentGame is ScootShootOnRailsGame scootShootOnRailsGame)
        {
            scootShootOnRailsGame.PlaySingleStage(_rng.RandiRange(0, scootShootOnRailsGame.stages.Length - 1));
            _timerMultiplier = 1.25;
            _lowerIsBad = false;
        }
        else if (_currentGame is AvoidBallsGameState or FireHopGameState)
        {
            _lowerIsBad = false;
        }

        transitioning = false;
    }

    public async void TestStart()
    {
        await ToSignal(GetTree(), "physics_frame");
        PreloadAllScenes();
        TvInteractable.instance.Interacted += TvInteracted;
        PlayerController.instance.GetNode<Label>("%TaskLabel").Text = "";
        LoadScene(CurrentTime.Time8Am);
        SetCurrentGame(currentTime, true);
        TvController.instance.ExitTv += ExitTv;
    }

    private async void SetCurrentGame(CurrentTime gameTime, bool forceInteract = false)
    {
        _canEnterTv = true;
        _currentGame?.QueueFree();
        _currentGame = (Node3D)TvController.instance.SetTvSubWorld(tvGameList[gameTime]);

        switch (gameTime)
        {
            case CurrentTime.Time8Am:
                TvInteractable.instance.InteractText = "Play Scoot Shoot";
                break;
            case CurrentTime.Time10Am:
                TvInteractable.instance.InteractText = "Watch Wall Cutter";
                break;
            case CurrentTime.Time12Pm:
                TvInteractable.instance.InteractText = "Watch Ad";
                break;
            case CurrentTime.Time3Pm:
                TvInteractable.instance.InteractText = "Play Frogger";
                break;
            case CurrentTime.Time6Pm:
                TvInteractable.instance.InteractText = "Watch Fall People";
                break;
            case CurrentTime.Time12Am:
                TvInteractable.instance.InteractText = "You shouldn't be able to read this";
                break;
            case CurrentTime.Time12AmStatic:
                break;
            case CurrentTime.Victory:
                break;
            case CurrentTime.RandomMinigame:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(gameTime), gameTime, null);
        }
        
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
        GameFlowHandler.currentTime = currentTime;

        if (currentTime == CurrentTime.RandomMinigame)
        {
            await RandomMicroGame();
        }
        else if (_loadedScenes.TryGetValue(currentTime, out var scene))
        {
            _currentScene?.QueueFree();
            _currentScene = scene;
            _currentScene.Visible = true;
            _currentScene.Position = new Vector3(0, 0, 0);
        }
    }

    private async Task RandomMicroGame()
    {
        await ToSignal(GetTree().CreateTimer(.3f), "timeout");
        _currentGame?.QueueFree();
        int randomGame = _rng.RandiRange(0, microGames.Count-1);
        while (lastMicrogames.Contains(randomGame))
        {
            randomGame = _rng.RandiRange(0, microGames.Count-1);
        }
        lastMicrogames.Add(randomGame);
        if (lastMicrogames.Count > 3)
        {
            lastMicrogames.RemoveAt(0);
        }
        _currentGame = (Node3D)TvController.instance.SetTvSubWorld(microGames[randomGame]);
        _label.Text = "";
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        SetupRandomGame();
        TvInteractable.instance.OnInteract(PlayerController.instance);
    }

    private bool _foodHasWorkedThisTimeslot;
    public void FoodInteract(FoodInteractable food)
    {
        PlayerController.instance.visualHandler.Eat();
        if (!_foodHasWorkedThisTimeslot)
        {
            _foodHasWorkedThisTimeslot = true;
            SetCurrentGame(currentTime);
            if (currentTime == CurrentTime.Time6Pm)
            {
                ((ObbyGameState)_currentGame).SpawnLevel(0);
            }
        }
        food.Visible = false;
        SetObjectiveText("");
    }

    public static bool transitioning;
    public async void FinishMinigame(BaseTvGameState gameState, bool failure)
    {
        if (transitioning)
            return;
        transitioning = true;
        if (failure)
        {
            Lives--;
            MusicManager.Instance.WarbleDreamMusic();
        }

        if (Lives != 0)
        {
            DreamTransition.instance.Play();
            await ToSignal(GetTree().CreateTimer(.5f), "timeout");
        }
        else
        {
            MusicManager.Instance.StopDreamMusic();
        }
        gameState.ExitTv();
        gameState.CanActivate = false;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (_macrogameTimer > 0 && !transitioning)
        {
            _dreamTimer.SetProgressBar(_macrogameTimer, 10, _lowerIsBad);
            _macrogameTimer -= delta * _timerMultiplier;
            if (_macrogameTimer < 0)
            {
                if (_lowerIsBad)
                {
                    ((BaseTvGameState)_currentGame).Failure();
                }
                else
                {
                    FinishMinigame((BaseTvGameState)_currentGame, false);
                }
            }
        }

        if (_timeToSleep && PlayerController.instance.Position.DistanceTo(new Vector3(2.725f, 6.962f, 5.685f)) < 1.5f)
        {
            
            BeginDream();
        }
    }

    private async void BeginDream()
    {
        SetCurrentGame(CurrentTime.Time12AmStatic);
        _timeToSleep = false;
        ResetPlayerPosition();
        PlayerController.instance.Position = new Vector3(0.879f, 8.3f, 7.5f);
        // PlayerController.instance.RotationDegrees = new Vector3(0, 0, 0);
        SetObjectiveText("");
        PlayerController.instance.visualHandler.Succ();
        TvInteractable.instance.InteractOverride = false;
        tvSucc.Play();
        succParticles.Emitting = true;
        TvInteractable.instance.staticNoise.Play();
        isInDreamSequence = true;
        PlayerController.instance.visualHandler.animationTree.AnimationFinished += AnimationTreeOnAnimationFinished;
        MusicManager.Instance.StartDreamSong();
        await ToSignal(GetTree().CreateTimer(7.85f), "timeout");
        DreamTransition.instance.Play();
        tvSucc.Stop();
        succParticles.Emitting = false;
    }

    public async void PreloadAllScenes()
    {
        foreach (var scene in sceneList.Keys)
        {
            var newScene = sceneList[scene].Instantiate<Node3D>();
            GetParent().AddChild(newScene);

            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            newScene.Visible = false;
            newScene.Position = new Vector3(100, 0, 0);
            _loadedScenes.Add(scene, newScene);
        }
    }

    private void ChangeSpeed(float newSpeed)
    {
        MusicManager.Instance.PlaybackRate = newSpeed;
        Engine.TimeScale = newSpeed;
    }
}