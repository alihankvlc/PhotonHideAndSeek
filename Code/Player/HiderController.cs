using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering.Universal;

public class HiderController : PlayerController, IDamageable
{
    [Header("Hider Variables")]
    [SerializeField] private PlayerForm m_PlayerForm;

    [Header("Interaction Settings")]
    [SerializeField] private LayerMask m_InteractableShapeLayerMask;
    [SerializeField] private float m_InteractionDistance;

    [Header("Visual Effects")]
    [SerializeField] private VisualEffects m_VisualEffects;

    [Header("Shape Shifting")]
    [SerializeField] private List<GameObject> m_ShapeList = new List<GameObject>();
    [SerializeField] private Transform m_HiderJockMesh;

    [Header("Ghost Shifting")]
    [SerializeField] private float m_FlySpeed;

    [Header("Ignored Objects in Ghost&Shape Form")]
    [SerializeField] private List<GameObject> m_IgnorePlayerHeadObjects;
    [SerializeField] private GameObject m_IgnorePlayerRootObject;

    [Header("Material Settings")]
    [SerializeField] private Material m_GhostMaterial;
    [SerializeField] private Material m_HumanMaterial;
    [Header("Others")]
    [SerializeField] private TextMeshProUGUI m_TMP_HiderUsername;

    private SendRayCaster m_RayCaster;
    private ShapeObjectPool m_ShapeObjectPool;

    private int m_CurrentShapeId;
    public bool IsAlive { get; set; } = true;

    private float m_HorizontalRotation;
    private float m_VerticalRotation;

    private float m_HorizontalFlyVelocity;
    private float m_VerticalFlyVelocity;

    private float m_FlyRotationSmoothDampTime = 0.12f;
    private float m_FlyRotationVelocity;

    private readonly int HIT_HASH_ID = Animator.StringToHash("Hit");
    private readonly int ALIVE_HASH_ID = Animator.StringToHash("Alive");
    private readonly int GHOSTFORM_HASH_ID = Animator.StringToHash("Ghost");
    public float Health { get; set; } = 100f;
    public static Action<bool> OnShowHiderTeamName;
    private void Awake()
    {
        m_ShapeObjectPool = new ShapeObjectPool(m_ShapeList, transform);
    }
    protected override void Start()
    {
        base.Start();


        UIManager.OnUpdatePlayerHealth?.Invoke(Health);

        if (!IsAlive)
            StartCoroutine(TransitionGhostForm(true));

        if (m_PlayerForm == PlayerForm.Shape)
        {
            TransitionShape_Shape(m_CurrentShapeId);
            SetActiveShapeObject(m_CurrentShapeId, true);
        }

        if (!m_PhotonView.IsMine)
        {
            m_TMP_HiderUsername.SetText(m_PhotonView.Owner.NickName);
            m_TMP_HiderUsername.transform.gameObject.SetActive(true);
        }

        OnShowHiderTeamName += UI_ShowPlayerName;
    }
    private void OnDestroy()
    {
        OnShowHiderTeamName -= UI_ShowPlayerName;
    }
    protected override void Update()
    {
        if (!m_PhotonView.IsMine) return;

        if (IsAlive)
        {
            base.Update();
            CastRay();
        }
        else
        {
            UpdateRotation();
            GhostMovement();
        }

        if (m_PlayerForm == PlayerForm.Shape)
        {
            if (Input.GetKeyDown(KeyCode.X))
                TransitionShape_Human();
        }
    }
    private void UI_ShowPlayerName(bool param)
    {
        m_TMP_HiderUsername.transform.gameObject.SetActive(param);
    }
    private void CastRay()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, m_InteractionDistance, m_InteractableShapeLayerMask, QueryTriggerInteraction.Ignore))
        {
            int hitLayer = hitInfo.collider.gameObject.layer;
            if (hitLayer != LayerMask.NameToLayer("Default") && hitLayer
                != LayerMask.NameToLayer("Interactable"))
            {
                Shape shape = hitInfo.collider.GetComponent<Shape>();
                if (shape != null)
                {
                    m_VisualEffects.VisibleOutline(hitInfo.collider.gameObject);
                    UIManager.OnShowTransistionShapePopup(true);

                    if (Input.GetKeyDown(KeyCode.E) && m_PhotonView.IsMine)
                        TransitionShape_Shape(shape.Id);

                    return;
                }
            }
        }
        else
        {
            UIManager.OnShowTransistionShapePopup(false);
            m_VisualEffects.HiddenOutline();
        }
    }
    private void GhostMovement()
    {

        Vector3 inputDirection = new Vector3(m_Input.Run.x, 0.0f, m_Input.Run.y);
        Vector3 moveDirection = Quaternion.Euler(m_VerticalRotation, m_HorizontalRotation, 0.0f) * inputDirection;
        moveDirection.Normalize();

        m_CharacterController.Move(moveDirection * m_FlySpeed * Time.deltaTime);

        m_HorizontalFlyVelocity = Input.GetAxis("Mouse X");
        m_VerticalFlyVelocity = Input.GetAxis("Mouse Y");

        m_Animator.SetFloat(HORIZONTAL_VELOCITY_ID, m_HorizontalFlyVelocity, m_AnimationStrafeDampTime, Time.deltaTime * m_SpeedChangeRate * 20f);
        m_Animator.SetFloat(VERTICAL_VELOCITY_ID, m_VerticalFlyVelocity, m_AnimationStrafeDampTime, Time.deltaTime * m_SpeedChangeRate * 20f);

    }
    private void UpdateRotation()
    {
        if (m_Input.Run != Vector2.zero)
        {
            m_HorizontalRotation = m_CameraMain.transform.eulerAngles.y;
            m_VerticalRotation = m_CameraMain.transform.eulerAngles.x;

            float horizontalRotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, m_HorizontalRotation, ref m_FlyRotationVelocity, m_FlyRotationSmoothDampTime);
            float verticalRotation = Mathf.SmoothDampAngle(transform.eulerAngles.x, m_VerticalRotation, ref m_FlyRotationVelocity, m_FlyRotationSmoothDampTime);

            transform.rotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0.0f);
        }
    }
    private IEnumerator TransitionGhostForm(bool param)
    {
        WaitForSeconds wait = new WaitForSeconds(1f);

        SkinnedMeshRenderer meshRenderer = m_HiderJockMesh.GetComponent<SkinnedMeshRenderer>();
        List<Material> materialsList = new List<Material>(meshRenderer.materials);

        int indexToRemove = 0;

        yield return wait;

        if (indexToRemove >= 0 && indexToRemove < materialsList.Count)
        {
            materialsList.RemoveAt(indexToRemove);
            materialsList.Add(param ? m_GhostMaterial : m_HumanMaterial);

            meshRenderer.materials = materialsList.ToArray();

            UpdatePlayerObjectVisibility(param);
            UpdateCameraOffsetAndUI(param, meshRenderer);
            UpdateShadowCastingMode(param, meshRenderer);

            m_Animator.SetBool(GHOSTFORM_HASH_ID, param);
            EnableAnimatorAndController();
        }
    }
    private void UpdatePlayerObjectVisibility(bool param, bool ignoreRoot = false)
    {
        m_IgnorePlayerHeadObjects.ForEach(r => r.SetActive(!param));
        if (ignoreRoot) m_IgnorePlayerRootObject.SetActive(!param);
    }
    private void UpdateCameraOffsetAndUI(bool param, SkinnedMeshRenderer meshRenderer)
    {
        if (!m_PhotonView.IsMine) return;

        if (param)
        {
            m_PhotonView.RPC(nameof(SyncDeathIsAliveForm), RpcTarget.AllBuffered, false);
            PlayerFollowCamera.OnGhostShiftingCameraOffset?.Invoke();
            UIManager.OnShowGhostFormScreen?.Invoke(true);
        }
        else
        {
            m_PhotonView.RPC(nameof(SyncDeathIsAliveForm), RpcTarget.AllBuffered, true);
            PlayerFollowCamera.OnResetTargetOffset?.Invoke();
            UIManager.OnShowGhostFormScreen?.Invoke(false);
        }
    }
    [PunRPC]
    private void UpdateShadowCastingMode(bool isGhostForm, SkinnedMeshRenderer meshRenderer)
    {
        meshRenderer.shadowCastingMode = isGhostForm ? UnityEngine.Rendering.ShadowCastingMode.Off
            : UnityEngine.Rendering.ShadowCastingMode.On;
    }
    [PunRPC]
    private void SyncDeathIsAliveForm(bool param)
    {
        m_PlayerForm = param ? PlayerForm.Human : PlayerForm.Ghost;
    }
    [PunRPC]
    private void SyncTransistionHuman()
    {
        m_CurrentShapeId = 0;
        m_PlayerForm = PlayerForm.Human;
        IgnoreHumanGameObjects(false);
    }
    private void EnableAnimatorAndController(bool param = true)
    {
        m_Animator.enabled = param;
        m_CharacterController.enabled = param;
    }
    private void TransitionShape_Human()
    {
        m_PhotonView.RPC(nameof(SetActiveShapeObject), RpcTarget.AllBuffered, m_CurrentShapeId, false);
        UIManager.OnPlayerFormShapeKeyInfo?.Invoke(false);
    }
    private void TransitionShape_Shape(int shapeId)
    {
        if (ShouldSkipTransition(shapeId))
            return;

        m_PhotonView.RPC(nameof(SetActiveShapeObject), RpcTarget.AllBuffered, m_CurrentShapeId, false);
        m_PhotonView.RPC(nameof(SetActiveShapeObject), RpcTarget.AllBuffered, shapeId, true);
        m_PhotonView.RPC(nameof(ApplyShapeTransitionEffects), RpcTarget.All, shapeId);

        UIManager.OnPlayerFormShapeKeyInfo?.Invoke(true);
        m_PhotonView.RPC(nameof(SyncTransisitonShapeId), RpcTarget.AllBuffered, shapeId);
    }
    [PunRPC]
    private void SyncTransisitonShapeId(int shapeId)
    {
        m_PlayerForm = PlayerForm.Shape;
        m_CurrentShapeId = shapeId;
    }
    [PunRPC]
    private void SetActiveShapeObject(int id, bool param)
    {
        GameObject shapeObject = m_ShapeObjectPool.GetShapeObject(id);

        if (shapeObject != null && id != 0)
        {
            shapeObject.SetActive(param);
            IgnoreHumanGameObjects(param);
            m_PlayerForm = param ? PlayerForm.Shape : PlayerForm.Human;
        }
    }
    [PunRPC]
    private void ApplyShapeTransitionEffects(int shapeId)
    {
        m_VisualEffects.ShakeObject(m_ShapeObjectPool.GetShapeObject(shapeId));
        m_VisualEffects.PlayTransistionShapeEffect();
    }
    private void IgnoreHumanGameObjects(bool param)
    {
        GameObject hiderJock = m_HiderJockMesh.transform.gameObject;
        hiderJock.SetActive(!param);
        UpdatePlayerObjectVisibility(param, true);
    }
    private bool ShouldSkipTransition(int shapeId)
    {
        return m_PlayerForm == PlayerForm.Shape && m_CurrentShapeId == shapeId;
    }
    public void TakeDamage(float amount)
    {
        if (m_PlayerForm == PlayerForm.Ghost || !IsAlive) return;

        if (m_PhotonView.IsMine && Health > 0)
        {
            UIManager.OnUpdatePlayerHealth?.Invoke(Health);
            m_PhotonView.RPC(nameof(SyncTakeDamage), RpcTarget.All, amount);
        }

        if (Health <= 0)
        {
            m_PhotonView.RPC(nameof(SyncDeath), RpcTarget.AllBuffered);
            m_PhotonView.RPC(nameof(SyncDeathEffect), RpcTarget.All);
        }
    }
    [PunRPC]
    private void SyncTakeDamage(float amount)
    {
        Health = Mathf.Max(0, Health - amount);
        m_Animator.Play(HIT_HASH_ID);
        AudioSource.PlayClipAtPoint(SoundManager.Instance.GetHit, transform.position, 1f);
    }
    [PunRPC]
    private void SyncDeath()
    {
        if (m_PlayerForm == PlayerForm.Shape)
            TransitionShape_Human();

        Death();
    }
    public void Death()
    {
        IsAlive = false;
        EnableAnimatorAndController(false);
        StartCoroutine(TransitionGhostForm(true));
    }
    [PunRPC]
    private void SyncDeathEffect()
    {
        AudioSource.PlayClipAtPoint(SoundManager.Instance.Death, transform.position, 1f);
        StartCoroutine(m_VisualEffects.PlayDeathEffectCoroutine());
    }
}
