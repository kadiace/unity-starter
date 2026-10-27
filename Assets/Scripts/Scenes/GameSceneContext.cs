using UnityEngine;

public class GameSceneContext : MonoBehaviour
{
    private void Start()
    {
        Managers.Input.SetInputMode(InputMode.Player);
        Debug.Log("[GameSceneContext] Game scene initialized.");
    }
}
