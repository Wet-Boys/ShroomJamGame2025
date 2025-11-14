using Godot;
using SettingsManager = ShroomGameReal.Utilities.Settings.SettingsManager;

namespace ShroomGameReal.Ui.PauseMenu;

[GlobalClass]
public partial class SettingsMenuController : Control
{
    private Slider _masterVolumeSlider;
    private Slider _musicVolumeSlider;
    private Slider _sfxVolumeSlider;
    
    private Slider _fovSlider;
    private Slider _horizontalSensitivitySlider;
    private Slider _verticalSensitivitySlider;
    
    private PauseMenuController _pauseMenuController;
    
    public override void _Ready()
    {
        _pauseMenuController = GetOwner<PauseMenuController>();
        
        _masterVolumeSlider = GetNode<Slider>("%Master Volume Slider");
        _masterVolumeSlider.Value = SettingsManager.Volume.Master.Value;
        
        _musicVolumeSlider = GetNode<Slider>("%Music Volume Slider");
        _musicVolumeSlider.Value = SettingsManager.Volume.Music.Value;
        
        _sfxVolumeSlider = GetNode<Slider>("%Sfx Volume Slider");
        _sfxVolumeSlider.Value = SettingsManager.Volume.Sfx.Value;
        
        _fovSlider = GetNode<Slider>("%Fov Slider");
        _fovSlider.MinValue = SettingsManager.Camera.Fov.Min;
        _fovSlider.MaxValue = SettingsManager.Camera.Fov.Max;
        _fovSlider.Value = SettingsManager.Camera.Fov.Value;
        
        _horizontalSensitivitySlider = GetNode<Slider>("%Horizontal Sensitivity Slider");
        _horizontalSensitivitySlider.MinValue = SettingsManager.Gameplay.Mouse.HorizontalSensitivity.Min;
        _horizontalSensitivitySlider.MaxValue = SettingsManager.Gameplay.Mouse.HorizontalSensitivity.Max;
        _horizontalSensitivitySlider.Value = SettingsManager.Gameplay.Mouse.HorizontalSensitivity.Value;
        
        _verticalSensitivitySlider = GetNode<Slider>("%Vertical Sensitivity Slider");
        _verticalSensitivitySlider.MinValue = SettingsManager.Gameplay.Mouse.VerticalSensitivity.Min;
        _verticalSensitivitySlider.MaxValue = SettingsManager.Gameplay.Mouse.VerticalSensitivity.Max;
        _verticalSensitivitySlider.Value = SettingsManager.Gameplay.Mouse.VerticalSensitivity.Value;
    }

    public void MasterVolumeChanged(float volume)
    {
        SettingsManager.Volume.Master.Value = volume;
    }

    public void MusicVolumeChanged(float volume)
    {
        SettingsManager.Volume.Music.Value = volume;
    }

    public void SfxVolumeChanged(float volume)
    {
        SettingsManager.Volume.Sfx.Value = volume;
    }

    public void FovChanged(float fov)
    {
        SettingsManager.Camera.Fov.Value = fov;
    }
    
    public void HorizontalSensitivityChanged(float value)
    {
        SettingsManager.Gameplay.Mouse.HorizontalSensitivity.Value = value;
    }

    public void VerticalSensitivityChanged(float value)
    {
        SettingsManager.Gameplay.Mouse.VerticalSensitivity.Value = value;
    }

    public void ToggleFullScreen()
    {
        var current = DisplayServer.WindowGetMode();
        if (current != DisplayServer.WindowMode.Fullscreen)
        {
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
        }
        else
        {
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
        }
    }

    public void Save()
    {
        SettingsManager.Instance.Save();
        _pauseMenuController.HideSettings();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("escape") && Visible)
        {
            Save();
        }
    }
}