using Godot;

namespace ShroomGameReal.Player.PlayerStates;

[GlobalClass]
public partial class PlayerStateList : Node
{
    [Export]
    public FirstPersonOverworldPlayerState firstPersonOverworld;
    
    [Export]
    public TvGamePlayerState tvGame;
}