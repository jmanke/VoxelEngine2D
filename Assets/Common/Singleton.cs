using UnityEngine;

namespace Hazel
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance { get; private set; }

        protected virtual void Awake()
        {
            if (Instance != null)
            {
                Destroy(this.gameObject);
                return;
            }

            Instance = this as T;
            DontDestroyOnLoad(this.gameObject);
        }
    }
}

