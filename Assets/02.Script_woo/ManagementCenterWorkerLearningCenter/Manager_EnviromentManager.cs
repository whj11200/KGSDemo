using UnityEngine;

public class Manager_EnviromentManager : MonoBehaviour
{
    [SerializeField] ManagerCenterUiManager managerCenterUiManager;



    public void CloseVavleStage()
    {
        managerCenterUiManager.ShowGuide("- CLOSE : M-31A,31B,21B,21A -");
    }
}
