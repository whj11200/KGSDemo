using System.Net;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.Rendering.DebugUI;

public class CharacterBase : ObjectBase
{

    protected override bool CheckCondition(ConditionData condition)
    {

        if (!base.CheckCondition(condition)) return false;

        switch (condition.ConditionType)
        {
            case EConditionType.Distance:

                if (float.TryParse(condition.ConditionValue, out float value))
                {
                    if (OnCheckDistance(player, value) == 1) 
                    {
                        condition.Result = true;
                        return true; 
                    }
                    else
                    {
                        condition.Result = false;
                        return false;
                    }
                    condition.IsStarted = true;
                }
                else
                {
                    Debug.LogWarning($"Distance value parse failed : {condition.ConditionValue}");
                }
                break;
            case EConditionType.Clicked:
                break;
            case EConditionType.Move:
                if (condition.IsStarted) return false;

                OnMoveToTarget(gameManager.GetObject(condition.ConditionValue).gameObject.transform);
                condition.IsStarted = true;
                break;
            case EConditionType.Dialogue:

                break;
        }
        return false;
    }
}
