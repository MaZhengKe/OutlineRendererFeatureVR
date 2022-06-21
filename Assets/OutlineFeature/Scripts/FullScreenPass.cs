using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

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

    public FullScreenPass(OutlineFeature outlineFeature, Material mat)
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