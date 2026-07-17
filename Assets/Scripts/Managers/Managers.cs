using UnityEngine;

public class Managers : MonoBehaviour
{
    private static Managers _instance;

    private readonly ResourceManager _resourceManager = new();
    private readonly PoolManager _poolManager = new();
    private readonly DataManager _dataManager = new();
    private readonly InputManager _inputManager = new();
    private readonly SceneManagerEx _sceneManager = new();
    private readonly UIManager _uiManager = new();
    private readonly SoundManager _soundManager = new();
    private readonly GameStateManager _gameStateManager = new();
    private readonly SpawnManager _spawnManager = new();

    private static Managers Instance
    {
        get
        {
            EnsureExists();
            return _instance;
        }
    }

    public static ResourceManager Resource => Instance._resourceManager;
    public static PoolManager Pool => Instance._poolManager;
    public static DataManager Data => Instance._dataManager;
    public static InputManager Input => Instance._inputManager;
    public static SceneManagerEx Scene => Instance._sceneManager;
    public static UIManager UI => Instance._uiManager;
    public static SoundManager Sound => Instance._soundManager;
    public static GameStateManager GameState => Instance._gameStateManager;
    public static SpawnManager Spawn => Instance._spawnManager;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        _poolManager.Init(transform);
        _dataManager.Init();
        _inputManager.Init();
        _sceneManager.Init();
        _uiManager.Init();
        _soundManager.Init(transform);
        _gameStateManager.Init();
        _spawnManager.Init();
    }

    public static void EnsureExists()
    {
        if (_instance != null)
            return;

        Managers existing = FindAnyObjectByType<Managers>();
        if (existing != null)
        {
            _instance = existing;
            return;
        }

        GameObject go = GameObject.Find("@App");
        if (go == null)
            go = new GameObject("@App");

        go.AddComponent<Managers>();
    }
}
