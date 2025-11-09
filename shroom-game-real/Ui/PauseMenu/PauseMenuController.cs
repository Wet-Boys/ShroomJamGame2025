using Godot;
using ShroomGameReal.Utilities;

namespace ShroomGameReal.Ui.PauseMenu;

[GlobalClass]
public partial class PauseMenuController : Control
{
    public Button baitQuitButton;
    private Button _quitButton;
    private Control _vhsEffect;
    
    public override void _Ready()
    {
        baitQuitButton = GetNode<Button>("%Bait Quit Button");
        _quitButton = GetNode<Button>("%Quit Button");
        _vhsEffect = GetNode<Control>("%VHS");
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("escape") && !GlobalGameState.Instance.InBaitMode)
        {
            if (Visible)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Pause()
    {
        if (GlobalGameState.Instance.InBaitMode)
        {
            GlobalGameState.Instance.GameTimeScale = 0f;
            _vhsEffect.Visible = false;
            _quitButton.Visible = false;
            baitQuitButton.Visible = true;
        }
        else
        {
            GlobalGameState.Instance.MainTimeScale = 0f;
            _vhsEffect.Visible = true;
            _quitButton.Visible = true;
            baitQuitButton.Visible = false;
        }
        
        Visible = true;
        MouseReleaser.Instance.RequestFreeMouse();
    }

    public void Resume()
    {
        Visible = false;
        MouseReleaser.Instance.RequestLockedMouse();
        
        if (GlobalGameState.Instance.InBaitMode)
        {
            GlobalGameState.Instance.GameTimeScale = 1f;
        }
        else
        {
            GlobalGameState.Instance.MainTimeScale = 1f;
        }
    }
}