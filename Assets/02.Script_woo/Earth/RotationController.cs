using UnityEngine;

public class RotationController : MonoBehaviour
{
    [Header("Self Rotation (자전)")]
    [SerializeField] Vector3 selfAxis = Vector3.up;
    [SerializeField] float selfSpeed = 30f;
    [SerializeField] bool selfWorldSpace = false;

    [Header("Orbit Rotation (공전)")]
    [SerializeField] Transform orbitCenter;
    [SerializeField] Vector3 orbitAxis = Vector3.up;
    [SerializeField] float orbitSpeed = 20f;


    bool selfRotateEnabled;
    bool orbitRotateEnabled;

    private void Awake()
    {
        StartSelfRotation();
    }
    void Update()
    {
        if (selfRotateEnabled)
            SelfRotate();

        if (orbitRotateEnabled)
            OrbitRotate();
    }



    public void StartSelfRotation()
    {
        selfRotateEnabled = true;
    }

    public void StopSelfRotation()
    {
        selfRotateEnabled = false;
    }

    public void StartOrbitRotation()
    {
        if (orbitCenter == null) return;
        orbitRotateEnabled = true;
    }

    public void StopOrbitRotation()
    {
        orbitRotateEnabled = false;
    }

    public void StopAllRotation()
    {
        selfRotateEnabled = false;
        orbitRotateEnabled = false;
    }



    void SelfRotate()
    {
        Vector3 axis = selfAxis.normalized;
        float angle = selfSpeed * Time.deltaTime;

        transform.Rotate(axis, angle, selfWorldSpace ? Space.World : Space.Self);
    }

    void OrbitRotate()
    {
        Vector3 axis = orbitAxis.normalized;
        float angle = orbitSpeed * Time.deltaTime;

        transform.RotateAround(orbitCenter.position, axis, angle);
    }
}
