using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Paneles")]
    [SerializeField] private UIPanel menuPanel;      // el propio UI_Menu (vacío en la escena Muerte)
    [SerializeField] private UIPanel tutorialPanel;
    [SerializeField] private UIPanel optionsPanel;

    [Header("Jugador")]
    [SerializeField] private FirstPersonController player;   // el script del Player (vacío en Muerte)

    // ESTRUCTURA DE DATOS: Dictionary para buscar la acción según el id del botón.
    private readonly Dictionary<string, IMenuAction> _actions = new Dictionary<string, IMenuAction>();

    private void Awake()
    {
        // Menú principal
        Register(new PlayAction("play", menuPanel, player));
        Register(new QuitAction("quit"));

        Register(new ShowPanelAction("instructions", tutorialPanel));
        Register(new HidePanelAction("close_instructions", tutorialPanel));

        Register(new ShowPanelAction("options", optionsPanel));
        Register(new HidePanelAction("close_options", optionsPanel));

        // Pantalla de muerte
        Register(new ReplayAction("replay"));
        Register(new BackToMenuAction("menu"));
    }

    private void Start()
    {
        // En la escena Muerte no hay menuPanel: no hay nada que mostrar ni congelar.
        if (menuPanel == null) return;

        // Si ya se tocó Play en esta partida, no mostramos el menú:
        // el jugador queda activo y el cursor lo maneja FirstPersonController.
        if (GameSession.Started)
        {
            menuPanel.Hide();
            return;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // El jugador queda quieto hasta que se toque Play.
        if (player != null)
            player.enabled = false;
    }

    private void Register(IMenuAction action)
    {
        _actions[action.Id] = action;
    }

    // Se llama desde el On Click () de cada botón, con el id como texto.
    public void OnButtonPressed(string id)
    {
        if (_actions.TryGetValue(id, out IMenuAction action))
            action.Execute();
        else
            Debug.LogWarning($"[Menu] No existe una acción con id '{id}'");
    }
}

// Recuerda si ya se tocó Play en esta partida. Es estático: sobrevive al cambio de escena.
public static class GameSession
{
    public static bool Started;

    // Se reinicia al arrancar el juego (necesario si el Editor no recarga el dominio).
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnLaunch()
    {
        Started = false;
    }
}

// INTERFAZ 1: contrato de toda acción de botón.
public interface IMenuAction
{
    string Id { get; }
    void Execute();
}

// INTERFAZ 2: contrato de todo panel que se muestra/oculta.
public interface IMenuPanel
{
    bool IsVisible { get; }
    void Show();
    void Hide();
}

// HERENCIA: clase base abstracta (no es MonoBehaviour).
public abstract class MenuActionBase : IMenuAction
{
    public string Id { get; }

    protected MenuActionBase(string id)
    {
        Id = id;
    }

    public void Execute()
    {
        Debug.Log($"[Menu] Acción ejecutada: {Id}");
        OnExecute();
    }

    protected abstract void OnExecute();
}

// PATRÓN STRATEGY: cada botón tiene su estrategia intercambiable.
// Play oculta el menú y activa al jugador. Al activarse, FirstPersonController
// bloquea el cursor solo (en su OnEnable).
public class PlayAction : MenuActionBase
{
    private readonly IMenuPanel _menuPanel;
    private readonly Behaviour _player;

    public PlayAction(string id, IMenuPanel menuPanel, Behaviour player) : base(id)
    {
        _menuPanel = menuPanel;
        _player = player;
    }

    protected override void OnExecute()
    {
        GameSession.Started = true;
        _menuPanel.Hide();

        if (_player != null)
            _player.enabled = true;
    }
}

public class ShowPanelAction : MenuActionBase
{
    private readonly IMenuPanel _panel;

    public ShowPanelAction(string id, IMenuPanel panel) : base(id)
    {
        _panel = panel;
    }

    protected override void OnExecute()
    {
        _panel.Show();
    }
}

public class HidePanelAction : MenuActionBase
{
    private readonly IMenuPanel _panel;

    public HidePanelAction(string id, IMenuPanel panel) : base(id)
    {
        _panel = panel;
    }

    protected override void OnExecute()
    {
        _panel.Hide();
    }
}

public class QuitAction : MenuActionBase
{
    public QuitAction(string id) : base(id) { }

    protected override void OnExecute()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}

// PLAY AGAIN: recorrido nuevo desde 0 y directo a Home, sin menú.
public class ReplayAction : MenuActionBase
{
    public ReplayAction(string id) : base(id) { }

    protected override void OnExecute()
    {
        GameSession.Started = true;   // Home no muestra el menú, el jugador queda activo

        if (SceneChanger.Instance != null)
            SceneChanger.Instance.StartNewRun();

        SceneLoader.Instance.Load("Home");
    }
}

// MENU: recorrido nuevo y vuelve a Home mostrando UI_Menu.
public class BackToMenuAction : MenuActionBase
{
    public BackToMenuAction(string id) : base(id) { }

    protected override void OnExecute()
    {
        GameSession.Started = false;  // MainMenu.Start() muestra el menú y congela al jugador

        if (SceneChanger.Instance != null)
            SceneChanger.Instance.StartNewRun();

        SceneLoader.Instance.Load("Home");
    }
}

// PATRÓN SINGLETON: una única instancia global para cambiar de escena.
// Clase normal de C#, no necesita ningún objeto en la escena.
public sealed class SceneLoader
{
    private static SceneLoader _instance;
    public static SceneLoader Instance => _instance ??= new SceneLoader();

    private SceneLoader() { }

    public void Load(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}