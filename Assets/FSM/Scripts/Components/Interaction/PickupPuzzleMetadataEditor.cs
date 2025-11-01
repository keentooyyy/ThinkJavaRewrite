#if UNITY_EDITOR
using UnityEditor;

namespace GameInteraction
{
    [CustomEditor(typeof(PickupPuzzleMetadata))]
    public class PickupPuzzleMetadataEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var kindProp = serializedObject.FindProperty("kind");
            EditorGUILayout.PropertyField(kindProp);

            var kind = (PickupPuzzleKind)kindProp.enumValueIndex;
            if (kind == PickupPuzzleKind.DataType)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("dataType"));
            }
            else
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("variableName"));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif


