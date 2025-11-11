using Godot;
using System;

public partial class DreamTransition : Control
{
    [Export] private AnimatedSprite2D _background;
    [Export] private Label _text;
    [Export] private AnimationPlayer _animator;
    public static DreamTransition instance;

    public override void _Ready()
    {
        base._Ready();
        _background.Play();
        _text.Text = "";
        _background.Visible = false;
        instance = this;
    }

    public void PlayWithText(string text)
    {
        _background.Visible = true;
        _text.Text = text;
        _animator.Play("Transition");
    }
}
