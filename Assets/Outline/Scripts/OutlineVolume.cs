using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace KuanMi
{
    public class OutlineVolume : VolumeComponent, IPostProcessComponent
    {
        public FloatParameter power = new FloatParameter(0f);

        public bool IsActive()
        {
            return power.value>0f;
        }

        public bool IsTileCompatible()
        {
            return false;
        }
    }
}