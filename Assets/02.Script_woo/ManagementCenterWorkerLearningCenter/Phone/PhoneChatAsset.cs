using System;
using System.Collections.Generic;
using UnityEngine;

public enum PhoneChatSide
{
    Left,   // 상대방
    Right,  // 플레이어/관리소 담당자
    Center  // 안내문
}

[CreateAssetMenu(menuName = "Phone/Phone Chat Asset")]
public class PhoneChatAsset : ScriptableObject
{
    public List<PhoneChatLine> lines = new();
}

[Serializable]
public class PhoneChatLine
{
    public PhoneChatSide side;

    public string speaker;

    [TextArea(2, 5)]
    public string message;
}