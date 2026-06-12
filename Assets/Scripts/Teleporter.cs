using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public struct LightData
{
    public Color filter;
    public float temparature;
    public float intensity;
}

public class Teleporter : MonoBehaviour, IMouseInteractable
{
    [SerializeField] Light globalLight;

    public static List<LightData> lightDatas = new()
    {
        new LightData { filter = Color.white, temparature = 4300f, intensity = 3f },
        new LightData { filter = Color.white, temparature = 10300f, intensity = 2f }
    };

    [SerializeField] private MeshRenderer mesh;
    public string nextSceneName = "AdditiveScene";
    public string TagName = "SpawnPos";
    public GameObject Player;
    public GameObject UiCanvas_Buttons;
    public PipeInterestion pipeInterestion;

    CameraController cc;

    [SerializeField] Transform spawn;

    [Header("Particle")]
    [SerializeField] private List<ParticleSystem> smokes = new();

    public static event Action<string> OnAddScene;

    private string baseColorProp = "_BaseColor";
    private Color originalColor;

    private Coroutine AddSceneRoutine = null;
    [Header("Bools")]
    [SerializeField] private bool isSmokePlaying = false;
    [SerializeField] public bool isCanvas = false;
    [SerializeField] public bool isButtonActive = false;

    [Header("Leak Mode")]
    [SerializeField] private LeakFire leakFire;
    [SerializeField] private LeakGas leakGas;

    private LeakMode currentLeakMode = LeakMode.None;
    private enum LeakMode
    {
        None,
        Fire,
        Gas
    }

    private void Awake()
    {
        UiCanvas_Buttons.SetActive(isCanvas);
        originalColor = mesh.material.GetColor(baseColorProp);
        cc = Player.GetComponent<CameraController>();

        InitSmokes();

        SceneManager.sceneLoaded += OnFieldSceneLoad;
        SceneManager.sceneUnloaded += OnFieldSceneUnLoad;
    }
    private void Start()
    {
        if (leakFire != null && leakFire.Button != null)
            leakFire.Button.onClick.AddListener(RequestFireLeak);

        if (leakGas != null && leakGas.Button != null)
            leakGas.Button.onClick.AddListener(RequestGasLeak);
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnFieldSceneLoad;
        SceneManager.sceneUnloaded -= OnFieldSceneUnLoad;

        if (leakFire != null && leakFire.Button != null)
            leakFire.Button.onClick.RemoveListener(RequestFireLeak);

        if (leakGas != null && leakGas.Button != null)
            leakGas.Button.onClick.RemoveListener(RequestGasLeak);
    }
    public void RequestFireLeak()
    {
        
        if (!isCanvas)
            return;

        if (currentLeakMode == LeakMode.Fire)
        {
            StopAllLeaks(false);
            return;
        }

        StopAllLeaks(false);

        if (leakFire != null)
            leakFire.PlayLeak();

        currentLeakMode = LeakMode.Fire;
        isButtonActive = true;
    }
    public void RequestGasLeak()
    {
        if (!isCanvas)
            return;

        if (currentLeakMode == LeakMode.Gas)
        {
            StopAllLeaks(false);
            return;
        }

        StopAllLeaks(false);

        if (leakGas != null)
            leakGas.PlayLeak();

        currentLeakMode = LeakMode.Gas;
        isButtonActive = true;
    }
    private void StopAllLeaks(bool clear)
    {
        currentLeakMode = LeakMode.None;

        if (leakFire != null)
            leakFire.StopLeak(clear);

        if (leakGas != null)
            leakGas.StopLeak(clear);

        StopAllSmokes(clear);
        isButtonActive = false;
    }
    public void ToggleCanvasActive()
    {
        isCanvas = !isCanvas;

        if (UiCanvas_Buttons != null)
            UiCanvas_Buttons.SetActive(isCanvas);

        if (!isCanvas)
        {
            StopAllLeaks(false);
            isButtonActive = false;
        }
    }

    public void AllCancel()
    {
        isCanvas = false;

        SetColor(originalColor);

        if (UiCanvas_Buttons != null)
            UiCanvas_Buttons.SetActive(false);

        StopAllLeaks(true);
        isButtonActive = false;
    }
    private void InitSmokes()
    {
        // Inspector에 직접 넣지 않았으면 자식에서 자동으로 찾음
        if (smokes == null)
        {
            smokes = new List<ParticleSystem>();
        }

        if (smokes.Count == 0)
        {
            ParticleSystem[] childSmokes = GetComponentsInChildren<ParticleSystem>(true);
            smokes.AddRange(childSmokes);
        }

        StopAllSmokes(true);
    }
  
   
    public void StopAllSmokes(bool clear = false)
    {
        for (int i = 0; i < smokes.Count; i++)
        {
            if (smokes[i] == null)
                continue;

            if (clear)
            {
                smokes[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
            else
            {
                smokes[i].Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }
    }







    private void OnFieldSceneLoad(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == nextSceneName)
        {
            StructureComp.isFieldLoaded = true;
        }
    }

    private void OnFieldSceneUnLoad(Scene scene)
    {
        if (scene.name == nextSceneName)
        {
            StructureComp.isFieldLoaded = false;
        }
    }

    public void LoadField()
    {
        if (AddSceneRoutine == null)
        {
            AddSceneRoutine = StartCoroutine(LoadAdditiveScene(nextSceneName));
        }
    }

    private IEnumerator LoadAdditiveScene(string sceneName)
    {
        Debug.Log($"Teleporter: Loading Scene {sceneName}");

        var nextScene = SceneManager.GetSceneByName(sceneName);
        cc.enabled = false;

        if (!nextScene.isLoaded)
        {
            var oper = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            yield return oper;

            var scene = SceneManager.GetSceneByName(sceneName);

            OnAddScene?.Invoke(nextSceneName);
            SceneManager.SetActiveScene(scene);
        }

        yield return null;

        cc.enabled = true;
        AddSceneRoutine = null;
    }

    private void SwitchLight(int sceneIDX)
    {
        if (sceneIDX < 0 || sceneIDX >= lightDatas.Count)
        {
            Debug.LogError($"Teleporter: Invalid sceneIDX {sceneIDX}");
            return;
        }

        LightData ligihtData = lightDatas[sceneIDX];
        globalLight.intensity = ligihtData.intensity;
        globalLight.colorTemperature = ligihtData.temparature;
        globalLight.color = ligihtData.filter;
    }

    private void MovePlayer(Transform target, int sceneIDX)
    {
        Player.transform.SetPositionAndRotation(target.position, Quaternion.identity);
    }

    public void UnloadField()
    {
        Scene fieldScene = SceneManager.GetSceneByName(nextSceneName);

        if (fieldScene.isLoaded)
        {
            SceneManager.UnloadSceneAsync(nextSceneName);
            Debug.Log("필드 씬을 언로드했습니다.");
        }
    }

    public void ClickEnter()
    {
    }

    public void ClickExit()
    {
        ToggleCanvasActive();
        SetColor(originalColor);
    }

    public void HoverEnter()
    {
        SetColor(Color.green);
    }

    public void HoverExit()
    {
        SetColor(originalColor);
    }

    public void ClickCancle()
    {
        SetColor(originalColor);

        // 필요하면 취소할 때 전체 Smoke 정지
        //StopAllSmokes(true);
    }

    private void SetColor(Color color)
    {
        mesh.material.SetColor(baseColorProp, color);
    }

}