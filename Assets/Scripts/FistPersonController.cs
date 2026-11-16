using UnityEngine;
using UnityEngine.InputSystem;
//Script hijo de CharacterBase. En este se determina el script de 
public class FirstPersonController : CharacterBase
{
    [SerializeField] private float sensitivity = 0.2f;
    [SerializeField] private float minLimit = -80f;
    [SerializeField] private float maxLimit = 80f;
    [SerializeField] private Transform cameraTransform;

    private PlayerInputAction _inputAction;
    private Vector2 _movement;
    private Vector2 _look;
    private float _currentRotationY;

    public float Sensitivity
    {
        get => sensitivity;
        set => sensitivity = Mathf.Max(0f, value);
    }

    protected override void Awake()
    {
        base.Awake();
        _inputAction = new PlayerInputAction();
    }

    private void OnEnable()
    {
        _inputAction.Player.Enable();

        _inputAction.Player.Move.performed += SetMovement;
        _inputAction.Player.Move.canceled += ResetMovement;

        _inputAction.Player.Look.performed += SetLook;
        _inputAction.Player.Look.canceled += ResetLook;
    }

    private void OnDisable()
    {
        _inputAction.Player.Move.performed -= SetMovement;
        _inputAction.Player.Move.canceled -= ResetMovement;

        _inputAction.Player.Look.performed -= SetLook;
        _inputAction.Player.Look.canceled -= ResetLook;

        _inputAction.Player.Disable();
    }

    private void Update()
    {
        Move(_movement);
        Look();
    }

    private void SetMovement(InputAction.CallbackContext ctx) => _movement = ctx.ReadValue<Vector2>();
    private void ResetMovement(InputAction.CallbackContext ctx) => _movement = Vector2.zero;

    private void SetLook(InputAction.CallbackContext ctx) => _look = ctx.ReadValue<Vector2>();
    private void ResetLook(InputAction.CallbackContext ctx) => _look = Vector2.zero;

    private void Look()
    {
        Vector2 mouseNormalized = _look * sensitivity;

        _currentRotationY = Mathf.Clamp(_currentRotationY - mouseNormalized.y, minLimit, maxLimit);

        cameraTransform.localRotation = Quaternion.Euler(_currentRotationY, 0f, 0f);
        transform.Rotate(Vector3.up * mouseNormalized.x);
    }
}