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

                var clipLength = node.Voice != null ? node.Voice.length + 1f : 3.5f;

                EventBus.Publish(
                    ScenarioEventType.ShowMessage,
                    new ScenarioEvent
                    {
                        EventType = ScenarioEventType.ShowMessage,
                        NodeID = idx,
                        EventId = $"{type}_{idx}_Content",
                        StringValue = content,
                        FloatValue = clipLength,
                        Callback = () =>
                        {
                            gameNode.OnTextEnd?.Invoke();

                            if (node.NoCondition)
                            {
                                ProcessSimulationStep();
                            }
                        },
                    });

                EventBus.Publish(
                    ScenarioEventType.Audio,
                    new ScenarioEvent
                    {
                        EventType = ScenarioEventType.Audio,
                        NodeID = idx,
                        ObjectValue = node.Voice,
                        StringValue = Speaker,
                    });
            };

            if (!string.IsNullOrEmpty(Speaker))
            {
                gameNode.OnStart += () =>
                {
                    EventBus.Publish( ScenarioEventType.Animation,
                        new ScenarioEvent
                        {
                            EventType = ScenarioEventType.Animation,
                            NodeID = idx,
                            EventId = "Talk",
                            StringValue = "SetBool",
                            BoolValue = true
                        }
                    );
                };

                gameNode.OnTextEnd += () =>
                {
                    EventBus.Publish(ScenarioEventType.Animation,
                        new ScenarioEvent
                        {
                            EventType = ScenarioEventType.Animation,
                            NodeID = idx,
                            EventId = "Talk",
                            StringValue = "SetBool",
                            BoolValue = false
                        }
                    );
                };
            }

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
                                EventId = "On"
                            });

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
            EventBus.Publish(ScenarioEventType.Alarm,
                            new ScenarioEvent
                            {
                                EventType = ScenarioEventType.Alarm,
                                NodeID = 1,
                                EventId = "Off"
                            });

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
            // 방산밸브 개방 확인
            EventBus.Publish(ScenarioEventType.ValveConsole,
                new ScenarioEvent
                {
                    EventType = ScenarioEventType.ValveConsole,
                    NodeID = 4,
                    IntValue = 5,
                });

            EventBus.Publish(ScenarioEventType.Animation,
                new ScenarioEvent
                {
                    EventType = ScenarioEventType.Animation,
                    NodeID = 4,
                    EventId = "Call",
                    StringValue = "SetTrigger",
                }
            );

            EventBus.Publish(ScenarioEventType.Monitor,
                new ScenarioEvent
                {
                    EventType = ScenarioEventType.Monitor,
                    NodeID = 4,
                    EventId = "StopWPBlink",
                });
        };

        // 구간차단
        GameNodes[5].OnTextEnd += () =>
        {
            EventBus.Publish(ScenarioEventType.ValveConsole,
                new ScenarioEvent
                {
                    EventType = ScenarioEventType.ValveConsole,
                    NodeID = 5,
                    IntValue = 1,
                });
        };

        // 방산밸브 개방
        GameNodes[6].OnTextEnd += () =>
        {
            EventBus.Publish(ScenarioEventType.ValveConsole,
                new ScenarioEvent
                {
                    EventType = ScenarioEventType.ValveConsole,
                    NodeID = 6,
                    IntValue = 2,
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

        // 구간차단 => 정상화
        GameNodes[10].OnTextEnd += () =>
        {
            EventBus.Publish(ScenarioEventType.ValveConsole,
                new ScenarioEvent
                {
                    EventType = ScenarioEventType.ValveConsole,
                    NodeID = 10,
                    IntValue = 4,
                });
        };

        // 방산밸브 확인
        GameNodes[11].OnTextEnd += () =>
        {
            EventBus.Publish(ScenarioEventType.ValveConsole,
                new ScenarioEvent
                {
                    EventType = ScenarioEventType.ValveConsole,
                    NodeID = 11,
                    IntValue = 5,
                });
        };

        CurrentNodeIndex = -1;
    }
}
