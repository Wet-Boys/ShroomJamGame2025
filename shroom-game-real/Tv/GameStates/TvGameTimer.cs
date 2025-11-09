using Godot;

namespace ShroomGameReal.Tv.GameStates;

[GlobalClass]
public partial class TvGameTimer : Node
{
    [Export(PropertyHint.Range, "0.001,4096,0.001,suffix:s")]
    public double WaitTime { get; set; } = 1.0;

    [Export]
    public bool OneShot { get; set; }
    
    [Export]
    public bool AutoStart { get; set; }

    [Signal]
    public delegate void TimeoutEventHandler();

    private BaseTvGameState _gameState;
    private bool _timerActive;
    private double _timeLeft;

    public override void _Ready()
    {
        _gameState = (BaseTvGameState)GetViewport().GetChild(0);

        if (AutoStart)
            Start();
    }

    public void Start()
    {
        _timeLeft = WaitTime;
        _timerActive = true;
    }

    public void Stop()
    {
        _timerActive = false;
    }

    public override void _Process(double delta)
    {
        if (!_timerActive)
            return;
        
        var scaledDelta = delta * _gameState.TimeScale;
        
        _timeLeft -= scaledDelta;
        if (_timeLeft <= 0)
            TimerFinished();
    }

    private void TimerFinished()
    {
        _timerActive = false;
        
        EmitSignalTimeout();
        
        if (OneShot)
            return;

        _timeLeft += WaitTime;
    }
}