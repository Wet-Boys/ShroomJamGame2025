using Godot;

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
    
    [ExportGroup("Tracks")]
    [Export]
    public AudioStreamSynchronized thirdStoryCuldesac;
    
    [Export]
    public AudioStreamSynchronized postMeridiem;
    
    [Export]
    public AudioStreamSynchronized cathodeStar;
    
    private AudioStreamPlayer _introPlayer;
    private AudioStreamPlayer _middlePlayer;

    private bool _introRepeatLock;
    private bool _middleRepeatLock;

    public override void _Ready()
    {
        _introPlayer = GetNode<AudioStreamPlayer>("%3rd Story Culdesac");
        _middlePlayer = GetNode<AudioStreamPlayer>("%Post Meridiem");

        _middlePlayer.Finished += () => _middlePlayer.Play();
        
        if (Engine.IsEditorHint())
            return;
        
        StartIntroSong();
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
            if (eventKey is { Keycode: Key.Key2, Pressed: true } && _isIntroSong)
            {
                StartMiddleSong();
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

    private void UpdatePitchShift()
    {
        var pitchShift = GetMusicBusEffect<AudioEffectPitchShift>(MusicBusEffects.PitchShift);
        pitchShift.PitchScale = 1f / PlaybackRate;
        
        _introPlayer.PitchScale = PlaybackRate;
        _middlePlayer.PitchScale = PlaybackRate;
    }

    private AudioEffect GetMusicBusEffect(MusicBusEffects effect) => Server.GetBusEffect(MusicBusIndex, (int)effect);
    private T GetMusicBusEffect<T>(MusicBusEffects effect)
        where T : AudioEffect
        => (T)Server.GetBusEffect(MusicBusIndex, (int)effect);

    public int MusicBusIndex => Server.GetBusIndex("Music");
    
    public int IntroBusIndex => Server.GetBusIndex("Overworld Intro Music");
    
    public int MiddleBusIndex => Server.GetBusIndex("Overworld Middle Music");
    
    public enum MusicBusEffects
    {
        PitchShift,
    }
}