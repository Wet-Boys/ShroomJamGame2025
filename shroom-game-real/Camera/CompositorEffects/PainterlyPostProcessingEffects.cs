using Godot;
using ShroomGameReal.Utilities;

namespace ShroomGameReal.Camera.CompositorEffects;

[Tool]
[GlobalClass]
public partial class PainterlyPostProcessingEffects : BaseCompositorEffect
{
    [ExportGroup("Kuwahara Parameters")]
    [Export]
    public float kuwaharaRadius = 4;

    [ExportGroup("Painterly Parameters")]
    [Export]
    public float painterlyNoiseAmplitude = 3.0f;
    [Export]
    public float painterlyNoiseScale = 0.5f;
    [Export]
    public float painterlyNoiseSpeed = 3.0f;
    
    [ExportGroup("Screen Texture Settings")]
    [Export]
    public RenderingDevice.SamplerFilter ScreenTextureSamplerFilter
    {
        get;
        set
        {
            field = value;
            Rebuild();
        }
    } = RenderingDevice.SamplerFilter.Linear;
    
    [Export]
    public RenderingDevice.SamplerRepeatMode ScreenTextureSamplerRepeatMode
    {
        get;
        set
        {
            field = value;
            Rebuild();
        }
    } = RenderingDevice.SamplerRepeatMode.ClampToEdge;
    
    private RDShaderFile _shaderFile = ResourceLoader.Load<RDShaderFile>("res://Camera/CompositorEffects/Shaders/Painterly-Post-Processing-Uber-Shader.glsl");

    private Rid _shader;
    private Rid _pipeline;
    private Rid _screenTextureSampler;

    private ulong _lastTime;
    private double _currentTime;

    public override void _RenderCallback(int effectCallbackType, RenderData renderData)
    {
        if (Device is null || _shaderFile is null || !_pipeline.IsValid)
            return;
        
        var renderSceneBuffers = renderData.GetRenderSceneBuffers();
        if (renderSceneBuffers is not RenderSceneBuffersRD sceneBuffers)
            return;

        var renderSceneData = renderData.GetRenderSceneData();
        if (renderSceneData is not RenderSceneDataRD sceneData)
            return;
        
        var renderSize = sceneBuffers.GetInternalSize();
        if (renderSize is { X: 0, Y: 0 })
        {
            GD.PushError("Render size is invalid!");
            return;
        }
        
        var xGroups = (uint)(renderSize.X - 1) / 8 + 1;
        var yGroups = (uint)(renderSize.Y - 1) / 8 + 1;
        const uint zGroups = 1;

        var pushConstants = new object[]
        {
            (float)renderSize.X,
            (float)renderSize.Y,
            GetEngineTime(),
        };
        var pushConstantsBytes = BitConverter.GetBytes(pushConstants);
        
        var parameters = new object[]
        {
            kuwaharaRadius,
            painterlyNoiseAmplitude,
            painterlyNoiseScale,
            painterlyNoiseSpeed,
        };
        var parametersBytes = BitConverter.GetBytes(parameters);
        
        var viewCount = sceneBuffers.GetViewCount();
        for (uint view = 0; view < viewCount; view++)
        {
            var parametersBuffer = Device.UniformBufferCreate(parametersBytes);
            var parametersBufferUniform = new RDUniform
            {
                UniformType = RenderingDevice.UniformType.UniformBuffer,
                Binding = 0
            };
            parametersBufferUniform.AddId(parametersBuffer);

            var sceneColorTexture = sceneBuffers.GetColorLayer(view);
            var srcSceneColorUniform = new RDUniform
            {
                UniformType = RenderingDevice.UniformType.SamplerWithTexture,
                Binding = 0
            };
            srcSceneColorUniform.AddId(_screenTextureSampler);
            srcSceneColorUniform.AddId(sceneColorTexture);

            var sceneDepthStencilTexture = sceneBuffers.GetDepthLayer(view);
            var sceneDepthStencilUniform = new RDUniform
            {
                UniformType = RenderingDevice.UniformType.SamplerWithTexture,
                Binding = 1
            };
            sceneDepthStencilUniform.AddId(_screenTextureSampler);
            sceneDepthStencilUniform.AddId(sceneDepthStencilTexture);

            var resultTextureUniform = new RDUniform
            {
                UniformType = RenderingDevice.UniformType.Image,
                Binding = 2
            };
            resultTextureUniform.AddId(sceneColorTexture);

            var paramsUniformSet = UniformSetCacheRD.GetCache(_shader, 0, [parametersBufferUniform]);
            var texturesUniformSet = UniformSetCacheRD.GetCache(_shader, 1, [srcSceneColorUniform, sceneDepthStencilUniform, resultTextureUniform]);
            
            var computeList = Device.ComputeListBegin();
            Device.ComputeListBindComputePipeline(computeList, _pipeline);
            Device.ComputeListBindUniformSet(computeList, paramsUniformSet, 0);
            Device.ComputeListBindUniformSet(computeList, texturesUniformSet, 1);
            Device.ComputeListSetPushConstant(computeList, pushConstantsBytes, (uint)pushConstantsBytes.Length);
            Device.ComputeListDispatch(computeList, xGroups, yGroups, zGroups);
            Device.ComputeListEnd();
        }
    }

    private double GetEngineTime()
    {
        var current = Time.GetTicksMsec();
        var delta = current - _lastTime;
        _lastTime = current;
        
        _currentTime += delta / 1000.0;
        
        return _currentTime;
    }

    protected override void ConstructEffect(RenderingDevice device)
    {
        if (_shaderFile is null)
        {
            GD.Print("No Shader file found!");
            return;
        }

        _shader = device.ShaderCreateFromSpirV(_shaderFile.GetSpirV());
        if (!_shader.IsValid)
        {
            GD.PushError("Failed to compile Volumetric Lighting Shader!");
            Destruct();
            return;
        }
        
        _pipeline = device.ComputePipelineCreate(_shader);
        if (!_pipeline.IsValid)
        {
            GD.PushError("Failed to compile Volumetric Lighting Pipeline!");
            Destruct();
            return;
        }
        
        _screenTextureSampler = device.SamplerCreate(new RDSamplerState
        {
            MagFilter = ScreenTextureSamplerFilter,
            MinFilter = ScreenTextureSamplerFilter,
            MipFilter = ScreenTextureSamplerFilter,
            RepeatU = ScreenTextureSamplerRepeatMode,
            RepeatV = ScreenTextureSamplerRepeatMode,
            RepeatW = ScreenTextureSamplerRepeatMode,
            LodBias = 0,
            UseAnisotropy = false,
            AnisotropyMax = 0,
            EnableCompare = false,
            CompareOp = RenderingDevice.CompareOperator.Never,
            MinLod = 0,
            MaxLod = 0,
            BorderColor = RenderingDevice.SamplerBorderColor.FloatTransparentBlack,
            UnnormalizedUvw = false
        });

        _currentTime = 0;
        _lastTime = Time.GetTicksMsec();
    }

    protected override void DestructEffect(RenderingDevice device)
    {
        if (_shader.IsValid)
        {
            device.FreeRid(_shader);
            _shader = default;
        }

        _pipeline = default;
        
        // device.FreeRid(_screenTextureSampler);
        _screenTextureSampler = default;
    }
}