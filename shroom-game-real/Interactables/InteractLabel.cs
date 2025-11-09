using Godot;

namespace ShroomGameReal.Interactables;

public partial class InteractLabel : PanelContainer
{
    [Export]
    public float padding = 24f;
    
    private Label _textLabel;
    
    public override void _Ready()
    {
        _textLabel = GetNode<Label>("%Interact Label Text");
        UpdatePivot();
    }

    public void SetText(string text)
    {
        _textLabel.Text = text;
        UpdatePivot();
    }   

    public void SetLabelPosition(Vector2 position)
    {
        GlobalPosition = position - Size / 2f;
        UpdatePivot();
    }

    public void UpdatePivot()
    {
        PivotOffset = Size / 2f;
    }
}