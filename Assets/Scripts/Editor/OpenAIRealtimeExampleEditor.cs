#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(OpenAIRealtimeExample))]
public class OpenAIRealtimeExampleEditor : Editor
{
    private string[] microphoneDevices;
    private SerializedProperty selectedMicrophoneIndexProp;

    private void OnEnable()
    {
        selectedMicrophoneIndexProp = serializedObject.FindProperty("_selectedMicrophoneIndex");
        
        RefreshMicrophoneDevices();
    }

    private void RefreshMicrophoneDevices()
    {
        string[] devices = Microphone.devices;
        
        if (devices.Length == 0)
        {
            microphoneDevices = new string[] { "Auto", "No microphone devices found" };
        }
        else
        {
            microphoneDevices = new string[devices.Length + 1];
            microphoneDevices[0] = "Auto";
            for (int i = 0; i < devices.Length; i++)
            {
                microphoneDevices[i + 1] = devices[i];
            }
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Microphone Settings", EditorStyles.boldLabel);

        if (microphoneDevices.Length > 0 && microphoneDevices[0] != "No microphone devices found")
        {
            selectedMicrophoneIndexProp.intValue = EditorGUILayout.Popup(
                "Microphone Device", 
                selectedMicrophoneIndexProp.intValue, 
                microphoneDevices
            );
        }
        else
        {
            EditorGUILayout.HelpBox("No microphone devices found", MessageType.Warning);
        }

        if (GUILayout.Button("Refresh Microphone Devices"))
        {
            RefreshMicrophoneDevices();
            selectedMicrophoneIndexProp.intValue = 0;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
