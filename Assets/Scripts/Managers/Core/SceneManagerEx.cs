using UnityEngine.SceneManagement;

public class SceneManagerEx
{
    public Define.Scene PendingScene { get; private set; } = Define.Scene.Unknown;
    public Define.Scene CurrentScene => ParseScene(SceneManager.GetActiveScene().name);

    public void Init()
    {
        PendingScene = CurrentScene;
    }

    public void ReloadCurrentScene()
    {
        Load(CurrentScene);
    }

    public void Load(Define.Scene scene)
    {
        PendingScene = scene;
        SceneManager.LoadScene(ToSceneName(scene));
    }

    public bool IsCurrentScene(Define.Scene scene)
    {
        return CurrentScene == scene;
    }

    private static string ToSceneName(Define.Scene scene)
    {
        return scene.ToString();
    }

    private static Define.Scene ParseScene(string sceneName)
    {
        if (System.Enum.TryParse(sceneName, out Define.Scene scene))
            return scene;

        return Define.Scene.Unknown;
    }
}
