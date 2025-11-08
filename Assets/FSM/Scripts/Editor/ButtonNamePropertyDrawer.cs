using UnityEngine;
using UnityEditor;
using GameInput;
using System.Linq;

[CustomPropertyDrawer(typeof(ButtonNameAttribute))]
public class ButtonNamePropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.String)
        {
            EditorGUI.LabelField(position, label.text, "ButtonName attribute can only be used on string fields");
            return;
        }

        ButtonNameAttribute buttonNameAttr = (ButtonNameAttribute)attribute;
        
        // Get the InputConfig from the serialized object
        InputConfig inputConfig = GetInputConfig(property, buttonNameAttr.configFieldName);
        
        if (inputConfig == null)
        {
            // No InputConfig found - show as regular text field with warning
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.PropertyField(position, property, label);
            
            Rect warningRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.HelpBox(warningRect, $"InputConfig '{buttonNameAttr.configFieldName}' not found or not assigned", MessageType.Warning);
            EditorGUI.EndProperty();
            return;
        }

        // Extract button names from InputConfig
        string[] buttonNames = inputConfig.keyboardBindings
            .Where(binding => !string.IsNullOrEmpty(binding.buttonName))
            .Select(binding => binding.buttonName)
            .Distinct()
            .ToArray();

        if (buttonNames.Length == 0)
        {
            // No buttons found - show as regular text field
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.PropertyField(position, property, label);
            EditorGUI.EndProperty();
            return;
        }

        // Get current value
        string currentValue = property.stringValue;
        int currentIndex = System.Array.IndexOf(buttonNames, currentValue);
        
        // Build options array - include current value if it's not in the list
        string[] options;
        if (currentIndex < 0 && !string.IsNullOrEmpty(currentValue))
        {
            // Current value is not in the list - add it as first option with "(Custom)" label
            options = new string[buttonNames.Length + 1];
            options[0] = currentValue + " (Custom)";
            System.Array.Copy(buttonNames, 0, options, 1, buttonNames.Length);
            currentIndex = 0;
        }
        else
        {
            options = buttonNames;
        }

        // Draw dropdown
        EditorGUI.BeginProperty(position, label, property);
        
        // Convert options to GUIContent array for the Popup
        GUIContent[] optionContents = options.Select(opt => new GUIContent(opt)).ToArray();
        int newIndex = EditorGUI.Popup(position, label, currentIndex, optionContents);
        
        if (newIndex != currentIndex)
        {
            // Get the selected value (remove "(Custom)" suffix if present)
            string selectedValue = options[newIndex].Replace(" (Custom)", "");
            property.stringValue = selectedValue;
        }
        
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        ButtonNameAttribute buttonNameAttr = (ButtonNameAttribute)attribute;
        InputConfig inputConfig = GetInputConfig(property, buttonNameAttr.configFieldName);
        
        if (inputConfig == null)
        {
            // Extra height for warning message
            return EditorGUIUtility.singleLineHeight * 2 + 4;
        }
        
        return EditorGUIUtility.singleLineHeight;
    }

    private InputConfig GetInputConfig(SerializedProperty property, string configFieldName)
    {
        // Try to find the InputConfig field using SerializedProperty (works in editor)
        SerializedProperty configProperty = property.serializedObject.FindProperty(configFieldName);
        
        if (configProperty != null && configProperty.propertyType == SerializedPropertyType.ObjectReference)
        {
            return configProperty.objectReferenceValue as InputConfig;
        }
        
        // Fallback to reflection (for runtime or if SerializedProperty doesn't work)
        UnityEngine.Object targetObject = property.serializedObject.targetObject;
        
        if (targetObject == null)
            return null;

        System.Type targetType = targetObject.GetType();
        var field = targetType.GetField(configFieldName, 
            System.Reflection.BindingFlags.Public | 
            System.Reflection.BindingFlags.NonPublic | 
            System.Reflection.BindingFlags.Instance);
        
        if (field == null)
            return null;

        object configValue = field.GetValue(targetObject);
        return configValue as InputConfig;
    }
}

