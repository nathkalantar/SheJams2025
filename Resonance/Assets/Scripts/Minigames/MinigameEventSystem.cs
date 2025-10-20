using UnityEngine;
using System;

public class MinigameEventSystem : MonoBehaviour
{
    public static event Action<MinigameData> OnMinigameStart;
    public static event Action<bool> OnMinigameComplete;
    public static event Action OnMinigameClose;

    public static void StartMinigame(MinigameData data)
    {
        OnMinigameStart?.Invoke(data);
    }

    public static void CompleteMinigame(bool success)
    {
        OnMinigameComplete?.Invoke(success);
    }

    public static void CloseMinigame()
    {
        OnMinigameClose?.Invoke();
    }
}