using NodeCanvas.Framework;
using ParadoxNotion.Design;
using GameState;

namespace NodeCanvas.Tasks.Actions
{
    [Category("Custom/Game State")]
    [Description("Set the current game freeze mode (None, Dialogue, Full).")]
    public class SetGameFreezeAction : ActionTask
    {
        public GameFreezeType freezeType = GameFreezeType.Full;

        protected override void OnExecute()
        {
            GameFreezeManager.SetFreeze(freezeType);
            EndAction(true);
        }
    }
}

