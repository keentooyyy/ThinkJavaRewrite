using NodeCanvas.Framework;
using ParadoxNotion.Design;
using GameState;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions
{
    [Category("Custom/Game State")]
    [Description("Configure which input buttons remain active during dialogue or full game freeze states.")]
    public class ConfigureGameFreezeButtons : ActionTask
    {
        [Tooltip("Buttons allowed while Dialogue freeze is active (e.g. confirm/advance dialogue).")]
        public string[] dialogueButtons = new[] { "ActionA", "ActionB" };

        [Tooltip("Buttons allowed while Full freeze is active.")]
        public string[] fullFreezeButtons = new string[0];

        protected override void OnExecute()
        {
            GameFreezeManager.ConfigureDialogueWhitelist(dialogueButtons);
            GameFreezeManager.ConfigureFullWhitelist(fullFreezeButtons);
            EndAction(true);
        }
    }
}

