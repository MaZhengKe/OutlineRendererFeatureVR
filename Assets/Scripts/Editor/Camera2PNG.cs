using UnityEditor;
using UnityEngine;

namespace UnityTemplateProjects.Editor
{
    public class Camera2PNG : MonoBehaviour
    {
        [MenuItem("Create/png")]
        public static void Create()
        {
            ScreenCapture.CaptureScreenshot("test.png");
        }
    }
}