using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class OutlineFeature : ScriptableRendererFeature
{
    public enum OutlineProfileId
    {
        Mask,
        FullScreen
    }

    [Tooltip("遮挡显示")] public bool show;
    [Range(0f, 5f), Tooltip("轮廓宽度")] public float width = 1;

    [Range(0f, 3f), Tooltip("采样次数")] public float samplePrecision = 3;

    [ColorUsage(true, true), Tooltip("轮廓颜色")]
    public Color color = new(1, 1, 1);

    [SerializeField] private LayerMask layerMask;

    RenderMaskPass renderMaskPass;
    FullScreenPass fullScreenPass;

    private Material m_MaskMaterial;
    private Material m_FullMaterial;

    private const string k_MaskShaderName = "MK/OutlineMask";
    private const string k_FullShaderName = "MK/OutlineFullScreen";

    [SerializeField, HideInInspector] private Shader m_MaskShader;
    [SerializeField, HideInInspector] private Shader m_fullShader;

    /// <inheritdoc/>
    public override void Create()
    {

        renderMaskPass = new RenderMaskPass(RenderQueueRange.opaque, layerMask)
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing
        };

        fullScreenPass = new FullScreenPass(this)
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing
        };
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (!GetMaterial() || !GetFullMaterial())
        {
            Debug.LogErrorFormat(
                "{0}.AddRenderPasses(): Missing material. {1} render pass will not be added. Check for missing reference in the renderer resources.",
                GetType().Name, name);
            return;
        }

        bool shouldAdd = renderMaskPass.Setup(renderer, m_MaskMaterial,show) && fullScreenPass.Setup(renderer, m_FullMaterial);
        if (shouldAdd)
        {
            renderer.EnqueuePass(renderMaskPass);
            renderer.EnqueuePass(fullScreenPass);
        }
    }

    private bool GetMaterial()
    {
        if (m_MaskMaterial != null)
        {
            return true;
        }

        if (m_MaskShader == null)
        {
            m_MaskShader = Shader.Find(k_MaskShaderName);
            if (m_MaskShader == null)
            {
                return false;
            }
        }

        m_MaskMaterial = CoreUtils.CreateEngineMaterial(m_MaskShader);

        return m_MaskMaterial != null;
    }
    private bool GetFullMaterial()
    {
        if (m_FullMaterial != null)
        {
            return true;
        }

        if (m_fullShader == null)
        {
            m_fullShader = Shader.Find(k_FullShaderName);
            if (m_fullShader == null)
            {
                return false;
            }
        }
        m_FullMaterial = CoreUtils.CreateEngineMaterial(m_fullShader);
        return m_FullMaterial != null;
    }


    protected override void Dispose(bool disposing)
    {
        renderMaskPass?.Dispose();
        renderMaskPass = null;
        
        fullScreenPass?.Dispose();
        fullScreenPass = null;
        
        CoreUtils.Destroy(m_MaskMaterial);
        CoreUtils.Destroy(m_FullMaterial);
    }
}