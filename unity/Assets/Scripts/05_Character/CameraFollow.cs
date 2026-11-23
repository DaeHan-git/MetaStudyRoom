using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target & Normal Offset")]
    public Transform target;
    public Vector3 offset = new Vector3(0f, 2.5f, -4f);
    public float smoothSpeed = 10f;
    public float lookAtHeight = 1.5f;

    [Header("Sitting Face Framing")]
    public Vector3 sittingOffset = new Vector3(0.3f, 0.95f, 1.5f);
    public float sittingFaceHeight = 0.75f;
    public float faceForwardSign = 1f;
    public bool ignoreObstacleWhenSitting = true; // 앉을 때 장애물 회피 임시 무시 여부

    [Header("Obstacle Avoidance")]
    public LayerMask collisionMask = ~0;
    public float minDistance = 0.5f;
    public float sphereRadius = 0.25f;
    public float collisionBuffer = 0.2f;

    private bool isSitting = false;

    public void SetSittingState(bool sitting, Transform chairTransform = null)
    {
        isSitting = sitting;
        Debug.Log($"[CameraFollow] isSitting = {isSitting}");
    }

    void LateUpdate()
    {
        if (target == null) return;

        float activeLookHeight = isSitting ? sittingFaceHeight : lookAtHeight;
        Vector3 pivot = target.position + Vector3.up * activeLookHeight;

        Vector3 desiredPosition;
        if (isSitting)
        {
            Vector3 localFaceOffset = new Vector3(
                sittingOffset.x,
                sittingOffset.y,
                Mathf.Abs(sittingOffset.z) * faceForwardSign
            );
            desiredPosition = target.position + target.TransformDirection(localFaceOffset);
        }
        else
        {
            desiredPosition = target.position + target.TransformDirection(offset);
        }

        Vector3 finalTargetPosition = desiredPosition;

        bool shouldCheckCollision = !(isSitting && ignoreObstacleWhenSitting);
        if (shouldCheckCollision)
        {
            Vector3 toDesired = desiredPosition - pivot;
            float desiredDistance = toDesired.magnitude;

            if (desiredDistance > 0.001f)
            {
                Vector3 direction = toDesired / desiredDistance;

                if (Physics.SphereCast(
                    pivot,
                    sphereRadius,
                    direction,
                    out RaycastHit hit,
                    desiredDistance,
                    collisionMask,
                    QueryTriggerInteraction.Ignore))
                {
                    if (!IsTargetOrChild(hit.transform))
                    {
                        float safeDistance = Mathf.Clamp(hit.distance - collisionBuffer, minDistance, desiredDistance);
                        finalTargetPosition = pivot + direction * safeDistance;
                    }
                }
            }
        }

        transform.position = Vector3.Lerp(transform.position, finalTargetPosition, smoothSpeed * Time.deltaTime);
        transform.LookAt(pivot);
    }

    private bool IsTargetOrChild(Transform hitTransform)
    {
        return hitTransform == target || hitTransform.IsChildOf(target);
    }
}