using Godot;
using System;

public partial class BombTimer : Control
{
    [Export] private AnimatedSprite2D _bombSprite;
    [Export] private AnimatedSprite2D _explosionSprite;

    public override void _Ready()
    {
        base._Ready();
        _bombSprite.AnimationFinished += BombSpriteOnAnimationFinished;
    }

    public void StartTimer(float duration)
    {
        _bombSprite.SpeedScale = 8.6666f / duration;
        _bombSprite.Frame = 0;
        _bombSprite.Play();
    }

    private void BombSpriteOnAnimationFinished()
    {
        _explosionSprite.Frame = 0;
        _explosionSprite.Play();
        GD.Print("Ran out of time");
        _explosionSprite.AnimationFinished += ExplosionSpriteOnAnimationFinished;
    }

    private void ExplosionSpriteOnAnimationFinished()
    {
        GD.Print("Explosion done");
    }
}
