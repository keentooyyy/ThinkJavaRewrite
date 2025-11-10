using System.Collections;
using UnityEngine;

namespace GameProgress
{
    /// <summary>
    /// Helper class for running coroutines from non-MonoBehaviour contexts
    /// </summary>
    public class CoroutineHelper : MonoBehaviour
    {
        private static CoroutineHelper instance;

        public static CoroutineHelper Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("CoroutineHelper");
                    instance = go.AddComponent<CoroutineHelper>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        public static Coroutine StartStaticCoroutine(IEnumerator coroutine)
        {
            return Instance.StartCoroutine(coroutine);
        }

        public static void StopStaticCoroutine(Coroutine coroutine)
        {
            if (instance != null && coroutine != null)
            {
                instance.StopCoroutine(coroutine);
            }
        }
    }
}

