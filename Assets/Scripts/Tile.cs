using UnityEngine;
using UnityEngine.Assertions;


public class Tile : MonoBehaviour
{
    public Vector3 pivot;
    public GameObject occupier;

    public bool IsOccupied() => occupier != null;

    public void Occupy(GameObject occupier)
    {
        Assert.IsFalse(IsOccupied());
        occupier.transform.position = transform.position + pivot;
        occupier.transform.SetParent(transform);
        this.occupier = occupier;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(GetWorldPivot(), 1f);
    }

    public Vector3 GetWorldPivot()
    {
        return transform.position + pivot;
    }
}
