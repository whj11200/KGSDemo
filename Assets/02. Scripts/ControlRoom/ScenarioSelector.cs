using Unity.Cinemachine;
using UnityEngine;

public class ScenarioSelector : MonoBehaviour
{
    [SerializeField] private ControlScenarioPlayer ScenarioPlayer;
    [SerializeField] private int ScenarioIndex;

    [SerializeField] private CinemachineCamera ScenarioCam;
    [SerializeField] CameraSwitcher CameraSwitcher;

    private void OnEnable()
    {
        ScenarioIndex = 0;
    }

    public void SelectScenario(int _chooseIdx)
    {
        ScenarioIndex = _chooseIdx;
    }

    public void ApplySelect()
    {
        ScenarioPlayer.InitializeScenario(ScenarioIndex);
    }

    // 착석
    public void EnterSelctionMode()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        CameraSwitcher.SetCamera(ScenarioCam);
    }

    public void ExitSelectionMode()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        ScenarioIndex = 0;
        CameraSwitcher.Revert();
    }
}
