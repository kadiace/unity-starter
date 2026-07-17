using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public class UIManager
{
    private int _order = 1;
    private UI_Scene _sceneUI;

    public GameObject Root
    {
        get
        {
            GameObject root = GameObject.Find("@UI_Root");
            if (root == null)
                root = new GameObject("@UI_Root");

            return root;
        }
    }

    public void Init()
    {
        _ = Root;
        EnsureEventSystem();
    }

    public T ShowSceneUI<T>(string prefabName = null) where T : UI_Scene
    {
        if (_sceneUI != null)
            Object.Destroy(_sceneUI.gameObject);

        T sceneUI = CreateScreenUI<T>("Scenes", prefabName);
        _sceneUI = sceneUI;
        return sceneUI;
    }

    public T ShowMenuUI<T>(string prefabName = null) where T : UI_Menu
    {
        return CreateScreenUI<T>("Menus", prefabName);
    }

    public T CreateWorldSpaceUI<T>(Transform parent, string prefabName = null) where T : UI_Base
    {
        if (string.IsNullOrWhiteSpace(prefabName))
            prefabName = typeof(T).Name;

        GameObject uiObject = Managers.Resource.Instantiate($"Prefabs/UIs/WorldSpace/{prefabName}", parent);
        ShowWorldCanvas(uiObject);

        T ui = uiObject.GetComponent<T>();
        return ui;
    }

    public void HideSceneUI()
    {
        if (_sceneUI == null)
            return;

        Object.Destroy(_sceneUI.gameObject);
        _sceneUI = null;
    }

    public void Clear()
    {
        GameObject root = Root;
        for (int i = root.transform.childCount - 1; i >= 0; i--)
            Object.Destroy(root.transform.GetChild(i).gameObject);

        _sceneUI = null;
        _order = 1;
    }

    private T CreateScreenUI<T>(string folderName, string prefabName) where T : UI_Base
    {
        if (string.IsNullOrWhiteSpace(prefabName))
            prefabName = typeof(T).Name;

        GameObject uiObject = Managers.Resource.Instantiate($"Prefabs/UIs/{folderName}/{prefabName}");

        uiObject.transform.SetParent(Root.transform, false);
        ShowCanvas(uiObject);

        T ui = uiObject.GetComponent<T>();
        return ui;
    }

    private void ShowCanvas(GameObject target)
    {
        Canvas canvas = target.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = _order++;
    }

    private void ShowWorldCanvas(GameObject target)
    {
        Canvas canvas = target.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;
    }

    private void EnsureEventSystem()
    {
        if (Object.FindAnyObjectByType<EventSystem>() != null)
            return;

        GameObject eventSystem = new("@EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<InputSystemUIInputModule>();
    }
}
