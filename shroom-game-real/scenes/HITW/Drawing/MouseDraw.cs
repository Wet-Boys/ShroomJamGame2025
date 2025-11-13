using Godot;

namespace ShroomGameReal.scenes.HITW.Drawing;

[GlobalClass]
public partial class MouseDraw : Control
{
    [Export]
    public int brushRadius = 10;
    
    // [Export]
    // public int maxBrushRadius = 15;
    //
    // [Export]
    // public int minBrushRadius = 1;

    public int CurrentUsage { get; private set; }
    
    public bool CanDraw { get; set; }
    
    public int maxBudget;

    private Image _image;
    private Viewport _viewport;
    private TextureRect _textureRect;
    private HoleInTheWallGame _game;
    private ProgressBar _usageBar;
    private RayCast3D _mouseRayCast;
    private StaticBody3D _wall;
    private MeshInstance3D _wallQuad;
    private Camera3D _camera;

    private Sprite2D _cursorSprite;

    private bool _drawButtonHeld;
    private bool _eraseButtonHeld;
    private bool _imageDirty;
    
    public ImageTexture ImageTexture { get; private set; }

    public override void _Ready()
    {
        _game = GetOwner<HoleInTheWallGame>();
        
        _image = Image.CreateEmpty(_game.imageSize.X, _game.imageSize.Y, false, Image.Format.Rgba8);
        _viewport = GetViewport();
        _textureRect = GetNode<TextureRect>("TextureRect");

        _usageBar = GetNode<ProgressBar>("%Usage Bar");
        _mouseRayCast = GetNode<RayCast3D>("%Mouse Ray");
        _wall = GetNode<StaticBody3D>("%Wall");
        _wallQuad = GetNode<MeshInstance3D>("%Wall Quad");
        _camera = GetNode<Camera3D>("%HITW Camera");

        _cursorSprite = GetNode<Sprite2D>("%ZapperCrosshair");
        
        _image.Fill(Colors.Black);
        
        ImageTexture = ImageTexture.CreateFromImage(_image);
    }

    public override void _Process(double delta)
    {
        _usageBar.MaxValue = maxBudget;
        _usageBar.Value = maxBudget - CurrentUsage;

        if (_imageDirty)
        {
            _imageDirty = false;
            ImageTexture.Update(_image);
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (!_game.IsActive)
            return;
        
        if (@event.IsActionPressed("primary_action"))
            _drawButtonHeld = true;
        if (@event.IsActionReleased("primary_action"))
            _drawButtonHeld = false;
        
        if (@event.IsActionPressed("secondary_action"))
            _eraseButtonHeld = true;
        if (@event.IsActionReleased("secondary_action"))
            _eraseButtonHeld = false;

        // if (@event.IsActionPressed("scroll_up"))
        // {
        //     brushRadius++;
        //     if (brushRadius >= maxBrushRadius)
        //         brushRadius = maxBrushRadius;
        // }
        //
        // if (@event.IsActionPressed("scroll_down"))
        // {
        //     brushRadius--;
        //     if (brushRadius <= minBrushRadius)
        //         brushRadius = minBrushRadius;
        // }

        if (@event is InputEventMouse mouseEvent)
        {
            _cursorSprite.GlobalPosition = mouseEvent.Position;
            
            _mouseRayCast.GlobalPosition = _camera.ProjectRayOrigin(mouseEvent.Position);
            _mouseRayCast.TargetPosition = _mouseRayCast.GlobalPosition + _camera.ProjectRayNormal(mouseEvent.Position) * Mathf.Abs(_camera.Far);
            _mouseRayCast.ForceRaycastUpdate();

            if (_mouseRayCast.IsColliding())
            {
                var wallSize = ((QuadMesh)_wallQuad.Mesh).Size;
                
                var pointLocalToWall = _wall.GlobalTransform.AffineInverse() * _mouseRayCast.GetCollisionPoint();
                var localPoint = new Vector2(pointLocalToWall.X, -pointLocalToWall.Y);
                localPoint += Vector2.One * 0.5f;
                localPoint *= wallSize;

                // localPoint /= wallSize;
                
                var sizeRatio = _game.imageSize / wallSize;
                var imagePos = (Vector2I)(localPoint * sizeRatio).Round();

                if (_eraseButtonHeld)
                {
                    DrawOnImage(imagePos, Colors.Black);

                    _textureRect.Texture = ImageTexture.CreateFromImage(_image);
                }
                else if (_drawButtonHeld)
                {
                    DrawOnImage(imagePos, Colors.White);

                    _textureRect.Texture = ImageTexture.CreateFromImage(_image);
                }
            }
        }
    }

    public void ResetImage()
    {
        var size = _game.imageSize;
        for (int x = 0; x < size.X; x++)
        for (int y = 0; y < size.Y; y++)
        {
            BudgetSetPixel(new Vector2I(x, y), Colors.Black);
        }
    }

    public void DrawOnImage(Vector2I pos, Color color)
    {
        if (!CanDraw)
            return;
        
        if (pos.X < _game.imageSize.X && pos.Y < _game.imageSize.Y && pos is { X: >= 0, Y: >= 0 })
            BudgetSetPixel(pos, color);
        
        for (int i = 0; i < brushRadius; i++)
        {
            for (int x = -i; x < i; x++)
            for (int y = -i; y < i; y++)
            {
                var offset = new Vector2I(x, y);
                var newPos = pos + offset;

                if (newPos.X >= _game.imageSize.X || newPos.Y >= _game.imageSize.Y || newPos.X < 0 || newPos.Y < 0)
                    continue;

                if (newPos.DistanceTo(pos) > brushRadius / 2f)
                    continue;
                
                BudgetSetPixel(newPos, color);
            }
        }
    }

    private void BudgetSetPixel(Vector2I pos, Color color)
    {
        if (color == Colors.White)
        {
            var existingPixel = _image.GetPixelv(pos);
            if (existingPixel == Colors.White)
                return;

            if (CurrentUsage >= maxBudget)
                return;

            CurrentUsage++;
        }
        else if (color == Colors.Black)
        {
            var existingPixel = _image.GetPixelv(pos);
            if (existingPixel == Colors.Black)
                return;

            CurrentUsage--;
            if (CurrentUsage < 0)
                CurrentUsage = 0;
        }

        _imageDirty = true;
        _image.SetPixelv(pos, color);
    }
}