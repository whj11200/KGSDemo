using UnityEngine;

[CreateAssetMenu(fileName = "PipelineTemplate", menuName = "Scriptable Objects/ControlRoom/PipelineScenarioTemplate")]
public class PipelineScenarioTemplate : ScenarioTemplate
{
    public override IContentSimulation CreateSimulation(ScenarioAsset asset)
    {
        return new PipelineSimulation(asset);
    }
}
