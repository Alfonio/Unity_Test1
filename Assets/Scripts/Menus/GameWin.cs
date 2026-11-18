using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Marca de victoria. Es estática: sobrevive al cambio de escena.
public static class GameWin
{
    public static bool Won;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnLaunch()
    {
        Won = false;
    }
}

/// <summary>
/// Se crea solo al iniciar el juego (no hace falta ponerlo en ningún objeto).
/// Cada vez que se carga la escena Home busca UI_WIN, UI_Menu y el Player por su cuenta:
///  - Si se ganó (GameWin.Won): oculta UI_Menu, prende UI_WIN, libera el cursor y congela al Player.
///  - Si no: se asegura de que UI_WIN esté oculto (por ejemplo después de Replay).
/// Los objetos tienen que llamarse exactamente "UI_WIN" y "UI_Menu".
/// </summary>
public class WinScreen : MonoBehaviour
{
    private const string HomeScene = "Home";
    private const string WinPanelName = "UI_WIN";
    private const string MenuPanelName = "UI_Menu";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        GameObject go = new GameObject("WinScreen");
        DontDestroyOnLoad(go);
        WinScreen ws = go.AddComponent<WinScreen>();

        // AfterSceneLoad ya pasó para la primera escena, así que la procesamos a mano.
        ws.OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != HomeScene) return;

        // Esperamos un frame para actuar después de los Start() de la escena
        // (MainMenu y FirstPersonController) y que no nos pisen el cursor ni el Player.
        StartCoroutine(ApplyNextFrame());
    }

    private IEnumerator ApplyNextFrame()
    {
        yield return null;

        UIPanel winPanel = null;
        UIPanel menuPanel = null;

        // Include: encuentra también los objetos desactivados.
        UIPanel[] panels = FindObjectsByType<UIPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (UIPanel panel in panels)
        {
            if (panel.gameObject.name == WinPanelName) winPanel = panel;
            else if (panel.gameObject.name == MenuPanelName) menuPanel = panel;
        }

        if (!GameWin.Won)
        {
            if (winPanel != null) winPanel.Hide();
            yield break;
        }

        GameWin.Won = false;   // se consume: Replay o Menu arrancan limpios

        FirstPersonController player = FindFirstObjectByType<FirstPersonController>(FindObjectsInactive.Include);

        if (menuPanel != null) menuPanel.Hide();
        if (winPanel != null) winPanel.Show();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (player != null)
            player.enabled = false;
    }
}