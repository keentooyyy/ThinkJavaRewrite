using NodeCanvas.Framework;
using ParadoxNotion;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace NodeCanvas.StateMachines
{
    /// <summary>
    /// Validates that required FSM blackboard variables are assigned.
    /// Prevents runtime errors from missing scene references.
    /// </summary>
    [RequireComponent(typeof(FSMOwner))]
    public class FSMBlackboardValidator : MonoBehaviour
    {
        [Title("Required Blackboard Variables")]
        [InfoBox("List all blackboard variable names that MUST be assigned before running.", InfoMessageType.Info)]
        [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "variableName")]
        public List<RequiredVariable> requiredVariables = new List<RequiredVariable>();

        [Space(10)]
        [ReadOnly, ShowInInspector]
        private FSMOwner fsmOwner;

        [System.Serializable]
        public class RequiredVariable
        {
            [HorizontalGroup("Variable")]
            [LabelWidth(100)]
            public string variableName;
            
            [HorizontalGroup("Variable")]
            [ReadOnly, ShowInInspector]
            public bool IsAssigned
            {
                get
                {
                    var owner = FindObjectOfType<FSMOwner>();
                    if (owner == null || owner.blackboard == null) return false;
                    
                    var variable = owner.blackboard.GetVariable(variableName);
                    if (variable == null) return false;
                    
                    var value = variable.value;
                    if (value == null) return false;
                    
                    // Check if it's a Unity Object and if it's destroyed
                    if (value is UnityEngine.Object unityObj)
                    {
                        return unityObj != null;
                    }
                    
                    return true;
                }
            }
        }

        private void Awake()
        {
            fsmOwner = GetComponent<FSMOwner>();
            ValidateBlackboard();
        }

        private void OnValidate()
        {
            fsmOwner = GetComponent<FSMOwner>();
        }

        [Button("Validate Now", ButtonSizes.Large)]
        [GUIColor(0.3f, 0.8f, 0.3f)]
        private void ValidateBlackboard()
        {
            if (fsmOwner == null || fsmOwner.blackboard == null)
            {
                Debug.LogError($"[FSM Validator] FSMOwner or Blackboard is null on {gameObject.name}!", this);
                return;
            }

            bool allValid = true;
            List<string> missingVars = new List<string>();

            foreach (var required in requiredVariables)
            {
                var variable = fsmOwner.blackboard.GetVariable(required.variableName);
                
                if (variable == null)
                {
                    missingVars.Add($"'{required.variableName}' - Variable doesn't exist in blackboard");
                    allValid = false;
                    continue;
                }

                var value = variable.value;
                if (value == null)
                {
                    missingVars.Add($"'{required.variableName}' - Value is NULL");
                    allValid = false;
                    continue;
                }

                // Check Unity Objects
                if (value is UnityEngine.Object unityObj && unityObj == null)
                {
                    missingVars.Add($"'{required.variableName}' - Reference is missing/destroyed");
                    allValid = false;
                }
            }

            if (!allValid)
            {
                string errorMsg = $"[FSM Validator] Missing required blackboard variables on {gameObject.name}:\n";
                foreach (var missing in missingVars)
                {
                    errorMsg += $"  - {missing}\n";
                }
                Debug.LogError(errorMsg, this);
                
#if UNITY_EDITOR
                if (UnityEngine.Application.isPlaying)
                {
                    Debug.Break(); // Pause editor
                }
#endif
            }
            else
            {
                Debug.Log($"[FSM Validator] ✓ All required blackboard variables are assigned on {gameObject.name}", this);
            }
        }

        [Button("Auto-Populate from Blackboard", ButtonSizes.Medium)]
        [GUIColor(0.4f, 0.7f, 1f)]
        private void AutoPopulateRequiredVariables()
        {
            if (fsmOwner == null || fsmOwner.blackboard == null)
            {
                Debug.LogWarning("No FSMOwner or Blackboard found!");
                return;
            }

            requiredVariables.Clear();

            var variables = fsmOwner.blackboard.variables;
            foreach (var varName in variables.Keys)
            {
                var variable = variables[varName];
                
                // Only add GameObject and Component references (scene-specific)
                var value = variable.value;
                if (value is GameObject || value is Component)
                {
                    requiredVariables.Add(new RequiredVariable { variableName = varName });
                }
            }

            Debug.Log($"Auto-populated {requiredVariables.Count} scene-reference variables");
        }

#if UNITY_EDITOR
        [PropertySpace(20)]
        [ShowInInspector, ReadOnly]
        [Title("Validation Status")]
        [ShowIf("@UnityEngine.Application.isPlaying")]
        private string ValidationStatus
        {
            get
            {
                if (fsmOwner == null || fsmOwner.blackboard == null)
                    return "⚠️ No FSMOwner/Blackboard";

                int missing = 0;
                foreach (var req in requiredVariables)
                {
                    if (!req.IsAssigned) missing++;
                }

                if (missing == 0)
                    return "✓ All variables assigned";
                else
                    return $"❌ {missing} variable(s) missing!";
            }
        }
#endif
    }
}

