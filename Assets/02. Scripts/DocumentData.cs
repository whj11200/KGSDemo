using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DocumentData", menuName = "Scriptable Objects/DocumentData")]
public class DocumentData : ScriptableObject
{
    public string Title;
    public char DocumentVersion;
    public List<Sprite> Pages = new();
    [Tooltip("0 : 벙커링 1 : 수급 2 : 터미널")]
    public List<bool> Responsibilitys = new();
}
