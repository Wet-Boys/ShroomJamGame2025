using Godot;
using System;

public partial class DreamTransition : Control
{
    [Export] private AnimatedSprite2D _background;
    [Export] private Label _text;
    [Export] private AnimationPlayer _animator;

    public override void _Ready()
    {
        base._Ready();
        _background.Play();
        _text.Text = "Amongus";
        _animator.Play("Transition");
        _animator.Stop();
    }
}
