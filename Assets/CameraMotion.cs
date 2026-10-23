using UnityEngine;

public class CameraMotion : MonoBehaviour
{
    public Movement movement;
    [SerializeField] Vector3 followOffset = new Vector3(0f, 2.5f, -6f);
    [SerializeField, Min(0.01f)] float followSharpness = 6f;

    public void SnapToTarget()
    {
        if (movement == null) return;
        Transform target = movement.transform;
        Quaternion heading = Quaternion.Euler(0f, target.eulerAngles.y, 0f);
        transform.position = target.position + heading * followOffset;
        transform.LookAt(target.position + Vector3.up);
    }

    void LateUpdate()
    {
        if (movement == null) return;
        Transform target = movement.transform;
        // Yaw-only offset keeps the camera upright when the car rolls or pitches.
        Quaternion heading = Quaternion.Euler(0f, target.eulerAngles.y, 0f);
        Vector3 desiredPosition = target.position + heading * followOffset;
        float blend = 1f - Mathf.Exp(-followSharpness * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, blend);
        Vector3 lookDirection = target.position + Vector3.up - transform.position;
        if (lookDirection.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(lookDirection, Vector3.up);
    }
}
