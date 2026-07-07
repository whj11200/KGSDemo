using Unity.VisualScripting;

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
            var Speaker = node.Speaker;
            var content = $"{Speaker}:{node.Content.Replace("{Title}", Asset.ScenarioName)}";

            var gameNode = new GameNode();

            gameNode.Node = node;

            gameNode.OnStart = () =>
            {
                IsProcessBlocked = !node.NoCondition;

                EventBus.Publish(
                    ScenarioEventType.ShowMessage,
                    new ScenarioEvent
                    {
                        EventType = ScenarioEventType.ShowMessage,
                        NodeID = idx,
                        EventId = $"{type}_{idx}_Content",
                        StringValue = content,
                        Callback = () => gameNode.OnTextEnd?.Invoke(),
                        Delay = 2f
                    });

                EventBus.Publish(
                    ScenarioEventType.Audio,
                    new ScenarioEvent
                    {
                        EventType = ScenarioEventType.Audio,
                        NodeID = idx,
                        ObjectValue = node.Voice,
                        Callback = () =>
                        {
                            if (node.NoCondition)
                                ProcessSimulationStep();
                        },
                        Delay = node.Voice != null ? node.Voice.length + 1.5f : 2f
                    });
            };

            GameNodes.Add(gameNode);
        }

        // 0번: 경보음
        GameNodes[0].OnStart += () =>
        {
            EventBus.Publish(ScenarioEventType.Alarm,
                            new ScenarioEvent { 
                                EventType = ScenarioEventType.Alarm,
                                NodeID = 0,
                            });
        };

        // 1번: 모니터에서 경고창 점멸
        GameNodes[1].OnStart += () =>
        {
            EventBus.Publish(ScenarioEventType.Monitor,
                            new ScenarioEvent { 
                                EventType = ScenarioEventType.Monitor,
                                NodeID = 1,
                                EventId = "Warning_Flash",
                            });
        };

        // 2번: 모니터에 누출 지점 표시
        GameNodes[2].OnEnd += () =>
        {
            EventBus.Publish(ScenarioEventType.Monitor,
                new ScenarioEvent
                {
                    EventType = ScenarioEventType.Monitor,
                    NodeID = 3, // 2번 노드는 자동 진행되므로 3번으로 검사
                    EventId = "WP_Flash",
                });
        };

        // 5번: 책임자에게 이동
        GameNodes[5].OnTextEnd += () =>
        {
            EventBus.Publish(ScenarioEventType.Camera,
                new ScenarioEvent
                {
                    EventType = ScenarioEventType.Camera,
                    NodeID = 5,
                    EventId = "Report",
                });
        };

        // 7번: 밸브 조작 시작
        GameNodes[7].OnTextEnd += () =>
        {
            EventBus.Publish(ScenarioEventType.ValveConsole,
                new ScenarioEvent
                {
                    EventType = ScenarioEventType.ValveConsole,
                    NodeID = 7,
                    EventId = "Valve_Close"
                });
        };

        // 8번: 밸브 조작 완료 및 Fade In
        GameNodes[8].OnTextEnd += () =>
        {
            EventBus.Publish(ScenarioEventType.UI,
                new ScenarioEvent
                {
                    EventType = ScenarioEventType.UI,
                    NodeID = 8,
                    StringValue = $"{Asset.Template.WaitTime}분 경과," +
                                  $"\r\n잔류 가스 방출 완료",
                    Callback = () =>
                    {
                        IsProcessBlocked = false;
                        ProcessSimulationStep();
                    }
                });
        };

        // 10번: Fade Out
        GameNodes[10].OnTextEnd += () =>
        {
            EventBus.Publish(ScenarioEventType.UI,
                new ScenarioEvent
                {
                    EventType = ScenarioEventType.UI,
                    NodeID = 10,
                    StringValue = "보수 작업 완료",
                    Callback = () =>
                    {
                        IsProcessBlocked = false;
                        ProcessSimulationStep();
                    },
                });
        };

        GameNodes[11].OnTextEnd += () =>
        {
            EventBus.Publish(ScenarioEventType.ValveConsole,
                new ScenarioEvent
                {
                    EventType = ScenarioEventType.ValveConsole,
                    NodeID = 11,
                    EventId = "Valve_Revert"
                });
        };

        GameNodes[12].OnTextEnd += () =>
        {
            EventBus.Publish(ScenarioEventType.ValveConsole,
                new ScenarioEvent
                {
                    EventType = ScenarioEventType.ValveConsole,
                    NodeID = 12,
                    StringValue = GameNodes[12].Node.Content,
                    EventId = "Valve_ConfirmVent"
                });
        };

        CurrentNodeIndex = -1;
    }

    public override void StartSimulation()
    {
        ProcessSimulationStep();
    }
}