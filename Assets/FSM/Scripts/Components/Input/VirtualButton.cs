using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using GameInput;

namespace GameInput
{
    /// <summary>
    /// Generic virtual button - supports ANY button name
    /// Just type the button name in the inspector!
    /// </summary>
    public class VirtualButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Button Settings")]
        [Tooltip("Button identifier (e.g., 'Jump', 'ActionA', 'ActionB', 'Dash', 'Attack', etc.)")]
        public string buttonName = "Jump";
        
        [Header("Directional Button (Optional)")]
        [Tooltip("If this is a directional button, check this and set the direction")]
        public bool isDirectional = false;
        public Vector2 direction = Vector2.zero; // e.g., (-1, 0) for Left, (1, 0) for Right
        
        private bool isPressed = false;
        private static VirtualButton currentlyPressedButton = null;
        private Button unityButton;
        
        private void Awake()
        {
            unityButton = GetComponent<Button>();
        }
        
        private void Update()
        {
            // Keep sending directional input while button is held
            if (isPressed && isDirectional)
            {
                InputManager.SetDirectionalInput(direction.x, direction.y);
            }
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            Press();
        }
        
        public void OnPointerUp(PointerEventData eventData)
        {
            Release();
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            // Drag-to-slide support
            if (currentlyPressedButton != null && currentlyPressedButton != this)
            {
                VirtualButton oldButton = currentlyPressedButton;
                if (oldButton.unityButton != null)
                {
                    oldButton.unityButton.OnPointerUp(eventData);
                }
                if (unityButton != null)
                {
                    unityButton.OnPointerDown(eventData);
                }
                
                oldButton.Release();
                Press();
            }
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            if (isPressed)
            {
                GameObject pointerEnter = eventData.pointerEnter;
                if (pointerEnter == null || pointerEnter.GetComponent<VirtualButton>() == null)
                {
                    if (unityButton != null)
                    {
                        unityButton.OnPointerUp(eventData);
                    }
                    Release();
                }
            }
        }
        
        private void Press()
        {
            if (isPressed) return;
            
            isPressed = true;
            currentlyPressedButton = this;
            
            // Send to InputManager
            if (!string.IsNullOrEmpty(buttonName))
            {
                InputManager.PressButton(buttonName);
            }
            
            // Handle directional input
            if (isDirectional)
            {
                InputManager.SetDirectionalInput(direction.x, direction.y);
            }
        }
        
        private void Release()
        {
            if (!isPressed) return;
            
            isPressed = false;
            if (currentlyPressedButton == this)
                currentlyPressedButton = null;
            
            // Send release to InputManager
            if (!string.IsNullOrEmpty(buttonName))
            {
                InputManager.ReleaseButton(buttonName);
            }
            
            // Clear directional input immediately to allow keyboard to take over
            if (isDirectional)
            {
                InputManager.SetDirectionalInput(0, 0);
            }
        }
        
        private void OnDisable()
        {
            if (isPressed)
                Release();
        }
    }
}
