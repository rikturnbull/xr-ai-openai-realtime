using System;
using System.Collections.Generic;
using UnityEngine;
using XrOpenAI;
using XrOpenAI.Data.Common;
using Newtonsoft.Json;

namespace Tools
{
    public class CubeMoveTool
    {
        private CubeBehaviour _cubeBehaviour;
        private OpenAIRealtimeClient _openAiRealTimeClient;

        public CubeMoveTool(CubeBehaviour cubeBehaviour, OpenAIRealtimeClient openAiRealTimeClient)
        {
            _cubeBehaviour = cubeBehaviour;
            _openAiRealTimeClient = openAiRealTimeClient;
        }

        public Dictionary<string, Action<string>> GetHandlers()
        {
            return new Dictionary<string, Action<string>>
            {
                { "MoveCube", args => MoveCube(args) }
            };
        }

        public List<Tool> GetTools()
        {
            return new List<Tool>
            {
                new() {
                    Name = "MoveCube",
                    Description = "Moves the cube in the Unity scene through a list of positions.",
                    Type = "function",
                    Parameters = new Parameter
                    {
                        Type = "object",
                        Properties = new Dictionary<string, Property>
                        {
                            {
                                "positions", new Property
                                {
                                    Type = "array",
                                    Description = "List of 3D positions (x, y, z coordinates) for the cube to move through",
                                    Items = new Property
                                    {
                                        Type = "object",
                                        Properties = new Dictionary<string, Property>
                                        {
                                            { "x", new Property { Type = "number", Description = "X coordinate" } },
                                            { "y", new Property { Type = "number", Description = "Y coordinate" } },
                                            { "z", new Property { Type = "number", Description = "Z coordinate" } }
                                        }
                                    }
                                }
                            }
                        },
                        Required = new List<string> { "positions" }
                    }
                }
            };
        }

        private void MoveCube(string arguments)
        {
            try
            {
                MoveCubeArgs args = JsonConvert.DeserializeObject<MoveCubeArgs>(arguments);

                if (args?.Positions == null || args.Positions.Count == 0)
                {
                    return;
                }

                List<Vector3> positions = new();
                foreach (var pos in args.Positions)
                {
                    positions.Add(new Vector3(pos.X, pos.Y, pos.Z));
                }

                if (_cubeBehaviour != null)
                {
                    _cubeBehaviour.MoveToCoordinates(positions);
                    _ = _openAiRealTimeClient.SendConversationItem("Cube is now at position " + positions[positions.Count - 1].ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        [Serializable]
        private class MoveCubeArgs
        {
            [JsonProperty("positions")]
            public List<Position> Positions { get; set; }
        }

        [Serializable]
        private class Position
        {
            [JsonProperty("x")]
            public float X { get; set; }

            [JsonProperty("y")]
            public float Y { get; set; }

            [JsonProperty("z")]
            public float Z { get; set; }
        }
    }
}