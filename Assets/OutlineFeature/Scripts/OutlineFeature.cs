using System.Collections.Generic;
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

    public Shader fullShader;
    [Range(0.1f, 1f), Tooltip("缩放比例")] public float scale = 1;
    [Range(0f, 5f), Tooltip("轮廓宽度")] public float width = 1;

    [Range(0f, 3f), Tooltip("采样次数")] public float samplePrecision = 3;

    [ColorUsage(true, true), Tooltip("轮廓颜色")]
    public Color color = new(1, 1, 1);

    [SerializeField] private LayerMask layerMask;

    RenderMaskPass renderMaskPass;
    FullScreenPass fullScreenPass;


    private Material m_MaskMaterial;
    private Material m_FullMaterial;


    private const string k_ShaderName = "MK/OutlineMask";


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
        if (!GetMaterial())
        {
            Debug.LogErrorFormat(
                "{0}.AddRenderPasses(): Missing material. {1} render pass will not be added. Check for missing reference in the renderer resources.",
                GetType().Name, name);
            return;
        }

        bool shouldAdd = renderMaskPass.Setup(renderingData.cameraData.cameraTargetDescriptor, scale, renderer,
            m_MaskMaterial);
        if (shouldAdd)
        {
            renderer.EnqueuePass(renderMaskPass);
        }
        
        m_FullMaterial = CoreUtils.CreateEngineMaterial(fullShader);
        fullScreenPass.Setup(renderer, m_FullMaterial);

        renderer.EnqueuePass(fullScreenPass);
    }

    [SerializeField, HideInInspector] private Shader m_Shader = null;

    private bool GetMaterial()
    {
        if (m_MaskMaterial != null)
        {
            return true;
        }

        if (m_Shader == null)
        {
            m_Shader = Shader.Find(k_ShaderName);
            if (m_Shader == null)
            {
                return false;
            }
        }

        m_MaskMaterial = CoreUtils.CreateEngineMaterial(m_Shader);

        return m_MaskMaterial != null;
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