using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SceneFader : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private float fadeInDuration = 10f;

    // Propiedades protegidas: las clases hijas pueden leerlas, pero no reemplazarlas
    protected Image Image => image;
    protected float FadeInDuration => fadeInDuration;

    // Alpha inicial y final: las hijas pueden sobrescribirlos para invertir el efecto
    protected virtual float StartAlpha => 1f;
    protected virtual float EndAlpha => 0f;

    protected virtual void Start()
    {
        StartCoroutine(FadeIn(fadeInDuration));
    }

    protected virtual IEnumerator FadeIn(float duration)
    {
        float t = 0f;
        SetAlpha(StartAlpha);

        while (t < duration)
        {
            t += Time.deltaTime;
            SetAlpha(Mathf.Lerp(StartAlpha, EndAlpha, t / duration));
            yield return null;
        }

        SetAlpha(EndAlpha);
        OnFadeInCompleted();
    }

    // Hook para que las hijas reaccionen cuando termina el fade
    protected virtual void OnFadeInCompleted() { }

    protected virtual void SetAlpha(float alpha)
    {
        Color c = image.color;
        c.a = alpha;
        image.color = c;
    }
}