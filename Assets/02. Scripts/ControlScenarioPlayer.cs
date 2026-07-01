using System;
using System.Collections;
using UnityEngine;

public class ControlScenarioPlayer : ScenarioPlayerBase<ScenarioAsset>
{
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
        action();
    }   

    public override void InitializeScenario(int assetIdx)
    {
        IsScenarioInitialized = true;
    }

    public override void ProcessScenario()
    {
        throw new System.NotImplementedException();
    }

    public override void StartScenario()
    {
        throw new System.NotImplementedException();
    }
}
