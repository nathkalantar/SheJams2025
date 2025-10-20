using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance;
    
    [Header("Dialogue Panel")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private Image characterPortrait;
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI instructionText;
    
    void Awake()
    {
        Instance = this;
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }
    
    public void ShowDialogue(string characterName, string message, Sprite portrait = null, string instruction = "Press E to continue...")
    {
        if (dialoguePanel == null) return;
        
        dialoguePanel.SetActive(true);
        
        if (characterNameText != null)
            characterNameText.text = characterName;
            
        if (dialogueText != null)
            dialogueText.text = message;
            
        if (instructionText != null)
            instructionText.text = instruction;
            
        if (characterPortrait != null && portrait != null)
        {
            characterPortrait.sprite = portrait;
            characterPortrait.gameObject.SetActive(true);
        }
        else if (characterPortrait != null)
        {
            characterPortrait.gameObject.SetActive(false);
        }
    }
    
    public void HideDialogue()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }
}