using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEndTrigger : MonoBehaviour
{
    public GameObject levelCompletePanel;
    private bool hasTriggered = false;  // Flag to prevent multiple activations

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Player") && !hasTriggered)
        {
            // Find all NPCInteraction instances and count completed ones
            NPCInteraction[] npcs = FindObjectsOfType<NPCInteraction>();
            int completedCount = 0;
            foreach (NPCInteraction npc in npcs)
            {
                if (npc.IsCompleted)
                {
                    completedCount++;
                }
            }

            if (completedCount >= 4)
            {
                Time.timeScale = 0f;  // Pause the game

                if (levelCompletePanel != null)
                {
                    levelCompletePanel.SetActive(true);
                    //AudioManager.instance.PlayWin();  // Uncomment if you have AudioManager
                }

                hasTriggered = true;  // Mark as triggered
                Debug.Log("Level complete! All 4 NPCs interacted with.");
            }
            else
            {
                Debug.Log($"Not all NPCs completed. Completed: {completedCount}/4. Keep interacting!");
            }
        }
    }
}


