using UnityEngine;
using System;

public class MinigameEventSystem : MonoBehaviour
{
    public static event Action<MinigameData> OnMinigameStart;
    public static event Action<MinigameData, bool> OnMinigameComplete;
    public static event Action OnMinigameClose;
    
    private static MinigameData currentMinigameData;

    public static void StartMinigame(MinigameData data)
    {
        currentMinigameData = data;
        OnMinigameStart?.Invoke(data);
    }

    public static void CompleteMinigame(bool success)
    {
        OnMinigameComplete?.Invoke(currentMinigameData, success);
    }

    public static void CloseMinigame()
    {
        currentMinigameData = null;
        OnMinigameClose?.Invoke();
    }
}