using Godot;
using ShroomGameReal.Utilities;

namespace ShroomGameReal.Ui.PauseMenu;

[GlobalClass]
public partial class PauseMenuController : Control
{
    public Button baitQuitButton;
    private Button _quitButton;
    private Control _vhsEffect;
    private Control _buttons;
    private Control _settingsUi;
    
    public override void _Ready()
    {
        baitQuitButton = GetNode<Button>("%Bait Quit Button");
        _quitButton = GetNode<Button>("%Quit Button");
        _vhsEffect = GetNode<Control>("%VHS");
        _buttons = GetNode<Control>("%Buttons");
        _settingsUi = GetNode<Control>("%Settings Menu");
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("escape") && !GlobalGameState.Instance.InBaitMode && !_settingsUi.Visible)
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

    public void ShowSettings()
    {
        _buttons.Visible = false;
        _settingsUi.Visible = true;
        
        MouseReleaser.Instance.RequestFreeMouse();
    }

    public void HideSettings()
    {
        _settingsUi.Visible = false;
        _buttons.Visible = true;
        
        MouseReleaser.Instance.RequestLockedMouse();
    }

    public void Pause()
    {
        if (GlobalGameState.Instance.InBaitMode)
        {
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
        
        GlobalGameState.Instance.GameTimeScale = 0f;
        Visible = true;
        MouseReleaser.Instance.RequestFreeMouse();
    }

    public void Resume()
    {
        Visible = false;
        MouseReleaser.Instance.RequestLockedMouse();
        GlobalGameState.Instance.GameTimeScale = 1f;
        
        if (!GlobalGameState.Instance.InBaitMode)
        {
            GlobalGameState.Instance.MainTimeScale = 1f;
        }
    }

    public void QuitToDesktop()
    {
        GetTree().Quit();
    }
}