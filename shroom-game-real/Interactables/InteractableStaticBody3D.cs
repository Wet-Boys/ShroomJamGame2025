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
    public float meshBoundaryPadding;
    
    [Export(PropertyHint.Range, "0, 1, 0.01")]
    public float outlineThickness = 0.1f;
    
    [Export]
    public Color outlineColor = Colors.White;
    
    [Export(PropertyHint.Layers3DRender)]
    public uint meshLayersOutlineMask;
    
    private readonly List<MeshBounds> _meshBounds = [];
    
    // private ColorRect _outlineRect;
    // private Material _outlineMaterial = ResourceLoader.Load<Material>("res://Interactables/Selected Outline Mat.tres");

    private List<MeshInstance3D> _meshInstances = [];
    
    private Material _stencilOutlineMaterial = ResourceLoader.Load<Material>("res://Interactables/Selected Outline Stencil Write.tres");
    private Material _renderOutlineMaterial = ResourceLoader.Load<Material>("res://Interactables/Selected Outline Render Outline.tres");
    
    public virtual bool CanInteract { get; }
    
    public override void _Ready()
    {
        _meshInstances = this.GetChildrenRecursively<MeshInstance3D>()
            .Where(meshInstance => (meshInstance.Layers & meshLayersOutlineMask) != 0)
            .ToList();

        foreach (var meshInstance in _meshInstances)
            _meshBounds.Add(new MeshBounds(meshInstance, meshBoundaryPadding));
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
    
    public virtual string GetInteractText() => "Interact";

    protected virtual void Interact(PlayerController player) { }

    public (Vector2 screenMin, Vector2 screenMax) GetScreenBounds()
    {
        var screenMin = Vector2.One * float.MaxValue;
        var screenMax = Vector2.One * float.MinValue;
        
        var camera = GetViewport().GetCamera3D();

        foreach (var meshBoundary in _meshBounds)
        {
            foreach (var point in meshBoundary.BoundaryPoints)
            {
                var worldPoint = meshBoundary.Owner.GlobalTransform * point;
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
    
    private class MeshBounds
    {
        public MeshInstance3D Owner { get; }
        public List<Vector3> BoundaryPoints { get; } = [];
        
        public MeshBounds(MeshInstance3D meshInstance, float padding)
        {
            Owner = meshInstance;
            
            if (meshInstance.Mesh is not ArrayMesh arrayMesh)
                return;

            // Calculate bounding box of mesh.
            var minVertex = Vector3.One * float.MaxValue;
            var maxVertex = Vector3.One * float.MinValue;
            
            for (int i = 0; i < arrayMesh.GetSurfaceCount(); i++)
            {
                var surfaceArrays = arrayMesh.SurfaceGetArrays(i);
                var vertices = surfaceArrays[(int)Mesh.ArrayType.Vertex].AsVector3Array();

                foreach (var vertex in vertices)
                {
                    if (vertex.X < minVertex.X)
                        minVertex.X = vertex.X;
                    if (vertex.Y < minVertex.Y)
                        minVertex.Y = vertex.Y;
                    if (vertex.Z < minVertex.Z)
                        minVertex.Z = vertex.Z;
                
                    if (vertex.X > maxVertex.X)
                        maxVertex.X = vertex.X;
                    if (vertex.Y > maxVertex.Y)
                        maxVertex.Y = vertex.Y;
                    if (vertex.Z > maxVertex.Z)
                        maxVertex.Z = vertex.Z;
                }
            }
            
            minVertex -= Vector3.One * (padding / 2f);
            maxVertex += Vector3.One * (padding / 2f);
            
            var pointA = new Vector3(minVertex.X, minVertex.Y, maxVertex.Z);
            var pointB = new Vector3(maxVertex.X, minVertex.Y, minVertex.Z);
            var pointC = new Vector3(maxVertex.X, minVertex.Y, maxVertex.Z);
            var pointD = new Vector3(minVertex.X, maxVertex.Y, minVertex.Z);
            var pointE = new Vector3(minVertex.X, maxVertex.Y, maxVertex.Z);
            var pointF = new Vector3(maxVertex.X, maxVertex.Y, minVertex.Z);
                
            BoundaryPoints.Add(minVertex);
            BoundaryPoints.Add(pointA);
            BoundaryPoints.Add(pointB);
            BoundaryPoints.Add(pointC);
            BoundaryPoints.Add(pointD);
            BoundaryPoints.Add(pointE);
            BoundaryPoints.Add(pointF);
            BoundaryPoints.Add(maxVertex);
        }
    }
}