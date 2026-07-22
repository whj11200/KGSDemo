using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class ScenarioSelector : MonoBehaviour
{
    [SerializeField] private ControlScenarioPlayer ScenarioPlayer;
    [SerializeField] private int ScenarioIndex;

    [SerializeField] List<Image> Buttons;
    [SerializeField] Color SelectedColor;

    [SerializeField] CinemachineCamera ScenarioCam;
    [SerializeField] CameraSwitcher CameraSwitcher;
    [SerializeField] CameraController CameraController;

    [SerializeField] AudioSource AudioSource;
    [SerializeField] AudioClip ClickClip;

    private void Start()
    {
        gameObject.SetActive(true);
    }

    private void OnEnable()
    {
        ScenarioIndex = 0;
    }

    public void SelectScenario(int _chooseIdx)
    {
        AudioSource.PlayOneShot(ClickClip);

        Buttons[ScenarioIndex].color = Color.white;

        ScenarioIndex = _chooseIdx;
        Buttons[ScenarioIndex].color = SelectedColor;
    }

    public void ApplySelect()
    {
        ScenarioPlayer.InitializeScenario(ScenarioIndex);
        gameObject.SetActive(false);
    }

    // 착석
    public void EnterSelctionMode()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        CameraController.SetMoveLockState(true);
        CameraSwitcher.SetCamera(ScenarioCam);
    }

    public void ExitSelectionMode()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        ScenarioIndex = 0;

        CameraController.SetMoveLockState(false);
        CameraSwitcher.Revert();
    }
}
