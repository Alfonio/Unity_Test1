using System;
using System.Collections;
using UnityEngine;

public class DeathSequence : MonoBehaviour
{
    public static event Action OnPlayerDied;

    [Header("Pantalla de muerte")]
    [SerializeField] private GameObject uiMuerte;
    [SerializeField] private float fadeDuration = 1.5f;

    [Header("Sonido")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sonidoDisparo;

    [Header("Retraso")]
    [SerializeField] private float delayAntesDelDisparo = 1.5f;

    private bool _isShowing;
    private bool _disparando;

    private void OnEnable()
    {
        OnPlayerDied += HandlePlayerDied;
    }

    private void OnDisable()
    {
        OnPlayerDied -= HandlePlayerDied;
    }

    // Lo llama el Animation Event del clip.(lo configure desde el modelo a este, se nota la flechita azul al tocar la linea de tiempo)
    public void Disparar()
    {
        if (_disparando) return;
        _disparando = true;
        StartCoroutine(DisparoConRetraso());
    }

    private IEnumerator DisparoConRetraso()
    {
        yield return new WaitForSeconds(delayAntesDelDisparo);

        if (audioSource != null && sonidoDisparo != null)
            audioSource.PlayOneShot(sonidoDisparo);

        OnPlayerDied?.Invoke();
    }

    private void HandlePlayerDied()
    {
        if (_isShowing) return;
        _isShowing = true;
        StartCoroutine(FadeInRoutine());
    }

    private IEnumerator FadeInRoutine()
    {
        CanvasGroup group = uiMuerte.GetComponent<CanvasGroup>();
        if (group == null) group = uiMuerte.AddComponent<CanvasGroup>();

        group.alpha = 0f;
        uiMuerte.SetActive(true);

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            group.alpha = Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }

        group.alpha = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}