using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace KuanMi
{
    public class OutlineFeature : ScriptableRendererFeature
    {
        [System.Flags]
        public enum RenderLayer
        {
            Outline01 = (1 << 20),
            Outline02 = (1 << 21),
            Outline03 = (1 << 22),
            Outline04 = (1 << 23),
        }

        public enum OutlineProfileId
        {
            Mask,
            FullScreen,
            Outline
        }

        public void CleanOutLayer()
        {
            var renderers = FindObjectsOfType<Renderer>();

            foreach (var renderer in renderers)
            {
                renderer.renderingLayerMask &= ~renderingLayerMask;
            }
            // var outlines = FindObjectsOfType<Outline>();
            // foreach (var outline in outlines)
            // {
            //     outline.SetLayer();
            // }
        }

        [Tooltip("遮挡显示")] public bool show;
        [Range(0f, 5f), Tooltip("轮廓宽度")] public float width = 1;
        [Range(0f, 3f), Tooltip("采样次数")] public float samplePrecision = 3;

        [ColorUsage(true, true), Tooltip("轮廓颜色")]
        public Color color = new(1, 1, 1);

        [SerializeField] private LayerMask layerMask;
        [SerializeField, HideInInspector] private Shader m_Shader;

        public RenderLayer renderLayer;

        private uint renderingLayerMask;
        private const string k_ShaderName = "KuanMi/Outline";
        private Material m_Material;
        private OutlinePass _outlinePass;

        public override void Create()
        {
            renderingLayerMask = (uint)renderLayer;

            _outlinePass = new OutlinePass(RenderQueueRange.opaque, layerMask, renderingLayerMask, this)
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

            bool shouldAdd = _outlinePass.Setup(renderer, m_Material);
            if (shouldAdd)
            {
                renderer.EnqueuePass(_outlinePass);
            }
        }

        private bool GetMaterial()
        {
            if (m_Material != null)
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

            m_Material = CoreUtils.CreateEngineMaterial(m_Shader);

            return m_Material != null;
        }


        protected override void Dispose(bool disposing)
        {
            _outlinePass?.Dispose();
            _outlinePass = null;

            CoreUtils.Destroy(m_Material);
        }
    }
}