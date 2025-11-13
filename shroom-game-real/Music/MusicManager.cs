using Godot;
using SettingsHelper;

namespace ShroomGameReal.Music;

[GlobalClass]
public partial class MusicManager : Node
{
    public static MusicManager Instance { get; private set; }

    private AudioServerInstance Server => AudioServer.Singleton;

    [Export(PropertyHint.Range, "0.8,2.0")]
    public float PlaybackRate
    {
        get;
        set
        {
            field = value;
            UpdatePitchShift();
        }
    } = 1f;
    
    [Export]
    public double firstSongLoopEndOffset = 0;
    [Export]
    public double middleSongLoopEndOffset = 0;

    [Export]
    private bool _isIntroSong;
    [Export]
    private bool _isMiddleSong;
    [Export]
    private bool _isDreamSong;
    
    [ExportGroup("Tracks")]
    [Export]
    public AudioStreamSynchronized thirdStoryCuldesac;
    
    [Export]
    public AudioStreamSynchronized postMeridiem;
    
    [Export]
    public AudioStreamSynchronized cathodeStar;
    
    private AudioStreamPlayer _introPlayer;
    private AudioStreamPlayer _middlePlayer;
    private AudioStreamPlayer _dreamPlayer; 

    private bool _introRepeatLock;
    private bool _middleRepeatLock;

    public override void _Ready()
    {
        _introPlayer = GetNode<AudioStreamPlayer>("%3rd Story Culdesac");
        _middlePlayer = GetNode<AudioStreamPlayer>("%Post Meridiem");
        _dreamPlayer = GetNode<AudioStreamPlayer>("%Cathode Star");

        _middlePlayer.Finished += () => _middlePlayer.Play();
        _dreamPlayer.Finished += () => _dreamPlayer.Play();
    }

    private void LoadAudioSettings()
    {
        Server.SetBusVolumeLinear(MasterBusIndex, SettingsManager.Volume.Master.Value);
        SettingsManager.Volume.Master.Changed += () => Server.SetBusVolumeLinear(MasterBusIndex, SettingsManager.Volume.Master.Value);
        
        Server.SetBusVolumeLinear(MusicBusIndex, SettingsManager.Volume.Music.Value);
        SettingsManager.Volume.Music.Changed += () => Server.SetBusVolumeLinear(MusicBusIndex, SettingsManager.Volume.Music.Value);
        
        Server.SetBusVolumeLinear(SfxBusIndex, SettingsManager.Volume.Sfx.Value);
        SettingsManager.Volume.Sfx.Changed += () => Server.SetBusVolumeLinear(SfxBusIndex, SettingsManager.Volume.Sfx.Value);
    }

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _Process(double delta)
    {
        if (_isIntroSong)
        {
            var currentTime = _introPlayer.GetPlaybackPosition() + Server.GetTimeSinceLastMix();
            currentTime -= Server.GetOutputLatency();

            if (currentTime > firstSongLoopEndOffset && !_introRepeatLock)
            {
                _introRepeatLock = true;
                _introPlayer.Play();
            }
            else if (currentTime < firstSongLoopEndOffset)
            {
                _introRepeatLock = false;
            }
        }
        else if (_isMiddleSong)
        {
            var currentTime = _middlePlayer.GetPlaybackPosition() + Server.GetTimeSinceLastMix();
            currentTime -= Server.GetOutputLatency();

            if (currentTime > middleSongLoopEndOffset && !_middleRepeatLock)
            {
                _middleRepeatLock = true;
                _middlePlayer.Play();
            }
            else if (currentTime < middleSongLoopEndOffset)
            {
                _middleRepeatLock = false;
            }
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey eventKey)
        {
            if (eventKey is { Keycode: Key.Key2, Pressed: true } && !_isMiddleSong)
            {
                StartMiddleSong();
            }
            
            if (eventKey is { Keycode: Key.Key3, Pressed: true } && !_isDreamSong)
            {
                StartDreamSong();
            }
        }
    }

    public void StartIntroSong()
    {
        _isIntroSong = true;
        _introPlayer.Play();
        _middlePlayer.Play((float)(_introPlayer.GetPlaybackPosition() + Server.GetTimeSinceLastMix()));
    }

    public void StartMiddleSong()
    {
        _isIntroSong = false;
        _isMiddleSong = true;

        var introTween = CreateTween();
        
        Server.SetBusMute(MiddleBusIndex, false);
        
        introTween.TweenMethod(Callable.From<float>(value =>
        {
            Server.SetBusVolumeDb(IntroBusIndex, value);
        }), 0.0f, -40.0f, 0.5f);
        introTween.Parallel().TweenMethod(Callable.From<float>(value =>
        {
            Server.SetBusVolumeDb(MiddleBusIndex, value);
        }), -40.0f, 0.0f, 0.5f);
        
        introTween.TweenCallback(Callable.From(() => Server.SetBusMute(IntroBusIndex, true)));
    }

    public void StartDreamSong()
    {
        _isIntroSong = false;
        _isMiddleSong = false;
        _isDreamSong = true;
        
        var dreamTween = CreateTween();
        
        Server.SetBusMute(DreamBusIndex, false);
        
        dreamTween.TweenMethod(Callable.From<float>(value =>
        {
            Server.SetBusVolumeDb(MiddleBusIndex, value);
        }), 0.0f, -40.0f, 0.5f);
        dreamTween.Parallel().TweenMethod(Callable.From<float>(value =>
        {
            Server.SetBusVolumeDb(DreamBusIndex, value);
        }), -40.0f, 0.0f, 0.5f);
        
        dreamTween.TweenCallback(Callable.From(() =>
        {
            Server.SetBusMute(IntroBusIndex, true);
            Server.SetBusMute(MiddleBusIndex, true);
            
            _introPlayer.Stop();
            _middlePlayer.Stop();
        }));

        // UnDuckOverworldMusic();
        _dreamPlayer.Play();
    }

    public void DuckOverworldMusic()
    {
        var duckTween = CreateTween().SetParallel();
        
        if (_isIntroSong)
        {
            duckTween.TweenMethod(Callable.From<float>(value =>
            {
                Server.SetBusVolumeDb(IntroBusIndex, value);
            }), 0.0f, -40.0f, 0.5f);
            duckTween.TweenCallback(Callable.From(() => Server.SetBusMute(IntroBusIndex, true)));
        }

        if (_isMiddleSong)
        {
            duckTween.TweenMethod(Callable.From<float>(value =>
            {
                Server.SetBusVolumeDb(MiddleBusIndex, value);
            }), 0.0f, -40.0f, 0.5f);
            duckTween.TweenCallback(Callable.From(() => Server.SetBusMute(MiddleBusIndex, true)));
        }
        
        if (_isDreamSong)
        {
            duckTween.TweenMethod(Callable.From<float>(value =>
            {
                Server.SetBusVolumeDb(DreamBusIndex, value);
            }), 0.0f, -40.0f, 0.5f);
            duckTween.TweenCallback(Callable.From(() => Server.SetBusMute(MiddleBusIndex, true)));
        }
    }
    
    public void UnDuckOverworldMusic()
    {
        var duckTween = CreateTween().SetParallel();
        
        if (_isIntroSong)
        {
            duckTween.TweenMethod(Callable.From<float>(value =>
            {
                Server.SetBusVolumeDb(IntroBusIndex, value);
            }), -40.0f, 0.0f, 0.5f);
            duckTween.TweenCallback(Callable.From(() => Server.SetBusMute(IntroBusIndex, false)));
        }

        if (_isMiddleSong)
        {
            duckTween.TweenMethod(Callable.From<float>(value =>
            {
                Server.SetBusVolumeDb(MiddleBusIndex, value);
            }), -40.0f, 0.0f, 0.5f);
            duckTween.TweenCallback(Callable.From(() => Server.SetBusMute(MiddleBusIndex, false)));
        }
        
        if (_isDreamSong)
        {
            duckTween.TweenMethod(Callable.From<float>(value =>
            {
                Server.SetBusVolumeDb(DreamBusIndex, value);
            }), -40.0f, 0.0f, 0.5f);
            duckTween.TweenCallback(Callable.From(() => Server.SetBusMute(DreamBusIndex, false)));
        }
    }

    private void UpdatePitchShift()
    {
        var pitchShift = GetMusicBusEffect<AudioEffectPitchShift>(MusicBusEffects.PitchShift);
        pitchShift.PitchScale = 1f / PlaybackRate;
        
        _introPlayer.PitchScale = PlaybackRate;
        _middlePlayer.PitchScale = PlaybackRate;
        _dreamPlayer.PitchScale = PlaybackRate;
    }

    private AudioEffect GetMusicBusEffect(MusicBusEffects effect) => Server.GetBusEffect(MusicBusIndex, (int)effect);
    private T GetMusicBusEffect<T>(MusicBusEffects effect)
        where T : AudioEffect
        => (T)Server.GetBusEffect(MusicBusIndex, (int)effect);

    public int MasterBusIndex => Server.GetBusIndex("Master");
    public int MusicBusIndex => Server.GetBusIndex("Music");
    public int SfxBusIndex => Server.GetBusIndex("SFX");
    
    public int IntroBusIndex => Server.GetBusIndex("Overworld Intro Music");
    
    public int MiddleBusIndex => Server.GetBusIndex("Overworld Middle Music");
    
    public int DreamBusIndex => Server.GetBusIndex("Dream Sequence Music");
    
    public enum MusicBusEffects
    {
        PitchShift,
    }
}