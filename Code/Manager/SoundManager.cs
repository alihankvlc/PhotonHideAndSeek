using System.Collections;
using UnityEngine;



/// <summary>
/// Tamamen test amaçlı daha sonra yenilenecek....
/// </summary>
public class SoundManager : MonoBehaviour
{
    private static SoundManager m_Instance;
    public static SoundManager Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = FindObjectOfType<SoundManager>();

                if (m_Instance == null)
                {
                    GameObject singletonObject = new GameObject(typeof(SoundManager).Name);
                    m_Instance = singletonObject.AddComponent<SoundManager>();
                }
            }
            return m_Instance;
        }
    }

    public AudioClip OpenDoor;
    public AudioClip CloseDoor;
    public AudioClip MeleeAttack;
    public AudioClip Death;
    public AudioClip GetHit;
    public AudioClip HiderSelection;
    public AudioClip SeekerSelection;
}