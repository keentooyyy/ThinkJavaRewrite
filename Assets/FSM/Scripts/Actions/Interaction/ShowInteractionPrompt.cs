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
        
        private IActionButtonProvider currentProvider;
        private Component currentProviderComponent;
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
                // Get provider component
                var provider = obj.GetComponent<IActionButtonProvider>();

                if (provider != null)
                {
                    var providerComponent = provider as Component;

                    // Check if this is a different provider than before
                    if (providerComponent != currentProviderComponent)
                    {
                        currentProvider = provider;
                        currentProviderComponent = providerComponent;
                        // Show appropriate prompt based on required button
                        ShowPromptForButton(provider.RequiredButton);
                    }
                }
            }
            else
            {
                // No interactable nearby - hide prompt
                if (currentProviderComponent != null)
                {
                    HidePrompt();
                    currentProvider = null;
                    currentProviderComponent = null;
                }
            }
        }
        
        protected override void OnStop()
        {
            // Hide prompt when action stops
            HidePrompt();
            currentProvider = null;
            currentProviderComponent = null;
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
                    return;
            }
            
            // Only trigger if different from last shown
            if (eventToTrigger != lastShownPrompt)
            {
                UIEventManager.Trigger(eventToTrigger);
                lastShownPrompt = eventToTrigger;
            }
        }
        
        private void HidePrompt()
        {
            if (!string.IsNullOrEmpty(lastShownPrompt))
            {
                UIEventManager.Trigger("HidePrompt");
                lastShownPrompt = "";
            }
        }
    }
}

