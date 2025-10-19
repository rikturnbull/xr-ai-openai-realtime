using XrOpenAI;
using XrOpenAI.Data.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Tools;

public class OpenAIRealtimeExample : MonoBehaviour
{
    enum VOICE { alloy, ash, ballad, coral, echo, sage, shimmer, verse, marin, cedar }

    [Header("OpenAI Configuration")]
    [SerializeField] private string _apiKey = "";
    [TextArea(3, 10)] [SerializeField] private string _systemPrompt = "You are a helpful assistant.";
    [SerializeField] private VOICE _voice = VOICE.alloy;

    [Header("UI/Debug")]
    [SerializeField] private TextMeshProUGUI _statusText;
    [SerializeField] private TextMeshProUGUI _agentTranscriptText;
    [SerializeField] private bool _autoConnectOnStart = false;
    [SerializeField] private CubeBehaviour _cubeBehaviour;

    [Header("Audio")]
    [HideInInspector] [SerializeField] private int _selectedMicrophoneIndex = 0;

    private OpenAIRealtimeClient _openAiRealTimeClient;
    private bool _isConnected = false;

    public string GetSelectedMicrophoneDevice()
    {
        if (_selectedMicrophoneIndex == 0)
        {
            return null;
        }
        
        int deviceIndex = _selectedMicrophoneIndex - 1;
        
        if (Microphone.devices.Length > 0 && deviceIndex < Microphone.devices.Length)
        {
            return Microphone.devices[deviceIndex];
        }
        
        return null;
    }

    private IEnumerator Start()
    {
        _openAiRealTimeClient = new OpenAIRealtimeClient(transform, GetSelectedMicrophoneDevice());
        _openAiRealTimeClient.onError += OnError;
        _openAiRealTimeClient.onStatus += OnStatus;
        _openAiRealTimeClient.onReady += OnReady;
        _openAiRealTimeClient.onTranscription += OnTranscription;
        OnStatus("Initialized. Ready to connect.");

        if (_autoConnectOnStart)
        {
            Task task = Connect();
            yield return new WaitUntil(() => task.IsCompleted);
        }
    }

    private async Task Connect()
    {
        if (_isConnected) return;

        CubeMoveTool cubeMoveTool = new(_cubeBehaviour, _openAiRealTimeClient);
        List<Tool> tools = cubeMoveTool.GetTools();
        Dictionary<string, Action<string>> functionHandlers = cubeMoveTool.GetHandlers();

        OnStatus("Connecting to OpenAI Realtime API...");
        _isConnected = await _openAiRealTimeClient.Connect(_apiKey, _systemPrompt, _voice.ToString(), tools, functionHandlers);

        if (!_isConnected) OnError("Failed to connect to OpenAI");
        else OnStatus("Connected to OpenAI Realtime API");
    }

    public async Task Disconnect()
    {
        if (!_isConnected) return;
        if (_openAiRealTimeClient != null)
        {
            await _openAiRealTimeClient.Close();
        }
        _isConnected = false;
        OnStatus("Disconnected");
    }

    private void OnError(string errorMessage) => _statusText.text = errorMessage;
    private void OnStatus(string status) => _statusText.text = status;
    private void OnTranscription(string transcription) => _agentTranscriptText.text = transcription;

    private void OnReady()
    {
        _ = _openAiRealTimeClient.SendConversationItem("Cube is now at position " + _cubeBehaviour.transform.position.ToString());
    }

    private List<Tool> CreateTools()
    {
        List<Tool> tools = new List<Tool>();

        Tool moveCubeTool = new Tool
        {
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
        };

        tools.Add(moveCubeTool);
        return tools;
    }

    // OpenAI Tool To Move Cube
    private void MoveCube(string arguments)
    {
        try
        {
            // Parse the JSON arguments
            var args = Newtonsoft.Json.JsonConvert.DeserializeObject<MoveCubeArgs>(arguments);
            
            if (args?.Positions == null || args.Positions.Count == 0)
            {
                OnError("MoveCube: No positions provided");
                return;
            }

            // Convert the position objects to Vector3
            List<Vector3> positions = new List<Vector3>();
            foreach (var pos in args.Positions)
            {
                positions.Add(new Vector3(pos.X, pos.Y, pos.Z));
                Debug.Log($"MoveCube: Adding position ({pos.X}, {pos.Y}, {pos.Z})");
            }

            if (_cubeBehaviour != null)
            {
                _cubeBehaviour.MoveToCoordinates(positions);
                OnStatus($"Moving cube through {positions.Count} positions");
                _ = _openAiRealTimeClient.SendConversationItem("Cube is now at position " + positions[positions.Count - 1].ToString());
            }
            else
            {
                OnError("MoveCube: CubeBehaviour is not assigned");
            }
        }
        catch (Exception ex)
        {
            OnError($"MoveCube error: {ex.Message}");
            Debug.LogException(ex);
        }
    }

    // Helper classes for parsing MoveCube arguments
    [Serializable]
    private class MoveCubeArgs
    {
        [Newtonsoft.Json.JsonProperty("positions")]
        public List<Position> Positions { get; set; }
    }

    [Serializable]
    private class Position
    {
        [Newtonsoft.Json.JsonProperty("x")]
        public float X { get; set; }
        
        [Newtonsoft.Json.JsonProperty("y")]
        public float Y { get; set; }
        
        [Newtonsoft.Json.JsonProperty("z")]
        public float Z { get; set; }
    }
    // private void OnDestroy() => Disconnect().GetAwaiter().GetResult();
    // private void OnApplicationQuit() => Disconnect().GetAwaiter().GetResult();
}
