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
            
            // Initialize properties when needed
            var dataTypeSlotProp = serializedObject.FindProperty("dataTypeSlot");
            var variableSlotProp = serializedObject.FindProperty("variableSlot");
            var puzzleConfigProp = serializedObject.FindProperty("puzzleConfig");
            var puzzleSuccessEventProp = serializedObject.FindProperty("puzzleSuccessEvent");
            
            EditorGUILayout.PropertyField(dataTypeSlotProp, new GUIContent("Data Type Slot", "The slot that accepts data type pickups"));
            EditorGUILayout.PropertyField(variableSlotProp, new GUIContent("Variable Slot", "The slot that accepts variable pickups"));
            EditorGUILayout.PropertyField(puzzleConfigProp, new GUIContent("Puzzle Config", "The configuration that defines valid puzzle combinations"));
            EditorGUILayout.PropertyField(puzzleSuccessEventProp, new GUIContent("Puzzle Success Event", "UI event name to trigger when puzzle is solved"));
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif

