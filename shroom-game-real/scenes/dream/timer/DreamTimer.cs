using Godot;
using System;

public partial class DreamTimer : Control
{
    [Export] private TextureProgressBar _progressBar;
    public void SetProgressBar(double currentTime, double maxTime)
    {
        float progress = (float)(currentTime / maxTime);
        _progressBar.TintProgress = new Color(1 - progress, progress,0);
        _progressBar.Value = progress * 100;
    }
}
