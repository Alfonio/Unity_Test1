using UnityEngine;

public class Button : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        switch (gameObject.name)
        {
            case "anomalyButton":
                Debug.Log("Botón rojo (anomalía)");
                break;

            case "safeButton":
                Debug.Log("Botón verde (seguro)");
                break;

            default:
                Debug.LogWarning("Botón sin nombre reconocido: " + gameObject.name);
                break;
        }
    }
}