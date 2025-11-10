using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NodeCanvas.Framework;
using GameEvents;

namespace GameProgress
{
    /// <summary>
    /// Listens for login button event and sets FSM blackboard values from input fields.
    /// Supports both TMP_InputField and legacy InputField.
    /// Can accept GameObject references and will auto-find InputField components.
    /// Event-driven - no direct button references needed.
    /// </summary>
    public class LoginInputHandler : MonoBehaviour
    {
        [Header("Input Fields")]
        [Tooltip("Student ID input field GameObject (will auto-find InputField component)")]
        public GameObject studentIdInputObject;
        
        [Tooltip("Password input field GameObject (will auto-find InputField component)")]
        public GameObject passwordInputObject;
        
        [Header("FSM Reference")]
        [Tooltip("The GraphOwner running the LoginFSM")]
        public GraphOwner loginFSM;
        
        [Header("Event Settings")]
        [Tooltip("Event name to listen for (should match button event)")]
        public string loginButtonEventName = "LoginButtonClicked";
        
        // Cached input field components
        private TMP_InputField studentIdTMPInput;
        private InputField studentIdLegacyInput;
        private TMP_InputField passwordTMPInput;
        private InputField passwordLegacyInput;
        
        private void Start()
        {
            // Subscribe to login button event
            UIEventManager.Subscribe(loginButtonEventName, OnLoginButtonClicked);
            
            // Cache input field components
            CacheInputFields();
        }
        
        /// <summary>
        /// Cache input field components from GameObjects
        /// </summary>
        private void CacheInputFields()
        {
            // Student ID input
            if (studentIdInputObject != null)
            {
                studentIdTMPInput = studentIdInputObject.GetComponent<TMP_InputField>();
                if (studentIdTMPInput == null)
                {
                    studentIdLegacyInput = studentIdInputObject.GetComponent<InputField>();
                }
                
                if (studentIdTMPInput == null && studentIdLegacyInput == null)
                {
                    Debug.LogWarning($"LoginInputHandler: No InputField found on studentIdInputObject: {studentIdInputObject.name}");
                }
            }
            
            // Password input
            if (passwordInputObject != null)
            {
                passwordTMPInput = passwordInputObject.GetComponent<TMP_InputField>();
                if (passwordTMPInput == null)
                {
                    passwordLegacyInput = passwordInputObject.GetComponent<InputField>();
                }
                
                if (passwordTMPInput == null && passwordLegacyInput == null)
                {
                    Debug.LogWarning($"LoginInputHandler: No InputField found on passwordInputObject: {passwordInputObject.name}");
                }
            }
        }
        
        private void OnLoginButtonClicked()
        {
            // Validate FSM reference
            if (loginFSM == null || loginFSM.blackboard == null)
            {
                Debug.LogError("LoginInputHandler: LoginFSM or blackboard is null!");
                return;
            }
            
            // Get values from input fields
            string studentId = GetInputFieldValue(true);
            string password = GetInputFieldValue(false);
            
            // Validate inputs
            if (string.IsNullOrEmpty(studentId))
            {
                Debug.LogWarning("LoginInputHandler: Student ID is empty!");
                UIEventManager.Trigger("LoginValidationFailed");
                return;
            }
            
            if (string.IsNullOrEmpty(password))
            {
                Debug.LogWarning("LoginInputHandler: Password is empty!");
                UIEventManager.Trigger("LoginValidationFailed");
                return;
            }
            
            // Set values to FSM blackboard
            loginFSM.blackboard.SetVariableValue("studentId", studentId);
            loginFSM.blackboard.SetVariableValue("password", password);
            
            Debug.Log($"LoginInputHandler: Set studentId='{studentId}' to FSM blackboard");
        }
        
        /// <summary>
        /// Get text value from cached input field
        /// </summary>
        private string GetInputFieldValue(bool isStudentId)
        {
            if (isStudentId)
            {
                if (studentIdTMPInput != null)
                    return studentIdTMPInput.text;
                if (studentIdLegacyInput != null)
                    return studentIdLegacyInput.text;
            }
            else
            {
                if (passwordTMPInput != null)
                    return passwordTMPInput.text;
                if (passwordLegacyInput != null)
                    return passwordLegacyInput.text;
            }
            
            return "";
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from event
            UIEventManager.Unsubscribe(loginButtonEventName, OnLoginButtonClicked);
        }
        
        // Editor validation
        private void OnValidate()
        {
            // Re-cache if in editor and GameObjects changed
            if (Application.isPlaying)
            {
                CacheInputFields();
            }
        }
    }
}

