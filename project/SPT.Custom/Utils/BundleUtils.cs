using UnityEngine;

namespace SPT.Custom.Utils
{
    public class BundleUtils : MonoBehaviour
    {
        private GameObject rootObject;
        private int current;
        private int maximum;
        private string bundleName;
        private Texture2D bgTexture;

        public static BundleUtils Create()
        {
            GameObject bundleUtilsObject = new GameObject("BundleUtilsObject");
            BundleUtils bundleUtils = bundleUtilsObject.AddComponent<BundleUtils>();
            bundleUtils.rootObject = bundleUtilsObject;
            bundleUtils.current = 0;
            bundleUtils.maximum = 0;
            bundleUtils.enabled = true;
            bundleUtils.bgTexture = new Texture2D(2, 2);
            return bundleUtils;
        }

        public void Init(int length)
        {
            maximum = length;
        }

        public void SetProgress(int progress, string fileName)
        {
            current = progress;
            bundleName = fileName;
        }

        public void Dispose()
        {
            Destroy(rootObject);
            Destroy(this);
        }

        public void OnGUI()
        {
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUI.skin.window.alignment = TextAnchor.MiddleCenter;
            GUI.skin.window.normal.background = bgTexture;
            GUI.backgroundColor = Color.black;

            GUI.Window(0, CreateRectangle(500, 80), DrawWindow, "Bundle Loading");
        }

        private Rect CreateRectangle(int width, int height)
        {
            return new Rect((Screen.width / 2) - (width / 2),
                (Screen.height / 2) - (height / 2),
                width,
                height);
        }

        private void DrawWindow(int windowId)
        {
            GUI.Label(new Rect(0, 35, 500, 20), $"Loading bundle: {current} / {maximum}");
            GUI.Label(new Rect(0, 50, 500, 20), bundleName);
        }
    }
}
