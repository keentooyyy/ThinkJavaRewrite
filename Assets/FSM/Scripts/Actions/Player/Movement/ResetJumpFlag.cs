using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace NodeCanvas.Tasks.Actions
{
    [Category("✫ Custom/Player Movement")]
    [Description("Reset jump pressed flag (prevents re-triggering while in air)")]
    public class ResetJumpFlag : ActionTask
    {
        [BlackboardOnly]
        public BBParameter<bool> jumpPressed;

        protected override string info
        {
            get { return "Reset Jump Flag"; }
        }

        protected override void OnExecute()
        {
            jumpPressed.value = false;
            EndAction(true);
        }
    }
}

