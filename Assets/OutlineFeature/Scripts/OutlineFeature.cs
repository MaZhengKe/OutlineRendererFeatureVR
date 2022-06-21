using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class OutlineFeature : ScriptableRendererFeature
{
    public Shader fullShader;
    [Range(0.1f, 1f), Tooltip("缩放比例")] public float scale = 1;
    [Range(0f, 5f), Tooltip("轮廓宽度")] public float width = 1;

    [Range(0f, 3f), Tooltip("采样次数")] public float samplePrecision = 3;

    [ColorUsage(true, true), Tooltip("轮廓颜色")]
    public Color color = new(1, 1, 1);

    class RenderMaskPass : ScriptableRenderPass
    {
        private readonly Material maskMaterial;

        readonly List<ShaderTagId> _shaderTagIds = new();

        readonly RenderTargetIdentifier _maskTargetIdentifier;
        
        private FilteringSettings _filteringSettings;
        
        private RenderTextureDescriptor blitTargetDescriptor;

        string m_ProfilerTag = "Mask pass";

        private RenderTargetHandle renderTargetHandle;
        private RenderTextureDescriptor descriptor { get; set; }
        

        public RenderMaskPass(RenderQueueRange renderQueueRange, LayerMask layerMask,  Material material)
        {
            _filteringSettings = new FilteringSettings(renderQueueRange, layerMask);
            maskMaterial = material;

            _shaderTagIds.Add(new ShaderTagId("SRPDefaultUnlit"));
            _shaderTagIds.Add(new ShaderTagId("UniversalForward"));
            _shaderTagIds.Add(new ShaderTagId("UniversalForwardOnly"));
            _shaderTagIds.Add(new ShaderTagId("LightweightForward"));
            
        }
        
        
        public void Setup(RenderTextureDescriptor baseDescriptor, RenderTargetHandle outRenderTargetHandle,float scale)
        {
            renderTargetHandle = outRenderTargetHandle;
            
            baseDescriptor.colorFormat = RenderTextureFormat.R8;
            //baseDescriptor.depthBufferBits = 0;
            baseDescriptor.height = (int)(baseDescriptor.height * scale);
            baseDescriptor.width = (int)(baseDescriptor.width * scale);
            baseDescriptor.msaaSamples = 1;
            descriptor = baseDescriptor;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            // blitTargetDescriptor = cameraTextureDescriptor;
            // blitTargetDescriptor.msaaSamples = 1;
            // blitTargetDescriptor.colorFormat = RenderTextureFormat.R8;
            // // blitTargetDescriptor.height /= 2;
            // // blitTargetDescriptor.width /= 2;

            cmd.GetTemporaryRT(renderTargetHandle.id, descriptor);
            ConfigureTarget(renderTargetHandle.Identifier());
            ConfigureClear(ClearFlag.All, Color.clear);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);

            using (new ProfilingScope(cmd, new ProfilingSampler(m_ProfilerTag)))
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


                drawSettings.overrideMaterial = maskMaterial;


                context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref _filteringSettings);

                cmd.SetGlobalTexture("_MaskTex", renderTargetHandle.id);
                
                
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
            
        }

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(renderTargetHandle.id);
        }
    }

    class FullScreenPass : ScriptableRenderPass
    {
        private static readonly string k_RenderTag = "Render Outline Effects";

        private readonly int leftMaskId;

        private static readonly int WidthId = Shader.PropertyToID("_OutlineWidth");
        private static readonly int ColorId = Shader.PropertyToID("_OutlineColor");
        private static readonly int SamplePrecisionId = Shader.PropertyToID("_SamplePrecision");

        private readonly OutlineFeature outlineFeature;
        private readonly Material fullScreenMaterial;

        readonly ProfilingSampler _profilingSampler;

        public FullScreenPass( OutlineFeature outlineFeature,Material mat)
        {
            this.outlineFeature = outlineFeature;
            _profilingSampler = new ProfilingSampler(k_RenderTag);
            fullScreenMaterial = mat;
        }

        // This method is called before executing the render pass.
        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in a performant manner.
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
        }

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (fullScreenMaterial == null)
            {
                Debug.LogError("Material not created");
                return;
            }

            if (!renderingData.cameraData.postProcessEnabled) return;


            if (outlineFeature == null) return;

            var cmd = CommandBufferPool.Get();

            using (new ProfilingScope(cmd, _profilingSampler))
            {
                // fullScreenMaterial.SetTexture("_MainTex",_maskIdentifier);
                // fullScreenMaterial.mainTexture = _maskIdentifier;

                fullScreenMaterial.SetFloat(WidthId, outlineFeature.width);
                fullScreenMaterial.SetFloat(SamplePrecisionId, outlineFeature.samplePrecision);
                fullScreenMaterial.SetColor(ColorId, outlineFeature.color);

                //var shaderPass = 0;

                //cmd.SetGlobalTexture("_CameraOpaqueTexture", renderingData.cameraData.renderer.cameraColorTarget);
                //cmd.GetTemporaryRT(destination,w,h,0,FilterMode.Point,RenderTextureFormat.Default);
                //cmd.Blit(renderingData.cameraData.renderer.cameraColorTarget, TmpRT);

                cmd.SetRenderTarget(renderingData.cameraData.renderer.cameraColorTarget);

                cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, fullScreenMaterial);
                //cmd.Blit(_maskIdentifier, renderingData.cameraData.renderer.cameraColorTarget, fullScreenMaterial,0);

                //cmd.Blit(destination,source,zoomBlurMaterial,shaderPass);
                //cmd.Blit(TmpRT, renderingData.cameraData.renderer.cameraColorTarget);
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            CommandBufferPool.Release(cmd);
        }

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            //Debug.Log("OnCameraCleanup");
            //cmd.ReleaseTemporaryRT(TmpId);
        }
    }

    [SerializeField] private LayerMask layerMask;

    RenderMaskPass renderMaskPass;
    FullScreenPass fullScreenPass;
    
    
    RenderTargetHandle maskTexture;

    /// <inheritdoc/>
    public override void Create()
    {
        var mat = CoreUtils.CreateEngineMaterial("MK/OutlineMask");
        var fullMat = CoreUtils.CreateEngineMaterial(fullShader);
        
        renderMaskPass = new RenderMaskPass(RenderQueueRange.opaque, layerMask, mat)
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing
        };

        fullScreenPass = new FullScreenPass( this,fullMat)
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing
        };
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderMaskPass.Setup(renderingData.cameraData.cameraTargetDescriptor, maskTexture,scale);
        renderer.EnqueuePass(renderMaskPass);
        renderer.EnqueuePass(fullScreenPass);
    }
}