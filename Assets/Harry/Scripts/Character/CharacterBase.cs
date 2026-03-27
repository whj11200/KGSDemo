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

        switch (condition.ActionType)
        {
            case EActorType.Distance:
                break;
            case EActorType.Clicked:
                break;
            case EActorType.Move:
                if (condition.IsStarted) return false;
                OnMoveToTarget(gameManager.GetObject(condition.TargetID).gameObject.transform);
                condition.IsStarted = true;
                break;
            case EActorType.Dialogue:
                break;
        }
        return false;
    }
    public virtual void AnimationStateMachine(int animationState) 
    {
        switch (animationState)
        {
            case (int)ECharacterState.Idle: 
                break;
            case (int)ECharacterState.Speak:
                break;
            case (int)ECharacterState.Smile:
                break;
            case (int)ECharacterState.Move:
                break;
            case (int)ECharacterState.Surprise:
                break;
            case (int)ECharacterState.Hello:
                break;
        }
    }
}
