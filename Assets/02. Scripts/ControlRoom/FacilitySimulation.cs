using System.Collections.Generic;
using UnityEngine;

public class FacilitySimulation : SimulationBase
{
    public FacilitySimulation(ScenarioAsset asset) : base(asset)
    {
        // Constructor logic for FacilitySimulation
    }

    public override void Initialize()
    {
        var type = Asset.Template.ScenarioType;
        bool IsNeedConfirmVent = Asset.VentValves.Count > 0;

        Dictionary<int, AudioClip> OverrideVoices = new();

        if (Asset.OverrideVoices.Count > 0)
        {
            foreach (var vo in Asset.OverrideVoices)
            {
                OverrideVoices[vo.Id] = vo.AudioClip;
            }
        }

        for (int idx = 0; idx < Asset.Template.Nodes.Count; idx++)
        {
            int nodeIndex = idx;
            var node = Asset.Template.Nodes[idx];
            var Speaker = node.Speaker;
            var content = node.Content.Replace("{Title}", Asset.ScenarioName)
                                      .Replace("{Part}", Asset.BrokenPart);

            var MessageParam = $"{Speaker}:{content}";

            var voice = node.Voice;

            if (voice == null)
            {
                if (OverrideVoices.TryGetValue(nodeIndex, out var vc))
                {
                    voice = vc;
                }
            }

            var gameNode = new GameNode();

            gameNode.Node = node;

            gameNode.OnStart = () =>
            {
                IsProcessBlocked = !node.NoCondition;

                var clipLength = voice != null ? voice.length + 1f : 3.5f;

                EventBus.Publish(
                    ScenarioEventType.ShowMessage,
                    new ScenarioEvent
                    {
                        EventType = ScenarioEventType.ShowMessage,
                        NodeID = nodeIndex,
                        EventId = $"{type}_{nodeIndex}_Content",
                        StringValue = MessageParam,
                        FloatValue = clipLength,
                        Callback = () =>
                        {
                            gameNode.OnTextEnd?.Invoke();

                            if (node.NoCondition)
                                ProcessSimulationStep();
                        },
                    });

                EventBus.Publish(
                    ScenarioEventType.Audio,
                    new ScenarioEvent
                    {
                        EventType = ScenarioEventType.Audio,
                        NodeID = idx,
                        ObjectValue = voice,
                        StringValue = Speaker,
                    });
            };

            if (!string.IsNullOrEmpty(Speaker))
            {
                gameNode.OnStart += () =>
                {
                    EventBus.Publish(ScenarioEventType.Animation,
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
                                EventId = "On",
                                NodeID = 0,
                            });

            EventBus.Publish(ScenarioEventType.Monitor,
                            new ScenarioEvent
                            {
                                EventType = ScenarioEventType.Monitor,
                                NodeID = 1,
                                EventId = "Warning_Flash",
                            });
        };

        GameNodes[1].OnEnd += () =>
        {
            EventBus.Publish(ScenarioEventType.Alarm,
                            new ScenarioEvent
                            {
                                EventType = ScenarioEventType.Alarm,
                                NodeID = 1,
                                EventId = "Off"
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

        //GameNodes[4].OnStart += () =>
        //{
        //    EventBus.Publish(ScenarioEventType.Monitor,
        //        new ScenarioEvent
        //        {
        //            EventType = ScenarioEventType.Monitor,
        //            NodeID = 4,
        //            EventId = "Show_Info",
        //        });
        //};

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

            EventBus.Publish(ScenarioEventType.Animation,
                new ScenarioEvent
                {
                    EventType = ScenarioEventType.Animation,
                    NodeID = 4,
                    EventId = "Call",
                    StringValue = "SetTrigger",
                }
            );
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
            if (!IsNeedConfirmVent)
            {
                CurrentNodeIndex++;
            }

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