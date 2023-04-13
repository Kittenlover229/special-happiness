using UnityEngine;

public abstract class CardDescriptor : ScriptableObject
{
    public string Name;
    public string Description;
    public Sprite Artwork;
    
    // Return false if failed to play, true otherwise
    public virtual bool TryPlay(Tile target) => false;
}
