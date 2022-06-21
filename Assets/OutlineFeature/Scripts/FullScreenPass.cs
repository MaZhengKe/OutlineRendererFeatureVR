using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

class FullScreenPass : ScriptableRenderPass
{
    private readonly int leftMaskId;

    private static readonly int WidthId = Shader.PropertyToID("_OutlineWidth");
    private static readonly int ColorId = Shader.PropertyToID("_OutlineColor");
    private static readonly int SamplePrecisionId = Shader.PropertyToID("_SamplePrecision");

    private readonly OutlineFeature outlineFeature;
    private Material m_Material;

    private ProfilingSampler m_ProfilingSampler = ProfilingSampler.Get(OutlineFeature.OutlineProfileId.FullScreen);

    private ScriptableRenderer m_Renderer;

    public FullScreenPass(OutlineFeature outlineFeature)
    {
        this.outlineFeature = outlineFeature;
    }

    public bool Setup(ScriptableRenderer renderer, Material material)
    {
        m_Material = material;
        m_Renderer = renderer;
        return true;
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        ConfigureTarget(m_Renderer.cameraColorTargetHandle);
        ConfigureClear(ClearFlag.None, Color.white);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (m_Material == null)
        {
            Debug.LogError("Material not created");
            return;
        }

        if (!renderingData.cameraData.postProcessEnabled) return;


        if (outlineFeature == null) return;

        var cmd = CommandBufferPool.Get();

        using (new ProfilingScope(cmd, m_ProfilingSampler))
        {
            m_Material.SetFloat(WidthId, outlineFeature.width);
            m_Material.SetFloat(SamplePrecisionId, outlineFeature.samplePrecision);
            m_Material.SetColor(ColorId, outlineFeature.color);

            cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, m_Material);
        }

        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();

        CommandBufferPool.Release(cmd);
    }

    public override void OnCameraCleanup(CommandBuffer cmd)
    {
    }

    public void Dispose()
    {
    }
}