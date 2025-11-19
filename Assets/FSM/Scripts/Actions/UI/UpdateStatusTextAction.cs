using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using TMPro;
using GameEvents;

namespace NodeCanvas.Tasks.Actions
{
    [Category("■ Custom/UI")]
    [Description("Update Status UI text based on HTTP response code or direct message. Maps status codes to user-friendly messages and optionally shows/hides the Status UI.")]
    public class UpdateStatusTextAction : ActionTask
    {
        public enum MessageMode
        {
            FromResponseCode,
            DirectMessage
        }

        [UnityEngine.Header("Status Text")]
        [Tooltip("The TMP_Text component to update (Status UI text child)")]
        public BBParameter<TMP_Text> statusText;

        [UnityEngine.Header("Message Source")]
        [Tooltip("How to get the message: From Response Code (uses HTTP code) or Direct Message (uses custom text)")]
        public MessageMode messageMode = MessageMode.FromResponseCode;

        [UnityEngine.Header("Status Code Source (when using FromResponseCode)")]
        [Tooltip("HTTP response code from blackboard variable (e.g., 'httpResponseCode')")]
        [BlackboardOnly]
        public BBParameter<long> responseCode;

        [UnityEngine.Header("Direct Message (when using DirectMessage)")]
        [Tooltip("Direct message text to display (used when Message Mode is DirectMessage)")]
        public BBParameter<string> directMessage = "Processing...";

        [UnityEngine.Header("UI Events (Optional)")]
        [Tooltip("Event to trigger to show Status UI (e.g., 'ShowStatusUI'). Leave empty to not trigger.")]
        public BBParameter<string> showStatusUIEvent;

        [Tooltip("Event to trigger to hide Status UI (e.g., 'HideStatusUI'). Leave empty to not trigger.")]
        public BBParameter<string> hideStatusUIEvent;

        [UnityEngine.Header("Custom Messages (Optional)")]
        [Tooltip("Custom message for 401 (Unauthorized). Leave empty for default.")]
        public BBParameter<string> message401 = "Invalid username or password";

        [Tooltip("Custom message for 404 (Not Found). Leave empty for default.")]
        public BBParameter<string> message404 = "Server connection error";

        [Tooltip("Custom message for 500 (Server Error). Leave empty for default.")]
        public BBParameter<string> message500 = "Server error. Please try again later";

        [Tooltip("Custom message for timeout/network errors. Leave empty for default.")]
        public BBParameter<string> messageTimeout = "Connection timeout. Please check your internet connection";

        [Tooltip("Custom message for logout. Leave empty for default.")]
        public BBParameter<string> messageLogout = "Logged out successfully";

        [Tooltip("Custom message for login success. Leave empty for default.")]
        public BBParameter<string> messageSuccess = "Login successful";

        [Tooltip("Custom message for unknown errors. Leave empty for default.")]
        public BBParameter<string> messageUnknown = "An error occurred. Please try again";

        [Tooltip("Custom message for logging in (in progress). Leave empty for default.")]
        public BBParameter<string> messageLoggingIn = "Logging in, please wait...";

        [Tooltip("Custom message for logging out (in progress). Leave empty for default.")]
        public BBParameter<string> messageLoggingOut = "Logging out, please wait...";

        protected override string info
        {
            get
            {
                if (statusText.value != null)
                {
                    if (messageMode == MessageMode.DirectMessage)
                        return $"Update Status Text [{directMessage.value}]";
                    return $"Update Status Text [{GetStatusMessage(responseCode.value)}]";
                }
                return "Update Status Text";
            }
        }

        protected override void OnExecute()
        {
            if (statusText.value == null)
            {
                Debug.LogWarning("UpdateStatusTextAction: Status text component is null!");
                EndAction(false);
                return;
            }

            string message;
            
            if (messageMode == MessageMode.DirectMessage)
            {
                // Use direct message
                message = !string.IsNullOrEmpty(directMessage.value) ? directMessage.value : "Processing...";
            }
            else
            {
                // Use response code to get message
                long code = 0;
                bool readFromBlackboard = false;
                bool variableExists = false;
                
                // Always try to read directly from blackboard variable first (most reliable)
                // This ensures we get the actual value even if BBParameter isn't properly bound
                if (blackboard != null)
                {
                    variableExists = blackboard.variables.ContainsKey("httpResponseCode");
                    if (variableExists)
                    {
                        try
                        {
                            code = blackboard.GetVariableValue<long>("httpResponseCode");
                            readFromBlackboard = true;
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogWarning($"UpdateStatusTextAction: Failed to read httpResponseCode from blackboard: {e.Message}");
                        }
                    }
                }
                
                // Fallback to BBParameter only if we couldn't read from blackboard
                if (!readFromBlackboard && responseCode != null)
                {
                    code = responseCode.value;
                }
                
                // Special handling: If code is 0 and variable doesn't exist, we're probably in a "processing" state
                // Don't show "Login successful" - instead show a processing message or check for -3/-4 codes
                if (code == 0 && !variableExists)
                {
                    // Variable doesn't exist yet - we're probably at the start of login
                    message = !string.IsNullOrEmpty(messageLoggingIn.value) ? messageLoggingIn.value : "Logging in, please wait...";
                }
                else
                {
                    message = GetStatusMessage(code);
                }
            }

            // Update the text
            statusText.value.text = message;

            // Trigger show/hide events if specified
            if (!string.IsNullOrEmpty(showStatusUIEvent.value))
            {
                UIEventManager.Trigger(showStatusUIEvent.value);
            }

            // Note: We don't auto-hide on success - let the FSM handle that if needed
            // If you want to auto-hide on success, you can add logic here

            EndAction(true);
        }

        private string GetStatusMessage(long responseCode)
        {
            // Map HTTP status codes to user-friendly messages
            switch (responseCode)
            {
                case 0:
                    // Success or no response code
                    return !string.IsNullOrEmpty(messageSuccess.value) ? messageSuccess.value : "Login successful";
                
                case -2:
                    // Special code for logout
                    return !string.IsNullOrEmpty(messageLogout.value) ? messageLogout.value : "Logged out successfully";
                
                case -3:
                    // Special code for logging in (in progress)
                    return !string.IsNullOrEmpty(messageLoggingIn.value) ? messageLoggingIn.value : "Logging in, please wait...";
                
                case -4:
                    // Special code for logging out (in progress)
                    return !string.IsNullOrEmpty(messageLoggingOut.value) ? messageLoggingOut.value : "Logging out, please wait...";
                
                case 401:
                    return !string.IsNullOrEmpty(message401.value) ? message401.value : "Invalid username or password";
                
                case 403:
                    return "Access forbidden. Please contact support";
                
                case 404:
                    return !string.IsNullOrEmpty(message404.value) ? message404.value : "Server connection error";
                
                case 408:
                    return !string.IsNullOrEmpty(messageTimeout.value) ? messageTimeout.value : "Connection timeout. Please check your internet connection";
                
                case 500:
                case 502:
                case 503:
                case 504:
                    return !string.IsNullOrEmpty(message500.value) ? message500.value : "Server error. Please try again later";
                
                case -1:
                    // UnityWebRequest timeout or network error
                    return !string.IsNullOrEmpty(messageTimeout.value) ? messageTimeout.value : "Connection timeout. Please check your internet connection";
                
                default:
                    if (responseCode > 0)
                    {
                        return $"Error {responseCode}. Please try again";
                    }
                    return !string.IsNullOrEmpty(messageUnknown.value) ? messageUnknown.value : "An error occurred. Please try again";
            }
        }

        /// <summary>
        /// Static helper method to get status message for logout
        /// </summary>
        public static string GetLogoutMessage()
        {
            return "Logged out successfully";
        }

        /// <summary>
        /// Static helper method to get status message for a specific response code
        /// </summary>
        public static string GetMessageForCode(long responseCode)
        {
            switch (responseCode)
            {
                case 0:
                    return "Login successful";
                case -2:
                    return "Logged out successfully";
                case -3:
                    return "Logging in, please wait...";
                case -4:
                    return "Logging out, please wait...";
                case 401:
                    return "Invalid username or password";
                case 404:
                    return "Server connection error";
                case 408:
                case -1:
                    return "Connection timeout. Please check your internet connection";
                case 500:
                case 502:
                case 503:
                case 504:
                    return "Server error. Please try again later";
                default:
                    if (responseCode > 0)
                    {
                        return $"Error {responseCode}. Please try again";
                    }
                    return "An error occurred. Please try again";
            }
        }
    }
}

