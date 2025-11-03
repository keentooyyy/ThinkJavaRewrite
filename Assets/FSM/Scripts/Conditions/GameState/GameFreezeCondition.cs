using NodeCanvas.Framework;
using ParadoxNotion.Design;
using GameState;

namespace NodeCanvas.Tasks.Conditions
{
    [Category("Custom/Game State")]
    [Name("Check Game Freeze")]
    [Description("Returns true when the requested freeze type matches the active state.")]
    public class GameFreezeCondition : ConditionTask
    {
        public GameFreezeType freezeType = GameFreezeType.Full;

        protected override bool OnCheck()
        {
            if (freezeType == GameFreezeType.None)
            {
                return !GameFreezeManager.IsFrozen;
            }

            return GameFreezeManager.CurrentFreeze == freezeType;
        }
    }
}

