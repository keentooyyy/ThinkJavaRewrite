using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using GameEvents;
using GameInteraction;

namespace NodeCanvas.Tasks.Actions
{
    [Category("✫ Custom/Interaction")]
    [Description("Show E or F prompt based on nearby interactable - triggers UI events")]
    public class ShowInteractionPrompt : ActionTask<Transform>
    {
        [BlackboardOnly]
        [Tooltip("Current nearby interactable GameObject (set by collision detection)")]
        public BBParameter<GameObject> nearbyInteractable;
        
        private Interactable currentInteractable;
        private string lastShownPrompt = "";
        
        protected override string info
        {
            get { return "Show Interaction Prompt"; }
        }
        
        protected override void OnUpdate()
        {
            GameObject obj = nearbyInteractable.value;
            
            if (obj != null)
            {
                // Get interactable component
                Interactable interactable = obj.GetComponent<Interactable>();
                
                if (interactable != null)
                {
                    // Check if this is a different interactable than before
                    if (interactable != currentInteractable)
                    {
                        currentInteractable = interactable;
                        Debug.Log($"[SHOW PROMPT] New interactable detected: {obj.name}, button: {interactable.requiredButton}");
                        
                        // Show appropriate prompt based on required button
                        ShowPromptForButton(interactable.requiredButton);
                    }
                }
            }
            else
            {
                // No interactable nearby - hide prompt
                if (currentInteractable != null)
                {
                    Debug.Log("[SHOW PROMPT] No interactable nearby, hiding prompt");
                    HidePrompt();
                    currentInteractable = null;
                }
            }
        }
        
        protected override void OnStop()
        {
            // Hide prompt when action stops
            HidePrompt();
        }
        
        private void ShowPromptForButton(string buttonName)
        {
            string eventToTrigger = "";
            
            // Map button name to UI event
            switch (buttonName)
            {
                case "ActionA":
                    eventToTrigger = "ShowPromptActionA";
                    break;
                case "ActionB":
                    eventToTrigger = "ShowPromptActionB";
                    break;
                default:
                    Debug.LogWarning($"[SHOW PROMPT] Unknown button '{buttonName}'");
                    return;
            }
            
            // Only trigger if different from last shown
            if (eventToTrigger != lastShownPrompt)
            {
                Debug.Log($"[SHOW PROMPT] Triggering UI event: {eventToTrigger}");
                UIEventManager.Trigger(eventToTrigger);
                lastShownPrompt = eventToTrigger;
            }
            else
            {
                Debug.Log($"[SHOW PROMPT] Event '{eventToTrigger}' already showing, skipping");
            }
        }
        
        private void HidePrompt()
        {
            if (!string.IsNullOrEmpty(lastShownPrompt))
            {
                Debug.Log("[SHOW PROMPT] Triggering HidePrompt event");
                UIEventManager.Trigger("HidePrompt");
                lastShownPrompt = "";
            }
        }
    }
}

