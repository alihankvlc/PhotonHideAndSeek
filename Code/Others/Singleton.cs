
using Photon.Pun;
using UnityEngine;
public class Singleton<T> : MonoBehaviourPun where T : MonoBehaviourPun
{
    private static T m_Instance;
    public static new bool DontDestroyOnLoad = false;
    public static T Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = FindObjectOfType<T>();

                if (m_Instance == null)
                {
                    GameObject singletonObject = new GameObject(typeof(T).Name);
                    m_Instance = singletonObject.AddComponent<T>();
                }
            }

            return m_Instance;
        }
    }
    protected virtual void Awake()
    {
        if (m_Instance != null && m_Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            if (!DontDestroyOnLoad) return;

            m_Instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
    }
}