using System;
using UnityEngine;

namespace GameInput
{
    /// <summary>
    /// Defines a single input binding (keyboard key + button name)
    /// </summary>
    [Serializable]
    public class InputBinding
    {
        [Tooltip("Name of the button (e.g., 'Jump', 'ActionA', 'Dash')")]
        public string buttonName = "Jump";
        
        [Tooltip("Keyboard key that triggers this button")]
        public KeyCode keyCode = KeyCode.Space;
        
        [Tooltip("Alternative key (optional)")]
        public KeyCode alternativeKey = KeyCode.None;
    }
}

