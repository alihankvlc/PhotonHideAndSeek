using Photon.Pun;
using System;
using UnityEngine;

public class PlayerFollowCamera : MonoBehaviour
{
    [Header("References")]
    public Transform m_Player;
    [SerializeField] private Transform m_CameraTransform;

    [Header("Offsets")]
    [SerializeField] private Vector3 m_PivotOffset = new Vector3(0.0f, 1.7f, 0.0f);
    [SerializeField] private Vector3 m_CamOffset = new Vector3(0.0f, 0.0f, -3.0f);

    [Header("Settings")]
    [SerializeField] private float m_Smooth = 10f;
    [SerializeField] private float m_HorizontalAimingSpeed = 6f;
    [SerializeField] private float m_VerticalAimingSpeed = 6f;
    [SerializeField] private float m_MaxVerticalAngle = 30f;
    [SerializeField] private float m_MinVerticalAngle = -60f;
    [SerializeField] private LayerMask m_CollisionLayerMask;

    [Header("GhostShifting Settings")]
    [SerializeField] private Vector3 m_GhostShiftingCameraPivot;
    [SerializeField] private Vector3 m_GhostShiftingCameraOffset;
    [SerializeField] private float m_GhostShiftingHorizontalOffset;
    [SerializeField] private float m_GhostShiftingVerticalOffset;

    private float m_HorizotanlAngel = 0;
    private float m_VerticalAngle = 0;
    private Vector3 m_SmoothPivotOffset;
    private Vector3 m_SmoothCamOffset;
    private Vector3 m_TargetPivotOffset;
    private Vector3 m_TargetCamOffset;
    private float m_DefaultFOV;
    private float m_TargetFOV;
    private float m_TargetMaxVerticalAngle;
    private bool m_IsCustomOffset;

    public static Action OnGhostShiftingCameraOffset;
    public static Action OnResetTargetOffset;

    private void Awake()
    {
        m_CameraTransform = this.transform;
    }
    private void Start()
    {
        m_CameraTransform.position = m_Player.position + Quaternion.identity * m_PivotOffset + Quaternion.identity * m_CamOffset;

        m_CameraTransform.rotation = Quaternion.identity;

        m_SmoothPivotOffset = m_PivotOffset;
        m_SmoothCamOffset = m_CamOffset;
        m_DefaultFOV = m_CameraTransform.GetComponent<Camera>().fieldOfView;
        m_HorizotanlAngel = m_Player.eulerAngles.y;


        OnGhostShiftingCameraOffset += GhostShiftingCameraSettings;
        OnResetTargetOffset += ResetTargetOffsets;

        ResetTargetOffsets();
        ResetFOV();
        ResetMaxVerticalAngle();
    }
    private void OnDestroy()
    {
        OnGhostShiftingCameraOffset += GhostShiftingCameraSettings;
        OnResetTargetOffset += ResetTargetOffsets;
    }
    private void GhostShiftingCameraSettings()
    {
        SetTargetOffsets(m_GhostShiftingCameraPivot, m_GhostShiftingCameraOffset);
        SetHorizontalCamOffset(m_GhostShiftingHorizontalOffset);
        SetVerticalCameraOffset(m_GhostShiftingVerticalOffset);
    }
    private void LateUpdate()
    {
        if (!PhotonNetwork.InRoom) return;

        m_HorizotanlAngel += Mathf.Clamp(Input.GetAxis("Mouse X"), -1, 1) * m_HorizontalAimingSpeed;
        m_VerticalAngle += Mathf.Clamp(Input.GetAxis("Mouse Y"), -1, 1) * m_VerticalAimingSpeed;


        m_VerticalAngle = Mathf.Clamp(m_VerticalAngle, m_MinVerticalAngle, m_TargetMaxVerticalAngle);


        Quaternion camYRotation = Quaternion.Euler(0, m_HorizotanlAngel, 0);
        Quaternion aimRotation = Quaternion.Euler(-m_VerticalAngle, m_HorizotanlAngel, 0);
        m_CameraTransform.rotation = aimRotation;


        m_CameraTransform.GetComponent<Camera>().fieldOfView = Mathf.Lerp(m_CameraTransform.GetComponent<Camera>().fieldOfView, m_TargetFOV, Time.deltaTime);

        Vector3 baseTempPosition = m_Player.position + camYRotation * m_TargetPivotOffset;
        Vector3 noCollisionOffset = m_TargetCamOffset;
        while (noCollisionOffset.magnitude >= 0.2f)
        {
            if (DoubleViewingPosCheck(baseTempPosition + aimRotation * noCollisionOffset))
                break;
            noCollisionOffset -= noCollisionOffset.normalized * 0.2f;
        }
        if (noCollisionOffset.magnitude < 0.2f)
            noCollisionOffset = Vector3.zero;


        bool customOffsetCollision = m_IsCustomOffset && noCollisionOffset.sqrMagnitude < m_TargetCamOffset.sqrMagnitude;

        m_SmoothPivotOffset = Vector3.Lerp(m_SmoothPivotOffset, customOffsetCollision ? m_PivotOffset : m_TargetPivotOffset, m_Smooth * Time.deltaTime);
        m_SmoothCamOffset = Vector3.Lerp(m_SmoothCamOffset, customOffsetCollision ? Vector3.zero : noCollisionOffset, m_Smooth * Time.deltaTime);

        m_CameraTransform.position = m_Player.position + camYRotation * m_SmoothPivotOffset + aimRotation * m_SmoothCamOffset;
    }

    public void SetTargetOffsets(Vector3 newPivotOffset, Vector3 newCamOffset)
    {
        m_TargetPivotOffset = newPivotOffset;
        m_TargetCamOffset = newCamOffset;
        m_IsCustomOffset = true;
    }

    public void ResetTargetOffsets()
    {
        m_TargetPivotOffset = m_PivotOffset;
        m_TargetCamOffset = m_CamOffset;
        m_IsCustomOffset = false;
    }
    public void ResetVerticalCameraOffset()
    {
        m_TargetCamOffset.y = m_CamOffset.y;
    }

    public void SetVerticalCameraOffset(float y)
    {
        m_TargetCamOffset.y = y;
    }

    public void SetHorizontalCamOffset(float x)
    {
        m_TargetCamOffset.x = x;
    }

    public void SetFOV(float customFOV)
    {
        this.m_TargetFOV = customFOV;
    }

    public void ResetFOV()
    {
        this.m_TargetFOV = m_DefaultFOV;
    }

    public void SetMaxVerticalAngle(float angle)
    {
        this.m_TargetMaxVerticalAngle = angle;
    }

    public void ResetMaxVerticalAngle()
    {
        this.m_TargetMaxVerticalAngle = m_MaxVerticalAngle;
    }

    bool DoubleViewingPosCheck(Vector3 checkPos)
    {
        return ViewingPosCheck(checkPos) && ReverseViewingPosCheck(checkPos);
    }

    bool ViewingPosCheck(Vector3 checkPos)
    {
        Vector3 target = m_Player.position + m_PivotOffset;
        Vector3 direction = target - checkPos;
        if (Physics.SphereCast(checkPos, 0.2f, direction, out RaycastHit hit, direction.magnitude, m_CollisionLayerMask))
        {
            if (hit.transform != m_Player && !hit.transform.GetComponent<Collider>().isTrigger)
            {
                return false;
            }
        }
        return true;
    }

    bool ReverseViewingPosCheck(Vector3 checkPos)
    {
        Vector3 origin = m_Player.position + m_PivotOffset;
        Vector3 direction = checkPos - origin;
        if (Physics.SphereCast(origin, 0.2f, direction, out RaycastHit hit, direction.magnitude, m_CollisionLayerMask))
        {
            if (hit.transform != m_Player && hit.transform != transform && !hit.transform.GetComponent<Collider>().isTrigger)
            {
                return false;
            }
        }
        return true;
    }
    public float GetCurrentPivotMagnitude(Vector3 finalPivotOffset)
    {
        return Mathf.Abs((finalPivotOffset - m_SmoothPivotOffset).magnitude);
    }
}
