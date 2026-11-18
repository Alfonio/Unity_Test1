using UnityEngine;

public class Button : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        if (SceneChanger.Instance == null) return;

        switch (gameObject.name)
        {
            case "anomalyButton":
                Debug.Log("Botón rojo (anomalía)");
                SceneChanger.Instance.PressButton(true);
                break;

            case "safeButton":
                Debug.Log("Botón verde (seguro)");
                SceneChanger.Instance.PressButton(false);
                break;

            default:
                Debug.LogWarning("Botón sin nombre reconocido: " + gameObject.name);
                break;
        }
    }
}