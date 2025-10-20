using UnityEngine;

public class NPCInteraction : MonoBehaviour
{
    [SerializeField] private MinigameData minigameData;
    [SerializeField] private float interactionRange = 3f;
    
    private Transform player;
    private bool canInteract = false;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            canInteract = distance <= interactionRange;
            
            if (canInteract && Input.GetKeyDown(KeyCode.E))
            {
                StartMinigame();
            }
        }
    }

    void StartMinigame()
    {
        if (minigameData != null)
        {
            MinigameEventSystem.StartMinigame(minigameData);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}