using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = (T)FindObjectOfType(typeof(T));
                if (_instance == null)
                {
                    Debug.Log($"{typeof(T)} 인스턴스가 없습니다! 씬에 배치되어 있어야 합니다.");
                }
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
            Debug.Log($"[Singleton] {typeof(T).Name} 생성됨 (DontDestroy)");
        }
        else if (_instance != this)
        {
            Debug.LogWarning($"[Singleton] 중복된 {typeof(T).Name} 감지됨 → 기존 인스턴스 유지, 이 인스턴스는 파괴됨");
            Destroy(gameObject);
        }
    }
}
