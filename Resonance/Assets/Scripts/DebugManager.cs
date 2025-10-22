using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugManager : MonoBehaviour
{
    [Header("Debug UI")]
    [SerializeField] private Button makeAllNPCsFollowButton;
    [SerializeField] private Button resetAllNPCsButton;
    [SerializeField] private Button checkNPCStatusButton;
    [SerializeField] private GameObject debugPanel;
    [SerializeField] private KeyCode toggleDebugKey = KeyCode.F1;
    
    [Header("Debug Settings")]
    [SerializeField] private bool showDebugOnStart = false;
    
    private List<NPCInteraction> allNPCs = new List<NPCInteraction>();
    private TowerTrigger towerTrigger;

    private void Start()
    {
        FindAllNPCs();
        FindTower();
        SetupDebugUI();
        
        if (debugPanel != null)
        {
            debugPanel.SetActive(showDebugOnStart);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleDebugKey))
        {
            ToggleDebugPanel();
        }
    }

    private void FindAllNPCs()
    {
        allNPCs.Clear();
        NPCInteraction[] npcs = FindObjectsOfType<NPCInteraction>();
        allNPCs.AddRange(npcs);
        
        Debug.Log($"Debug Manager found {allNPCs.Count} NPCs in the scene");
    }

    private void FindTower()
    {
        towerTrigger = FindObjectOfType<TowerTrigger>();
        if (towerTrigger != null)
        {
            Debug.Log("Tower found for debugging");
        }
    }

    private void SetupDebugUI()
    {
        if (makeAllNPCsFollowButton != null)
        {
            makeAllNPCsFollowButton.onClick.RemoveAllListeners();
            makeAllNPCsFollowButton.onClick.AddListener(MakeAllNPCsFollow);
        }

        if (resetAllNPCsButton != null)
        {
            resetAllNPCsButton.onClick.RemoveAllListeners();
            resetAllNPCsButton.onClick.AddListener(ResetAllNPCs);
        }

        if (checkNPCStatusButton != null)
        {
            checkNPCStatusButton.onClick.RemoveAllListeners();
            checkNPCStatusButton.onClick.AddListener(CheckNPCStatus);
        }
    }

    public void MakeAllNPCsFollow()
    {
        Debug.Log("=== DEBUG: Making all NPCs follow ===");
        
        foreach (NPCInteraction npc in allNPCs)
        {
            if (npc != null)
            {
                // Usar reflection para simular que completaron el minigame
                ForceNPCToFollow(npc);
            }
        }
        
        Debug.Log($"Forced {allNPCs.Count} NPCs to follow the player");
    }

    public void ResetAllNPCs()
    {
        Debug.Log("=== DEBUG: Resetting all NPCs ===");
        
        foreach (NPCInteraction npc in allNPCs)
        {
            if (npc != null)
            {
                ResetNPC(npc);
            }
        }
        
        Debug.Log($"Reset {allNPCs.Count} NPCs to initial state");
    }

    public void CheckNPCStatus()
    {
        Debug.Log("=== DEBUG: NPC Status Check ===");
        
        int followingCount = 0;
        
        foreach (NPCInteraction npc in allNPCs)
        {
            if (npc != null)
            {
                bool following = npc.IsFollowing;
                Debug.Log($"{npc.name}: Following = {following}");
                
                if (following) followingCount++;
            }
        }
        
        Debug.Log($"NPCs following: {followingCount}/{allNPCs.Count}");
        Debug.Log($"All NPCs following: {followingCount == allNPCs.Count}");
        
        // También verificar el estado de la torre si existe
        if (towerTrigger != null)
        {
            towerTrigger.CheckNPCStatus();
        }
    }

    private void ForceNPCToFollow(NPCInteraction npc)
    {
        // Usar reflection para acceder a campos privados y simular completación
        var isMinigameCompletedField = typeof(NPCInteraction).GetField("isMinigameCompleted", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        var isFollowingField = typeof(NPCInteraction).GetField("isFollowing", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (isMinigameCompletedField != null && isFollowingField != null)
        {
            isMinigameCompletedField.SetValue(npc, true);
            isFollowingField.SetValue(npc, true);
            
            Debug.Log($"Forced {npc.name} to follow");
        }
        else
        {
            Debug.LogWarning($"Could not force {npc.name} to follow - reflection failed");
        }
    }

    private void ResetNPC(NPCInteraction npc)
    {
        // Resetear el NPC a su estado inicial
        var isMinigameCompletedField = typeof(NPCInteraction).GetField("isMinigameCompleted", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        var isFollowingField = typeof(NPCInteraction).GetField("isFollowing", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        var showingDialogueField = typeof(NPCInteraction).GetField("showingDialogue", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (isMinigameCompletedField != null && isFollowingField != null && showingDialogueField != null)
        {
            isMinigameCompletedField.SetValue(npc, false);
            isFollowingField.SetValue(npc, false);
            showingDialogueField.SetValue(npc, false);
            
            Debug.Log($"Reset {npc.name} to initial state");
        }
        else
        {
            Debug.LogWarning($"Could not reset {npc.name} - reflection failed");
        }
    }

    public void ToggleDebugPanel()
    {
        if (debugPanel != null)
        {
            debugPanel.SetActive(!debugPanel.activeSelf);
            Debug.Log($"Debug panel: {(debugPanel.activeSelf ? "Shown" : "Hidden")}");
        }
    }

    // Método para refrescar la lista de NPCs (útil si se crean NPCs dinámicamente)
    public void RefreshNPCList()
    {
        FindAllNPCs();
        Debug.Log("NPC list refreshed");
    }

    private void OnDrawGizmos()
    {
        // Mostrar información de debug en el editor
        if (allNPCs.Count > 0)
        {
            int followingCount = 0;
            foreach (var npc in allNPCs)
            {
                if (npc != null && npc.IsFollowing)
                {
                    followingCount++;
                }
            }
            
            // Dibujar texto en la escena (solo visible en Scene view)
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, 
                $"NPCs Following: {followingCount}/{allNPCs.Count}");
            #endif
        }
    }
}