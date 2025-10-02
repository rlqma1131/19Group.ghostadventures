using UnityEngine;

namespace _01.Scripts.Utilities
{
    public class TemporalSingleton<T> : MonoBehaviour where T : Component
    {
        protected static T instance;

        public static bool HasInstance => instance != null;
        public static T TryGetInstance() => HasInstance ? instance : null;

        public static T Instance {
            get {
                if (instance != null) return instance;
                
                instance = FindAnyObjectByType<T>();
                if (instance != null) return instance;
                
                var go = new GameObject(typeof(T).Name + " Auto-Generated");
                instance = go.AddComponent<T>();
                return instance;
            }
        }

        /// <summary>
        /// Make sure to call base.Awake() in override if you need awake
        /// </summary>
        protected virtual void Awake()
        {
            InitializeSingleton();
        }

        protected virtual void InitializeSingleton()
        {
            if (!Application.isPlaying) return;

            instance = this as T;
        }
    }
}