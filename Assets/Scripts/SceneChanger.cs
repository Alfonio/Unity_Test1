using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Maneja el recorrido de escenas y valida los botones.
/// Se crea solo al iniciar el juego (no hace falta ponerlo en ningún objeto).
/// Reglas:
///  - Cada recorrido tiene como máximo "maxScenes" escenas (10).
///  - Las escenas Anomaly NO se repiten dentro de un recorrido.
///  - Home SÍ puede repetirse (se intercala entre anomalías).
/// Estructuras de datos:
///  - List<string>: escenas Anomaly disponibles y armado del recorrido.
///  - Queue<string>: recorrido pendiente (FIFO), se consume con cada botón correcto.
///  - Dictionary<string, int>: cuántas veces se visitó cada escena.
/// Singleton persistente para que la cola sobreviva al cambio de escena.
/// </summary>
public class SceneChanger : MonoBehaviour
{
    public static SceneChanger Instance { get; private set; }

    private string homeScene = "Home";
    private string deathScene = "Muerte";
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

    /// Arma un recorrido nuevo de hasta maxScenes escenas.
    public void StartNewRun()
    {
        // Anomalías mezcladas, sin repetir (tomo como máximo maxScenes).
        List<string> sequence = new List<string>(anomalyScenes);
        Shuffle(sequence);
        int anomalyCount = Mathf.Min(sequence.Count, maxScenes);
        sequence.RemoveRange(anomalyCount, sequence.Count - anomalyCount);

        // Lo que falta para llegar a maxScenes se completa con Home (osea 2 en este caso).
        int homeCount = maxScenes - anomalyCount;
        for (int i = 0; i < homeCount; i++)
        {
            InsertHome(sequence);
        }

        // Cola + diccionario de visitas.
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
            GameWin.Won = true;
            SceneManager.LoadScene(homeScene);   // Home muestra UI_WIN 
            return;
        }

        string next = pendingScenes.Dequeue();
        visitCount[next]++;
        Debug.Log($"Cambio {maxScenes - pendingScenes.Count}/{maxScenes}: cargando {next}");
        SceneManager.LoadScene(next);
    }

    /// <summary>
    // Verifica si el botón apretado corresponde al tipo de la escena actual.
    /// Correcto: avanza a la siguiente escena. Incorrecto: debug, sin cambiar de escena.
    /// </summary>
    public void PressButton(bool isAnomalyButton)
    {
        string current = SceneManager.GetActiveScene().name;
        bool inHome = current == homeScene;
        bool inAnomaly = anomalyScenes.Contains(current);

        // Escena que no es ni Home ni Anomaly (menú, etc.): no hace nada.
        if (!inHome && !inAnomaly) return;

        bool correct = isAnomalyButton ? inAnomaly : inHome;

        if (correct)
        {
            Debug.Log("Correcto");
            LoadNextScene();
        }
        else
        {
            Debug.Log("Apretaste el equivocado: yendo a " + deathScene);
            SceneManager.LoadScene(deathScene);
        }
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