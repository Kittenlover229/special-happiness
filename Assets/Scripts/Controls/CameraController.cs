using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Camera Camera;
    public Transform Pivot;
    public float CameraMovementSpeed;

    void Start()
    {
        Camera = Camera.main;
    }

    void Update()
    {
        var horizontal = Input.GetAxisRaw("Horizontal");
        var vertical = Input.GetAxisRaw("Vertical");

        var verticalAxis = vertical * Vector3.ProjectOnPlane(Camera.transform.forward, Vector3.up).normalized;
        var horizontalAxis = horizontal * Vector3.ProjectOnPlane(Camera.transform.right, Vector3.up).normalized;

        var movement = (verticalAxis + horizontalAxis) * Time.deltaTime * CameraMovementSpeed;
        Pivot.transform.Translate(movement, Space.World);
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawSphere(Pivot.position, 0.1f);
    }
}
