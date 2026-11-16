using UnityEngine;
//Este es el script core para despues heredar el movimiento y la gravedad en el videojuego para cualquier enemigo o personaje

[RequireComponent(typeof(CharacterController))]
public abstract class CharacterBase : MonoBehaviour
{
    [SerializeField] private float movementSpeed = 8f;
    [SerializeField] private float gravity = -9.8f;

    private CharacterController _characterController;
    private Vector3 _velocity;

    public float MovementSpeed
    {
        get => movementSpeed;
        set => movementSpeed = Mathf.Max(0f, value);
    }

    protected virtual void Awake()
    {
        _characterController = GetComponent<CharacterController>();
    }

    // Mueve al personaje en el plano XZ según el input y aplica la gravedad
    protected void Move(Vector2 input)
    {
        Vector3 move = transform.right * input.x + transform.forward * input.y;
        _characterController.Move(move * movementSpeed * Time.deltaTime);

        ApplyGravity();
    }

    private void ApplyGravity()
    {
        if (_characterController.isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f;
        }

        _velocity.y += gravity * Time.deltaTime;
        _characterController.Move(_velocity * Time.deltaTime);
    }
}