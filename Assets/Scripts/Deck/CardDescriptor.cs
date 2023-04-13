using UnityEngine;

[CreateAssetMenu(fileName = "Card Description", order = 1)]
public class CardDescriptor : ScriptableObject
{
    public string Name;
    public string Description;
    public Sprite Artwork;
}
