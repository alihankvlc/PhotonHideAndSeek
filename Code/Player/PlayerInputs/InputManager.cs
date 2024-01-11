using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [SerializeField] private PlayerInput m_PlayerInput;

    private InputActionMap m_InputActionMap;
    private InputAction m_RunInputAction;
    private InputAction m_LookInputAction;
    private InputAction m_SprintInputAction;
    private InputAction m_JumpInputAction;
    private InputAction m_CrouchInputAction;
    private InputAction m_SlideInputAction;

    public Vector2 Run { get; private set; }
    public Vector2 Look { get; private set; }
    public bool Sprint { get; private set; }
    public bool Jump { get; private set; }
    public bool Crouch { get; private set; }
    public bool Slide { get; private set; }
    public bool Interact { get; private set; }

    private void Awake()
    {
        m_InputActionMap = m_PlayerInput.currentActionMap;

        m_RunInputAction = m_InputActionMap.FindAction("Run");
        m_LookInputAction = m_InputActionMap.FindAction("Look");
        m_SprintInputAction = m_InputActionMap.FindAction("Sprint");
        m_JumpInputAction = m_InputActionMap.FindAction("Jump");
        m_CrouchInputAction = m_InputActionMap.FindAction("Crouch");
        m_SlideInputAction = m_InputActionMap.FindAction("Slide");

    }
    private void Start()
    {
        m_PlayerInput = m_PlayerInput ?? GetComponent<PlayerInput>();
    }
    private void OnEnable()
    {
        m_InputActionMap.Enable();

        m_RunInputAction.performed += OnMove;
        m_RunInputAction.canceled += OnMove;

        m_LookInputAction.performed += OnLook;
        m_LookInputAction.canceled += OnLook;

        m_SprintInputAction.performed += OnRun;
        m_SprintInputAction.canceled += OnRun;

        m_CrouchInputAction.performed += OnCrouch;
        m_CrouchInputAction.canceled += OnCrouch;

        m_SlideInputAction.started += OnSlide;
        m_SlideInputAction.canceled += OnSlide;

        m_JumpInputAction.performed += OnJump;
        m_JumpInputAction.canceled += OnJump;
    }
    private void OnDisable()
    {
        m_InputActionMap.Disable();

        m_RunInputAction.performed -= OnMove;
        m_RunInputAction.canceled -= OnMove;

        m_LookInputAction.performed -= OnLook;
        m_LookInputAction.canceled -= OnLook;

        m_SprintInputAction.performed -= OnRun;
        m_SprintInputAction.canceled -= OnRun;

        m_CrouchInputAction.performed -= OnCrouch;
        m_CrouchInputAction.canceled -= OnCrouch;

        m_SlideInputAction.started -= OnSlide;
        m_SlideInputAction.canceled -= OnSlide;

        m_JumpInputAction.performed -= OnJump;
        m_JumpInputAction.canceled -= OnJump;

    }
    private void OnMove(InputAction.CallbackContext context) => Run = context.ReadValue<Vector2>();
    private void OnLook(InputAction.CallbackContext context) => Look = context.ReadValue<Vector2>();
    private void OnRun(InputAction.CallbackContext context) => Sprint = context.ReadValueAsButton();
    private void OnCrouch(InputAction.CallbackContext context) => Crouch = context.ReadValueAsButton();
    private void OnSlide(InputAction.CallbackContext context) => Slide = context.ReadValueAsButton();
    private void OnJump(InputAction.CallbackContext context) => Jump = context.ReadValueAsButton();
}
