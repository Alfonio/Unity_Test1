using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Cambia de escena al apretar la tecla E.
/// Se crea solo al iniciar el juego (no hace falta ponerlo en ningún objeto).
/// Reglas:
///  - Cada recorrido tiene como máximo "maxScenes" escenas (10).
///  - Las escenas Anomaly NO se repiten dentro de un recorrido.
///  - Home SÍ puede repetirse (se intercala entre anomalías).
/// Estructuras de datos:
///  - List<string>: escenas Anomaly disponibles y armado del recorrido.
///  - Queue<string>: recorrido pendiente (FIFO), se consume con cada E.
///  - Dictionary<string, int>: cuántas veces se visitó cada escena.
/// Singleton persistente para que la cola sobreviva al cambio de escena.
/// </summary>
public class SceneChanger : MonoBehaviour
{
    public static SceneChanger Instance { get; private set; }

    private string homeScene = "Home";
    private int maxScenes = 10;
    private List<string> anomalyScenes = new List<string>
    {
        "Anomaly1", "Anomaly2", "Anomaly3", "Anomaly4",
        "Anomaly5", "Anomaly6", "Anomaly7", "Anomaly8"
    };

    private Queue<string> pendingScenes;
    private Dictionary<string, int> visitCount;

    public int RemainingScenes => pendingScenes.Count;

    /// <summary>
    /// Unity llama a este método solo, una vez, al arrancar el juego,
    /// antes de cargar la primera escena. Crea el objeto persistente.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null) return;

        GameObject go = new GameObject("SceneChanger");
        go.AddComponent<SceneChanger>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        StartNewRun();
    }

    private void Update()
    {
        if (WasChangeKeyPressed())
        {
            LoadNextScene();
        }
    }

    // Funciona con el Input System nuevo y con el viejo.
    private bool WasChangeKeyPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(KeyCode.E);
#else
        return false;
#endif
    }

    /// <summary>Arma un recorrido nuevo de hasta maxScenes escenas.</summary>
    public void StartNewRun()
    {
        // 1) Anomalías mezcladas, sin repetir (se toman como máximo maxScenes).
        List<string> sequence = new List<string>(anomalyScenes);
        Shuffle(sequence);
        int anomalyCount = Mathf.Min(sequence.Count, maxScenes);
        sequence.RemoveRange(anomalyCount, sequence.Count - anomalyCount);

        // 2) Lo que falta para llegar a maxScenes se completa con Home.
        int homeCount = maxScenes - anomalyCount;
        for (int i = 0; i < homeCount; i++)
        {
            InsertHome(sequence);
        }

        // 3) Cola + diccionario de visitas.
        pendingScenes = new Queue<string>(sequence);
        visitCount = new Dictionary<string, int>();
        foreach (string sceneName in sequence)
        {
            visitCount[sceneName] = 0;
        }
    }

    public void LoadNextScene()
    {
        // Ya se recorrieron las 10 escenas: la "escena 11" es la victoria.
        if (pendingScenes.Count == 0)
        {
            Debug.Log("ganaste");
            return;
        }

        string next = pendingScenes.Dequeue();
        visitCount[next]++;
        Debug.Log($"Cambio {maxScenes - pendingScenes.Count}/{maxScenes}: cargando {next}");
        SceneManager.LoadScene(next);
    }

    public int GetVisitCount(string sceneName)
    {
        return visitCount.TryGetValue(sceneName, out int count) ? count : 0;
    }

    // Inserta un Home en una posición válida: ni primera, ni última, ni pegado a otro Home.
    private void InsertHome(List<string> sequence)
    {
        if (sequence.Count < 2)
        {
            sequence.Add(homeScene);
            return;
        }

        for (int attempt = 0; attempt < 50; attempt++)
        {
            int index = Random.Range(1, sequence.Count); // 1 .. Count-1
            if (sequence[index - 1] != homeScene && sequence[index] != homeScene)
            {
                sequence.Insert(index, homeScene);
                return;
            }
        }

        sequence.Insert(1, homeScene); // fallback
    }

    // Fisher-Yates
    private void Shuffle(List<string> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}