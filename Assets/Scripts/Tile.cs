using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector3 pivot;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(GetWorldPivot(), 1f);
    }

    public Vector3 GetWorldPivot() {
        return transform.position + pivot;
    }
}
