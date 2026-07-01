using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class FacilitySimulation : SimulationBase
{
    public FacilitySimulation(ScenarioAsset asset) : base(asset)
    {
        // Constructor logic for FacilitySimulation
    }

    public override void Initialize()
    {
        foreach(var node in Asset.Template.Nodes)
        {
            GameNodes.Add(new GameNode
            {
                Node = node,
            });
        }

        // 0번: 경보음
        GameNodes[0].OnStart += () => {
            
        };
        // 1번: 모니터 화면 전환
        // 7번: 밸브 조작 시작
        // 8번: 밸브 조작 완료 및 Fade In
        // 9번: Fade Out
        // 11번 후: Fade In

        CurrentNodeIndex = 0;
    }

    public override void StartSimulation()
    {
        GameNodes[CurrentNodeIndex]?.OnStart?.Invoke();
    }

    // 다음 노드 진행
    public override void ProcessSimulationStep()
    {
        GameNodes[CurrentNodeIndex]?.OnEnd?.Invoke();
        CurrentNodeIndex++;

        if (CurrentNodeIndex >= GameNodes.Count)
        {
            EndSimulation();
            return;
        }

        GameNodes[CurrentNodeIndex]?.OnStart?.Invoke();
    }
}