using System;
using System.Collections;
using UnityEngine;

public class SceneSpawnManager : MonoBehaviour
{
    [Serializable]
    public class SpawnPointData
    {
        public SpawnPointType spawnPointType;
        public Transform spawnTransform;
    }

    [Header("Player 설정")]
    [SerializeField] private string playerTag = "Player";

    [Header("씬 내 도착 위치")]
    [SerializeField] private SpawnPointData[] spawnPoints;

    private IEnumerator Start()
    {
        // Player의 Awake, Start 처리가 끝날 때까지 한 프레임 대기
        yield return null;

        GameObject player = GameObject.FindGameObjectWithTag(playerTag);

        if (player == null)
        {
            Debug.LogError("Player 태그를 가진 오브젝트를 찾지 못했습니다.", this);
            yield break;
        }

        Transform spawnPoint = GetSpawnPoint(SceneMoveData.NextSpawnPoint);

        if (spawnPoint == null)
        {
            Debug.LogWarning(
                $"SpawnPoint를 찾지 못했습니다: {SceneMoveData.NextSpawnPoint}",
                this
            );

            yield break;
        }

        MovePlayer(player, spawnPoint);

        // 사용 후 초기화
        SceneMoveData.NextSpawnPoint = SpawnPointType.Default;
    }

    private Transform GetSpawnPoint(SpawnPointType spawnPointType)
    {
        foreach (SpawnPointData spawnPoint in spawnPoints)
        {
            if (spawnPoint.spawnPointType == spawnPointType)
                return spawnPoint.spawnTransform;
        }

        return null;
    }

    private void MovePlayer(GameObject player, Transform spawnPoint)
    {
        CharacterController characterController =
            player.GetComponent<CharacterController>();

        Rigidbody playerRigidbody =
            player.GetComponent<Rigidbody>();

        // CharacterController가 켜진 상태에서는 위치 이동이 제대로 안 될 수 있음
        if (characterController != null)
            characterController.enabled = false;

        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
        }

        player.transform.SetPositionAndRotation(
            spawnPoint.position,
            spawnPoint.rotation
        );

        if (characterController != null)
            characterController.enabled = true;
    }
}