using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class ControlScenarioPlayer : ScenarioPlayerBase<ScenarioAsset>
{
    [SerializeField] CameraSwitcher CameraSwitcher;
    [SerializeField] List<CinemachineCamera> MonitorCameras = new();

    [SerializeField] int CameraIdx = 0;

    private void Awake()
    {
        CameraIdx = MonitorCameras.Count / 2; // 중앙 카메라로 초기화
    }

    private void Start()
    {
        Invoke("EnterControlRoom", 1.5f);
    }

    public override void EndScenario()
    {
        
    }

    public void EnterControlRoom()
    {
        DialogueUI.SetSpeaker();
        DialogueUI.SetBodyText("가스 누출 조치 훈련을 시작합니다." +
                                "\r\n통제실에 입장 후 관제석에 착석하십시오.");
        DialogueUI.Show(true);

        StartCoroutine(Delay(()=>
        {
            DialogueUI.Show(false);
        }, 5f));
    }

    IEnumerator Delay(Action action, float delay)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke();
    }   

    public override void InitializeScenario(int assetIdx)
    {
        var selectedAsset = ScenarioAssets[assetIdx];

        foreach (var node in selectedAsset.Template.Nodes)
        {
            GameNodes.Add(new GameNode
            {
                Node = node,
            });
        }


        IsScenarioInitialized = true;
    }

    public override void ProcessScenario()
    {
        
    }

    public override void StartScenario()
    {
        
    }

    public void SwitchCameraLeft()
    {
        if (CameraIdx < MonitorCameras.Count - 1)
        {
            CameraIdx++;
            CameraSwitcher.SetCamera(MonitorCameras[CameraIdx]);
        }
    }

    public void SwitchCameraRight()
    {
        if (CameraIdx > 0)
        {
            CameraIdx--;
            CameraSwitcher.SetCamera(MonitorCameras[CameraIdx]);
        }
    }
}
