using UnityEngine;

[CreateAssetMenu(fileName = "Card Description", order = 1)]
class CardDescription : ScriptableObject
{
    public string CardName;
    public DiceOrder order;
    public Sprite artwork;
}
