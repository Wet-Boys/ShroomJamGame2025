using Godot;

namespace ShroomGameReal.Interactables;

public partial class InteractLabel : PanelContainer
{
    [Export]
    public float padding = 24f;

    private Label _textLabel;
    private Vector2 _pointFrom;
    private Vector2 _pointTo;
    
    public override void _Ready()
    {
        _textLabel = GetNode<Label>("%Interact Label Text");
        UpdatePivot();
    }

    public override void _Draw()
    {
        DrawLine(_pointFrom, _pointTo, Colors.White, 8f);
    }

    public void SetText(string text)
    {
        _textLabel.Size = Vector2.Zero;
        _textLabel.Text = text;
        UpdatePivot();
    }   

    public void SetLabelPosition(Vector2 position)
    {
        GlobalPosition = position - Size / 2f;
        UpdatePivot();
    }

    public void SetLinePoints(Vector2 from, Vector2 to)
    {
        var localFrom = from - GlobalPosition;
        var localTo = to - GlobalPosition;
        
        var dir = (localTo - localFrom).Normalized();

        _pointTo = localTo;
        _pointFrom = (Size / 2f) + dir * (Size / 2f);
        
        
        QueueRedraw();
    }

    public void UpdatePivot()
    {
        PivotOffset = Size / 2f;
    }
}