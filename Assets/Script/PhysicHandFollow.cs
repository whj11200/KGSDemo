using UnityEngine;

public class PhysicsHandFollow : MonoBehaviour
{
    [Header("Refs")]
    public Transform target;     // XR controller transform
    public Rigidbody rb;         // hand physics rigidbody

    [Header("Tuning")]
    public float followStrength = 40f;   // position
    public float rotateStrength = 25f;   // rotation
    public float maxSpeed = 5f;          // clamp
    public float maxAngSpeed = 30f;      // clamp

    Vector3 cachedPos;
    Quaternion cachedRot;

    void Reset()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.maxAngularVelocity = 50f;
    }

    void Update()
    {
        if (!target) return;
        cachedPos = target.position;
        cachedRot = target.rotation;
    }

    void FixedUpdate()
    {
        if (!target || !rb) return;

        // Position follow (velocity drive)
        Vector3 posError = cachedPos - rb.position;
        Vector3 v = posError * followStrength;
        rb.linearVelocity = Vector3.ClampMagnitude(v, maxSpeed);

        // Rotation follow (angular velocity drive)
        Quaternion q = cachedRot * Quaternion.Inverse(rb.rotation);
        q.ToAngleAxis(out float angle, out Vector3 axis);

        if (angle > 180f) angle -= 360f;
        if (Mathf.Abs(angle) < 0.01f || axis == Vector3.zero)
        {
            rb.angularVelocity = Vector3.zero;
            return;
        }

        Vector3 av = axis * (angle * Mathf.Deg2Rad * rotateStrength);
        rb.angularVelocity = Vector3.ClampMagnitude(av, maxAngSpeed);
    }
}
