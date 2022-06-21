using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

class RenderMaskPass : ScriptableRenderPass
{
    readonly List<ShaderTagId> _shaderTagIds = new();
    private FilteringSettings _filteringSettings;

    readonly RenderTargetIdentifier _maskTargetIdentifier;
    private RenderTextureDescriptor descriptor { get; set; }
    private RTHandle m_MaskTexture;
    private static readonly int s_MaskTextureID = Shader.PropertyToID("_MaskTex");

    private ProfilingSampler m_ProfilingSampler = ProfilingSampler.Get(OutlineFeature.OutlineProfileId.Mask);

    private ScriptableRenderer m_Renderer = null;
    private Material m_Material;

    public RenderMaskPass(RenderQueueRange renderQueueRange, LayerMask layerMask)
    {
        _filteringSettings = new FilteringSettings(renderQueueRange, layerMask);

        _shaderTagIds.Add(new ShaderTagId("SRPDefaultUnlit"));
        _shaderTagIds.Add(new ShaderTagId("UniversalForward"));
        _shaderTagIds.Add(new ShaderTagId("UniversalForwardOnly"));
        _shaderTagIds.Add(new ShaderTagId("LightweightForward"));

        m_MaskTexture = RTHandles.Alloc(new RenderTargetIdentifier(s_MaskTextureID), "_MaskTex");
    }

    public bool Setup(RenderTextureDescriptor baseDescriptor, float scale, ScriptableRenderer renderer,
        Material material)
    {
        m_Material = material;
        m_Renderer = renderer;

        baseDescriptor.colorFormat = RenderTextureFormat.R8;
        //baseDescriptor.depthBufferBits = 0;
        baseDescriptor.height = (int)(baseDescriptor.height * scale);
        baseDescriptor.width = (int)(baseDescriptor.width * scale);
        baseDescriptor.msaaSamples = 1;
        descriptor = baseDescriptor;

        return true;
    }

    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
        cmd.GetTemporaryRT(s_MaskTextureID, descriptor, FilterMode.Bilinear);

        ConfigureTarget(m_MaskTexture);
        ConfigureClear(ClearFlag.All, Color.clear);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (m_Material == null)
        {
            Debug.LogErrorFormat(
                "{0}.Execute(): Missing material. ScreenSpaceAmbientOcclusion pass will not execute. Check for missing reference in the renderer resources.",
                GetType().Name);
            return;
        }

        var cmd = CommandBufferPool.Get();

        using (new ProfilingScope(cmd, m_ProfilingSampler))
        {
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            var sortFlags = renderingData.cameraData.defaultOpaqueSortFlags;
            var drawSettings = CreateDrawingSettings(_shaderTagIds, ref renderingData, sortFlags);
            drawSettings.perObjectData = PerObjectData.None;

            ref var cameraData = ref renderingData.cameraData;
            var camera = cameraData.camera;
            if (cameraData.xrRendering)
                context.StartMultiEye(camera);

            drawSettings.overrideMaterial = m_Material;

            context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref _filteringSettings);
        }

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public override void OnCameraCleanup(CommandBuffer cmd)
    {
        if (cmd == null)
        {
            throw new ArgumentNullException(nameof(cmd));
        }
        cmd.ReleaseTemporaryRT(s_MaskTextureID);
    }

    public void Dispose()
    {
        m_MaskTexture.Release();
    }
}