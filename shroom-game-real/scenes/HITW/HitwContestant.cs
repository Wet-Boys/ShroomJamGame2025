using System.Collections.Generic;
using System.Linq;
using Godot;
using ShroomGameReal.Utilities;

namespace ShroomGameReal.scenes.HITW;

[GlobalClass]
public partial class HitwContestant : Node3D
{
    [Signal]
    public delegate void ImageReadyEventHandler();

    [Export(PropertyHint.Range, "0.5,20,0.25,suffix:s")]
    public float timeToComplete = 10f;

    public HoleInTheWallGame game;
    
    private SubViewport _renderViewport;
    private Node3D _contestantRenderRoot;
    
    public Image ShapeImage { get; private set; }   
    public int NumberPixelsFilled { get; private set; }

    public override async void _Ready()
    {
        game = GetParent().GetOwner<HoleInTheWallGame>();
        _renderViewport = GetNode<SubViewport>("%Contestant Render Viewport");
        _contestantRenderRoot = GetNode<Node3D>("%Contestant Render Root");

        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        SetImageReady();
        // RenderingServer.Singleton.FramePostDraw += SetImageReady;
    }
    
    private void SetImageReady()
    {
        ShapeImage = _renderViewport.GetTexture().GetImage();
        
        var imageSize = ShapeImage.GetSize();
        for (int x = 0; x < imageSize.X; x++)
        for (int y = 0; y < imageSize.Y; y++)
        {
            var color = ShapeImage.GetPixel(x, y);

            if (color != Colors.White)
                continue;

            NumberPixelsFilled++;
        }
        
        _renderViewport.RenderTargetClearMode = SubViewport.ClearMode.Never;
        _renderViewport.RenderTargetUpdateMode = SubViewport.UpdateMode.Disabled;
        
        foreach (var node in _contestantRenderRoot.GetChildren().OfType<Node3D>())
        {
            var instance = node.Duplicate();
            AddChild(instance);

            foreach (var meshInstance in instance.GetChildrenRecursively<MeshInstance3D>())
            {
                var mesh = (Mesh)meshInstance.Mesh.DuplicateDeep();
                meshInstance.Mesh = mesh;

                for (int i = 0; i < mesh.GetSurfaceCount(); i++)
                {
                    var material = (StandardMaterial3D)mesh.SurfaceGetMaterial(i);
                    if (material is null)
                        continue;

                    material.Metallic = 0f;
                    
                    meshInstance.SetSurfaceOverrideMaterial(i, (Material)material.DuplicateDeep());
                }
            }
        }
        
        EmitSignalImageReady();
    }

    public bool CanFitThrough(Image cutImage)
    {
        var size = cutImage.GetSize();
        
        for (int x = 0; x < size.X; x++)
        for (int y = 0; y < size.Y; y++)
        {
            var color = ShapeImage.GetPixel(x, y);
            if (color != Colors.White)
                continue;

            var cutColor = cutImage.GetPixel(x, y);

            if (cutColor != Colors.White)
                return false;
        }

        return true;
    }
}