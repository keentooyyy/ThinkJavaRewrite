using UnityEngine;
using UnityEditor;
using GameProgress;
using System.Linq;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(AchievementIdAttribute))]
public class AchievementIdPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // This drawer works for both regular string fields and BBParameter<string> internal _value field
        if (property.propertyType != SerializedPropertyType.String)
        {
            EditorGUI.LabelField(position, label.text, "AchievementId attribute can only be used on string fields");
            return;
        }

        // Get achievements from AchievementManager
        Dictionary<string, AchievementSaveData> achievements = AchievementManager.GetAllAchievements();

        if (achievements == null || achievements.Count == 0)
        {
            // No achievements found - show as regular text field with info
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.PropertyField(position, property, label);
            
            Rect infoRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.HelpBox(infoRect, "No achievements found in save data. Login first or achievements will appear after login.", MessageType.Info);
            EditorGUI.EndProperty();
            return;
        }

        // Extract achievement IDs and create display names (ID + Title)
        string[] achievementIds = achievements.Keys.OrderBy(k => k).ToArray();
        string[] displayNames = achievementIds.Select(id =>
        {
            var achievement = achievements[id];
            string title = !string.IsNullOrEmpty(achievement.title) ? achievement.title : id;
            return $"{id} - {title}";
        }).ToArray();

        // Get current value
        string currentValue = property.stringValue;
        int currentIndex = System.Array.IndexOf(achievementIds, currentValue);
        
        // Build options array - include current value if it's not in the list
        string[] options;
        string[] optionIds;
        if (currentIndex < 0 && !string.IsNullOrEmpty(currentValue))
        {
            // Current value is not in the list - add it as first option with "(Custom)" label
            options = new string[displayNames.Length + 1];
            optionIds = new string[achievementIds.Length + 1];
            options[0] = currentValue + " (Custom)";
            optionIds[0] = currentValue;
            System.Array.Copy(displayNames, 0, options, 1, displayNames.Length);
            System.Array.Copy(achievementIds, 0, optionIds, 1, achievementIds.Length);
            currentIndex = 0;
        }
        else
        {
            options = displayNames;
            optionIds = achievementIds;
        }

        // Draw dropdown
        EditorGUI.BeginProperty(position, label, property);
        
        // Convert options to GUIContent array for the Popup
        GUIContent[] optionContents = options.Select(opt => new GUIContent(opt)).ToArray();
        int newIndex = EditorGUI.Popup(position, label, currentIndex >= 0 ? currentIndex : 0, optionContents);
        
        if (newIndex != currentIndex || currentIndex < 0)
        {
            // Get the selected achievement ID
            string selectedId = optionIds[newIndex];
            property.stringValue = selectedId;
        }
        
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        Dictionary<string, AchievementSaveData> achievements = AchievementManager.GetAllAchievements();
        
        if (achievements == null || achievements.Count == 0)
        {
            // Extra height for info message
            return EditorGUIUtility.singleLineHeight * 2 + 4;
        }
        
        return EditorGUIUtility.singleLineHeight;
    }
}

