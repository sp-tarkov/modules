using UnityEngine;

namespace SPT.Reflection.Utils
{
    public static class HookObject
    {
        public static GameObject _object
        {
            get
            {
                GameObject result = GameObject.Find("SPT.Hook");

                if (result == null)
                {
                    result = new GameObject("SPT.Hook");
                    Object.DontDestroyOnLoad(result);
                }

                return result;
            }
        }

        public static T AddOrGetComponent<T>() where T : MonoBehaviour
        {
            return _object.GetOrAddComponent<T>();
        }
    }
}
