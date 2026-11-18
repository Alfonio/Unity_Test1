using UnityEngine;

// Implementa IMenuPanel (definida en MainMenu.cs).
// Sirve para cualquier panel: Tutorial, Opciones, Pausa, etc.
public class UIPanel : MonoBehaviour, IMenuPanel
{
    public bool IsVisible => gameObject.activeSelf;

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}