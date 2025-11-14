using Godot;
using System;
using System.Net.Mime;

public partial class FadeOut : Control
{
    [Export] private AnimationPlayer _animationPlayer;
    [Export] private Sprite2D _sprite;
    public static FadeOut instance;
    public override void _Ready()
    {
        base._Ready();
        _animationPlayer.AnimationFinished += AnimationPlayerOnAnimationFinished;
        instance = this;
        _sprite.Visible = false;
    }

    public void FadeOutAnimation()
    {
        _animationPlayer.Play("new_animation");
        _sprite.Visible = true;
    }

    private void AnimationPlayerOnAnimationFinished(StringName animName)
    {
        GetTree().Quit();
    }
}
