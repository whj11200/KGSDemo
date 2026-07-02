public class FacilitySimulation : SimulationBase
{
    public FacilitySimulation(ScenarioAsset asset) : base(asset)
    {
        // Constructor logic for FacilitySimulation
    }

    public override void Initialize()
    {
        var type = Asset.Template.ScenarioType;

        for (int idx = 0; idx < Asset.Template.Nodes.Count; idx++)
        {
            var node = Asset.Template.Nodes[idx];

            GameNodes.Add(new GameNode
            {
                Node = node,
                OnStart = ()=>
                {
                    // 기본 동작: 노드 시작 시 대사 출력
                    EventBus.Publish(ScenarioEventType.ShowMessage, 
                                    new ScenarioEvent { EventType = ScenarioEventType.ShowMessage,
                                                        EventId = $"{type}_{idx}_Content",
                                                        StringValue = node.Content,
                                                        Delay = 2f});
                }
            });
        }

        // 0번: 경보음
        GameNodes[0].OnStart += () =>
        {
            EventBus.Publish(ScenarioEventType.Alarm,
                            new ScenarioEvent { EventType = ScenarioEventType.Alarm,
                                                Callback = () => ProcessSimulationStep()
                            });
        };

        // 1번: 모니터에서 경고창 점멸
        GameNodes[1].OnStart += () =>
        {
            EventBus.Publish(ScenarioEventType.Monitor,
                            new ScenarioEvent { EventType = ScenarioEventType.Monitor,
                                                EventId = "Monitor_Flash",
                            });
        };
        // 7번: 밸브 조작 시작
        // 8번: 밸브 조작 완료 및 Fade In
        // 9번: Fade Out
        // 11번 후: Fade In

        CurrentNodeIndex = -1;
    }

    public override void StartSimulation()
    {
        ProcessSimulationStep();
    }

    // 다음 노드 진행
    public override void ProcessSimulationStep()
    {
        if (CurrentNodeIndex >= 0) 
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