using System;
using System.Collections;
using UnityEngine;

public class SendRayCaster
{
    private LayerMask m_RayLayerMask;
    private float m_RayMaxDistance;

    public delegate void ShapeRayDelegate(RaycastHit hitInfo);
    public event ShapeRayDelegate OnRayEvent;

    public delegate void SendRayMissDelegate();
    public event SendRayMissDelegate OnRayMissEvent;

    public SendRayCaster(float distance, LayerMask layerMask)
    {
        m_RayLayerMask = layerMask;
        m_RayMaxDistance = distance;
    }

    public void CastRay()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, m_RayMaxDistance, m_RayLayerMask, QueryTriggerInteraction.Ignore))
        {
            int hitLayer = hitInfo.collider.gameObject.layer;
            if (hitLayer != LayerMask.NameToLayer("Default"))
            {
                OnRayEvent?.Invoke(hitInfo);
                return;
            }
        }
        OnRayMissEvent?.Invoke();
    }
}