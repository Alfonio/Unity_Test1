using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Se crea solo al cargar la escena "Muerte" (no hace falta ponerlo en ningún objeto).
/// Congela al jugador apenas carga la escena.
/// </summary>
public class DeathScene : MonoBehaviour
{
    private const string DeathSceneName = "Muerte";

    /// <summary>
    /// Unity llama a este método solo, una vez, al arrancar el juego.
    /// Se suscribe al evento de carga de escenas.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != DeathSceneName) return;

        GameObject go = new GameObject("DeathScene");
        go.AddComponent<DeathScene>();
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("DeathScene: no encontré ningún objeto con el tag Player");
            return;
        }

        // Apaga todos los scripts del jugador (movimiento, mirar, interactuar).
        foreach (MonoBehaviour script in player.GetComponentsInChildren<MonoBehaviour>())
        {
            if (script != null) script.enabled = false;
        }

        // Apaga el CharacterController si tiene.
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        // Si usa Rigidbody, lo deja quieto.
        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        Debug.Log("Moriste");
    }
}