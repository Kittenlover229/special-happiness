using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Camera Camera;
    public Transform Pivot;
    public float CameraMovementSpeed;
    public float CameraMovementDampening;

    void Start()
    {
        Camera = Camera.main;
    }

    void Update()
    {
        var horizontal = Input.GetAxis("Horizontal");
        horizontal = Mathf.Pow(Mathf.Abs(horizontal), CameraMovementDampening) * Mathf.Sign(horizontal);

        var vertical = Input.GetAxis("Vertical");
        vertical = Mathf.Pow(Mathf.Abs(vertical), CameraMovementDampening) * Mathf.Sign(vertical);

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
