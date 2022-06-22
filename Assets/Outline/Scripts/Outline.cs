using System;
using UnityEngine;

namespace KuanMi
{
    [RequireComponent(typeof(Renderer))]
    public class Outline : MonoBehaviour
    {
        [SerializeField] private bool show;

        private Renderer _renderer;

        public OutlineFeature.RenderLayer renderLayer;

        public void Show()
        {
            show = true;
            SetLayer();
        }

        public void Hide()
        {
            show = false;
            SetLayer();
        }

        private void OnValidate()
        {
            SetLayer();
        }

        private void SetLayer()
        {
            if (_renderer == null)
                _renderer = GetComponent<Renderer>();
            if (show)
                _renderer.renderingLayerMask |= (uint)renderLayer;
            else
                _renderer.renderingLayerMask &= ~(uint)renderLayer;
        }
    }
}