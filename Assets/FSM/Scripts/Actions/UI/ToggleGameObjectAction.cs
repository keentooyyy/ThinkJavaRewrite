using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions
{
    /// <summary>
    /// Toggles GameObject active state or sets it to a specific state.
    /// Useful for showing/hiding UI elements.
    /// </summary>
    [Category("GameObject")]
    [Description("Show, hide, or toggle GameObjects (UI elements, panels, etc.)")]
    public class ToggleGameObjectAction : ActionTask
    {
        public enum ToggleMode
        {
            Show,
            Hide,
            Toggle
        }

        [Tooltip("The GameObject to show/hide")]
        public BBParameter<GameObject> targetObject;

        [Tooltip("What to do with the GameObject")]
        public ToggleMode mode = ToggleMode.Toggle;

        protected override string info
        {
            get { return string.Format("{0} {1}", mode, targetObject); }
        }

        protected override void OnExecute()
        {
            if (targetObject.value == null)
            {
                EndAction(false);
                return;
            }

            switch (mode)
            {
                case ToggleMode.Show:
                    targetObject.value.SetActive(true);
                    break;

                case ToggleMode.Hide:
                    targetObject.value.SetActive(false);
                    break;

                case ToggleMode.Toggle:
                    targetObject.value.SetActive(!targetObject.value.activeSelf);
                    break;
            }

            EndAction(true);
        }
    }
}
