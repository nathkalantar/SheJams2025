using UnityEngine;

[CreateAssetMenu(fileName = "MinigameData", menuName = "Minigames/MinigameData")]
public class MinigameData : ScriptableObject
{
    [System.Serializable]
    public class ItemData
    {
        public Sprite sprite;
        public Vector2 startPosition;
        public Vector2 size = new Vector2(100, 100);
        public int correctDropZoneIndex;
    }

    [System.Serializable]
    public class DropZoneData
    {
        public Vector2 position;
        public Vector2 size;
    }

    public string minigameName;
    public Sprite backgroundSprite;
    public Color backgroundColor = Color.white;
    public ItemData[] items;
    public DropZoneData[] dropZones;
    public AudioClip successSound;
    public AudioClip failSound;
}