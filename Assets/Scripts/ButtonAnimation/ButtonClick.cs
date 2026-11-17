using UnityEngine;
using UnityEngine.Events;

public class ButtonInteractable : MonoBehaviour
{
    [Header("Eventos")]
    [SerializeField] private UnityEvent onPressed;

    public void Press()
    {
        Debug.Log("click");
        onPressed?.Invoke();
    }
}
