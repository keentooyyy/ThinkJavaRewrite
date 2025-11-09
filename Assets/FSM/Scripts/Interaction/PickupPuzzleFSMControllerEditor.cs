#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using NodeCanvas.Editor;
using NodeCanvas.StateMachines;

namespace GameInteraction.Editor
{
    [CustomEditor(typeof(PickupPuzzleFSMController))]
    [CanEditMultipleObjects]
    public class PickupPuzzleFSMControllerEditor : GraphOwnerInspector
    {
        protected override void OnPostExtraGraphOptions()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            
            EditorGUILayout.LabelField("Puzzle Configuration", EditorStyles.boldLabel);
            
            var puzzleConfigProp = serializedObject.FindProperty("puzzleConfig");
            EditorGUILayout.PropertyField(puzzleConfigProp, new GUIContent("Puzzle Config", "The configuration that defines valid puzzle combinations"));
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            
            EditorGUILayout.LabelField("Slot Pairs", EditorStyles.boldLabel);
            var slotPairsProp = serializedObject.FindProperty("slotPairs");
            EditorGUILayout.PropertyField(slotPairsProp, new GUIContent("Slot Pairs", "List of slot pairs. Each pair can have its own success event. Configure success events in config or per-pair."), true);
            
            EditorGUILayout.HelpBox("Success Events: Configure in PickupPuzzleConfig per RequiredPair, or set per SlotPair to override. Only wrong slots are rejected, not both.", MessageType.Info);
            
            if (slotPairsProp != null && slotPairsProp.arraySize == 0)
            {
                EditorGUILayout.HelpBox("At least one slot pair is required. Click the '+' button to add a pair.", MessageType.Warning);
            }
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif

