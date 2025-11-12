using Godot;
using System;
using ShroomGameReal;

public partial class DreamTransition : Control
{
    [Export] private AnimatedSprite2D _background;
    [Export] public Label text;
    [Export] private AnimationPlayer _animator;
    public static DreamTransition instance;
    [Export] public AnimatedSprite2D[] activeHealth;
    [Export] public AnimatedSprite2D[] dyingHealth;
    private int _prevHealth = 4;

    public override void _Ready()
    {
        base._Ready();
        _background.Play();
        text.Text = "";
        _background.Visible = false;
        instance = this;
        _animator.AnimationFinished += AnimFinished;
        for (int i = 0; i < 4; i++)
        {
            activeHealth[i].Visible = false;
            dyingHealth[i].Visible = false;
        }
    }

    private void AnimFinished(StringName animName)
    {
        GlobalGameState.Instance.MainTimeScale = 1;
    }

    public void Play()
    {
        GlobalGameState.Instance.MainTimeScale = 0;
        _background.Visible = true;
        _animator.Play("Transition");
        for (int i = 3; i > -1; i--)
        {
            if (GameFlowHandler.Lives >= i + 1)
            {
                activeHealth[i].Visible = true;
                activeHealth[i].Play();
                activeHealth[i].Frame = 0;
                dyingHealth[i].Visible = false;
            }
            else
            {
                activeHealth[i].Visible = false;
                dyingHealth[i].Play();
                dyingHealth[i].Frame = 0;
                dyingHealth[i].Visible = _prevHealth >= i + 1;
            }
        }
        _prevHealth = GameFlowHandler.Lives;
    }
}
