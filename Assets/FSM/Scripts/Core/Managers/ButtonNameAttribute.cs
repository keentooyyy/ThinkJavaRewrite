using UnityEngine;

namespace GameInput
{
    /// <summary>
    /// Attribute to show a dropdown of button names from InputConfig
    /// Usage: [ButtonName] public string buttonName;
    /// The field will show a dropdown populated from the InputConfig's keyboard bindings
    /// </summary>
    public class ButtonNameAttribute : PropertyAttribute
    {
        public string configFieldName;
        
        /// <summary>
        /// Creates a ButtonName dropdown attribute
        /// </summary>
        /// <param name="configFieldName">Name of the InputConfig field on the same component (default: "inputConfig")</param>
        public ButtonNameAttribute(string configFieldName = "inputConfig")
        {
            this.configFieldName = configFieldName;
        }
    }
}

