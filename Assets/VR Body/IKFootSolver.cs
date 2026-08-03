using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKFootSolver : MonoBehaviour
{
    public bool isMovingForward;

    [SerializeField] LayerMask terrainLayer = default;
    [SerializeField] Transform body = default;
    [SerializeField] IKFootSolver otherFoot = default;
    [SerializeField] float speed = 4;
    [SerializeField] float stepDistance = .2f;
    [SerializeField] float stepLength = .2f;
    [SerializeField] float sideStepLength = .1f;

    [SerializeField] float stepHeight = .3f;
    [SerializeField] Vector3 footOffset = default;

    public Vector3 footRotOffset;
    public float footYPosOffset = 0.1f;

    public float rayStartYOffset = 0;
    public float rayLength = 1.5f;
    
    float footSpacing;
    Vector3 oldPosition, currentPosition, newPosition;
    Vector3 oldNormal, currentNormal, newNormal;
    float lerp;
    [Header("Adaptive Step Settings")]
    [SerializeField] private float maxReferenceMoveSpeed = 3.5f;

    [SerializeField] private float minStepSpeed = 4f;
    [SerializeField] private float maxStepSpeed = 14f;

    [SerializeField] private float maxStepLength = 0.55f;
    [SerializeField] private float maxSideStepLength = 0.3f;

    [SerializeField, Range(0.5f, 1f)]
    private float runOverlapProgress = 0.65f;

    [SerializeField] private float emergencyStepDistance = 0.7f;

    private Vector3 previousBodyPosition;
    private float currentBodySpeed;

    public float StepProgress => lerp;

    private void Start()
    {
        footSpacing = transform.localPosition.x;
        currentPosition = newPosition = oldPosition = transform.position;
        currentNormal = newNormal = oldNormal = transform.up;
        lerp = 1;
        previousBodyPosition = body.position;
    }

    // Update is called once per frame

    private void LateUpdate()
    {
        Vector3 bodyDelta = body.position - previousBodyPosition;
        bodyDelta.y = 0f;

        currentBodySpeed =
            bodyDelta.magnitude /
            Mathf.Max(Time.deltaTime, 0.0001f);

        previousBodyPosition = body.position;

        float moveT = Mathf.InverseLerp(
            0f,
            maxReferenceMoveSpeed,
            currentBodySpeed);

        float currentStepSpeed = Mathf.Lerp(
            minStepSpeed,
            maxStepSpeed,
            moveT);

        float currentForwardStepLength = Mathf.Lerp(
            stepLength,
            maxStepLength,
            moveT);

        float currentSideStepLength = Mathf.Lerp(
            sideStepLength,
            maxSideStepLength,
            moveT);

        transform.position =
            currentPosition +
            Vector3.up * footYPosOffset;

        transform.localRotation =
            Quaternion.Euler(footRotOffset);

        Vector3 rayOrigin =
            body.position +
            body.right * footSpacing +
            Vector3.up * rayStartYOffset;

        Ray ray = new Ray(rayOrigin, Vector3.down);

        bool hitGround = Physics.Raycast(
            ray,
            out RaycastHit info,
            rayLength,
            terrainLayer,
            QueryTriggerInteraction.Ignore);

        Debug.DrawRay(
            rayOrigin,
            Vector3.down * rayLength,
            hitGround ? Color.green : Color.red);

        if (hitGround)
        {
            float distanceToTarget =
                Vector3.Distance(newPosition, info.point);

            float requiredOtherFootProgress = Mathf.Lerp(
                1f,
                runOverlapProgress,
                moveT);

            bool otherFootReady =
                otherFoot == null ||
                otherFoot.StepProgress >= requiredOtherFootProgress;

            bool emergencyStep =
                distanceToTarget >= emergencyStepDistance;

            if (distanceToTarget > stepDistance &&
                (otherFootReady || emergencyStep) &&
                lerp >= 1f)
            {
                oldPosition = currentPosition;
                oldNormal = currentNormal;

                Vector3 direction = Vector3.ProjectOnPlane(
                    info.point - currentPosition,
                    Vector3.up
                ).normalized;

                float angle = Vector3.Angle(
                    body.forward,
                    direction);

                isMovingForward =
                    angle < 50f ||
                    angle > 130f;

                float selectedStepLength = isMovingForward
                    ? currentForwardStepLength
                    : currentSideStepLength;

                newPosition =
                    info.point +
                    direction * selectedStepLength +
                    footOffset;

                newNormal = info.normal;
                lerp = 0f;
            }
        }

        if (lerp < 1f)
        {
            lerp = Mathf.Clamp01(
                lerp + Time.deltaTime * currentStepSpeed);

            float smoothLerp =
                Mathf.SmoothStep(0f, 1f, lerp);

            Vector3 tempPosition = Vector3.Lerp(
                oldPosition,
                newPosition,
                smoothLerp);

            tempPosition.y +=
                Mathf.Sin(smoothLerp * Mathf.PI) *
                stepHeight;

            currentPosition = tempPosition;

            currentNormal = Vector3.Lerp(
                oldNormal,
                newNormal,
                smoothLerp).normalized;
        }
        else
        {
            lerp = 1f;

            currentPosition = newPosition;
            currentNormal = newNormal;

            oldPosition = newPosition;
            oldNormal = newNormal;
        }
    }

    private void OnDrawGizmos()
    {

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(newPosition, 0.1f);
    }



    public bool IsMoving()
    {
        return lerp < 1;
    }



}
