using UnityEngine;

namespace GameInput
{
    /// <summary>
    /// Input configuration asset - create in Unity via:
    /// Assets > Create > Game Config > Input Config
    /// </summary>
    [CreateAssetMenu(fileName = "InputConfig", menuName = "Game Config/Input Config", order = 1)]
    public class InputConfig : ScriptableObject
    {
        [Header("Keyboard Bindings")]
        [Tooltip("Add all your button bindings here - visible in Inspector!")]
        public InputBinding[] keyboardBindings = new InputBinding[]
        {
            new InputBinding { buttonName = "Jump", keyCode = KeyCode.Space },
            new InputBinding { buttonName = "ActionA", keyCode = KeyCode.E },
            new InputBinding { buttonName = "ActionB", keyCode = KeyCode.F },
        };
        
        [Header("Axis Settings")]
        [Tooltip("Unity Input Manager axis name for horizontal movement")]
        public string horizontalAxisName = "Horizontal";
        
        [Tooltip("Unity Input Manager axis name for vertical movement")]
        public string verticalAxisName = "Vertical";
    }
}

