using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class CharacterBase : ObjectBase
{


    protected override bool CheckCondition(ConditionData condition)
    {
        if (condition == null || condition.IsProcessing) return false;
        switch (condition.ConditionType)
        {
            case EConditionType.Distance:
                break;
            case EConditionType.Clicked:
                break;
            case EConditionType.Move:
                OnMoveToTarget(gameManager.GetObject(condition.ConditionValue).gameObject.transform);
                condition.IsProcessing = true;
                break;
        }
        return false;
    }

    /// <summary>
    /// Move 조건이 발생했을 때, ConditionValue로 전달된 ObjectID에 해당하는 오브젝트의 위치로 케릭터를 이동시키는 메서드
    /// 한번 작동 후 ConditionData의 IsProcessing을 true로 바꿔서 중복 작동 방지
    /// </summary>
    /// <param name="location">
    /// 이 위치로 케릭터를 이동시킨다.
    /// </param>
    protected void OnMoveToTarget(Transform location)
    {
        if (location == null) return;
        this.gameObject.transform.position = location.position;
        
    }

    protected void OnAnimationMove(int state)
    {

    }
}
