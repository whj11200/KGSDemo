using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieSpawner : MonoBehaviour
{
    [SerializeField] private GameObject zombiePrefab;
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();
    [SerializeField] private float spawnInterval = 0.6f;

    private Coroutine waveRoutine;

    public void StartWave(int count)
    {
        if (waveRoutine != null)
        {
            StopCoroutine(waveRoutine);
        }
        waveRoutine = StartCoroutine(SpawnWave(count));
    }

    private IEnumerator SpawnWave(int count)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnOne();
            yield return new WaitForSeconds(spawnInterval);
        }

        waveRoutine = null;
    }

    private void SpawnOne()
    {
        if (zombiePrefab == null || spawnPoints.Count == 0)
        {
            return;
        }

        Transform point = spawnPoints[Random.Range(0, spawnPoints.Count)];
        Instantiate(zombiePrefab, point.position, point.rotation);
    }
}
