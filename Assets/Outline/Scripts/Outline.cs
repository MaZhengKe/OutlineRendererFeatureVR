using System;
using UnityEngine;

namespace KuanMi
{
    [RequireComponent(typeof(Renderer))]
    public class Outline : MonoBehaviour
    {
        public bool outline;

        private Renderer _renderer;

        private void Update()
        {
            SetLayer();
        }

        private void OnValidate()
        {
            SetLayer();
        }

        public void SetLayer()
        {
            if (_renderer == null)
                _renderer = GetComponent<Renderer>();
            if (outline)
                _renderer.renderingLayerMask |= OutlineFeature.renderingLayerMask;
            else
                _renderer.renderingLayerMask &= OutlineFeature.unMask;
        }
    }
}