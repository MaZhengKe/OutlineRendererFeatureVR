using KuanMi;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.Rendering.Universal;
using UnityEngine;

namespace Outline.Editor
{
    
    [CustomEditor(typeof(OutlineFeature))]
    public class OutlineRenderFeatureEditor : ScriptableRendererFeatureEditor
    {
        public override void OnInspectorGUI()
        {
            var outlineFeature = target as OutlineFeature;
            
            base.OnInspectorGUI();
            if (GUILayout.Button("清理 render layer mask"))
            {
                outlineFeature.CleanOutLayer();
            }
        }
    }
}