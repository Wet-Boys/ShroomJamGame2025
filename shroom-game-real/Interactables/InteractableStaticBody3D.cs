using System.Collections.Generic;
using Godot;
using ShroomGameReal.Player;

namespace ShroomGameReal.Interactables;

[GlobalClass]
public partial class InteractableStaticBody3D : StaticBody3D, IInteractable
{
    [Export]
    public float outlinePadding;
    
    private readonly List<BoundaryShape> _boundaryShapes = [];
    private ColorRect _outlineRect;
    private Material _outlineMaterial = ResourceLoader.Load<Material>("res://Interactables/Selected Outline Mat.tres");
    
    public virtual bool CanInteract { get; }
    
    public override void _Ready()
    {
        // Iterate through all shape owners and store their boundary points.
        foreach (uint shapeOwnerId in GetShapeOwners())
        {
            var shapeOwner = ShapeOwnerGetOwner(shapeOwnerId) as CollisionShape3D;

            if (shapeOwner is null)
                continue;
            
            _boundaryShapes.Add(new BoundaryShape(shapeOwner, outlinePadding));
        }
    }
    
    
    
    public virtual void OnSelected()
    {
        if (!CanInteract)
            return;
        
        var (screenMin, screenMax) = GetScreenBounds();
        var camera = GetViewport().GetCamera3D();

        if (_outlineRect is null)
        {
            _outlineRect = new ColorRect();
            _outlineRect.MouseFilter = Control.MouseFilterEnum.Ignore;
            _outlineRect.Material = _outlineMaterial;
            camera.AddChild(_outlineRect);
        }
        
        _outlineRect.Position = screenMin;
        var size = screenMax - screenMin;
        _outlineRect.Size = size;
        _outlineRect.SetInstanceShaderParameter("rect_size", size);
    }

    public virtual void OnDeselected()
    {
        if (_outlineRect is not null)
        {
            _outlineRect.Free();
            _outlineRect = null;
        }
    }

    public virtual void OnInteract(PlayerController player) { }

    private (Vector2 min, Vector2 max) GetScreenBounds()
    {
        var screenMin = Vector2.One * float.MaxValue;
        var screenMax = Vector2.One * float.MinValue;
        
        var camera = GetViewport().GetCamera3D();

        foreach (var shape in _boundaryShapes)
        {
            foreach (var point in shape.Points)
            {
                var worldPoint = shape.Owner.GlobalTransform * point;
                var screenPoint = camera.UnprojectPosition(worldPoint);

                if (screenPoint.X > screenMax.X)
                    screenMax.X = screenPoint.X;
                if (screenPoint.Y > screenMax.Y)
                    screenMax.Y = screenPoint.Y;

                if (screenPoint.X < screenMin.X)
                    screenMin.X = screenPoint.X;
                if (screenPoint.Y < screenMin.Y)
                    screenMin.Y = screenPoint.Y;
            }
        }
        
        return (screenMin, screenMax);
    }
    
    private class BoundaryShape
    {
        public CollisionShape3D Owner { get; private set; }
        public List<Vector3> Points { get; } = [];
        
        public BoundaryShape(CollisionShape3D owner, float padding)
        {
            Owner = owner;
            var shape = owner?.Shape;
            
            if (shape is BoxShape3D boxShape)
            {
                var size = boxShape.Size + Vector3.One * padding;

                var localMin = -size / 2f;
                var localMax = size / 2f;
                
                var pointA = new Vector3(localMin.X, localMin.Y, localMax.Z);
                var pointB = new Vector3(localMax.X, localMin.Y, localMin.Z);
                var pointC = new Vector3(localMax.X, localMin.Y, localMax.Z);
                var pointD = new Vector3(localMin.X, localMax.Y, localMin.Z);
                var pointE = new Vector3(localMin.X, localMax.Y, localMax.Z);
                var pointF = new Vector3(localMax.X, localMax.Y, localMin.Z);
                
                Points.Add(localMin);
                Points.Add(pointA);
                Points.Add(pointB);
                Points.Add(pointC);
                Points.Add(pointD);
                Points.Add(pointE);
                Points.Add(pointF);
                Points.Add(localMax);
            }
        }
    }
}