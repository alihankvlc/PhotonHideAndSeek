using Photon.Pun;
using Photon.Realtime;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(InputManager))]
public class PlayerController : MonoBehaviourPunCallbacks
{
    [Header("Cursor Visible")]
    [SerializeField] private bool m_CursorLocked;

    [Header("PlayerInfo")]
    public PlayerInfo m_PlayerInfo;

    [Header("Movement Varabiles")]
    [SerializeField] private float m_RunSpeed = 4f;
    [SerializeField] private float m_SprintSpeed = 7f;
    [SerializeField] private float m_CrounchSpeed = 1f;
    [SerializeField] private float m_JumpHeight = 1f;
    [SerializeField] private float m_JumpableDuration = 1.35f;
    [SerializeField] private float m_RotationSmoothDampTime = 0.12f;
    [SerializeField] private float m_SpeedOffset = 0.1f;

    [Header("Ground Check Variables")]
    [SerializeField] private bool m_IsGrounded;
    [SerializeField] private float m_GroundRadius = 0.12f;
    [SerializeField] private float m_GroundOffset = 0.08f;
    [SerializeField] private LayerMask m_GroundLayerMask;

    private float m_TargetSpeed;
    private float m_TargetVelocitySpeed;
    private float m_TargetRotation;

    protected float m_AnimationStrafeDampTime = 50f;
    protected float m_SpeedChangeRate = 20f;

    private Vector3 m_VerticalVelocity;

    private const float GRAVITY = -20f;


    private float m_JumpableIntervalTimer;

    private float m_RotationCurrentVelocity;
    private float m_MovementAnimBlend;

    private bool m_IsJump = true;


    protected CharacterController m_CharacterController;
    protected Animator m_Animator;
    protected InputManager m_Input;
    protected Camera m_CameraMain;
    protected PhotonView m_PhotonView;
    protected PlayerBoard m_PlayerBoard;

    private readonly int SPEED_HASH_ID = Animator.StringToHash("Speed");
    private readonly int GROUNDED_HASH_ID = Animator.StringToHash("Grounded");
    private readonly int JUMP_HASH_ID = Animator.StringToHash("Jump");
    private readonly int CROUCH_HASH_ID = Animator.StringToHash("Crouch");

    protected readonly int HORIZONTAL_VELOCITY_ID = Animator.StringToHash("HorizontalVelocity");
    protected readonly int VERTICAL_VELOCITY_ID = Animator.StringToHash("VerticalVelocity");
    public PlayerTeam GetPlayerTeam => m_PlayerInfo.Team;
    public string GetPlayerName { get => m_PlayerInfo.PlayerName; set => m_PlayerInfo.PlayerName = value; }

    public void UI_RemovePlayerContent()
    {
        m_PlayerBoard.RemoveContentCache(m_PhotonView.Owner.ActorNumber);
    }
    protected virtual void Start()
    {
        m_CharacterController = GetComponent<CharacterController>();
        m_Animator = GetComponent<Animator>();
        m_Input = GetComponent<InputManager>();
        m_PhotonView = GetComponent<PhotonView>();

        m_CharacterController.height = 1.55f;
        m_CharacterController.center = new Vector3(0.0f, 0.93f, 0.0f);

        m_CameraMain = Camera.main;

        if (m_PhotonView.IsMine)
        {
            Cursor.lockState = m_CursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = m_CursorLocked;

            m_CameraMain.GetComponent<PlayerFollowCamera>().m_Player = transform;
            m_CameraMain.GetComponent<PlayerFollowCamera>().enabled = true;

            m_PlayerInfo.PlayerMS = PhotonNetwork.GetPing();
            m_PlayerInfo.PlayerName = UIManager.OnPlayerSetName?.Invoke();
            PhotonNetwork.NickName = m_PlayerInfo.PlayerName;

            UIManager.OnPlayerStatusInfo?.Invoke(m_PlayerInfo.PlayerName,
                 m_PlayerInfo.Team == PlayerTeam.Hider ? PlayerStatus.JoinedHiderTeam :
                    m_PlayerInfo.Team == PlayerTeam.Seeker ? PlayerStatus.JoinedSeekerTeam : PlayerStatus.EnteredGame); // BUG VAR

            UIManager.OnShowPlayerStat?.Invoke(m_PlayerInfo.Team, true);
            GameManager.OnIncreasePlayerCount(m_PlayerInfo.Team);

        }
        GameManager.OnRegisterPlayerCache(m_PhotonView.Owner.ActorNumber, this);
        m_PlayerBoard = new PlayerBoard(m_PhotonView.Owner.ActorNumber, m_PhotonView.Owner.NickName, 0, m_PlayerInfo.Team);
    }
    protected virtual void Update()
    {
        if (!m_PhotonView.IsMine) return;

        Movement();
        Jump();
        Crouch();

        HiderController.OnShowHiderTeamName?.Invoke(m_PlayerInfo.Team == PlayerTeam.Hider);
        SeekerController.OnShowSeekerPlayerName?.Invoke(m_PlayerInfo.Team == PlayerTeam.Seeker);
        m_PlayerBoard.SetUpdatePlayerMs(PhotonNetwork.GetPing());
    }
    private void Movement()
    {
        m_TargetSpeed = CanMove() ? (!IsCrouch() ? (IsSprint() ? m_SprintSpeed : m_RunSpeed) : 1) : 0;

        Vector3 inputDirection = new Vector3(m_Input.Run.x, 0.0f, m_Input.Run.y);

        UpdateRotation();

        Vector3 moveDirection = Quaternion.Euler(0.0f, m_TargetRotation, 0.0f) * inputDirection;
        moveDirection.Normalize();

        m_CharacterController.Move((moveDirection * CalculateVelocitySpeed() + Gravity()) * Time.deltaTime);

        UpdateAnimationBlend();
    }
    private float CalculateVelocitySpeed()
    {
        float currentSpeed = new Vector3(m_CharacterController.velocity.x, 0.0f, m_CharacterController.velocity.z).magnitude;
        float speedDifference = Mathf.Abs(currentSpeed - m_TargetSpeed);

        if (speedDifference > m_SpeedOffset)
        {
            m_TargetVelocitySpeed = Mathf.MoveTowards(currentSpeed, m_TargetSpeed, Time.deltaTime * m_SpeedChangeRate);
            m_TargetVelocitySpeed = Mathf.Round(m_TargetVelocitySpeed * 1000f) / 1000f;
        }
        else
            m_TargetVelocitySpeed = m_TargetSpeed;

        return m_TargetVelocitySpeed;
    }
    private void UpdateAnimationBlend()
    {
        m_MovementAnimBlend = Mathf.Lerp(m_MovementAnimBlend, m_TargetSpeed, Time.deltaTime * m_SpeedChangeRate);
        if (m_MovementAnimBlend < 0.01f) m_MovementAnimBlend = 0f;

        float horizontalVelocity = m_Input.Run.x;
        float verticalVelocity = m_Input.Run.y;

        m_Animator.SetFloat(SPEED_HASH_ID, m_TargetVelocitySpeed);
        m_Animator.SetFloat(HORIZONTAL_VELOCITY_ID, horizontalVelocity, m_AnimationStrafeDampTime, Time.deltaTime * m_SpeedChangeRate * 20f);
        m_Animator.SetFloat(VERTICAL_VELOCITY_ID, verticalVelocity, m_AnimationStrafeDampTime, Time.deltaTime * m_SpeedChangeRate * 20f);
    }
    private void UpdateRotation()
    {
        if (m_Input.Run != Vector2.zero)
        {
            m_TargetRotation = m_CameraMain.transform.eulerAngles.y;

            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, m_TargetRotation, ref m_RotationCurrentVelocity, m_RotationSmoothDampTime);

            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }
    }
    public Vector3 Gravity()
    {
        if (IsGrounded() && m_VerticalVelocity.y < 0.0f)
        {
            m_VerticalVelocity.y = -2f;
        }

        m_VerticalVelocity.y += Time.deltaTime * GRAVITY;
        return m_VerticalVelocity;
    }
    private bool IsGrounded()
    {
        Vector3 spherePosition = transform.position + Vector3.up * m_GroundOffset;
        m_IsGrounded = Physics.CheckSphere(spherePosition, m_GroundRadius, m_GroundLayerMask, QueryTriggerInteraction.Ignore);

        m_Animator.SetBool(GROUNDED_HASH_ID, m_IsGrounded);
        return m_IsGrounded;
    }
    private void Crouch()
    {
        m_Animator.SetBool(CROUCH_HASH_ID, IsCrouch());
    }
    private void Jump()
    {
        UpdateJumpableInterval();

        if (IsReadyToJump())
        {
            PerformJump();
            ResetJumpState();
        }
    }

    private void UpdateJumpableInterval()
    {
        if (!m_IsJump)
        {
            m_JumpableIntervalTimer += Time.deltaTime;
            if (m_JumpableIntervalTimer >= m_JumpableDuration)
            {
                m_IsJump = true;
            }
        }
    }

    private bool IsReadyToJump()
    {
        return m_IsJump && m_Input.Jump && m_IsGrounded && !IsCrouch() && CanMove();
    }

    private void PerformJump()
    {
        m_VerticalVelocity.y = Mathf.Sqrt(m_JumpHeight * -2 * GRAVITY);
        m_Animator.CrossFade(JUMP_HASH_ID, 0.5f);
    }
    private void ResetJumpState()
    {
        m_JumpableIntervalTimer = 0;
        m_IsJump = false;
    }
    #region CheckAction
    private bool IsSprint()
    {
        return m_Input.Sprint && m_Input.Run.y > 0;
    }
    private bool IsCrouch()
    {
        return m_Input.Crouch;
    }
    private bool CanMove()
    {
        return m_Input.Run != Vector2.zero;
    }
    #endregion
    private void OnDrawGizmos()
    {
        Gizmos.color = m_IsGrounded ? Color.yellow : Color.red;
        Vector3 spherePosition = transform.position + Vector3.up * m_GroundOffset;
        Gizmos.DrawWireSphere(spherePosition, m_GroundRadius);
    }
}