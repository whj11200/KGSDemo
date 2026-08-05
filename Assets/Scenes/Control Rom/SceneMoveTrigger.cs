using UnityEngine;
using UnityEngine.SceneManagement;

public enum SpawnPointType
{
    Default,
    MiniatureToKriso,
    KrisoToMiniature
}

public static class SceneMoveData
{
    public static SpawnPointType NextSpawnPoint = SpawnPointType.Default;
}

public class SceneMoveTrigger : MonoBehaviour
{
    public enum TargetScene
    {
        Kriso_Scene,
        Miniature_Scene,
    }

    [Header("이동할 씬 선택")]
    [SerializeField] private TargetScene targetScene;

    [Header("도착 위치 선택")]
    [SerializeField] private SpawnPointType targetSpawnPoint;

    [Header("씬 이름")]
    [SerializeField] private string krisoSceneName = "KRISOScene";
    [SerializeField] private string miniatureSceneName = "K_Miniature";

    [Header("충돌 대상 태그")]
    [SerializeField] private string playerTag = "Player";

    private bool isLoading;

    private void OnTriggerEnter(Collider other)
    {
        if (isLoading)
            return;

        if (!other.CompareTag(playerTag))
            return;

        string sceneName = GetTargetSceneName();

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("이동할 씬 이름이 설정되지 않았습니다.", this);
            return;
        }

        isLoading = true;

        // 다음 씬에서 이동할 위치 전달
        SceneMoveData.NextSpawnPoint = targetSpawnPoint;

        SceneManager.LoadScene(sceneName);
    }

    private string GetTargetSceneName()
    {
        switch (targetScene)
        {
            case TargetScene.Kriso_Scene:
                return krisoSceneName;

            case TargetScene.Miniature_Scene:
                return miniatureSceneName;

            default:
                return krisoSceneName;
        }
    }
}