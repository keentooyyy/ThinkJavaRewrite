using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameInput
{
    /// <summary>
    /// Scalable input manager - all bindings configured via InputConfig asset
    /// </summary>
    public static class InputManager
    {
        private static Dictionary<string, ButtonState> buttons = new Dictionary<string, ButtonState>();
        private static Vector2 directionalInput = Vector2.zero;
        private static bool virtualDirectionActive = false;
        
        public static event Action<string> OnButtonPressed;
        public static event Action<string> OnButtonReleased;
        
        private class ButtonState
        {
            public bool isPressed = false;
            public bool pressedThisFrame = false;
        }
        
        /// <summary>
        /// Update keyboard input using the provided config
        /// </summary>
        public static void UpdateKeyboardInput(InputConfig config)
        {
            if (config == null) return;
            
            // Clear frame-based flags
            foreach (var state in buttons.Values)
            {
                state.pressedThisFrame = false;
            }
            
            // Get keyboard input
            float horizontal = Input.GetAxisRaw(config.horizontalAxisName);
            float vertical = Input.GetAxisRaw(config.verticalAxisName);
            
            // If no virtual buttons active, use keyboard input
            // Virtual buttons have priority and are updated via SetDirectionalInput()
            if (!virtualDirectionActive)
            {
                directionalInput = new Vector2(horizontal, vertical);
            }
            
            // Reset flag after checking - VirtualButton.Update() will set it again if still pressed
            virtualDirectionActive = false;
            
            // Check all configured keyboard bindings
            foreach (var binding in config.keyboardBindings)
            {
                CheckKeyboardButton(binding.keyCode, binding.buttonName);
                
                // Check alternative key if set
                if (binding.alternativeKey != KeyCode.None)
                {
                    CheckKeyboardButton(binding.alternativeKey, binding.buttonName);
                }
            }
        }
        
        private static void CheckKeyboardButton(KeyCode key, string buttonName)
        {
            if (Input.GetKeyDown(key))
            {
                PressButton(buttonName);
            }
            else if (Input.GetKeyUp(key))
            {
                ReleaseButton(buttonName);
            }
        }
        
        /// <summary>
        /// Press a button (called by VirtualButton or keyboard)
        /// </summary>
        public static void PressButton(string buttonName)
        {
            if (string.IsNullOrEmpty(buttonName)) return;
            
            if (!buttons.ContainsKey(buttonName))
            {
                buttons[buttonName] = new ButtonState();
            }
            
            if (!buttons[buttonName].isPressed)
            {
                buttons[buttonName].isPressed = true;
                buttons[buttonName].pressedThisFrame = true;
                OnButtonPressed?.Invoke(buttonName);
            }
        }
        
        /// <summary>
        /// Release a button
        /// </summary>
        public static void ReleaseButton(string buttonName)
        {
            if (string.IsNullOrEmpty(buttonName)) return;
            
            if (buttons.ContainsKey(buttonName) && buttons[buttonName].isPressed)
            {
                buttons[buttonName].isPressed = false;
                OnButtonReleased?.Invoke(buttonName);
            }
        }
        
        /// <summary>
        /// Is button currently held down?
        /// </summary>
        public static bool GetButton(string buttonName)
        {
            return buttons.ContainsKey(buttonName) && buttons[buttonName].isPressed;
        }
        
        /// <summary>
        /// Was button pressed this frame? (consumes the press)
        /// </summary>
        public static bool GetButtonDown(string buttonName)
        {
            if (buttons.ContainsKey(buttonName) && buttons[buttonName].pressedThisFrame)
            {
                buttons[buttonName].pressedThisFrame = false;
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Set directional input (from virtual buttons)
        /// </summary>
        public static void SetDirectionalInput(float horizontal, float vertical)
        {
            directionalInput = new Vector2(horizontal, vertical);
            virtualDirectionActive = (Mathf.Abs(horizontal) > 0.01f || Mathf.Abs(vertical) > 0.01f);
        }
        
        public static float GetHorizontalAxis()
        {
            return directionalInput.x;
        }
        
        public static float GetVerticalAxis()
        {
            return directionalInput.y;
        }
        
        public static void Clear()
        {
            buttons.Clear();
            directionalInput = Vector2.zero;
        }
    }
}

