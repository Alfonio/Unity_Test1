using UnityEngine;
using UnityEngine.InputSystem;

public interface IInteractable
{
    void Interact();
}

public class InteractionSystem : MonoBehaviour
{
    public Camera cam;
    public float distance = 3f;

    void Update()
    {
        // Solo actúa cuando se hace click izquierdo (una vez por click)
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
            return;

        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, distance)
            && hit.collider.CompareTag("AnomalyButton")
            && hit.collider.TryGetComponent(out IInteractable i))
        {
            i.Interact();
        }
    }
}