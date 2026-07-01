using UnityEngine;

[CreateAssetMenu(fileName = "FacilityTemplate", menuName = "Scriptable Objects/ControlRoom/FacilityScenarioTemplate")]
public class FacilityScenarioTemplate : ScenarioTemplate
{
    public override IContentSimulation CreateSimulation(ScenarioAsset asset)
    {
        return new FacilitySimulation(asset);
    }
}
