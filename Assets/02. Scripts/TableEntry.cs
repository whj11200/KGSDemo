using TMPro;
using UnityEngine;

public class TableEntry : MonoBehaviour
{
    [SerializeField] TMP_Text Content;

    public void SetContent(bool content)
    {
        Content.text = content ? "O" : "";
    }
}
