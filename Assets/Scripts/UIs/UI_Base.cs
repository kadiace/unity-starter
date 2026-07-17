using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class UI_Base : MonoBehaviour
{
    private readonly Dictionary<Type, UnityEngine.Object[]> _objects = new();

    private void Start()
    {
        Init();
    }

    public abstract void Init();

    protected void Bind<T>(Type type) where T : UnityEngine.Object
    {
        string[] names = Enum.GetNames(type);
        UnityEngine.Object[] objects = new UnityEngine.Object[names.Length];
        _objects[typeof(T)] = objects;

        for (int i = 0; i < names.Length; i++)
        {
            if (typeof(T) == typeof(GameObject))
                objects[i] = FindChild(gameObject, names[i]);
            else
                objects[i] = FindChild<T>(gameObject, names[i]);

            if (objects[i] == null)
                Debug.LogWarning($"[UI] Failed to bind {typeof(T).Name}: {names[i]}");
        }
    }

    protected T Get<T>(int index) where T : UnityEngine.Object
    {
        if (!_objects.TryGetValue(typeof(T), out UnityEngine.Object[] objects))
            return null;

        return objects[index] as T;
    }

    protected GameObject GetObject(int index)
    {
        return Get<GameObject>(index);
    }

    protected Button GetButton(int index)
    {
        return Get<Button>(index);
    }

    protected Text GetText(int index)
    {
        return Get<Text>(index);
    }

    protected Image GetImage(int index)
    {
        return Get<Image>(index);
    }

    public static void BindEvent(GameObject target, Action<PointerEventData> action, Define.UIEvent type = Define.UIEvent.Click)
    {
        UI_EventHandler handler = target.GetComponent<UI_EventHandler>();

        switch (type)
        {
            case Define.UIEvent.Click:
                handler.OnClickHandler -= action;
                handler.OnClickHandler += action;
                break;
            case Define.UIEvent.Drag:
                handler.OnDragHandler -= action;
                handler.OnDragHandler += action;
                break;
        }
    }

    private static GameObject FindChild(GameObject root, string name)
    {
        Transform[] children = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i].name == name)
                return children[i].gameObject;
        }

        return null;
    }

    private static T FindChild<T>(GameObject root, string name) where T : UnityEngine.Object
    {
        GameObject child = FindChild(root, name);
        return child != null ? child.GetComponent<T>() : null;
    }
}
