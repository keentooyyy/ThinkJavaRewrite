using UnityEngine;
using TMPro;
using GameProgress;
using GameEvents;

namespace GameUI
{
    /// <summary>
    /// Displays student profile information in the Profile UI menu
    /// Updates automatically when ProfileButton event is triggered
    /// </summary>
    [DisallowMultipleComponent]
    public class ProfileUIDisplay : MonoBehaviour
    {
        [Header("UI Text Fields")]
        [Tooltip("Text field to display student ID")]
        [SerializeField] private TMP_Text studentIdText;
        
        [Tooltip("Text field to display student name (first + last)")]
        [SerializeField] private TMP_Text studentNameText;
        
        [Tooltip("Text field to display student role")]
        [SerializeField] private TMP_Text studentRoleText;

        [Header("Events")]
        [Tooltip("Event name that triggers profile UI update (default: ProfileButton)")]
        [SerializeField] private string updateEventName = "ProfileButton";

        private void Reset()
        {
            TryResolveTextFields();
        }

        private void Awake()
        {
            TryResolveTextFields();
        }

        private void OnEnable()
        {
            if (!string.IsNullOrEmpty(updateEventName))
            {
                UIEventManager.Subscribe(updateEventName, UpdateProfileDisplay);
            }
            // Update immediately when enabled
            UpdateProfileDisplay();
        }

        private void OnDisable()
        {
            if (!string.IsNullOrEmpty(updateEventName))
            {
                UIEventManager.Unsubscribe(updateEventName, UpdateProfileDisplay);
            }
        }

        /// <summary>
        /// Update the profile display with student data from ES3
        /// </summary>
        public void UpdateProfileDisplay()
        {
            var studentData = LoginManager.GetStudentData();
            
            if (studentData == null)
            {
                // No student data available - show placeholder or empty
                if (studentIdText != null)
                    studentIdText.text = "Not logged in";
                if (studentNameText != null)
                    studentNameText.text = "";
                if (studentRoleText != null)
                    studentRoleText.text = "";
                return;
            }

            // Update student ID
            if (studentIdText != null)
            {
                studentIdText.text = !string.IsNullOrEmpty(studentData.student_id) 
                    ? studentData.student_id 
                    : "N/A";
            }

            // Update student name (first + last)
            if (studentNameText != null)
            {
                string firstName = !string.IsNullOrEmpty(studentData.first_name) ? studentData.first_name : "";
                string lastName = !string.IsNullOrEmpty(studentData.last_name) ? studentData.last_name : "";
                string fullName = $"{firstName} {lastName}".Trim();
                studentNameText.text = !string.IsNullOrEmpty(fullName) ? fullName : "N/A";
            }

            // Update student role
            if (studentRoleText != null)
            {
                studentRoleText.text = !string.IsNullOrEmpty(studentData.role) 
                    ? studentData.role 
                    : "N/A";
            }
        }

        private void TryResolveTextFields()
        {
            // Try to find TMP_Text components if not assigned
            if (studentIdText == null)
                studentIdText = GetComponentInChildren<TMP_Text>(true);
            if (studentNameText == null && studentIdText != null)
            {
                // Try to find other TMP_Text components
                var texts = GetComponentsInChildren<TMP_Text>(true);
                if (texts.Length > 1)
                    studentNameText = texts[1];
            }
            if (studentRoleText == null && studentNameText != null)
            {
                var texts = GetComponentsInChildren<TMP_Text>(true);
                if (texts.Length > 2)
                    studentRoleText = texts[2];
            }
        }
    }
}

