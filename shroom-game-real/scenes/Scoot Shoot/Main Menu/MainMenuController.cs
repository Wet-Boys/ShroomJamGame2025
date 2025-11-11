using Godot;

namespace ShroomGameReal.scenes.Scoot_Shoot.Main_Menu;

[GlobalClass]
public partial class MainMenuController : Control
{
    private ScootShootOnRailsGame _game;

    public override void _Ready()
    {
        _game = GetOwner<ScootShootOnRailsGame>();
        _game.mainMenu = this;
    }

    public void OnStartPressed()
    {
        Visible = false;
        _game.StartGame();
    }

    public void OnQuitPressed()
    {
        _game.ExitTv();
    }
}