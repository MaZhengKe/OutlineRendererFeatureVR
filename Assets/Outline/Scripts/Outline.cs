using System;
using UnityEngine;

namespace KuanMi
{
    [RequireComponent(typeof(Renderer))]
    public class Outline : MonoBehaviour
    { 
        // public bool outline;
        //
        // private Renderer _renderer;
        //
        // public  uint renderingLayerMask ;
        // public  uint unMask ;
        // [Range(8, 31)] public int renderingLayer = 20;
        //
        //
        // private void Update()
        // {
        //     SetLayer();
        // }
        //
        // private void OnValidate()
        // {
        //     SetLayer();
        // }
        //
        // public void SetLayer()
        // {
        //     if (_renderer == null)
        //         _renderer = GetComponent<Renderer>();
        //     if (outline)
        //         _renderer.renderingLayerMask |= renderingLayerMask;
        //     else
        //         _renderer.renderingLayerMask &= unMask;
        // }
    }
}