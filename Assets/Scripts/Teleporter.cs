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
        new LightData { filter = Color.white, temparature = 4300f, intensity = 3f }, // 기본 씬
        new LightData { filter = Color.white, temparature = 10300f, intensity = 2f } // 이동된 씬
    };
    [SerializeField] private MeshRenderer mesh;
    public string nextSceneName = "AdditiveScene";
    public string TagName = "SpawnPos";
    public GameObject Player;
    CameraController cc;
    [SerializeField] Transform spawn;
    [SerializeField] ParticleSystem smoke;
    public static event Action<string> OnAddScene;
    private string baseColorProp = "_BaseColor";
    private Color originalColor;

    private void Awake()
    {
        originalColor = mesh.material.GetColor(baseColorProp);
        cc = Player.GetComponent<CameraController>();

        if (smoke == null)
        {
            smoke = GetComponentInChildren<ParticleSystem>();
            smoke.Stop();
        }

        //StructureParent.OnSwitchScene += SwitchLight;
        SceneManager.sceneLoaded += OnFieldSceneLoad;
        SceneManager.sceneUnloaded += OnFieldSceneUnLoad;
        //CameraController.OnResetPosition += SwitchLight;
    }

    private void OnDestroy()
    {
        //StructureParent.OnSwitchScene -= SwitchLight;
        SceneManager.sceneLoaded -= OnFieldSceneLoad;
        SceneManager.sceneUnloaded -= OnFieldSceneUnLoad;
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
            AddSceneRoutine = StartCoroutine(LoadAdditiveScene(nextSceneName));
    }

    Coroutine AddSceneRoutine = null; 
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
        }
        else
        {
            LightData ligihtData = lightDatas[sceneIDX];
            globalLight.intensity = ligihtData.intensity;
            globalLight.colorTemperature = ligihtData.temparature;
            globalLight.color = ligihtData.filter;
        }
    }

    private void MovePlayer(Transform target, int sceneIDX)
    {
        //SwitchLight(sceneIDX);

        Player.transform.SetPositionAndRotation(target.position, Quaternion.identity);
    }

    //private bool isSmoking = false;
    //private void ToggleSmoke()
    //{
    //    isSmoking = !isSmoking;
    //    if (isSmoking)
    //    {
    //        smoke.Play();
    //    }
    //    else
    //    {
    //        smoke.Stop();
    //    }   
    //}

    public void ClickEnter()
    {
        
    }

    
    public void ClickExit()
    {
        smoke.Play();
        LoadField();
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
    }

    private void SetColor(Color color)
    {
        mesh.material.SetColor(baseColorProp, color);
    }
}
