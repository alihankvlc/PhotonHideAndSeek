using Photon.Pun;
using System.Collections;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    [SerializeField] private LayerMask m_InteractableLayerMask;
    [SerializeField] private float m_InteractableDistance;

    private SendRayCaster m_RayCaster;
    private VisualEffects m_VisualEffects;

    private void Start()
    {
        m_RayCaster = new SendRayCaster(m_InteractableDistance, m_InteractableLayerMask);
        m_VisualEffects = new VisualEffects();
    }

    private void Update()
    {
        CastRay();
    }
    private void CastRay()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, m_InteractableDistance, m_InteractableLayerMask, QueryTriggerInteraction.Ignore))
        {
            int hitLayer = hitInfo.collider.gameObject.layer;
            if (hitLayer != LayerMask.NameToLayer("Default") && hitLayer
                != LayerMask.NameToLayer("Shape"))
            {
                m_VisualEffects.VisibleOutline(hitInfo.collider.gameObject);
                UIManager.OnShowInteractPopup(true);
                if (!Input.GetKeyDown(KeyCode.E)) return;

                IInteractable interactableObject = hitInfo.collider.GetComponent<IInteractable>();
                interactableObject?.Interact();
            }
        }
        else
        {
            m_VisualEffects.HiddenOutline();
            UIManager.OnShowInteractPopup(false);
        }

    }
}