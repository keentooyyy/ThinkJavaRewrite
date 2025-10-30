#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using GameInput;

namespace GameInput.Editor
{
    /// <summary>
    /// Custom property drawer for ButtonNameAttribute
    /// Shows a dropdown of button names from InputConfig
    /// </summary>
    [CustomPropertyDrawer(typeof(ButtonNameAttribute))]
    public class ButtonNameDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ButtonNameAttribute buttonAttr = (ButtonNameAttribute)attribute;
            
            // Find the InputConfig field on the same object
            SerializedProperty configProp = property.serializedObject.FindProperty(buttonAttr.configFieldName);
            
            if (configProp == null || configProp.objectReferenceValue == null)
            {
                // No config found - show normal string field with hint
                Rect fieldRect = new Rect(position.x, position.y, position.width - 100, position.height);
                Rect labelRect = new Rect(position.x + position.width - 95, position.y, 95, position.height);
                
                EditorGUI.PropertyField(fieldRect, property, label);
                EditorGUI.LabelField(labelRect, "(assign config)", EditorStyles.miniLabel);
                return;
            }
            
            InputConfig config = configProp.objectReferenceValue as InputConfig;
            if (config == null || config.keyboardBindings == null || config.keyboardBindings.Length == 0)
            {
                EditorGUI.PropertyField(position, property, label);
                EditorGUI.LabelField(new Rect(position.x + position.width - 120, position.y, 120, position.height), 
                    "(config has no buttons)", EditorStyles.miniLabel);
                return;
            }
            
            // Build button name array
            string[] buttonNames = new string[config.keyboardBindings.Length];
            for (int i = 0; i < config.keyboardBindings.Length; i++)
            {
                buttonNames[i] = config.keyboardBindings[i].buttonName;
            }
            
            // Find current selection
            int currentIndex = System.Array.IndexOf(buttonNames, property.stringValue);
            if (currentIndex < 0) currentIndex = 0;
            
            // Show dropdown
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            int newIndex = EditorGUI.Popup(position, label.text, currentIndex, buttonNames);
            if (EditorGUI.EndChangeCheck() && newIndex >= 0 && newIndex < buttonNames.Length)
            {
                property.stringValue = buttonNames[newIndex];
            }
            EditorGUI.EndProperty();
        }
    }
}
#endif

