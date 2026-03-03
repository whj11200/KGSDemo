using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridObjectPlacer : MonoBehaviour
{
    public GameObject prefab;

    public Transform plane;   // 기준 Plane
    public int countX = 7;
    public int countZ = 7;
    public float yOffset = 0f;
    public GameObject player;
    public bool IsPlayerMoving { get; private set; }
    public ControlTower CurrentTower { get; private set; }
    private CameraController cameraController;
    // [SerializeField] List<List<ControlTower>> Grid = new();

    private void Awake()
    {
        if (ControlTower.Grid == null)
        {
            ControlTower.Grid = this;
        }

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            cameraController = player.GetComponent<CameraController>();
        }

        Generate();
    }

    private void Start()
    {
        FindNearTower();
    }

    private void OnDestroy()
    {
        //if (ControlTower.Instance == this)
        //{
        //    ControlTower.Instance = null;
        //}
    }

    public void Generate()
    {
        if (prefab == null || plane == null)
        {
            Debug.LogError("Prefab 또는 Plane이 지정되지 않았습니다.");
            return;
        }

        Clear();

        // Plane 월드 크기
        BoxCollider box = GetComponent<BoxCollider>();
        Vector3 size = box.bounds.size;
        float width = size.x;
        float depth = size.z;

        // 간격
        float spacingX = (countX > 1) ? width / (countX - 1) : 0f;
        float spacingZ = (countZ > 1) ? depth / (countZ - 1) : 0f;

        // 시작 오프셋
        float startX = -width * 0.5f;
        float startZ = -depth * 0.5f;

        // 배치
        for (int x = 0; x < countX; x++)
        {
            // Grid 행 추가
            // Grid.Add(new List<ControlTower>());

            for (int z = 0; z < countZ; z++)
            {
                Vector3 worldPos = new Vector3(
                    plane.position.x + startX + x * spacingX,
                    plane.position.y + yOffset,
                    plane.position.z + startZ + z * spacingZ
                );

                var obj = Instantiate(prefab);
                obj.transform.position = worldPos;
                obj.transform.rotation = prefab.transform.rotation;
                obj.transform.localScale = prefab.transform.localScale;

                // 부모 설정 (월드 기준 유지)
                obj.transform.SetParent(transform, true);

                obj.name = $"ControlTower_{x}_{z}";

                // ControlTower 좌표 정보 설정
                //var ct = obj.GetComponent<ControlTower>();
                //ct.selfPos = new Vector2(x, z);

                //// Grid에 추가
                //Grid[x].Add(ct);
            }
        }
    }

    public void Clear()
    {
        // Grid.Clear();

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }

    // 직접 클릭 방식으로 변경 => 좌표 이동 불필요
    //public void MoveOtherControlTower(Vector2 curPos, Vector2 direction)
    //{
    //    Debug.Log($"MoveOtherControlTower: CurPos={curPos}, Direction={direction}");
    //    Vector2 newPos = curPos + direction;
    //    Debug.Log($"NewPos={newPos}");

    //    // 유효 범위 체크
    //    if (newPos.x < 0 || newPos.x >= countX || newPos.y < 0 || newPos.y >= countZ)
    //    {
    //        Debug.Log("Invalid Position" );
    //        return;
    //    }
    //    Debug.Log("Valid Position");

    //    var targetTower = Grid[(int)newPos.x][(int)newPos.y];
    //    Debug.Log($"Target Tower: {targetTower.gameObject.name}");

    //    // 플레이어 위치 이동
    //    if (player == null)
    //    {
    //        player = GameObject.FindGameObjectWithTag("Player");
    //        cameraController = player.GetComponent<CameraController>();
    //    }

    //    if (player == null)
    //    {
    //        Debug.LogError("Player 오브젝트를 찾을 수 없습니다.");
    //        return;
    //    }

    //    Debug.Log($"Moving Player to {targetTower.GetViewPos()}");
    //    StartCoroutine(SafePlayerMove(targetTower.GetViewPos()));
    //}

    //private IEnumerator SafePlayerMove(Vector3 movePos)
    //{
    //    cameraController.enabled = false; // 카메라 컨트롤러 비활성화
    //    yield return new WaitForEndOfFrame(); // 프레임 끝까지 대기
    //    player.transform.position = movePos;
    //    Debug.Log($"Player moved to {player.transform.position}");
    //    yield return new WaitForEndOfFrame(); // 프레임 끝까지 대기
    //    cameraController.enabled = true; // 카메라 컨트롤러 활성화
    //}

    // TODO: 플레이어가 타워 밖으로 이동했을 경우의 처리
    public void RequestMove()
    {
        CurrentTower = null;
    }

    private Queue<ControlTower> moveQueue = new();
    private Coroutine moveCoroutine = null;
    private bool isProcessingQueue = false;

    public void FindNearTower()
    {
        Collider[] hits = Physics.OverlapSphere(player.transform.position, 30f);

        foreach (var hit in hits)
        {
            Debug.Log(hit.gameObject.name);
            ControlTower nearTower = hit.GetComponent<ControlTower>();
            if (nearTower != null)
            {
                CurrentTower = nearTower;
                break;
            }
        }

        // 못 찾았으면 null
        CurrentTower = null;
    }

    public void RequestMove(ControlTower tower)
    {
        if (CurrentTower == null)
        {
            FindNearTower();
        }   

        if (moveQueue.Contains(tower) || CurrentTower == tower || moveCoroutine != null)
            return;

        moveQueue.Enqueue(tower);

        if (!isProcessingQueue)
        {
            isProcessingQueue = true;
            moveCoroutine = StartCoroutine(ProcessMoveQueue());
        }
    }

    private IEnumerator ProcessMoveQueue()
    {
        // Camera 간섭 방지
        cameraController.ignoreMovement = true;
        cameraController.enabled = false;
        cameraController.characterController.enabled = false;

        yield return null;

        while (moveQueue.Count > 0)
        {
            ControlTower target = moveQueue.Dequeue();

            player.transform.position = target.GetViewPos();

            CurrentTower?.ToggleMesh(true); // 이전 타워 메시 활성화
            CurrentTower = target;

            target.ToggleMesh(false); // 현재 타워 메시 비활성화
            yield return null;
        }

        isProcessingQueue = false;
        moveCoroutine = null;

        // 이동 완료 후 카메라 활성화
        cameraController.ignoreMovement = false;
        cameraController.enabled = true;
        cameraController.characterController.enabled = true;
    }

    private IEnumerator MovePlayer(ControlTower target)
    {
        cameraController.enabled = false;

        yield return null;

        player.transform.position = target.GetViewPos();

        yield return null;

        CurrentTower = target;

        cameraController.enabled = true;
        IsPlayerMoving = false;
        moveCoroutine = null;
    }
}