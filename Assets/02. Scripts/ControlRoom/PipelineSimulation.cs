using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class PipelineSimulation : SimulationBase
{
    public PipelineSimulation(ScenarioAsset asset) : base(asset)
    {
        
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

                var clipLength = node.Voice != null ? node.Voice.length + 2.5f : 2.5f;

                EventBus.Publish(
                    ScenarioEventType.ShowMessage,
                    new ScenarioEvent
                    {
                        EventType = ScenarioEventType.ShowMessage,
                        NodeID = idx,
                        EventId = $"{type}_{idx}_Content",
                        StringValue = content,
                        ObjectValue = node.Voice,
                        Callback = () =>
                        {
                            gameNode.OnTextEnd?.Invoke();

                            if (node.NoCondition)
                            {
                                ProcessSimulationStep();
                            }
                        },
                        Delay = clipLength
                    });

                EventBus.Publish(
                    ScenarioEventType.Audio,
                    new ScenarioEvent
                    {
                        EventType = ScenarioEventType.Audio,
                        NodeID = idx,
                        ObjectValue = node.Voice,
                    });
            };

            GameNodes.Add(gameNode);
        }

        // 0번: 경보음
        GameNodes[0].OnStart += () =>
        {
            EventBus.Publish(ScenarioEventType.Alarm,
                            new ScenarioEvent
                            {
                                EventType = ScenarioEventType.Alarm,
                                NodeID = 0,
                            });
        };

        // 0번: 모니터에서 경고창 점멸
        GameNodes[1].OnStart += () =>
        {
            EventBus.Publish(ScenarioEventType.Monitor,
                            new ScenarioEvent
                            {
                                EventType = ScenarioEventType.Monitor,
                                NodeID = 1,
                                EventId = "Warning_Flash",
                            });
        };

        // 0번: 모니터에 누출 지점 표시
        GameNodes[1].OnEnd += () =>
        {
            EventBus.Publish(ScenarioEventType.Monitor,
                new ScenarioEvent
                {
                    EventType = ScenarioEventType.Monitor,
                    NodeID = -1,
                    EventId = "WP_Flash",
                });
        };

        // 2번: 책임자에게 이동
        GameNodes[3].OnTextEnd += () =>
        {
            EventBus.Publish(ScenarioEventType.Camera,
                new ScenarioEvent
                {
                    EventType = ScenarioEventType.Camera,
                    NodeID = 3,
                    EventId = "Report",
                });
        };

        GameNodes[4].OnTextEnd += () =>
        {
            EventBus.Publish(ScenarioEventType.ValveConsole,
                new ScenarioEvent
                {
                    EventType = ScenarioEventType.ValveConsole,
                    NodeID = 4,
                    EventId = "Valve_ConfirmVent"
                });
        };

        // 7번: 밸브 조작 시작
        GameNodes[5].OnTextEnd += () =>
        {
            EventBus.Publish(ScenarioEventType.ValveConsole,
                new ScenarioEvent
                {
                    EventType = ScenarioEventType.ValveConsole,
                    NodeID = 5,
                    EventId = "Valve_Close"
                });
        };

        // 8번: 밸브 조작 완료 및 Fade In
        GameNodes[7].OnTextEnd += () =>
        {
            EventBus.Publish(ScenarioEventType.UI,
                new ScenarioEvent
                {
                    EventType = ScenarioEventType.UI,
                    NodeID = 7,
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
        GameNodes[9].OnTextEnd += () =>
        {
            EventBus.Publish(ScenarioEventType.UI,
                new ScenarioEvent
                {
                    EventType = ScenarioEventType.UI,
                    NodeID = 9,
                    StringValue = "보수 작업 완료",
                    Callback = () =>
                    {
                        IsProcessBlocked = false;
                        ProcessSimulationStep();
                    },
                });
        };

        GameNodes[10].OnTextEnd += () =>
        {
            EventBus.Publish(ScenarioEventType.ValveConsole,
                new ScenarioEvent
                {
                    EventType = ScenarioEventType.ValveConsole,
                    NodeID = 10,
                    StringValue = "IsolateOnly",
                    EventId = "Valve_Revert"
                });
        };

        GameNodes[11].OnTextEnd += () =>
        {
            EventBus.Publish(ScenarioEventType.ValveConsole,
                new ScenarioEvent
                {
                    EventType = ScenarioEventType.ValveConsole,
                    NodeID = 11,
                    EventId = "Valve_ConfirmVent"
                });
        };

        CurrentNodeIndex = -1;
    }
}
