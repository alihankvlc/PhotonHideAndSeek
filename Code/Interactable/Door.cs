using DG.Tweening;
using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;

public class Door : MonoBehaviourPun, IInteractable
{
    [SerializeField] private DoorEventType m_DoorState = DoorEventType.Closed;
    [SerializeField] private float m_OpenRotationValue;
    [SerializeField] private float m_CloseRotationValue;

    private const float ROTATION_DURATION = 0.75f;
    private const float DOOR_CLOSE_TIME = 10f;

    private bool m_IsAnimating = false;
    private float m_CloseElapsedTimer;

    private Outline m_Outline;

    private void Start()
    {
        m_Outline = GetComponent<Outline>();
        m_Outline.enabled = false;
    }

    private void Update()
    {
        if (m_DoorState == DoorEventType.Open)
        {
            m_CloseElapsedTimer += Time.deltaTime;
            if (m_CloseElapsedTimer >= DOOR_CLOSE_TIME || m_DoorState == DoorEventType.Closed)
                photonView.RPC(nameof(CloseDoor), RpcTarget.AllBuffered);
        }
    }
    public void Interact()
    {
        if (m_IsAnimating) return;

        m_IsAnimating = true;

        m_DoorState = (m_DoorState == DoorEventType.Open) ? DoorEventType.Closed : DoorEventType.Open;

        if (m_DoorState == DoorEventType.Open)
            photonView.RPC(nameof(OpenDoor), RpcTarget.AllBuffered);
        else
            photonView.RPC(nameof(CloseDoor), RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void OpenDoor()
    {
        AudioSource.PlayClipAtPoint(SoundManager.Instance.OpenDoor,transform.position,1f);
        PlayAnimation(m_OpenRotationValue, () =>
        {
            m_DoorState = DoorEventType.Open;
        });
    }
    private AudioClip clip;
    [PunRPC]
    private void CloseDoor()
    {
        PlayAnimation(m_CloseRotationValue, () =>
        {
            m_DoorState = DoorEventType.Closed;
            AudioSource.PlayClipAtPoint(SoundManager.Instance.CloseDoor, transform.position, 1f);
        });
        m_CloseElapsedTimer = 0;
    }

    private void PlayAnimation(float targetRotation, System.Action onComplete = null)
    {
        Vector3 openCloseRotation = new Vector3(0f, targetRotation, 0f);

        transform.DORotate(openCloseRotation, ROTATION_DURATION, RotateMode.Fast)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                m_IsAnimating = false;
                onComplete?.Invoke();
            });
    }
}
