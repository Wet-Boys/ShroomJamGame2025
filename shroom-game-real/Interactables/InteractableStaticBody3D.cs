using System.Collections.Generic;
using System.Linq;
using Godot;
using ShroomGameReal.Player;
using ShroomGameReal.Utilities;

namespace ShroomGameReal.Interactables;

[GlobalClass]
public partial class InteractableStaticBody3D : StaticBody3D, IInteractable
{
    [Export]
    public float outlinePadding;
    
    [Export(PropertyHint.Range, "0, 1, 0.01")]
    public float outlineThickness = 0.1f;
    
    [Export]
    public Color outlineColor = Colors.White;
    
    [Export(PropertyHint.Layers3DRender)]
    public uint meshLayersOutlineMask;
    
    private readonly List<BoundaryShape> _boundaryShapes = [];
    // private ColorRect _outlineRect;
    // private Material _outlineMaterial = ResourceLoader.Load<Material>("res://Interactables/Selected Outline Mat.tres");

    private List<MeshInstance3D> _meshInstances = [];
    
    private Material _stencilOutlineMaterial = ResourceLoader.Load<Material>("res://Interactables/Selected Outline Stencil Write.tres");
    private Material _renderOutlineMaterial = ResourceLoader.Load<Material>("res://Interactables/Selected Outline Render Outline.tres");
    
    public virtual bool CanInteract { get; }
    
    public override void _Ready()
    {
        // Iterate through all shape owners and store their boundary points.
        // foreach (uint shapeOwnerId in GetShapeOwners())
        // {
        //     var shapeOwner = ShapeOwnerGetOwner(shapeOwnerId) as CollisionShape3D;
        //
        //     if (shapeOwner is null)
        //         continue;
        //     
        //     _boundaryShapes.Add(new BoundaryShape(shapeOwner, outlinePadding));
        // }

        _meshInstances = this.GetChildrenRecursively<MeshInstance3D>()
            .Where(meshInstance => (meshInstance.Layers & meshLayersOutlineMask) != 0)
            .ToList();
    }
    
    public virtual void OnSelected()
    {
        if (!CanInteract)
            return;
        
        _renderOutlineMaterial.Set("shader_parameter/grow", outlineThickness);
        _renderOutlineMaterial.Set("shader_parameter/albedo", outlineColor);

        foreach (var meshInstance in _meshInstances)
        {
            meshInstance.SetMaterialOverlay(_stencilOutlineMaterial);
        }
        
        // var (screenMin, screenMax) = GetScreenBounds();
        // var camera = GetViewport().GetCamera3D();
        //
        // if (_outlineRect is null)
        // {
        //     _outlineRect = new ColorRect();
        //     _outlineRect.MouseFilter = Control.MouseFilterEnum.Ignore;
        //     _outlineRect.Material = _outlineMaterial;
        //     camera.AddChild(_outlineRect);
        // }
        //
        // _outlineRect.Position = screenMin;
        // var size = screenMax - screenMin;
        // _outlineRect.Size = size;
        // _outlineRect.SetInstanceShaderParameter("rect_size", size);
    }

    public virtual void OnDeselected()
    {
        foreach (var meshInstance in _meshInstances)
        {
            meshInstance.SetMaterialOverlay(null);
        }
        
        // if (_outlineRect is not null)
        // {
        //     _outlineRect.Free();
        //     _outlineRect = null;
        // }
    }

    public void OnInteract(PlayerController player)
    {
        if (!CanInteract)
            return;

        Interact(player);
        
        OnDeselected();
    }

    protected virtual void Interact(PlayerController player) { }

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