using UnityEngine;

public class TPSGameManager : MonoBehaviour
{
    public static TPSGameManager Instance { get; private set; }

    [SerializeField] private ZombieSpawner spawner;
    [SerializeField] private int baseZombieCount = 5;
    [SerializeField] private int addPerWave = 2;
    [SerializeField] private float waveStartDelay = 2f;

    private int wave = 0;
    private int aliveZombieCount = 0;
    private int totalKillCount = 0;
    private bool gameOver;

    public int Wave => wave;
    public int AliveZombieCount => aliveZombieCount;
    public int TotalKillCount => totalKillCount;
    public bool IsGameOver => gameOver;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        Invoke(nameof(StartNextWave), waveStartDelay);
    }

    private void StartNextWave()
    {
        if (gameOver || spawner == null)
        {
            return;
        }

        wave++;
        int spawnCount = baseZombieCount + (wave - 1) * addPerWave;
        aliveZombieCount += spawnCount;
        spawner.StartWave(spawnCount);
        Debug.Log($"[TPS] Wave {wave} start. Spawn: {spawnCount}");
    }

    public void OnZombieKilled()
    {
        if (gameOver)
        {
            return;
        }

        totalKillCount++;
        aliveZombieCount = Mathf.Max(0, aliveZombieCount - 1);

        if (aliveZombieCount == 0)
        {
            Invoke(nameof(StartNextWave), waveStartDelay);
        }
    }

    public void OnPlayerDead()
    {
        if (gameOver)
        {
            return;
        }

        gameOver = true;
        Debug.Log($"[TPS] Game Over. Wave: {wave}, Kills: {totalKillCount}");
    }
}
