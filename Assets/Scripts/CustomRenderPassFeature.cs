using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CustomRenderPassFeature : ScriptableRendererFeature
{
    enum MKProfileId
    {
        Custom
    }


    class CustomRenderPass : ScriptableRenderPass
    {
        private RTHandle m_customTexture1;
        
        private RenderTextureDescriptor m_AOPassDescriptor;

        private static readonly int s_customTexture1ID = Shader.PropertyToID("_customTexture1");
        
        private ScriptableRenderer m_Renderer = null;
        

        private Material m_Material;

        private ProfilingSampler m_ProfilingSampler = ProfilingSampler.Get(MKProfileId.Custom);

        public bool Setup(ScriptableRenderer renderer,Material material)
        {
            m_Material = material;
            m_Renderer = renderer;
            return true;
        }

        public CustomRenderPass()
        {
            //Debug.Log("CustomRenderPass");
            m_customTexture1 = RTHandles.Alloc(new RenderTargetIdentifier(s_customTexture1ID), "_customTexture1");
        }

        // This method is called before executing the render pass.
        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in a performant manner.
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            
            RenderTextureDescriptor cameraTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;
            
            RenderTextureDescriptor descriptor = cameraTargetDescriptor;
            descriptor.msaaSamples = 1;
            descriptor.depthBufferBits = 0;

            
            m_AOPassDescriptor = descriptor;
            m_AOPassDescriptor.colorFormat = RenderTextureFormat.ARGB32;

            // Get temporary render textures
            cmd.GetTemporaryRT(s_customTexture1ID, m_AOPassDescriptor, FilterMode.Bilinear);

            // Configure targets and clear color
            ConfigureTarget( m_Renderer.cameraColorTargetHandle);
            ConfigureClear(ClearFlag.None, Color.white);
        }

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (m_Material == null)
            {
                Debug.LogErrorFormat(
                    "{0}.Execute(): Missing material. ScreenSpaceAmbientOcclusion pass will not execute. Check for missing reference in the renderer resources.",
                    GetType().Name);
                return;
            }

            CommandBuffer cmd = CommandBufferPool.Get();


            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                //Debug.Log("VAR" + m_ProfilingSampler.name);
                Render(cmd, m_customTexture1, 0);
            }
            

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }


        private enum ShaderPasses
        {
            AO = 0,
            BlurHorizontal = 1,
            BlurVertical = 2,
            BlurFinal = 3,
            AfterOpaque = 4
        }


        private void Render(CommandBuffer cmd, RTHandle target, ShaderPasses pass)
        {
            CoreUtils.SetRenderTarget(
                cmd,
                target,
                RenderBufferLoadAction.DontCare,
                RenderBufferStoreAction.Store,
                ClearFlag.None,
                Color.clear
            );
            cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, m_Material, 0, (int)pass);
        }


        public void Dispose()
        {
            m_customTexture1.Release();
        }
    }

    CustomRenderPass m_ScriptablePass;

    private Material m_Material;

    private const string k_ShaderName = "MK/OutlineFullScreenSimple";

    /// <inheritdoc/>
    public override void Create()
    {
        m_ScriptablePass = new CustomRenderPass();

        // Configures where the render pass should be injected.
        m_ScriptablePass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
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

        bool shouldAdd = m_ScriptablePass.Setup(renderer,m_Material);

        if (shouldAdd)
        {
            renderer.EnqueuePass(m_ScriptablePass);
        }
    }

    [SerializeField, HideInInspector] private Shader m_Shader = null;

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
        m_ScriptablePass?.Dispose();
        m_ScriptablePass = null;
        CoreUtils.Destroy(m_Material);
    }
    
}