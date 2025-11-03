using System;
using System.Collections.Generic;
using UnityEngine;
using GameState;

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
            public bool isVirtualButton = false; // Track if this is from virtual button
        }
        
        /// <summary>
        /// Update keyboard input using the provided config
        /// </summary>
        public static void UpdateKeyboardInput(InputConfig config)
        {
            if (config == null)
            {
                return;
            }

            foreach (var kvp in buttons)
            {
                if (!kvp.Value.isVirtualButton)
                {
                    kvp.Value.pressedThisFrame = false;
                }
            }

            bool movementAllowed = GameFreezeManager.AllowsMovementInput;
            float horizontal = movementAllowed ? Input.GetAxisRaw(config.horizontalAxisName) : 0f;
            float vertical = movementAllowed ? Input.GetAxisRaw(config.verticalAxisName) : 0f;

            if (!virtualDirectionActive)
            {
                directionalInput = movementAllowed ? new Vector2(horizontal, vertical) : Vector2.zero;
            }

            virtualDirectionActive = false;

            foreach (var binding in config.keyboardBindings)
            {
                CheckKeyboardButton(binding.keyCode, binding.buttonName);

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
        public static void PressButton(string buttonName, bool isVirtual = false)
        {
            if (string.IsNullOrEmpty(buttonName))
            {
                return;
            }

            if (!buttons.TryGetValue(buttonName, out var state))
            {
                state = new ButtonState();
                buttons[buttonName] = state;
            }

            if (!GameFreezeManager.IsButtonAllowed(buttonName))
            {
                state.isPressed = false;
                state.pressedThisFrame = false;
                state.isVirtualButton = isVirtual;
                return;
            }

            if (!state.isPressed)
            {
                state.isPressed = true;
                state.pressedThisFrame = true;
                state.isVirtualButton = isVirtual;
                OnButtonPressed?.Invoke(buttonName);
            }
        }
        
        /// <summary>
        /// Release a button
        /// </summary>
        public static void ReleaseButton(string buttonName)
        {
            if (string.IsNullOrEmpty(buttonName))
            {
                return;
            }

            if (buttons.TryGetValue(buttonName, out var state) && state.isPressed)
            {
                state.isPressed = false;
                OnButtonReleased?.Invoke(buttonName);
            }
        }
        
        /// <summary>
        /// Is button currently held down?
        /// </summary>
        public static bool GetButton(string buttonName)
        {
            if (!GameFreezeManager.IsButtonAllowed(buttonName))
            {
                return false;
            }

            return buttons.ContainsKey(buttonName) && buttons[buttonName].isPressed;
        }
        
        /// <summary>
        /// Was button pressed this frame? (consumes the press)
        /// </summary>
        public static bool GetButtonDown(string buttonName)
        {
            if (!GameFreezeManager.IsButtonAllowed(buttonName))
            {
                return false;
            }

            if (buttons.ContainsKey(buttonName) && buttons[buttonName].pressedThisFrame)
            {
                buttons[buttonName].pressedThisFrame = false;
                buttons[buttonName].isVirtualButton = false;
                return true;
            }

            return false;
        }
        
        /// <summary>
        /// Set directional input (from virtual buttons)
        /// </summary>
        public static void SetDirectionalInput(float horizontal, float vertical)
        {
            if (!GameFreezeManager.AllowsMovementInput)
            {
                directionalInput = Vector2.zero;
                virtualDirectionActive = false;
                return;
            }

            directionalInput = new Vector2(horizontal, vertical);
            virtualDirectionActive = Mathf.Abs(horizontal) > 0.01f || Mathf.Abs(vertical) > 0.01f;
        }
        
        public static float GetHorizontalAxis()
        {
            return GameFreezeManager.AllowsMovementInput ? directionalInput.x : 0f;
        }
        
        public static float GetVerticalAxis()
        {
            return GameFreezeManager.AllowsMovementInput ? directionalInput.y : 0f;
        }
        
        public static void Clear()
        {
            buttons.Clear();
            directionalInput = Vector2.zero;
        }
    }
}

