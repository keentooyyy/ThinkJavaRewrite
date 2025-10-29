using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TouchControls
{
    /// <summary>
    /// Virtual button with drag-to-slide support.
    /// Put this on your directional/jump buttons alongside Unity's Button component.
    /// The Button component handles visuals, this handles touch input events.
    /// Supports dragging from one button to another!
    /// </summary>
    public class VirtualButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public enum ButtonType { Up, Down, Left, Right, Jump }

        [Header("Button Settings")]
        public ButtonType buttonType = ButtonType.Jump;

        private bool isPressed = false;
        private static VirtualButton currentlyPressedButton = null; // Track which button is currently held
        private Button unityButton; // Unity Button component for visual feedback

        private void Awake()
        {
            unityButton = GetComponent<Button>();
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
            // Drag-to-slide: If finger slides INTO this button while another is pressed
            if (currentlyPressedButton != null && currentlyPressedButton != this)
            {
                // Update Unity Button visual states for drag
                VirtualButton oldButton = currentlyPressedButton;
                if (oldButton.unityButton != null)
                {
                    oldButton.unityButton.OnPointerUp(eventData);
                }
                if (unityButton != null)
                {
                    unityButton.OnPointerDown(eventData);
                }
                
                // Release the old button input
                oldButton.Release();
                
                // Press this button input
                Press();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // If dragging outside all buttons, release this one
            if (isPressed)
            {
                // Check if not entering another VirtualButton
                GameObject pointerEnter = eventData.pointerEnter;
                if (pointerEnter == null || pointerEnter.GetComponent<VirtualButton>() == null)
                {
                    // Update Unity Button visual state
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

            // Send event to TouchInputManager
            switch (buttonType)
            {
                case ButtonType.Up:
                    TouchInputManager.PressDirection(TouchInputManager.Direction.Up);
                    break;
                case ButtonType.Down:
                    TouchInputManager.PressDirection(TouchInputManager.Direction.Down);
                    break;
                case ButtonType.Left:
                    TouchInputManager.PressDirection(TouchInputManager.Direction.Left);
                    break;
                case ButtonType.Right:
                    TouchInputManager.PressDirection(TouchInputManager.Direction.Right);
                    break;
                case ButtonType.Jump:
                    TouchInputManager.PressJump();
                    break;
            }
        }

        private void Release()
        {
            if (!isPressed) return;

            isPressed = false;
            if (currentlyPressedButton == this)
                currentlyPressedButton = null;

            // Send release event
            switch (buttonType)
            {
                case ButtonType.Up:
                    TouchInputManager.ReleaseDirection(TouchInputManager.Direction.Up);
                    break;
                case ButtonType.Down:
                    TouchInputManager.ReleaseDirection(TouchInputManager.Direction.Down);
                    break;
                case ButtonType.Left:
                    TouchInputManager.ReleaseDirection(TouchInputManager.Direction.Left);
                    break;
                case ButtonType.Right:
                    TouchInputManager.ReleaseDirection(TouchInputManager.Direction.Right);
                    break;
                case ButtonType.Jump:
                    TouchInputManager.ReleaseJump();
                    break;
            }
        }

        private void OnDisable()
        {
            // Auto-release if button is disabled while pressed
            if (isPressed)
                Release();
        }
    }
}

