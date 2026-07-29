using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTurnController : MonoBehaviour
{
    public enum TurnMode
    {
        Snap,
        Continuous
    }

    [Header("References")]
    [SerializeField] private Transform playerRoot;
    [SerializeField] private Transform headCamera;
    [SerializeField] private InputActionReference turnInputAction;

    [Header("Turn Settings")]
    [SerializeField] private TurnMode turnMode = TurnMode.Snap;
    [SerializeField] private float snapTurnAmount = 45f;
    [SerializeField] private float continuousTurnSpeed = 60f;
    [SerializeField, Range(0f, 1f)] private float deadZone = 0.5f;

    private bool snapTurnReady = true;

    private void OnEnable()
    {
        turnInputAction?.action.Enable();
    }

    private void OnDisable()
    {
        turnInputAction?.action.Disable();
    }

    private void Update()
    {
        if (playerRoot == null || turnInputAction == null)
            return;

        Vector2 input = turnInputAction.action.ReadValue<Vector2>();

        if (turnMode == TurnMode.Snap)
            HandleSnapTurn(input);
        else
            HandleContinuousTurn(input);
    }

    private void HandleSnapTurn(Vector2 input)
    {
        float absX = Mathf.Abs(input.x);
        float absY = Mathf.Abs(input.y);

        // 좌우 입력이 데드존보다 작거나,
        // 위아래 입력이 좌우 입력보다 크면 회전하지 않음
        if (absX < deadZone || absX <= absY)
        {
            snapTurnReady = true;
            return;
        }

        if (!snapTurnReady)
            return;

        float angle =
            Mathf.Sign(input.x) *
            snapTurnAmount;

        RotatePlayer(angle);

        snapTurnReady = false;
    }

    private void HandleContinuousTurn(Vector2 input)
    {
        float absX = Mathf.Abs(input.x);
        float absY = Mathf.Abs(input.y);

        if (absX < deadZone || absX <= absY)
            return;

        float angle =
            input.x *
            continuousTurnSpeed *
            Time.deltaTime;

        RotatePlayer(angle);
    }

    public void RotatePlayer(float angle)
    {
        if (playerRoot == null || Mathf.Approximately(angle, 0f))
            return;

        if (headCamera != null)
        {
            playerRoot.RotateAround(
                headCamera.position,
                Vector3.up,
                angle
            );
        }
        else
        {
            playerRoot.Rotate(
                Vector3.up,
                angle,
                Space.World
            );
        }
    }
}