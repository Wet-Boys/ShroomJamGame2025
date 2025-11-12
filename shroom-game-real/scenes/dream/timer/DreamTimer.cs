using Godot;
using System;

public partial class DreamTimer : Control
{
    [Export] private TextureProgressBar _progressBar;
    public void SetProgressBar(double currentTime, double maxTime, bool lowerIsBad)
    {
        float progress = (float)(currentTime / maxTime);
        if (lowerIsBad)
        {
            _progressBar.TintProgress = new Color(1 - progress, progress,0);
        }
        else
        {
            _progressBar.TintProgress = new Color(0, 1,0);
        }
        _progressBar.Value = progress * 100;
    }
}
