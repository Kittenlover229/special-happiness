using UnityEngine;


[CreateAssetMenu(fileName = "Summon Card", order = 1)]
public class SummonCardDescriptor : CardDescriptor {
    public GameObject SummonedPrefab;

    public override bool TryPlay(Tile target)
    {
        if (target.IsOccupied())
            return false;

        var summon = Instantiate(SummonedPrefab, target.pivot, Quaternion.identity);
        target.Occupy(summon);
        return true;
    }
}
